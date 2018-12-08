using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using RandomizerMod.Extensions;
using RandomizerMod.FsmStateActions;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace RandomizerMod
{
    public class RandomizerMod : Mod<SaveSettings>
    {
        private static FieldInfo sceneLoad = typeof(GameManager).GetField("sceneLoad", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo sceneLoadRunner = typeof(SceneLoad).GetField("runner", BindingFlags.NonPublic | BindingFlags.Instance);

        private static Dictionary<string, Sprite> sprites;

        private static Thread logicParseThread;

        public static RandomizerMod Instance { get; private set; }

        [SuppressMessage(
            "Microsoft.StyleCop.CSharp.OrderingRules",
            "SA1204:StaticElementsMustAppearBeforeInstanceElements",
            Justification = "Initialize is essentially the class constructor")]
        public override void Initialize()
        {
            if (Instance != null)
            {
                LogWarn("Attempting to make multiple instances of mod, ignoring");
                return;
            }

            // Set instance for outside use
            Instance = this;

            // Make sure the play mode screen is always unlocked
            GameManager.instance.EnablePermadeathMode();

            // Unlock godseeker too because idk why not
            GameManager.instance.SetStatusRecordInt("RecBossRushMode", 1);

            sprites = new Dictionary<string, Sprite>();

            // Load logo and xml from embedded resources
            Assembly randoDLL = GetType().Assembly;
            foreach (string res in randoDLL.GetManifestResourceNames())
            {
                if (res.EndsWith(".png"))
                {
                    // Read bytes of image
                    Stream imageStream = randoDLL.GetManifestResourceStream(res);
                    byte[] buffer = new byte[imageStream.Length];
                    imageStream.Read(buffer, 0, buffer.Length);
                    imageStream.Dispose();

                    // Create texture from bytes
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(buffer);

                    // Create sprite from texture
                    sprites.Add(
                        Path.GetFileNameWithoutExtension(res.Replace("RandomizerMod.Resources.", string.Empty)),
                        Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));

                    LogDebug("Created sprite from embedded image: " + res);
                }
                else if (res.EndsWith("language.xml"))
                {
                    // No sense having the whole init die if this xml is formatted improperly
                    try
                    {
                        LanguageStringManager.LoadLanguageXML(randoDLL.GetManifestResourceStream(res));
                    }
                    catch (Exception e)
                    {
                        LogError("Could not process language xml:\n" + e);
                    }
                }
                else if (res.EndsWith("items.xml"))
                {
                    // Thread the xml parsing because it's kinda slow
                    logicParseThread = new Thread(new ParameterizedThreadStart(Randomization.LogicManager.ParseXML));
                    logicParseThread.Start(randoDLL.GetManifestResourceStream(res));
                }
                else
                {
                    Log("Unknown resource " + res);
                }
            }

            // Add hooks
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += HandleSceneChanges;
            ModHooks.Instance.LanguageGetHook += LanguageStringManager.GetLanguageString;
            ModHooks.Instance.GetPlayerIntHook += IntOverride;
            ModHooks.Instance.GetPlayerBoolHook += BoolGetOverride;
            ModHooks.Instance.SetPlayerBoolHook += BoolSetOverride;

            On.PlayerData.SetBenchRespawn_RespawnMarker_string_int += BenchHandler.HandleBenchSave;
            On.PlayerData.SetBenchRespawn_string_string_bool += BenchHandler.HandleBenchSave;
            On.PlayerData.SetBenchRespawn_string_string_int_bool += BenchHandler.HandleBenchSave;
            On.HutongGames.PlayMaker.Actions.BoolTest.OnEnter += BenchHandler.HandleBenchBoolTest;

            On.PlayMakerFSM.OnEnable += Actions.RandomizerAction.ProcessFSM;

            ModHooks.Instance.ObjectPoolSpawnHook += FixExplosionPogo;

            // Preload shiny item
            // Can't thread this because Unity sucks
            Components.Preloader.Preload();

            // Load fonts
            FontManager.LoadFonts();
        }

        public static Sprite GetSprite(string name)
        {
            if (sprites != null && sprites.TryGetValue(name, out Sprite sprite))
            {
                return sprite;
            }

            return null;
        }

        public static bool LoadComplete()
        {
            return logicParseThread == null || !logicParseThread.IsAlive;
        }

        public void StartNewGame()
        {
            // Charm tutorial popup is annoying, get rid of it
            PlayerData.instance.hasCharm = true;

            // Fast boss intros
            PlayerData.instance.unchainedHollowKnight = true;
            PlayerData.instance.encounteredMimicSpider = true;
            PlayerData.instance.infectedKnightEncountered = true;
            PlayerData.instance.mageLordEncountered = true;
            PlayerData.instance.mageLordEncountered_2 = true;

            if (Settings.AllBosses)
            {
                // TODO: Think of a better way to handle Zote
                PlayerData.instance.zoteRescuedBuzzer = true;
                PlayerData.instance.zoteRescuedDeepnest = true;
            }

            if (Settings.Randomizer)
            {
                if (!LoadComplete())
                {
                    logicParseThread.Join();
                }

                try
                {
                    Randomization.Randomizer.Randomize();
                }
                catch (Exception e)
                {
                    LogError("Error in randomization:\n" + e);
                }

                Settings.actions = new List<Actions.RandomizerAction>();
                Settings.actions.AddRange(Randomization.Randomizer.Actions);
            }
        }

        public void ChangeToScene(string sceneName, string gateName, float delay = 0f)
        {
            if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(gateName))
            {
                Log("Empty string passed into ChangeToScene, ignoring");
                return;
            }

            SceneLoad.FinishDelegate loadScene = () =>
            {
                GameManager.instance.StopAllCoroutines();
                sceneLoad.SetValue(GameManager.instance, null);

                GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo()
                {
                    IsFirstLevelForPlayer = false,
                    SceneName = sceneName,
                    HeroLeaveDirection = GetGatePosition(gateName),
                    EntryGateName = gateName,
                    EntryDelay = delay,
                    PreventCameraFadeOut = false,
                    WaitForSceneTransitionCameraFade = true,
                    Visualization = GameManager.SceneLoadVisualizations.Default,
                    AlwaysUnloadUnusedAssets = false
                });
            };

            SceneLoad load = (SceneLoad)sceneLoad.GetValue(GameManager.instance);
            if (load != null)
            {
                load.Finish += loadScene;
            }
            else
            {
                loadScene.Invoke();
            }
        }

        public override string GetVersion()
        {
            string ver = "2b.10";
            int minAPI = 45;

            bool apiTooLow = Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minAPI;
            if (apiTooLow)
            {
                return ver + " (Update API)";
            }

            return ver;
        }

        // Deleting this would break another mod
        private static bool SceneHasPreload(string sceneName) => false;

        private static GlobalEnums.GatePosition GetGatePosition(string name)
        {
            if (name.Contains("top"))
            {
                return GlobalEnums.GatePosition.top;
            }

            if (name.Contains("bot"))
            {
                return GlobalEnums.GatePosition.bottom;
            }

            if (name.Contains("left"))
            {
                return GlobalEnums.GatePosition.left;
            }

            if (name.Contains("right"))
            {
                return GlobalEnums.GatePosition.right;
            }

            if (name.Contains("door"))
            {
                return GlobalEnums.GatePosition.door;
            }

            return GlobalEnums.GatePosition.unknown;
        }

        private GameObject FixExplosionPogo(GameObject go)
        {
            if (go.name.StartsWith("Gas Explosion Recycle M"))
            {
                go.layer = (int)GlobalEnums.PhysLayers.ENEMIES;
                NonBouncer noFun = go.GetComponent<NonBouncer>();
                if (noFun)
                {
                    noFun.active = false;
                }
            }

            return go;
        }

        private void UpdateCharmNotches(PlayerData pd, HeroController controller)
        {
            // Update charm notches
            if (Settings.CharmNotch)
            {
                if (pd == null)
                {
                    return;
                }

                pd.CountCharms();
                int charms = pd.charmsOwned;
                int notches = pd.charmSlots;

                if (!pd.salubraNotch1 && charms >= 5)
                {
                    pd.SetBool("salubraNotch1", true);
                    notches++;
                }

                if (!pd.salubraNotch2 && charms >= 10)
                {
                    pd.SetBool("salubraNotch2", true);
                    notches++;
                }

                if (!pd.salubraNotch3 && charms >= 18)
                {
                    pd.SetBool("salubraNotch3", true);
                    notches++;
                }

                if (!pd.salubraNotch4 && charms >= 25)
                {
                    pd.SetBool("salubraNotch4", true);
                    notches++;
                }

                pd.SetInt("charmSlots", notches);
                GameManager.instance.RefreshOvercharm();
            }
        }

#warning Fix this mess
        [SuppressMessage(
            "Microsoft.StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "Suppressing to turn into one warning. I'll deal with this mess later")]
        private bool BoolGetOverride(string boolName)
        {
            // Fake spell bools
            if (boolName == "hasVengefulSpirit") return PlayerData.instance.fireballLevel > 0;
            if (boolName == "hasShadeSoul") return PlayerData.instance.fireballLevel > 1;
            if (boolName == "hasDesolateDive") return PlayerData.instance.quakeLevel > 0;
            if (boolName == "hasDescendingDark") return PlayerData.instance.quakeLevel > 1;
            if (boolName == "hasHowlingWraiths") return PlayerData.instance.screamLevel > 0;
            if (boolName == "hasAbyssShriek") return PlayerData.instance.screamLevel > 1;
            if (boolName == "gotSlyCharm") return Settings.SlyCharm;

            if (boolName.StartsWith("RandomizerMod.")) return Settings.GetBool(false, boolName.Substring(14));

            return PlayerData.instance.GetBoolInternal(boolName);
        }

        [SuppressMessage(
            "Microsoft.StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "Suppressing to turn into one warning. I'll deal with this mess later")]
        private void BoolSetOverride(string boolName, bool value)
        {
            // Check for Salubra notches if it's a charm
            if (boolName.StartsWith("gotCharm_"))
            {
                UpdateCharmNotches(PlayerData.instance, HeroController.instance);
            }

            // For some reason these all have two bools
            if (boolName == "hasDash") PlayerData.instance.SetBool("canDash", value);
            else if (boolName == "hasShadowDash") PlayerData.instance.SetBool("canShadowDash", value);
            else if (boolName == "hasSuperDash") PlayerData.instance.SetBool("canSuperDash", value);
            else if (boolName == "hasWalljump") PlayerData.instance.SetBool("canWallJump", value);
            else if (boolName == "gotCharm_23") PlayerData.instance.SetBool("fragileHealth_unbreakable", value); // Shade skips make these charms not viable, unbreakable is a nice fix for that
            else if (boolName == "gotCharm_24") PlayerData.instance.SetBool("fragileGreed_unbreakable", value);
            else if (boolName == "gotCharm_25") PlayerData.instance.SetBool("fragileStrength_unbreakable", value);
            else if (boolName == "hasAcidArmour" && value) PlayMakerFSM.BroadcastEvent("GET ACID ARMOUR"); // Gotta update the acid pools after getting this
            else if (boolName == "hasCyclone" || boolName == "hasUpwardSlash" || boolName == "hasDashSlash")
            {
                // Make nail arts work
                PlayerData.instance.SetBoolInternal(boolName, value);
                PlayerData.instance.hasNailArt = PlayerData.instance.hasCyclone || PlayerData.instance.hasUpwardSlash || PlayerData.instance.hasDashSlash;
                PlayerData.instance.hasAllNailArts = PlayerData.instance.hasCyclone && PlayerData.instance.hasUpwardSlash && PlayerData.instance.hasDashSlash;
                return;
            }
            else if (boolName == "hasVengefulSpirit" && value && PlayerData.instance.fireballLevel <= 0) PlayerData.instance.SetInt("fireballLevel", 1); // It's just way easier if I can treat spells as bools
            else if (boolName == "hasVengefulSpirit" && !value) PlayerData.instance.SetInt("fireballLevel", 0);
            else if (boolName == "hasShadeSoul" && value) PlayerData.instance.SetInt("fireballLevel", 2);
            else if (boolName == "hasShadeSoul" && !value && PlayerData.instance.fireballLevel >= 2) PlayerData.instance.SetInt("fireballLevel", 1);
            else if (boolName == "hasDesolateDive" && value && PlayerData.instance.quakeLevel <= 0) PlayerData.instance.SetInt("quakeLevel", 1);
            else if (boolName == "hasDesolateDive" && !value) PlayerData.instance.SetInt("quakeLevel", 0);
            else if (boolName == "hasDescendingDark" && value) PlayerData.instance.SetInt("quakeLevel", 2);
            else if (boolName == "hasDescendingDark" && !value && PlayerData.instance.quakeLevel >= 2) PlayerData.instance.SetInt("quakeLevel", 1);
            else if (boolName == "hasHowlingWraiths" && value && PlayerData.instance.screamLevel <= 0) PlayerData.instance.SetInt("screamLevel", 1);
            else if (boolName == "hasHowlingWraiths" && !value) PlayerData.instance.SetInt("screamLevel", 0);
            else if (boolName == "hasAbyssShriek" && value) PlayerData.instance.SetInt("screamLevel", 2);
            else if (boolName == "hasAbyssShriek" && !value && PlayerData.instance.screamLevel >= 2) PlayerData.instance.SetInt("screamLevel", 1);
            else if (boolName.StartsWith("RandomizerMod."))
            {
                boolName = boolName.Substring(14);
                if (boolName.StartsWith("ShopFireball")) PlayerData.instance.IncrementInt("fireballLevel");
                else if (boolName.StartsWith("ShopQuake")) PlayerData.instance.IncrementInt("quakeLevel");
                else if (boolName.StartsWith("ShopScream")) PlayerData.instance.IncrementInt("screamLevel");
                else if (boolName.StartsWith("ShopDash"))
                {
                    if (PlayerData.instance.hasDash)
                    {
                        PlayerData.instance.SetBool("hasShadowDash", true);
                    }
                    else
                    {
                        PlayerData.instance.SetBool("hasDash", true);
                    }
                }

                Settings.SetBool(value, boolName);
                return;
            }

            PlayerData.instance.SetBoolInternal(boolName, value);
        }

        private int IntOverride(string intName)
        {
            if (intName == "RandomizerMod.Zero")
            {
                return 0;
            }

            return PlayerData.instance.GetIntInternal(intName);
        }

        private void HandleSceneChanges(Scene from, Scene to)
        {
            if (GameManager.instance.GetSceneNameString() == Constants.MENU_SCENE)
            {
                try
                {
                    MenuChanger.EditUI();
                }
                catch (Exception e)
                {
                    LogError("Error editing menu:\n" + e);
                }
            }
            else if (GameManager.instance.GetSceneNameString() == Constants.END_CREDITS && Settings != null && Settings.Randomizer && Settings.itemPlacements.Count != 0)
            {
#warning Unfinished functionality here
                /*foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    Object.Destroy(obj);
                }

                GameObject canvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
                float y = -30;
                foreach (KeyValuePair<string, string> item in Settings.itemPlacements)
                {
                    y -= 1020 / Settings.itemPlacements.Count;
                    CanvasUtil.CreateTextPanel(canvas, item.Key + " - " + item.Value, 16, TextAnchor.UpperLeft, new CanvasUtil.RectData(new Vector2(1920, 50), new Vector2(0, y), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0f, 0f)), FontManager.GetFont("Perpetua"));
                }*/
            }

            if (GameManager.instance.IsGameplayScene())
            {
                try
                {
                    // In rare cases, this is called before the previous scene has unloaded
                    // Deleting old randomizer shinies to prevent issues
                    GameObject oldShiny = GameObject.Find("Randomizer Shiny");
                    if (oldShiny != null)
                    {
                        Object.DestroyImmediate(oldShiny);
                    }

                    EditShinies(to);
                }
                catch (Exception e)
                {
                    LogError($"Error applying RandomizerActions to scene {to.name}:\n" + e);
                }
            }

            try
            {
                // These changes should always be applied
                switch (GameManager.instance.GetSceneNameString())
                {
                    case "Room_temple":
                        // Handle completion restrictions
                        RestrictionManager.ProcessRestrictions();
                        break;
                    case "Room_Final_Boss_Core":
                        // Trigger Radiance fight without requiring dream nail hit
                        // Prevents skipping the fight in all bosses mode
                        if (Settings.AllBosses)
                        {
                            PlayMakerFSM dreamFSM = FSMUtility.LocateFSM(to.FindGameObject("Dream Enter"), "Control");
                            SendEvent enterRadiance = new SendEvent
                            {
                                eventTarget = new FsmEventTarget()
                                {
                                    target = FsmEventTarget.EventTarget.FSMComponent,
                                    fsmComponent = dreamFSM
                                },
                                sendEvent = FsmEvent.FindEvent("NAIL HIT"),
                                delay = 0,
                                everyFrame = false
                            };

                            PlayMakerFSM bossFSM = FSMUtility.LocateFSM(to.FindGameObject("Hollow Knight Boss"), "Control");
                            bossFSM.GetState("H Collapsed").AddAction(enterRadiance);
                        }

                        break;
                    case "Cliffs_06":
                        // Prevent banish ending in all bosses
                        if (Settings.AllBosses)
                        {
                            Object.Destroy(GameObject.Find("Brumm Lantern NPV"));
                        }

                        break;
                    case "Ruins1_05b":
                        // Lemm sell all
                        if (Settings.Lemm)
                        {
                            PlayMakerFSM lemm = FSMUtility.LocateFSM(GameObject.Find("Relic Dealer"), "npc_control");
                            lemm.GetState("Convo End").AddAction(new RandomizerSellRelics());
                        }

                        break;
                }

                // These ones are randomizer specific
                if (Settings.Randomizer)
                {
                    switch (GameManager.instance.GetSceneNameString())
                    {
                        case "Abyss_10":
                            // Something might be required here after properly processing shade cloak
                            break;
                        case "Abyss_12":
                            // Destroy shriek pickup if the player doesn't have wraiths
                            if (PlayerData.instance.screamLevel == 0)
                            {
                                Object.Destroy(GameObject.Find("Randomizer Shiny"));
                            }

                            break;
                        case "Ruins1_32":
                            // Platform after soul master
                            if (!PlayerData.instance.hasWalljump)
                            {
                                GameObject plat = Object.Instantiate(GameObject.Find("ruind_int_plat_float_02 (3)"));
                                plat.SetActive(true);
                                plat.transform.position = new Vector2(40.5f, 72f);
                            }

                            // Fall through because there's quake floors to remove here
                            goto case "Ruins1_30";
                        case "Ruins1_30":
                        case "Ruins1_23":
                            // Remove quake floors
                            if (PlayerData.instance.quakeLevel <= 0 && PlayerData.instance.killedMageLord)
                            {
                                foreach (GameObject obj in to.GetRootGameObjects())
                                {
                                    if (obj.name.Contains("Quake Floor") || obj.name.Contains("Quake Window"))
                                    {
                                        Object.Destroy(obj);
                                    }
                                }
                            }

                            break;
                        case "Ruins2_04":
                            // Shield husk doesn't walk as far as on old patches, making something pogoable to make up for this
                            if (!PlayerData.instance.hasWalljump && !PlayerData.instance.hasDoubleJump)
                            {
                                GameObject.Find("Direction Pole White Palace").GetComponent<NonBouncer>().active = false;
                            }

                            break;
                        case "Fungus2_21":
                            // Remove city crest gate
                            if (PlayerData.instance.hasCityKey)
                            {
                                Object.Destroy(GameObject.Find("City Gate Control"));
                                Object.Destroy(GameObject.Find("Ruins_front_gate"));
                            }

                            break;
                        case "Fungus2_26":
                            // Prevent leg eater from doing anything but opening the shop
                            PlayMakerFSM legEater = FSMUtility.LocateFSM(GameObject.Find("Leg Eater"), "Conversation Control");
                            FsmState legEaterChoice = legEater.GetState("Convo Choice");
                            legEaterChoice.RemoveTransitionsTo("Convo 1");
                            legEaterChoice.RemoveTransitionsTo("Convo 2");
                            legEaterChoice.RemoveTransitionsTo("Convo 3");
                            legEaterChoice.RemoveTransitionsTo("Infected Crossroad");
                            legEaterChoice.RemoveTransitionsTo("Bought Charm");
                            legEaterChoice.RemoveTransitionsTo("Gold Convo");
                            legEaterChoice.RemoveTransitionsTo("All Gold");
                            legEaterChoice.RemoveTransitionsTo("Ready To Leave");
                            legEater.GetState("All Gold?").RemoveTransitionsTo("No Shop");

                            // Just in case something other than the "Ready To Leave" state controls this
                            PlayerData.instance.legEaterLeft = false;
                            break;
                        case "Crossroads_ShamanTemple":
                            // Remove gate in shaman hut
                            Object.Destroy(GameObject.Find("Bone Gate"));

                            // Add hard save to shaman shiny
                            FSMUtility.LocateFSM(GameObject.Find("Randomizer Shiny"), "Shiny Control").GetState("Finish").AddAction(new RandomizerSetHardSave());

                            // Fall through to patch mound baldur as well
                            goto case "Crossroads_11_alt";
                        case "Crossroads_11_alt":
                        case "Fungus1_28":
                            // Make baldurs always able to spit rollers
                            foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
                            {
                                if (obj.name.Contains("Blocker"))
                                {
                                    PlayMakerFSM fsm = FSMUtility.LocateFSM(obj, "Blocker Control");
                                    if (fsm != null)
                                    {
                                        fsm.GetState("Can Roller?").RemoveActionsOfType<IntCompare>();
                                    }
                                }
                            }

                            break;
                        case "Ruins1_01":
                            // Add platform to stop quirrel bench soft lock
                            if (!PlayerData.instance.hasWalljump)
                            {
                                GameObject plat2 = Object.Instantiate(GameObject.Find("ruind_int_plat_float_01"));
                                plat2.SetActive(true);
                                plat2.transform.position = new Vector2(116, 14);
                            }

                            break;
                        case "Ruins1_02":
                            // Add platform to stop quirrel bench soft lock
                            if (!PlayerData.instance.hasWalljump)
                            {
                                GameObject plat3 = Object.Instantiate(GameObject.Find("ruind_int_plat_float_01"));
                                plat3.SetActive(true);
                                plat3.transform.position = new Vector2(2, 61.5f);
                            }

                            break;
                        case "Ruins1_05":
                            // Slight adjustment to breakable so wings is enough to progress, just like on old patches
                            if (!PlayerData.instance.hasWalljump)
                            {
                                GameObject chandelier = GameObject.Find("ruind_dressing_light_02 (10)");
                                chandelier.transform.SetPositionX(chandelier.transform.position.x - 2);
                                chandelier.GetComponent<NonBouncer>().active = false;
                            }

                            break;
                        case "Mines_33":
                            // Make tolls always interactable
                            GameObject[] tolls = new GameObject[] { GameObject.Find("Toll Gate Machine"), GameObject.Find("Toll Gate Machine (1)") };
                            foreach (GameObject toll in tolls)
                            {
                                Object.Destroy(FSMUtility.LocateFSM(toll, "Disable if No Lantern"));
                            }

                            break;
                        case "Mines_35":
                            foreach (NonBouncer nonBounce in Object.FindObjectsOfType<NonBouncer>())
                            {
                                if (nonBounce.gameObject.name.StartsWith("Spike Collider"))
                                {
                                    nonBounce.active = false;
                                    TinkEffect spikeTink = nonBounce.gameObject.AddComponent<TinkEffect>();
                                    spikeTink.blockEffect = Object.Instantiate(ObjectCache.TinkEffect);
                                    spikeTink.blockEffect.transform.SetParent(nonBounce.transform);
                                    spikeTink.useNailPosition = true;

                                    // Spawn extension does not work the first time it is called
                                    // Need to call it once here so that the TinkEffect component works on the first try
                                    spikeTink.blockEffect.Spawn();
                                }
                            }

                            break;
                        case "Fungus1_04":
                            // Open gates after Hornet fight
                            foreach (PlayMakerFSM childFSM in GameObject.Find("Cloak Corpse").GetComponentsInChildren<PlayMakerFSM>(true))
                            {
                                if (childFSM.FsmName == "Shiny Control")
                                {
                                    SendEvent openGate = new SendEvent
                                    {
                                        eventTarget = new FsmEventTarget()
                                        {
                                            target = FsmEventTarget.EventTarget.BroadcastAll,
                                            excludeSelf = true
                                        },
                                        sendEvent = FsmEvent.FindEvent("BG OPEN"),
                                        delay = 0,
                                        everyFrame = false
                                    };
                                    childFSM.GetState("Destroy").AddFirstAction(openGate);
                                    childFSM.GetState("Finish").AddFirstAction(openGate);

                                    break;
                                }
                            }

                            // Destroy everything relating to the dreamer cutscene
                            // This stuff is in another scene and doesn't exist immediately, so I can't use Object.Destroy
                            Components.ObjectDestroyer.Destroy("Dreamer Scene 1");
                            Components.ObjectDestroyer.Destroy("Hornet Saver");
                            Components.ObjectDestroyer.Destroy("Cutscene Dreamer");
                            Components.ObjectDestroyer.Destroy("Dream Scene Activate");

                            // Fix the camera lock zone by removing the FSM that destroys it
                            if (!PlayerData.instance.hornet1Defeated)
                            {
                                Object.Destroy(FSMUtility.LocateFSM(GameObject.Find("Camera Locks Boss"), "FSM"));
                            }

                            break;
                        case "Ruins1_24":
                            // Pickup (Quake Pickup) -> Idle -> GetPlayerDataInt (quakeLevel)
                            // Quake (Quake Item) -> Get -> SetPlayerDataInt (quakeLevel)
                            // Stop spell container from destroying itself
                            PlayMakerFSM quakePickup = FSMUtility.LocateFSM(GameObject.Find("Quake Pickup"), "Pickup");
                            quakePickup.GetState("Idle").RemoveActionsOfType<IntCompare>();

                            foreach (PlayMakerFSM childFSM in quakePickup.gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
                            {
                                if (childFSM.FsmName == "Shiny Control")
                                {
                                    // Make spell container spawn shiny instead
                                    quakePickup.GetState("Appear").GetActionsOfType<ActivateGameObject>()[1].gameObject.GameObject.Value = childFSM.gameObject;

                                    // Make shiny open gates on pickup/destroy
                                    SendEvent openGate = new SendEvent
                                    {
                                        eventTarget = new FsmEventTarget()
                                        {
                                            target = FsmEventTarget.EventTarget.BroadcastAll,
                                            excludeSelf = true
                                        },
                                        sendEvent = FsmEvent.FindEvent("BG OPEN"),
                                        delay = 0,
                                        everyFrame = false
                                    };
                                    childFSM.GetState("Destroy").AddFirstAction(openGate);
                                    childFSM.GetState("Finish").AddFirstAction(openGate);

                                    // Add hard save after picking up item
                                    childFSM.GetState("Finish").AddFirstAction(new RandomizerSetHardSave());

                                    break;
                                }

                                // Stop the weird invisible floor from appearing if dive has been obtained
                                // I don't think it really serves any purpose, so destroying it should be fine
                                if (PlayerData.instance.quakeLevel > 0)
                                {
                                    Object.Destroy(GameObject.Find("Roof Collider Battle"));
                                }
                            }

                            break;
                        case "Dream_Nailcollection":
                            // Make picking up shiny load new scene
                            FSMUtility.LocateFSM(GameObject.Find("Randomizer Shiny"), "Shiny Control").GetState("Finish").AddAction(new RandomizerChangeScene("RestingGrounds_07", "right1"));
                            break;
                        case "Room_nailmaster_03":
                            // Dash slash room
                            // Remove pickup if the player doesn't have enough geo for it
                            if (PlayerData.instance.geo < 800)
                            {
                                Object.Destroy(GameObject.Find("Randomizer Shiny"));
                            }
                            else
                            {
                                // Otherwise, make them lose the geo on picking it up
                                FSMUtility.LocateFSM(GameObject.Find("Randomizer Shiny"), "Shiny Control").GetState("Finish").AddAction(new RandomizerTakeGeo(800));
                            }

                            break;
                        case "Room_Sly_Storeroom":
                            // Make Sly pickup send Sly back upstairs
                            FsmState slyFinish = FSMUtility.LocateFSM(GameObject.Find("Randomizer Shiny"), "Shiny Control").GetState("Finish");
                            slyFinish.AddAction(new RandomizerSetBool("SlyCharm", true));

                            // The game breaks if you leave the storeroom after this, so just send the player out of the shop completely
                            // People will think it's an intentional feature to cut out pointless walking anyway
                            slyFinish.AddAction(new RandomizerChangeScene("Town", "door_sly"));
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                LogError($"Error applying changes to scene {to.name}:\n" + e);
            }
        }

        private void EditShinies(Scene to)
        {
            string scene = GameManager.instance.GetSceneNameString();

            foreach (Actions.RandomizerAction action in Settings.actions)
            {
                if (action.Type == Actions.RandomizerAction.ActionType.GameObject)
                {
                    try
                    {
                        action.Process(scene, null);
                    }
                    catch (Exception e)
                    {
                        LogError($"Error processing action of type {action.GetType()}:\n{JsonUtility.ToJson(action)}\n{e}");
                    }
                }
            }
        }
    }
}
