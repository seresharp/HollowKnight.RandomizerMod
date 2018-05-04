using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Xml;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using RandomizerMod.Actions;

using Object = UnityEngine.Object;

namespace RandomizerMod
{
    public class RandomizerMod : Mod<SaveSettings>
    {
        private static FieldInfo smallGeoPrefabField = typeof(HealthManager).GetField("smallGeoPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo mediumGeoPrefabField = typeof(HealthManager).GetField("mediumGeoPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo largeGeoPrefabField = typeof(HealthManager).GetField("largeGeoPrefab", BindingFlags.NonPublic | BindingFlags.Instance);

        public static GameObject smallGeoPrefab;
        public static GameObject mediumGeoPrefab;
        public static GameObject largeGeoPrefab;

        public static Dictionary<string, Sprite> sprites;
        private static Dictionary<string, Dictionary<string, string>> languageStrings;
        private static Dictionary<string, string> skills;
        private static Dictionary<string, string> bosses;

        private NewGameSettings newGameSettings;
        
        public static RandomizerMod instance;

        public override void Initialize()
        {
            sprites = new Dictionary<string, Sprite>();
            languageStrings = new Dictionary<string, Dictionary<string, string>>();

            //Load logo and xml from embedded resources
            foreach (string res in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (res.EndsWith(".png"))
                {
                    //Read bytes of image
                    Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res);
                    byte[] buffer = new byte[imageStream.Length];
                    imageStream.Read(buffer, 0, buffer.Length);
                    imageStream.Dispose();

                    //Create texture from bytes
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(buffer);

                    //Create sprite from texture
                    sprites.Add(res.Replace("RandomizerMod.Resources.", ""), Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
                    Log("Created sprite from embedded image: " + res);
                }
                else if (res.EndsWith("language.xml"))
                {
                    //No sense having the whole init die if this xml is formatted improperly
                    try
                    {
                        //Load XmlDocument from resource stream
                        Stream xmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res);
                        XmlDocument xml = new XmlDocument();
                        xml.Load(xmlStream);
                        xmlStream.Dispose();

                        foreach (XmlNode node in xml.SelectNodes("Language/entry"))
                        {
                            string sheet = node.Attributes["sheet"].Value;
                            string key = node.Attributes["key"].Value;

                            if (!languageStrings.ContainsKey(sheet))
                            {
                                languageStrings[sheet] = new Dictionary<string, string>();
                            }

                            languageStrings[sheet][key] = node.InnerText;
                        }

                        Log("Language xml processed");
                    }
                    catch (Exception e)
                    {
                        LogError("Could not process language xml:\n" + e);
                    }
                }
                else if (res.EndsWith(".xml"))
                {
                    Stream xmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res);
                    XmlLoader.LoadXml(xmlStream);
                    xmlStream.Dispose();
                    Log("Randomizer xml processed");
                }
                else
                {
                    Log("Unknown resource " + res);
                }
            }

            //Set up dictionaries for restriction checking
            skills = new Dictionary<string, string>();
            skills.Add("hasDash", "Mothwing Cloak");
            skills.Add("hasShadowDash", "Shade Cloak");
            skills.Add("hasWalljump", "Mantis Claw");
            skills.Add("hasDoubleJump", "Monarch Wings");
            skills.Add("hasAcidArmour", "Isma's Tear");
            skills.Add("hasDashSlash", "Great Slash");
            skills.Add("hasUpwardSlash", "Dash Slash");
            skills.Add("hasCyclone", "Cyclone Slash");

            bosses = new Dictionary<string, string>();
            bosses.Add("killedInfectedKnight", "Broken Vessel");
            bosses.Add("killedMawlek", "Brooding Mawlek");
            bosses.Add("collectorDefeated", "The Collector");
            bosses.Add("defeatedMegaBeamMiner", "Crystal Guardian 1");
            bosses.Add("killedDungDefender", "Dung Defender");
            bosses.Add("killedGhostHu", "Elder Hu");
            bosses.Add("falseKnightDreamDefeated", "Failed Champion");
            bosses.Add("killedFalseKnight", "False Knight");
            bosses.Add("killedFlukeMother", "Flukemarm");
            bosses.Add("killedGhostGalien", "Galien");
            bosses.Add("colosseumGoldCompleted", "God Tamer");
            bosses.Add("killedGhostAladar", "Gorb");
            bosses.Add("killedGreyPrince", "Grey Prince Zote");
            bosses.Add("killedBigFly", "Gruz Mother");
            bosses.Add("killedHiveKnight", "Hive Knight");
            bosses.Add("killedHornet", "Hornet 1");
            bosses.Add("hornetOutskirtsDefeated", "Hornet 2");
            bosses.Add("infectedKnightDreamDefeated", "Lost Kin");
            bosses.Add("defeatedMantisLords", "Mantis Lords");
            bosses.Add("killedGhostMarkoth", "Markoth");
            bosses.Add("killedGhostMarmu", "Marmu");
            bosses.Add("killedNightmareGrimm", "Nightmare King Grimm");
            bosses.Add("killedGhostNoEyes", "No Eyes");
            bosses.Add("killedMimicSpider", "Nosk");
            bosses.Add("killedMageLord", "Soul Master");
            bosses.Add("mageLordDreamDefeated", "Soul Tyrant");
            bosses.Add("killedTraitorLord", "Traitor Lord");
            bosses.Add("killedGrimm", "Troupe Master Grimm");
            bosses.Add("killedMegaJellyfish", "Uumuu");
            bosses.Add("killedBlackKnight", "Watcher Knights");
            bosses.Add("killedWhiteDefender", "White Defender");
            bosses.Add("killedGhostXero", "Xero");
            bosses.Add("killedZote", "Zote");

            //Add hooks
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += HandleSceneChanges;
            ModHooks.Instance.OnEnableEnemyHook += GetGeoPrefabs;
            ModHooks.Instance.LanguageGetHook += LanguageOverride;
            ModHooks.Instance.GetPlayerIntHook += IntOverride;
            ModHooks.Instance.GetPlayerBoolHook += BoolGetOverride;
            ModHooks.Instance.SetPlayerBoolHook += BoolSetOverride;

            //Set instance for outside use
            instance = this;
        }

        public override string GetVersion()
        {
            string ver = "2a.1";
            int minAPI = 41;

            if (Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minAPI) ver += " (Some features may not work, update API)";

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

            return PlayerData.instance.GetBoolInternal(boolName);
        }

        private void BoolSetOverride(string boolName, bool value)
        {
            //For some reason these all have two bools
            if (boolName == "hasDash") PlayerData.instance.canDash = value;
            else if (boolName == "hasShadowDash") PlayerData.instance.canShadowDash = value;
            else if (boolName == "hasSuperDash") PlayerData.instance.canSuperDash = value;
            else if (boolName == "hasWalljump") PlayerData.instance.canWallJump = value;
            //Shade skips make these charms not viable, unbreakable is a nice fix for that
            else if (boolName == "gotCharm_23") PlayerData.instance.fragileHealth_unbreakable = value;
            else if (boolName == "gotCharm_24") PlayerData.instance.fragileGreed_unbreakable = value;
            else if (boolName == "gotCharm_25") PlayerData.instance.fragileStrength_unbreakable = value;
            //Gotta update the acid pools after getting this
            else if (boolName == "hasAcidArmour" && value) PlayMakerFSM.BroadcastEvent("GET ACID ARMOUR");
            //It's just way easier if I can treat spells as bools
            else if (boolName == "hasVengefulSpirit" && value && PlayerData.instance.fireballLevel <= 0) PlayerData.instance.fireballLevel = 1;
            else if (boolName == "hasVengefulSpirit" && !value) PlayerData.instance.fireballLevel = 0;
            else if (boolName == "hasShadeSoul" && value) PlayerData.instance.fireballLevel = 2;
            else if (boolName == "hasShadeSoul" && !value && PlayerData.instance.fireballLevel >= 2) PlayerData.instance.fireballLevel = 1;
            else if (boolName == "hasDesolateDive" && value && PlayerData.instance.quakeLevel <= 0) PlayerData.instance.quakeLevel = 1;
            else if (boolName == "hasDesolateDive" && !value) PlayerData.instance.quakeLevel = 0;
            else if (boolName == "hasDescendingDark" && value) PlayerData.instance.quakeLevel = 2;
            else if (boolName == "hasDescendingDark" && !value && PlayerData.instance.quakeLevel >= 2) PlayerData.instance.quakeLevel = 1;
            else if (boolName == "hasHowlingWraiths" && value && PlayerData.instance.screamLevel <= 0) PlayerData.instance.screamLevel = 1;
            else if (boolName == "hasHowlingWraiths" && !value) PlayerData.instance.screamLevel = 0;
            else if (boolName == "hasAbyssShriek" && value) PlayerData.instance.screamLevel = 2;
            else if (boolName == "hasAbyssShriek" && !value && PlayerData.instance.screamLevel >= 2) PlayerData.instance.screamLevel = 1;

            PlayerData.instance.SetBoolInternal(boolName, value);
        }

        private int IntOverride(string intName)
        {
            if (intName == "RandomizerMod.Zero") return 0;
            return PlayerData.instance.GetIntInternal(intName);
        }

        private string LanguageOverride(string key, string sheetTitle)
        {
            if (languageStrings.ContainsKey(sheetTitle) && languageStrings[sheetTitle].ContainsKey(key))
            {
                return languageStrings[sheetTitle][key];
            }

            return Language.Language.GetInternal(key, sheetTitle);
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
            //TODO: Prevent player from skipping Radiance in all bosses randomizer
            try
            {
                if (GameManager.instance.GetSceneNameString() == Constants.MENU_SCENE) EditUI();
                if (GameManager.instance.IsGameplayScene()) EditShinies(from, to);
                if (GameManager.instance.GetSceneNameString() == "Room_temple") ProcessRestrictions();
                if (GameManager.instance.GetSceneNameString() == "Cliffs_06" && Settings.allBosses)
                {
                    GameObject brumm = GameObject.Find("Brumm Lantern NPV");
                    if (brumm != null) Object.Destroy(brumm);
                    else
                    {
                        LogWarn("Could not find Brumm. Maybe possible to soft lock?");
                    }
                }
            }
            catch (Exception e)
            {
                LogError("Error applying changes to scene " + to.name + ":\n" + e);
            }
        }

        private void ProcessRestrictions()
        {
            if (Settings.allBosses || Settings.allCharms || Settings.allSkills)
            {
                languageStrings["Hornet"] = new Dictionary<string, string>();

                //Close the door and get rid of Quirrel
                PlayerData.instance.openedBlackEggDoor = false;
                PlayerData.instance.quirrelLeftEggTemple = true;

                //Make Hornet appear
                GameObject hornet = GameObject.Find("Hornet Black Egg NPC");
                hornet.SetActive(true);
                FsmState activeCheck = FSMUtility.LocateFSM(hornet, "Conversation Control").GetState("Active?");
                activeCheck.RemoveActionsOfType<IntCompare>();
                activeCheck.RemoveActionsOfType<PlayerDataBoolTest>();

                //Check dreamers
                if (!PlayerData.instance.lurienDefeated || !PlayerData.instance.monomonDefeated || !PlayerData.instance.hegemolDefeated)
                {
                    languageStrings["Hornet"].Add("HORNET_DOOR_UNOPENED", "What kind of idiot comes here without even killing the dreamers?");
                    return;
                }

                //Check all charms
                if (Settings.allCharms)
                {
                    PlayerData.instance.CountCharms();
                    if (PlayerData.instance.charmsOwned < 40)
                    {
                        languageStrings["Hornet"].Add("HORNET_DOOR_UNOPENED", "What are you doing here? Go get the rest of the charms.");
                        return;
                    }
                    else if (PlayerData.instance.royalCharmState < 3)
                    {
                        languageStrings["Hornet"].Add("HORNET_DOOR_UNOPENED", "Nice try, but half of a charm doesn't count. Go get the rest of the kingsoul.");
                        return;
                    }
                }

                //Check all skills
                if (Settings.allSkills)
                {
                    List<string> missingSkills = new List<string>();

                    foreach (KeyValuePair<string, string> kvp in skills)
                    {
                        if (!PlayerData.instance.GetBool(kvp.Key))
                        {
                            missingSkills.Add(kvp.Value);
                        }
                    }

                    //These aren't as easy to check in a loop, so I'm just gonna check them manually
                    if (PlayerData.instance.fireballLevel == 0) missingSkills.Add("Vengeful Spirit");
                    if (PlayerData.instance.fireballLevel < 2) missingSkills.Add("Shade Soul");
                    if (PlayerData.instance.quakeLevel == 0) missingSkills.Add("Desolate Dive");
                    if (PlayerData.instance.quakeLevel < 2) missingSkills.Add("Descending Dark");
                    if (PlayerData.instance.screamLevel == 0) missingSkills.Add("Howling Wraiths");
                    if (PlayerData.instance.screamLevel < 2) missingSkills.Add("Abyss Shriek");

                    if (missingSkills.Count > 0)
                    {
                        string hornetStr = "You are still missing ";
                        for (int i = 0; i < missingSkills.Count; i++)
                        {
                            if (i != 0 && i == missingSkills.Count - 1)
                            {
                                hornetStr += " and ";
                            }

                            hornetStr += missingSkills[i];

                            if (i != missingSkills.Count - 1)
                            {
                                hornetStr += ", ";
                            }
                        }
                        hornetStr += ".";

                        languageStrings["Hornet"].Add("HORNET_DOOR_UNOPENED", hornetStr);
                        return;
                    }
                }

                //Check all bosses
                if (Settings.allBosses)
                {
                    List<string> missingBosses = new List<string>();

                    foreach (KeyValuePair<string, string> kvp in bosses)
                    {
                        if (!PlayerData.instance.GetBool(kvp.Key))
                        {
                            missingBosses.Add(kvp.Value);
                        }
                    }

                    //CG2 has no bool
                    if (PlayerData.instance.killsMegaBeamMiner > 0) missingBosses.Add("Crystal Guardian 2");

                    if (missingBosses.Count > 0)
                    {
                        if (missingBosses.Count >= 10)
                        {
                            languageStrings["Hornet"].Add("HORNET_DOOR_UNOPENED", $"You haven't killed {missingBosses.Count} bosses.");
                            return;
                        }
                        else
                        {
                            string hornetStr = "You haven't killed ";
                            for (int i = 0; i < missingBosses.Count; i++)
                            {
                                if (i != 0 && i == missingBosses.Count - 1)
                                {
                                    hornetStr += " and ";
                                }

                                hornetStr += missingBosses[i];

                                if (i != missingBosses.Count - 1)
                                {
                                    hornetStr += ", ";
                                }
                            }
                            hornetStr += ".";

                            languageStrings["Hornet"].Add("HORNET_DOOR_UNOPENED", hornetStr);
                            return;
                        }
                    }
                    else if (PlayerData.instance.royalCharmState != 4)
                    {
                        languageStrings["Hornet"].Add("HORNET_DOOR_UNOPENED", "You chose all bosses, go get void heart ya dip.");
                        return;
                    }
                }

                //All checks passed, time to open up
                PlayerData.instance.openedBlackEggDoor = true;
            }
        }

        private void EditShinies(Scene from, Scene to)
        {
            RandomizerAction.FetchFSMList(to);
            foreach (RandomizerAction action in Settings.actions)
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
            //Reset new game settings
            newGameSettings.SetDefaults();

            //Fetch data from vanilla screen
            MenuScreen playScreen = UIManager.instance.playModeMenuScreen;

            MenuButton classic = (MenuButton)playScreen.defaultHighlight;
            MenuButton steel = (MenuButton)classic.FindSelectableOnDown();
            MenuButton back = (MenuButton)steel.FindSelectableOnDown();

            //Create new buttons
            MenuButton startRandoBtn = classic.Clone("StartRando", MenuButton.MenuButtonType.Proceed, new Vector2(650, -480), "Start Game", "Randomizer v2", sprites["UI.logo.png"]);
            MenuButton startNormalBtn = classic.Clone("StartNormal", MenuButton.MenuButtonType.Proceed, new Vector2(-650, -480), "Start Game", "Non-Randomizer");

            startNormalBtn.transform.localScale = startRandoBtn.transform.localScale = new Vector2(0.75f, 0.75f);

            MenuButton backBtn = back.Clone("Back", MenuButton.MenuButtonType.Proceed, new Vector2(0, -100), "Back");

            MenuButton allBossesBtn = back.Clone("AllBosses", MenuButton.MenuButtonType.Activate, new Vector2(0, 750), "All Bosses: False");
            MenuButton allSkillsBtn = back.Clone("AllSkills", MenuButton.MenuButtonType.Activate, new Vector2(0, 660), "All Skills: False");
            MenuButton allCharmsBtn = back.Clone("AllCharms", MenuButton.MenuButtonType.Activate, new Vector2(0, 570), "All Charms: False");

            MenuButton charmNotchBtn = back.Clone("SalubraNotches", MenuButton.MenuButtonType.Activate, new Vector2(900, 750), "Salubra Notches: True");
            MenuButton lemmBtn = back.Clone("LemmSellAll", MenuButton.MenuButtonType.Activate, new Vector2(900, 660), "Lemm Sell All: True");

            MenuButton presetBtn = back.Clone("RandoPreset", MenuButton.MenuButtonType.Activate, new Vector2(-900, 750), "Preset: Easy");
            MenuButton shadeSkipsBtn = back.Clone("ShadeSkips", MenuButton.MenuButtonType.Activate, new Vector2(-900, 660), "Shade Skips: False");
            MenuButton acidSkipsBtn = back.Clone("AcidSkips", MenuButton.MenuButtonType.Activate, new Vector2(-900, 570), "Acid Skips: False");
            MenuButton spikeTunnelsBtn = back.Clone("SpikeTunnelSkips", MenuButton.MenuButtonType.Activate, new Vector2(-900, 480), "Spike Tunnels: False");
            MenuButton miscSkipsBtn = back.Clone("MiscSkips", MenuButton.MenuButtonType.Activate, new Vector2(-900, 390), "Misc Skips: False");
            MenuButton magolorBtn = back.Clone("MagolorSkips", MenuButton.MenuButtonType.Activate, new Vector2(-900, 300), "Mag Skips: False");

            //Dirty way of making labels
            Object.Destroy(back.Clone("ModeLabel", MenuButton.MenuButtonType.Activate, new Vector2(-900, 860), "Required Skips"));
            Object.Destroy(back.Clone("RestrictionsLabel", MenuButton.MenuButtonType.Activate, new Vector2(0, 860), "Restrictions"));
            Object.Destroy(back.Clone("QoLLabel", MenuButton.MenuButtonType.Activate, new Vector2(900, 860), "Quality of Life"));

            //We don't need these old buttons anymore
            Object.Destroy(classic.gameObject);
            Object.Destroy(steel.gameObject);
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
            miscSkipsBtn.SetNavigation(spikeTunnelsBtn, allCharmsBtn, magolorBtn, lemmBtn);
            magolorBtn.SetNavigation(miscSkipsBtn, allCharmsBtn, startNormalBtn, lemmBtn);

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
            Text magolorText = magolorBtn.transform.Find("Text").GetComponent<Text>();

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
            dict.Add("Mag Skips", magolorText);

            //Add useful events
            startNormalBtn.AddEvent(EventTriggerType.Submit, data => StartNewGame(false));
            startRandoBtn.AddEvent(EventTriggerType.Submit, data => StartNewGame(true));
            allBossesBtn.AddEvent(EventTriggerType.Submit, data => MenuButtonClicked("All Bosses", dict));
            allSkillsBtn.AddEvent(EventTriggerType.Submit, data => MenuButtonClicked("All Skills", dict));
            allCharmsBtn.AddEvent(EventTriggerType.Submit, data => MenuButtonClicked("All Charms", dict));
            charmNotchBtn.AddEvent(EventTriggerType.Submit, data => MenuButtonClicked("Salubra Notches", dict));
            lemmBtn.AddEvent(EventTriggerType.Submit, data => MenuButtonClicked("Lemm Sell All", dict));
            presetBtn.AddEvent(EventTriggerType.Submit, data => MenuButtonClicked("Preset", dict));
            shadeSkipsBtn.AddEvent(EventTriggerType.Submit, data => MenuButtonClicked("Shade Skips", dict));
            acidSkipsBtn.AddEvent(EventTriggerType.Submit, data => MenuButtonClicked("Acid Skips", dict));
            spikeTunnelsBtn.AddEvent(EventTriggerType.Submit, data => MenuButtonClicked("Spike Tunnels", dict));
            miscSkipsBtn.AddEvent(EventTriggerType.Submit, data => MenuButtonClicked("Misc Skips", dict));
            magolorBtn.AddEvent(EventTriggerType.Submit, data => MenuButtonClicked("Mag Skips", dict));
        }

        //Don't look at this
        private void MenuButtonClicked(string baseName, Dictionary<string, Text> dict)
        {
            Text text = dict[baseName];

            //This is awful because for some reason creating a new string moves the text position
            switch (baseName)
            {
                case "All Bosses":
                    newGameSettings.allBosses = !newGameSettings.allBosses;
                    text.text = text.text.Replace((!newGameSettings.allBosses).ToString(), newGameSettings.allBosses.ToString());
                    break;
                case "All Skills":
                    newGameSettings.allSkills = !newGameSettings.allSkills;
                    text.text = text.text.Replace((!newGameSettings.allSkills).ToString(), newGameSettings.allSkills.ToString());
                    break;
                case "All Charms":
                    newGameSettings.allCharms = !newGameSettings.allCharms;
                    text.text = text.text.Replace((!newGameSettings.allCharms).ToString(), newGameSettings.allCharms.ToString());
                    break;
                case "Salubra Notches":
                    newGameSettings.charmNotch = !newGameSettings.charmNotch;
                    text.text = text.text.Replace((!newGameSettings.charmNotch).ToString(), newGameSettings.charmNotch.ToString());
                    break;
                case "Lemm Sell All":
                    newGameSettings.lemm = !newGameSettings.lemm;
                    text.text = text.text.Replace((!newGameSettings.lemm).ToString(), newGameSettings.lemm.ToString());
                    break;
                case "Preset":
                    if (text.text.Contains("Easy"))
                    {
                        text.text = text.text.Replace("Easy", "Hard");
                        newGameSettings.SetHard();
                    }
                    else if (text.text.Contains("Hard"))
                    {
                        text.text = text.text.Replace("Hard", "Moglar");
                        newGameSettings.SetMagolor();
                    }
                    else
                    {
                        //Could be "Moglar" or "Custom"
                        text.text = text.text.Replace(text.text.Substring(8), "Easy");
                        newGameSettings.SetEasy();
                    }

                    //Future me is gonna hate this
                    dict["Shade Skips"].text = dict["Shade Skips"].text.Replace((!newGameSettings.shadeSkips).ToString(), newGameSettings.shadeSkips.ToString());
                    dict["Acid Skips"].text = dict["Acid Skips"].text.Replace((!newGameSettings.acidSkips).ToString(), newGameSettings.acidSkips.ToString());
                    dict["Spike Tunnels"].text = dict["Spike Tunnels"].text.Replace((!newGameSettings.spikeTunnels).ToString(), newGameSettings.spikeTunnels.ToString());
                    dict["Misc Skips"].text = dict["Misc Skips"].text.Replace((!newGameSettings.miscSkips).ToString(), newGameSettings.miscSkips.ToString());
                    dict["Mag Skips"].text = dict["Mag Skips"].text.Replace((!newGameSettings.magolorSkips).ToString(), newGameSettings.magolorSkips.ToString());

                    break;
                case "Shade Skips":
                    newGameSettings.shadeSkips = !newGameSettings.shadeSkips;
                    text.text = text.text.Replace((!newGameSettings.shadeSkips).ToString(), newGameSettings.shadeSkips.ToString());
                    break;
                case "Acid Skips":
                    newGameSettings.acidSkips = !newGameSettings.acidSkips;
                    text.text = text.text.Replace((!newGameSettings.acidSkips).ToString(), newGameSettings.acidSkips.ToString());
                    break;
                case "Spike Tunnels":
                    newGameSettings.spikeTunnels = !newGameSettings.spikeTunnels;
                    text.text = text.text.Replace((!newGameSettings.spikeTunnels).ToString(), newGameSettings.spikeTunnels.ToString());
                    break;
                case "Misc Skips":
                    newGameSettings.miscSkips = !newGameSettings.miscSkips;
                    text.text = text.text.Replace((!newGameSettings.miscSkips).ToString(), newGameSettings.miscSkips.ToString());
                    break;
                case "Mag Skips":
                    newGameSettings.magolorSkips = !newGameSettings.magolorSkips;
                    text.text = text.text.Replace((!newGameSettings.magolorSkips).ToString(), newGameSettings.magolorSkips.ToString());
                    break;
            }

            //Two switches on the same variable is good code
            switch (baseName)
            {
                case "Shade Skips":
                case "Acid Skips":
                case "Spike Tunnels":
                case "Misc Skips":
                case "Mag Skips":
                    dict["Preset"].text = dict["Preset"].text.Replace(dict["Preset"].text.Substring(8), "Custom");
                    break;
            }
        }

        private void StartNewGame(bool randomizer)
        {
            //TODO: Remove this before release
            PlayerData.instance.infiniteAirJump = true;

            //TODO: Think of a better way to handle Zote
            PlayerData.instance.zoteRescuedBuzzer = true;
            PlayerData.instance.zoteRescuedDeepnest = true;

            Settings = new SaveSettings();

            //No reason to limit these to only when randomizer is enabled
            Settings.charmNotch = newGameSettings.charmNotch;
            Settings.lemm = newGameSettings.lemm;
            Settings.allBosses = newGameSettings.allBosses;
            Settings.allCharms = newGameSettings.allCharms;
            Settings.allSkills = newGameSettings.allSkills;

            //Charm tutorial popup is annoying, get rid of it
            PlayerData.instance.hasCharm = true;

            if (randomizer)
            {
                PlayerData.instance.screamLevel = 1;
                Settings.actions.Add(new ChangeShinyIntoBigItem("Tutorial_01", "Shiny Item (1)", "Shiny Control", new BigItemDef[]
                {
                    new BigItemDef()
                    {
                        boolName = "hasHowlingWraiths",
                        spriteKey = "Prompts.Scream1.png",
                        takeKey = "GET_ITEM_INTRO3",
                        nameKey = "INV_NAME_SPELL_SCREAM1",
                        buttonKey = "RANDOMIZER_BUTTON_DESC",
                        descOneKey = "GET_SCREAM_1",
                        descTwoKey = "GET_SCREAM_2"
                    },
                    new BigItemDef()
                    {
                        boolName = "hasAbyssShriek",
                        spriteKey = "Prompts.Scream2.png",
                        takeKey = "GET_ITEM_INTRO3",
                        nameKey = "INV_NAME_SPELL_SCREAM2",
                        buttonKey = "RANDOMIZER_BUTTON_DESC",
                        descOneKey = "GET_SCREAM2_1",
                        descTwoKey = "GET_SCREAM2_2"
                    }
                }));

                //Testing dream nail pillar
                Settings.actions.Add(new ChangeBoolTest("RestingGrounds_04", "Binding Shield Activate", "FSM", "Check", "gotCharm_1"));
                Settings.actions.Add(new ChangeBoolTest("RestingGrounds_04", "Dreamer Plaque Inspect", "Conversation Control", "End", "gotCharm_1"));
                Settings.actions.Add(new ChangeBoolTest("RestingGrounds_04", "Dreamer Scene 2", "Control", "Init", "gotCharm_1"));
                Settings.actions.Add(new ChangeBoolTest("RestingGrounds_04", "PreDreamnail", "FSM", "Check", "gotCharm_1"));
                Settings.actions.Add(new ChangeBoolTest("RestingGrounds_04", "PostDreamnail", "FSM", "Check", "gotCharm_1"));
            }
        }
    }
}
