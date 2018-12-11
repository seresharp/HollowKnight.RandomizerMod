using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace RandomizerMod
{
    public class RandomizerMod : Mod<SaveSettings>
    {
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
                    logicParseThread = new Thread(Randomization.LogicManager.ParseXML);
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

            Actions.RandomizerAction.Hook();
            BenchHandler.Hook();
            MiscSceneChanges.Hook();

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

        public override string GetVersion()
        {
            string ver = "2b.12";
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
                    pd.SetBool(nameof(PlayerData.salubraNotch1), true);
                    notches++;
                }

                if (!pd.salubraNotch2 && charms >= 10)
                {
                    pd.SetBool(nameof(PlayerData.salubraNotch2), true);
                    notches++;
                }

                if (!pd.salubraNotch3 && charms >= 18)
                {
                    pd.SetBool(nameof(PlayerData.salubraNotch3), true);
                    notches++;
                }

                if (!pd.salubraNotch4 && charms >= 25)
                {
                    pd.SetBool(nameof(PlayerData.salubraNotch4), true);
                    notches++;
                }

                pd.SetInt(nameof(PlayerData.charmSlots), notches);
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

            // This variable is incredibly stubborn, not worth the effort to make it cooperate
            // Just override it completely
            if (boolName == nameof(PlayerData.gotSlyCharm)) return Settings.SlyCharm;

            if (boolName.StartsWith("RandomizerMod.")) return Settings.GetBool(false, boolName.Substring(14));

            return PlayerData.instance.GetBoolInternal(boolName);
        }

        [SuppressMessage(
            "Microsoft.StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "Suppressing to turn into one warning. I'll deal with this mess later")]
        private void BoolSetOverride(string boolName, bool value)
        {
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

            // Check for Salubra notches if it's a charm
            if (boolName.StartsWith("gotCharm_"))
            {
                UpdateCharmNotches(PlayerData.instance, HeroController.instance);
            }
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
            if (GameManager.instance.GetSceneNameString() == SceneNames.Menu_Title)
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
            else if (GameManager.instance.GetSceneNameString() == SceneNames.End_Credits && Settings != null && Settings.Randomizer && Settings.itemPlacements.Count != 0)
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
                RestrictionManager.SceneChanged(to);
                MiscSceneChanges.SceneChanged(to);
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
