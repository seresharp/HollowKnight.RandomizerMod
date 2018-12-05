using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using RandomizerMod.FsmStateActions;

using Object = UnityEngine.Object;

namespace RandomizerMod
{
    public class RandomizerMod : Mod<SaveSettings>
    {
        private static List<string> sceneNames;

        private static FieldInfo smallGeoPrefabField = typeof(HealthManager).GetField("smallGeoPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo mediumGeoPrefabField = typeof(HealthManager).GetField("mediumGeoPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo largeGeoPrefabField = typeof(HealthManager).GetField("largeGeoPrefab", BindingFlags.NonPublic | BindingFlags.Instance);

        private static FieldInfo sceneLoad = typeof(GameManager).GetField("sceneLoad", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo sceneLoadRunner = typeof(SceneLoad).GetField("runner", BindingFlags.NonPublic | BindingFlags.Instance);

        public static GameObject smallGeoPrefab;
        public static GameObject mediumGeoPrefab;
        public static GameObject largeGeoPrefab;

        public static Dictionary<string, Sprite> sprites;

        private Thread randomizeThread;

        public static RandomizerMod instance { get; private set; }

        public override void Initialize()
        {
            //Set instance for outside use
            instance = this;

            //Make sure the play mode screen is always unlocked
            GameManager.instance.EnablePermadeathMode();

            //Unlock godseeker too because idk why not
            GameManager.instance.SetStatusRecordInt("RecBossRushMode", 1);

            sprites = new Dictionary<string, Sprite>();

            //Load logo and xml from embedded resources
            Assembly randoDLL = GetType().Assembly;
            foreach (string res in randoDLL.GetManifestResourceNames())
            {
                if (res.EndsWith(".png"))
                {
                    //Read bytes of image
                    Stream imageStream = randoDLL.GetManifestResourceStream(res);
                    byte[] buffer = new byte[imageStream.Length];
                    imageStream.Read(buffer, 0, buffer.Length);
                    imageStream.Dispose();

                    //Create texture from bytes
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(buffer);

                    //Create sprite from texture
                    sprites.Add(res.Replace("RandomizerMod.Resources.", ""), Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
                    LogDebug("Created sprite from embedded image: " + res);
                }
                else if (res.EndsWith("language.xml"))
                {
                    //No sense having the whole init die if this xml is formatted improperly
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
                    try
                    {
                        Randomization.LogicManager.ParseXML(randoDLL.GetManifestResourceStream(res));
                    }
                    catch (Exception e)
                    {
                        LogError("Could not process items xml:\n" + e);
                    }
                }
                else
                {
                    Log("Unknown resource " + res);
                }
            }

            //Add hooks
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += HandleSceneChanges;
            ModHooks.Instance.OnEnableEnemyHook += GetGeoPrefabs;
            ModHooks.Instance.LanguageGetHook += LanguageStringManager.GetLanguageString;
            ModHooks.Instance.GetPlayerIntHook += IntOverride;
            ModHooks.Instance.GetPlayerBoolHook += BoolGetOverride;
            ModHooks.Instance.SetPlayerBoolHook += BoolSetOverride;

            On.PlayerData.SetBenchRespawn_RespawnMarker_string_int += BenchHandler.HandleBenchSave;
            On.PlayerData.SetBenchRespawn_string_string_bool += BenchHandler.HandleBenchSave;
            On.PlayerData.SetBenchRespawn_string_string_int_bool += BenchHandler.HandleBenchSave;
            On.HutongGames.PlayMaker.Actions.BoolTest.OnEnter += BenchHandler.HandleBenchBoolTest;

            // Preload shiny item
            Components.ShinyPreloader.Preload();
        }

        private void UpdateCharmNotches(PlayerData pd, HeroController controller)
        {
            //Update charm notches
            if (Settings.charmNotch)
            {
                if (pd == null) return;

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

        public override string GetVersion()
        {
            string ver = "2b.9";
            int minAPI = 45;

            bool apiTooLow = Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minAPI;
            if (apiTooLow) ver += " (Update API)";

            return ver;
        }

        private bool BoolGetOverride(string boolName)
        {
            //Fake spell bools
            if (boolName == "hasVengefulSpirit") return PlayerData.instance.fireballLevel > 0;
            if (boolName == "hasShadeSoul") return PlayerData.instance.fireballLevel > 1;
            if (boolName == "hasDesolateDive") return PlayerData.instance.quakeLevel > 0;
            if (boolName == "hasDescendingDark") return PlayerData.instance.quakeLevel > 1;
            if (boolName == "hasHowlingWraiths") return PlayerData.instance.screamLevel > 0;
            if (boolName == "hasAbyssShriek") return PlayerData.instance.screamLevel > 1;
            if (boolName == "gotSlyCharm") return Settings.slyCharm;

            if (boolName.StartsWith("RandomizerMod.")) return Settings.GetBool(false, boolName.Substring(14));

            return PlayerData.instance.GetBoolInternal(boolName);
        }

        private void BoolSetOverride(string boolName, bool value)
        {
            // Check for Salubra notches if it's a charm
            if (boolName.StartsWith("gotCharm_"))
            {
                UpdateCharmNotches(PlayerData.instance, HeroController.instance);
            }

            //For some reason these all have two bools
            if (boolName == "hasDash") PlayerData.instance.SetBool("canDash", value);
            else if (boolName == "hasShadowDash") PlayerData.instance.SetBool("canShadowDash", value);
            else if (boolName == "hasSuperDash") PlayerData.instance.SetBool("canSuperDash", value);
            else if (boolName == "hasWalljump") PlayerData.instance.SetBool("canWallJump", value);
            //Shade skips make these charms not viable, unbreakable is a nice fix for that
            else if (boolName == "gotCharm_23") PlayerData.instance.SetBool("fragileHealth_unbreakable", value);
            else if (boolName == "gotCharm_24") PlayerData.instance.SetBool("fragileGreed_unbreakable", value);
            else if (boolName == "gotCharm_25") PlayerData.instance.SetBool("fragileStrength_unbreakable", value);
            //Gotta update the acid pools after getting this
            else if (boolName == "hasAcidArmour" && value) PlayMakerFSM.BroadcastEvent("GET ACID ARMOUR");
            //Make nail arts work
            else if (boolName == "hasCyclone" || boolName == "hasUpwardSlash" || boolName == "hasDashSlash")
            {
                PlayerData.instance.SetBoolInternal(boolName, value);
                PlayerData.instance.hasNailArt = PlayerData.instance.hasCyclone || PlayerData.instance.hasUpwardSlash || PlayerData.instance.hasDashSlash;
                PlayerData.instance.hasAllNailArts = PlayerData.instance.hasCyclone && PlayerData.instance.hasUpwardSlash && PlayerData.instance.hasDashSlash;
                return;
            }
            //It's just way easier if I can treat spells as bools
            else if (boolName == "hasVengefulSpirit" && value && PlayerData.instance.fireballLevel <= 0) PlayerData.instance.SetInt("fireballLevel", 1);
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
            if (intName == "RandomizerMod.Zero") return 0;
            return PlayerData.instance.GetIntInternal(intName);
        }

        private bool GetGeoPrefabs(GameObject enemy, bool isAlreadyDead)
        {
            if (smallGeoPrefab == null || mediumGeoPrefab == null || largeGeoPrefab == null)
            {
                HealthManager hm = enemy.GetComponent<HealthManager>();

                if (hm != null)
                {
                    smallGeoPrefab = (GameObject)smallGeoPrefabField.GetValue(hm);
                    mediumGeoPrefab = (GameObject)mediumGeoPrefabField.GetValue(hm);
                    largeGeoPrefab = (GameObject)largeGeoPrefabField.GetValue(hm);
                }
            }

            return isAlreadyDead;
        }

        private void HandleSceneChanges(Scene from, Scene to)
        {
            if (GameManager.instance.GetSceneNameString() == Constants.MENU_SCENE)
            {
                try
                {
                    EditUI();
                }
                catch (Exception e)
                {
                    LogError("Error editing menu:\n" + e);
                }
            }
            else if (GameManager.instance.GetSceneNameString() == Constants.END_CREDITS && Settings != null && Settings.randomizer && Settings.itemPlacements.Count != 0)
            {
                foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    Object.Destroy(obj);
                }

                GameObject canvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
                float y = -30;
                foreach (KeyValuePair<string, string> item in Settings.itemPlacements)
                {
                    y -= 1020 / Settings.itemPlacements.Count;
                    CanvasUtil.CreateTextPanel(canvas, item.Key + " - " + item.Value, 16, TextAnchor.UpperLeft, new CanvasUtil.RectData(new Vector2(1920, 50), new Vector2(0, y), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0f, 0f)), Components.BigItemPopup.perpetua);
                }
            }

            if (GameManager.instance.IsGameplayScene())
            {
                try
                {
                    if (randomizeThread != null)
                    {
                        if (randomizeThread.IsAlive)
                        {
                            randomizeThread.Join();
                        }

                        if (Randomization.Randomizer.randomizeDone)
                        {
                            Settings.actions = Randomization.Randomizer.actions;
                        }
                        else
                        {
                            LogWarn("Gameplay starting before randomization completed");
                        }
                    }

                    //This is called too late when unloading scenes with preloads
                    //Reload to fix this
                    if (SceneHasPreload(from.name) && WorldInfo.NameLooksLikeGameplayScene(to.name) && !string.IsNullOrEmpty(GameManager.instance.entryGateName))
                    {
                        Log($"Detected preload scene {from.name}, reloading {to.name} ({GameManager.instance.entryGateName})");
                        ChangeToScene(to.name, GameManager.instance.entryGateName);
                        return;
                    }

                    //In rare cases, this is called before the previous scene has unloaded
                    //Deleting old randomizer shinies to prevent issues
                    GameObject oldShiny = GameObject.Find("Randomizer Shiny");
                    if (oldShiny != null) Object.DestroyImmediate(oldShiny);

                    EditShinies(to);
                }
                catch (Exception e)
                {
                    LogError($"Error applying RandomizerActions to scene {to.name}:\n" + e);
                }
            }

            try
            {
                //These changes should always be applied
                switch (GameManager.instance.GetSceneNameString())
                {
                    case "Room_temple":
                        //Handle completion restrictions
                        RestrictionManager.ProcessRestrictions();
                        break;
                    case "Room_Final_Boss_Core":
                        //Trigger Radiance fight without requiring dream nail hit
                        //Prevents skipping the fight in all bosses mode
                        if (Settings.allBosses)
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
                        //Prevent banish ending in all bosses
                        if (Settings.allBosses) Object.Destroy(GameObject.Find("Brumm Lantern NPV"));
                        break;
                    case "Ruins1_05b":
                        //Lemm sell all
                        if (Settings.lemm)
                        {
                            PlayMakerFSM lemm = FSMUtility.LocateFSM(GameObject.Find("Relic Dealer"), "npc_control");
                            lemm.GetState("Convo End").AddAction(new RandomizerSellRelics());
                        }
                        break;
                }

                //These ones are randomizer specific
                if (Settings.randomizer)
                {
                    switch (GameManager.instance.GetSceneNameString())
                    {
                        case "Crossroads_ShamanTemple":
                            //Remove gate in shaman hut
                            //Will be unnecessary if I get around to patching spell FSMs
                            Object.Destroy(GameObject.Find("Bone Gate"));

                            //Stop baldur from closing
                            PlayMakerFSM blocker = FSMUtility.LocateFSM(GameObject.Find("Blocker"), "Blocker Control");
                            blocker.GetState("Idle").RemoveTransitionsTo("Close");
                            blocker.GetState("Shot Anim End").RemoveTransitionsTo("Close");

                            //Add hard save to shaman shiny
                            FSMUtility.LocateFSM(GameObject.Find("Randomizer Shiny"), "Shiny Control").GetState("Finish").AddAction(new RandomizerSetHardSave());
                            break;
                        case "Abyss_10":
                            //Something might be required here after properly processing shade cloak
                            break;
                        case "Abyss_12":
                            //Destroy shriek pickup if the player doesn't have wraiths
                            if (PlayerData.instance.screamLevel == 0)
                            {
                                Object.Destroy(GameObject.Find("Randomizer Shiny"));
                            }
                            break;
                        case "Ruins1_32":
                            //Platform after soul master
                            if (!PlayerData.instance.hasWalljump)
                            {
                                GameObject plat = Object.Instantiate(GameObject.Find("ruind_int_plat_float_02 (3)"));
                                plat.SetActive(true);
                                plat.transform.position = new Vector2(40.5f, 72f);
                            }

                            //Fall through because there's quake floors to remove here
                            goto case "Ruins1_30";
                        case "Ruins1_30":
                        case "Ruins1_23":
                            //Remove quake floors
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
                            //Shield husk doesn't walk as far as on old patches, making something pogoable to make up for this
                            if (!PlayerData.instance.hasWalljump && !PlayerData.instance.hasDoubleJump)
                            {
                                GameObject.Find("Direction Pole White Palace").GetComponent<NonBouncer>().active = false;
                            }
                            break;
                        case "Fungus2_21":
                            //Remove city crest gate
                            if (PlayerData.instance.hasCityKey)
                            {
                                Object.Destroy(GameObject.Find("City Gate Control"));
                                Object.Destroy(GameObject.Find("Ruins_front_gate"));
                            }
                            break;
                        case "Fungus2_26":
                            //Prevent leg eater from doing anything but opening the shop
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

                            //Just in case something other than the "Ready To Leave" state controls this
                            PlayerData.instance.legEaterLeft = false;
                            break;
                        case "Crossroads_11_alt":
                        case "Fungus1_28":
                            //Make baldurs always able to spit rollers
                            foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
                            {
                                if (obj.name.Contains("Blocker"))
                                {
                                    PlayMakerFSM fsm = FSMUtility.LocateFSM(obj, "Blocker Control");
                                    if (fsm != null)
                                    {
                                        fsm.GetState("Can Roller?").RemoveTransitionsTo("Goop");
                                    }
                                }
                            }
                            break;
                        case "Ruins1_01":
                            //Add platform to stop quirrel bench soft lock
                            if (!PlayerData.instance.hasWalljump)
                            {
                                GameObject plat2 = Object.Instantiate(GameObject.Find("ruind_int_plat_float_01"));
                                plat2.SetActive(true);
                                plat2.transform.position = new Vector2(116, 14);
                            }
                            break;
                        case "Ruins1_02":
                            //Add platform to stop quirrel bench soft lock
                            if (!PlayerData.instance.hasWalljump)
                            {
                                GameObject plat3 = Object.Instantiate(GameObject.Find("ruind_int_plat_float_01"));
                                plat3.SetActive(true);
                                plat3.transform.position = new Vector2(2, 61.5f);
                            }
                            break;
                        case "Ruins1_05":
                            //Slight adjustment to breakable so wings is enough to progress, just like on old patches
                            if (!PlayerData.instance.hasWalljump)
                            {
                                GameObject chandelier = GameObject.Find("ruind_dressing_light_02 (10)");
                                chandelier.transform.SetPositionX(chandelier.transform.position.x - 2);
                                chandelier.GetComponent<NonBouncer>().active = false;
                            }
                            break;
                        case "Mines_33":
                            //Make tolls always interactable
                            GameObject[] tolls = new GameObject[] { GameObject.Find("Toll Gate Machine"), GameObject.Find("Toll Gate Machine (1)") };
                            foreach (GameObject toll in tolls)
                            {
                                Object.Destroy(FSMUtility.LocateFSM(toll, "Disable if No Lantern"));
                            }
                            break;
                        case "Fungus1_04":
                            //Open gates after Hornet fight
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

                            //Destroy everything relating to the dreamer cutscene
                            //This stuff is in another scene and doesn't exist immediately, so I can't use Object.Destroy
                            Components.ObjectDestroyer.Destroy("Dreamer Scene 1");
                            Components.ObjectDestroyer.Destroy("Hornet Saver");
                            Components.ObjectDestroyer.Destroy("Cutscene Dreamer");
                            Components.ObjectDestroyer.Destroy("Dream Scene Activate");

                            //Fix the camera lock zone by removing the FSM that destroys it
                            if (!PlayerData.instance.hornet1Defeated)
                            {
                                Object.Destroy(FSMUtility.LocateFSM(GameObject.Find("Camera Locks Boss"), "FSM"));
                            }
                            break;
                        case "Ruins1_24":
                            //Pickup (Quake Pickup) -> Idle -> GetPlayerDataInt (quakeLevel)
                            //Quake (Quake Item) -> Get -> SetPlayerDataInt (quakeLevel)
                            //Stop spell container from destroying itself
                            PlayMakerFSM quakePickup = FSMUtility.LocateFSM(GameObject.Find("Quake Pickup"), "Pickup");
                            quakePickup.GetState("Idle").RemoveActionsOfType<IntCompare>();

                            foreach (PlayMakerFSM childFSM in quakePickup.gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
                            {
                                if (childFSM.FsmName == "Shiny Control")
                                {
                                    //Make spell container spawn shiny instead
                                    quakePickup.GetState("Appear").GetActionsOfType<ActivateGameObject>()[1].gameObject.GameObject.Value = childFSM.gameObject;

                                    //Make shiny open gates on pickup/destroy
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

                                //Stop the weird invisible floor from appearing if dive has been obtained
                                //I don't think it really serves any purpose, so destroying it should be fine
                                if (PlayerData.instance.quakeLevel > 0)
                                {
                                    Object.Destroy(GameObject.Find("Roof Collider Battle"));
                                }
                            }
                            break;
                        case "Dream_Nailcollection":
                            //Make picking up shiny load new scene
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
                            slyFinish.AddAction(new RandomizerSetBool("slyCharm", true));

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
            Actions.RandomizerAction.FetchFSMList(to);
            foreach (Actions.RandomizerAction action in Settings.actions)
            {
                try
                {
                    action.Process();
                }
                catch (Exception e)
                {
                    LogError($"Error processing action of type {action.GetType()}:\n{JsonUtility.ToJson(action)}\n{e}");
                }
            }
        }

        private void EditUI()
        {
            //Reset settings
            Settings = new SaveSettings();

            //Fetch data from vanilla screen
            MenuScreen playScreen = UIManager.instance.playModeMenuScreen;

            playScreen.title.gameObject.transform.localPosition = new Vector3(0, 520.56f);
               
            Object.Destroy(playScreen.topFleur.gameObject);
            
            MenuButton classic = (MenuButton)playScreen.defaultHighlight;
            MenuButton steel = (MenuButton)classic.FindSelectableOnDown();
            MenuButton back = (MenuButton)steel.FindSelectableOnDown();
            
            GameObject parent = steel.transform.parent.gameObject;
            
            Object.Destroy(parent.GetComponent<VerticalLayoutGroup>());

            //Create new buttons
            MenuButton startRandoBtn = classic.Clone("StartRando", MenuButton.MenuButtonType.Proceed, new Vector2(650, -480), "Start Game", "Randomizer v2", sprites["UI.logo.png"]);
            MenuButton startNormalBtn = classic.Clone("StartNormal", MenuButton.MenuButtonType.Proceed, new Vector2(-650, -480), "Start Game", "Non-Randomizer");
            
            startNormalBtn.transform.localScale = startRandoBtn.transform.localScale = new Vector2(0.75f, 0.75f);

            MenuButton backBtn = back.Clone("Back", MenuButton.MenuButtonType.Proceed, new Vector2(0, -100), "Back");

            MenuButton allBossesBtn = back.Clone("AllBosses", MenuButton.MenuButtonType.Activate, new Vector2(0, 850), "All Bosses: False");
            MenuButton allSkillsBtn = back.Clone("AllSkills", MenuButton.MenuButtonType.Activate, new Vector2(0, 760), "All Skills: False");
            MenuButton allCharmsBtn = back.Clone("AllCharms", MenuButton.MenuButtonType.Activate, new Vector2(0, 670), "All Charms: False");

            MenuButton charmNotchBtn = back.Clone("SalubraNotches", MenuButton.MenuButtonType.Activate, new Vector2(900, 850), "Salubra Notches: True");
            MenuButton lemmBtn = back.Clone("LemmSellAll", MenuButton.MenuButtonType.Activate, new Vector2(900, 760), "Lemm Sell All: True");

            MenuButton presetBtn = back.Clone("RandoPreset", MenuButton.MenuButtonType.Activate, new Vector2(-900, 850), "Preset: Easy");
            MenuButton shadeSkipsBtn = back.Clone("ShadeSkips", MenuButton.MenuButtonType.Activate, new Vector2(-900, 760), "Shade Skips: False");
            MenuButton acidSkipsBtn = back.Clone("AcidSkips", MenuButton.MenuButtonType.Activate, new Vector2(-900, 670), "Acid Skips: False");
            MenuButton spikeTunnelsBtn = back.Clone("SpikeTunnelSkips", MenuButton.MenuButtonType.Activate, new Vector2(-900, 580), "Spike Tunnels: False");
            MenuButton miscSkipsBtn = back.Clone("MiscSkips", MenuButton.MenuButtonType.Activate, new Vector2(-900, 490), "Misc Skips: False");
            MenuButton fireballSkipsBtn = back.Clone("FireballSkips", MenuButton.MenuButtonType.Activate, new Vector2(-900, 400), "Fireball Skips: False");
            MenuButton magolorBtn = back.Clone("MagolorSkips", MenuButton.MenuButtonType.Activate, new Vector2(-900, 310), "Mag Skips: False");
            
            #region seed
            GameObject seedGameObject = back.Clone("Seed", MenuButton.MenuButtonType.Activate, new Vector2(0, 1130), "Click to type a custom seed").gameObject;
            Object.DestroyImmediate(seedGameObject.GetComponent<MenuButton>());
            Object.DestroyImmediate(seedGameObject.GetComponent<EventTrigger>());
            Object.DestroyImmediate(seedGameObject.transform.Find("Text").GetComponent<AutoLocalizeTextUI>());
            Object.DestroyImmediate(seedGameObject.transform.Find("Text").GetComponent<FixVerticalAlign>());
            Object.DestroyImmediate(seedGameObject.transform.Find("Text").GetComponent<ContentSizeFitter>());

            RectTransform seedRect = seedGameObject.transform.Find("Text").GetComponent<RectTransform>();
            seedRect.anchorMin = seedRect.anchorMax = new Vector2(0.5f, 0.5f);
            seedRect.sizeDelta = new Vector2(337, 63.2f);

            InputField customSeedInput = seedGameObject.AddComponent<InputField>();
            customSeedInput.transform.localPosition = new Vector3(0, 1240);
            customSeedInput.textComponent = seedGameObject.transform.Find("Text").GetComponent<Text>();

            Settings.Seed = new System.Random().Next(999999999);
            customSeedInput.text = Settings.Seed.ToString();

            customSeedInput.caretColor = Color.white;
            customSeedInput.contentType = InputField.ContentType.IntegerNumber;
            customSeedInput.onEndEdit.AddListener(data => Settings.Seed = Convert.ToInt32(data));
            customSeedInput.navigation = Navigation.defaultNavigation;
            customSeedInput.caretWidth = 8;
            customSeedInput.characterLimit = 9;

            ColorBlock cb = new ColorBlock
            {
                highlightedColor = Color.yellow,
                pressedColor = Color.red,
                disabledColor = Color.black,
                normalColor = Color.white,
                colorMultiplier = 2f
            };

            customSeedInput.colors = cb;
            #endregion

            //Dirty way of making labels
            GameObject modeLabel = back.Clone("ModeLabel", MenuButton.MenuButtonType.Activate, new Vector2(-900, 960), "Required Skips").gameObject;
            GameObject restrictionsLabel = back.Clone("RestrictionsLabel", MenuButton.MenuButtonType.Activate, new Vector2(0, 960), "Restrictions").gameObject;
            GameObject qolLabel = back.Clone("QoLLabel", MenuButton.MenuButtonType.Activate, new Vector2(900, 960), "Quality of Life").gameObject;
            GameObject seedLabel = back.Clone("SeedLabel", MenuButton.MenuButtonType.Activate, new Vector2(0, 1300), "Seed:").gameObject;

            Object.Destroy(modeLabel.GetComponent<EventTrigger>());
            Object.Destroy(restrictionsLabel.GetComponent<EventTrigger>());
            Object.Destroy(qolLabel.GetComponent<EventTrigger>());
            Object.Destroy(seedLabel.GetComponent<EventTrigger>());

            Object.Destroy(modeLabel.GetComponent<MenuButton>());
            Object.Destroy(restrictionsLabel.GetComponent<MenuButton>());
            Object.Destroy(qolLabel.GetComponent<MenuButton>());
            Object.Destroy(seedLabel.GetComponent<MenuButton>());

            //We don't need these old buttons anymore
            Object.Destroy(classic.gameObject);
            Object.Destroy(steel.gameObject);
            Object.Destroy(parent.FindGameObjectInChildren("GGButton"));
            Object.Destroy(back.gameObject);

            //Gotta put something here, we destroyed the old default
            UIManager.instance.playModeMenuScreen.defaultHighlight = startRandoBtn;

            //Apply navigation info (up, right, down, left)
            startNormalBtn.SetNavigation(magolorBtn, startRandoBtn, backBtn, startRandoBtn);
            startRandoBtn.SetNavigation(lemmBtn, startNormalBtn, backBtn, startNormalBtn);
            backBtn.SetNavigation(startNormalBtn, backBtn, allBossesBtn, backBtn);
            allBossesBtn.SetNavigation(backBtn, charmNotchBtn, allSkillsBtn, presetBtn);
            allSkillsBtn.SetNavigation(allBossesBtn, lemmBtn, allCharmsBtn, shadeSkipsBtn);
            allCharmsBtn.SetNavigation(allSkillsBtn, lemmBtn, startNormalBtn, acidSkipsBtn);
            charmNotchBtn.SetNavigation(backBtn, presetBtn, lemmBtn, allBossesBtn);
            lemmBtn.SetNavigation(charmNotchBtn, shadeSkipsBtn, startRandoBtn, allSkillsBtn);
            presetBtn.SetNavigation(backBtn, allBossesBtn, shadeSkipsBtn, charmNotchBtn);
            shadeSkipsBtn.SetNavigation(presetBtn, allSkillsBtn, acidSkipsBtn, lemmBtn);
            acidSkipsBtn.SetNavigation(shadeSkipsBtn, allCharmsBtn, spikeTunnelsBtn, lemmBtn);
            spikeTunnelsBtn.SetNavigation(acidSkipsBtn, allCharmsBtn, miscSkipsBtn, lemmBtn);
            miscSkipsBtn.SetNavigation(spikeTunnelsBtn, allCharmsBtn, fireballSkipsBtn, lemmBtn);
            fireballSkipsBtn.SetNavigation(miscSkipsBtn, allCharmsBtn, magolorBtn, lemmBtn);
            magolorBtn.SetNavigation(fireballSkipsBtn, allCharmsBtn, startNormalBtn, lemmBtn);

            //Clear out all the events we don't need anymore
            allBossesBtn.ClearEvents();
            allSkillsBtn.ClearEvents();
            allCharmsBtn.ClearEvents();
            charmNotchBtn.ClearEvents();
            lemmBtn.ClearEvents();
            presetBtn.ClearEvents();
            shadeSkipsBtn.ClearEvents();
            acidSkipsBtn.ClearEvents();
            spikeTunnelsBtn.ClearEvents();
            miscSkipsBtn.ClearEvents();
            fireballSkipsBtn.ClearEvents();
            magolorBtn.ClearEvents();

            //Fetch text objects for use in events
            Text allBossesText = allBossesBtn.transform.Find("Text").GetComponent<Text>();
            Text allSkillsText = allSkillsBtn.transform.Find("Text").GetComponent<Text>();
            Text allCharmsText = allCharmsBtn.transform.Find("Text").GetComponent<Text>();
            Text charmNotchText = charmNotchBtn.transform.Find("Text").GetComponent<Text>();
            Text lemmText = lemmBtn.transform.Find("Text").GetComponent<Text>();
            Text presetText = presetBtn.transform.Find("Text").GetComponent<Text>();
            Text shadeSkipsText = shadeSkipsBtn.transform.Find("Text").GetComponent<Text>();
            Text acidSkipsText = acidSkipsBtn.transform.Find("Text").GetComponent<Text>();
            Text spikeTunnelsText = spikeTunnelsBtn.transform.Find("Text").GetComponent<Text>();
            Text miscSkipsText = miscSkipsBtn.transform.Find("Text").GetComponent<Text>();
            Text fireballSkipsText = fireballSkipsBtn.transform.Find("Text").GetComponent<Text>();
            Text magolorText = magolorBtn.transform.Find("Text").GetComponent<Text>();

            //Also for use in events
            FixVerticalAlign allBossesAlign = allBossesBtn.gameObject.GetComponentInChildren<FixVerticalAlign>(true);
            FixVerticalAlign allSkillsAlign = allSkillsBtn.gameObject.GetComponentInChildren<FixVerticalAlign>(true);
            FixVerticalAlign allCharmsAlign = allCharmsBtn.gameObject.GetComponentInChildren<FixVerticalAlign>(true);
            FixVerticalAlign charmNotchAlign = charmNotchBtn.gameObject.GetComponentInChildren<FixVerticalAlign>(true);
            FixVerticalAlign lemmAlign = lemmBtn.gameObject.GetComponentInChildren<FixVerticalAlign>(true);
            FixVerticalAlign presetAlign = presetBtn.gameObject.GetComponentInChildren<FixVerticalAlign>(true);
            FixVerticalAlign shadeSkipsAlign = shadeSkipsBtn.gameObject.GetComponentInChildren<FixVerticalAlign>(true);
            FixVerticalAlign acidSkipsAlign = acidSkipsBtn.gameObject.GetComponentInChildren<FixVerticalAlign>(true);
            FixVerticalAlign spikeTunnelsAlign = spikeTunnelsBtn.gameObject.GetComponentInChildren<FixVerticalAlign>(true);
            FixVerticalAlign miscSkipsAlign = miscSkipsBtn.gameObject.GetComponentInChildren<FixVerticalAlign>(true);
            FixVerticalAlign fireballSkipsAlign = fireballSkipsBtn.gameObject.GetComponentInChildren<FixVerticalAlign>(true);
            FixVerticalAlign magolorAlign = magolorBtn.gameObject.GetComponentInChildren<FixVerticalAlign>(true);

            //Create dictionary to pass into events
            Dictionary<string, Text> dict = new Dictionary<string, Text>();
            dict.Add("All Bosses", allBossesText);
            dict.Add("All Skills", allSkillsText);
            dict.Add("All Charms", allCharmsText);
            dict.Add("Salubra Notches", charmNotchText);
            dict.Add("Lemm Sell All", lemmText);
            dict.Add("Preset", presetText);
            dict.Add("Shade Skips", shadeSkipsText);
            dict.Add("Acid Skips", acidSkipsText);
            dict.Add("Spike Tunnels", spikeTunnelsText);
            dict.Add("Misc Skips", miscSkipsText);
            dict.Add("Fireball Skips", fireballSkipsText);
            dict.Add("Mag Skips", magolorText);

            //Add useful events
            startNormalBtn.AddEvent(EventTriggerType.Submit, data => StartNewGame(false));
            startRandoBtn.AddEvent(EventTriggerType.Submit, data => StartNewGame(true));
            allBossesBtn.AddEvent(EventTriggerType.Submit, data =>
            {
                Settings.allBosses = !Settings.allBosses;
                allBossesText.text = "All Bosses: " + Settings.allBosses;
                allBossesAlign.AlignText();
            });
            allSkillsBtn.AddEvent(EventTriggerType.Submit, data =>
            {
                Settings.allSkills = !Settings.allSkills;
                allSkillsText.text = "All Skills: " + Settings.allSkills;
                allSkillsAlign.AlignText();
            });
            allCharmsBtn.AddEvent(EventTriggerType.Submit, data =>
            {
                Settings.allCharms = !Settings.allCharms;
                allCharmsText.text = "All Charms: " + Settings.allCharms;
                allCharmsAlign.AlignText();
            });
            charmNotchBtn.AddEvent(EventTriggerType.Submit, data =>
            {
                Settings.charmNotch = !Settings.charmNotch;
                charmNotchText.text = "Salubra Notches: " + Settings.charmNotch;
                charmNotchAlign.AlignText();
            });
            lemmBtn.AddEvent(EventTriggerType.Submit, data =>
            {
                Settings.lemm = !Settings.lemm;
                lemmText.text = "Lemm Sell All: " + Settings.lemm;
                lemmAlign.AlignText();
            });
            presetBtn.AddEvent(EventTriggerType.Submit, data =>
            {
                if (presetText.text.Contains("Easy"))
                {
                    presetText.text = "Preset: Hard";
                    Settings.ShadeSkips = true;
                    Settings.AcidSkips = true;
                    Settings.SpikeTunnels = true;
                    Settings.MiscSkips = true;
                    Settings.FireballSkips = true;
                    Settings.MagSkips = false;
                }
                else if (presetText.text.Contains("Hard"))
                {
                    presetText.text = "Preset: Moglar";
                    Settings.ShadeSkips = true;
                    Settings.AcidSkips = true;
                    Settings.SpikeTunnels = true;
                    Settings.MiscSkips = true;
                    Settings.FireballSkips = true;
                    Settings.MagSkips = true;
                }
                else
                {
                    presetText.text = "Preset: Easy";
                    Settings.ShadeSkips = false;
                    Settings.AcidSkips = false;
                    Settings.SpikeTunnels = false;
                    Settings.MiscSkips = false;
                    Settings.FireballSkips = false;
                    Settings.MagSkips = false;
                }

                shadeSkipsText.text = "Shade Skips: " + Settings.ShadeSkips;
                acidSkipsText.text = "Acid Skips: " + Settings.AcidSkips;
                spikeTunnelsText.text = "Spike Tunnels: " + Settings.SpikeTunnels;
                miscSkipsText.text = "Misc Skips: " + Settings.MiscSkips;
                fireballSkipsText.text = "Fireball Skips: " + Settings.FireballSkips;
                magolorText.text = "Mag Skips: " + Settings.MagSkips;

                presetAlign.AlignText();
                shadeSkipsAlign.AlignText();
                acidSkipsAlign.AlignText();
                spikeTunnelsAlign.AlignText();
                miscSkipsAlign.AlignText();
                fireballSkipsAlign.AlignText();
                magolorAlign.AlignText();
            });
            shadeSkipsBtn.AddEvent(EventTriggerType.Submit, data =>
            {
                Settings.ShadeSkips = !Settings.ShadeSkips;
                shadeSkipsText.text = "Shade Skips: " + Settings.ShadeSkips;
                shadeSkipsAlign.AlignText();

                presetText.text = "Preset: Custom";
                presetAlign.AlignText();
            });
            acidSkipsBtn.AddEvent(EventTriggerType.Submit, data =>
            {
                Settings.AcidSkips = !Settings.AcidSkips;
                acidSkipsText.text = "Acid Skips: " + Settings.AcidSkips;
                acidSkipsAlign.AlignText();

                presetText.text = "Preset: Custom";
                presetAlign.AlignText();
            });
            spikeTunnelsBtn.AddEvent(EventTriggerType.Submit, data =>
            {
                Settings.SpikeTunnels = !Settings.SpikeTunnels;
                spikeTunnelsText.text = "Spike Tunnels: " + Settings.SpikeTunnels;
                spikeTunnelsAlign.AlignText();

                presetText.text = "Preset: Custom";
                presetAlign.AlignText();
            });
            miscSkipsBtn.AddEvent(EventTriggerType.Submit, data =>
            {
                Settings.MiscSkips = !Settings.MiscSkips;
                miscSkipsText.text = "Misc Skips: " + Settings.MiscSkips;
                miscSkipsAlign.AlignText();

                presetText.text = "Preset: Custom";
                presetAlign.AlignText();
            });
            fireballSkipsBtn.AddEvent(EventTriggerType.Submit, data =>
            {
                Settings.FireballSkips = !Settings.FireballSkips;
                fireballSkipsText.text = "Fireball Skips: " + Settings.FireballSkips;
                fireballSkipsAlign.AlignText();

                presetText.text = "Preset: Custom";
                presetAlign.AlignText();
            });
            magolorBtn.AddEvent(EventTriggerType.Submit, data =>
            {
                Settings.MagSkips = !Settings.MagSkips;
                magolorText.text = "Mag Skips: " + Settings.MagSkips;
                magolorAlign.AlignText();

                presetText.text = "Preset: Custom";
                presetAlign.AlignText();
            });
        }

        private void StartNewGame(bool randomizer)
        {
            //Charm tutorial popup is annoying, get rid of it
            PlayerData.instance.hasCharm = true;

            if (Settings.allBosses)
            {
                //TODO: Think of a better way to handle Zote
                PlayerData.instance.zoteRescuedBuzzer = true;
                PlayerData.instance.zoteRescuedDeepnest = true;
            }

            if (randomizer)
            {
                Settings.randomizer = true;

                randomizeThread = new Thread(new ThreadStart(Randomization.Randomizer.Randomize));
                randomizeThread.Start();
            }
        }

        private static bool SceneHasPreload(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return false;
            }

            sceneName = sceneName.ToLowerInvariant();

            //Build scene list if necessary
            if (sceneNames == null)
            {
                sceneNames = new List<string>();

                for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
                {
                    sceneNames.Add(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)).ToLowerInvariant());
                }
            }

            //Check if scene has preload attached to it
            if (sceneNames.Contains($"{sceneName}_preload") || sceneNames.Contains($"{sceneName}_boss") || sceneNames.Contains($"{sceneName}_boss_defeated"))
            {
                return true;
            }

            //Also check if the scene is a preload since this is also passed to activeSceneChanged sometimes
            return sceneName.EndsWith("_preload") || sceneName.EndsWith("_boss") || sceneName.EndsWith("_boss_defeated");
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
                    PreventCameraFadeOut = true,
                    WaitForSceneTransitionCameraFade = false,
                    Visualization = GameManager.SceneLoadVisualizations.Default,
                    AlwaysUnloadUnusedAssets = true
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

        private static GlobalEnums.GatePosition GetGatePosition(string name)
        {
            if (name.Contains("top")) return GlobalEnums.GatePosition.top;
            if (name.Contains("bot")) return GlobalEnums.GatePosition.bottom;
            if (name.Contains("left")) return GlobalEnums.GatePosition.left;
            if (name.Contains("right")) return GlobalEnums.GatePosition.right;
            if (name.Contains("door")) return GlobalEnums.GatePosition.door;
            return GlobalEnums.GatePosition.unknown;
        }
    }
}
