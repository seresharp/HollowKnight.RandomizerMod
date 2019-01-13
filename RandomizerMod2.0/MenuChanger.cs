using System;
using RandomizerMod.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Object = UnityEngine.Object;

namespace RandomizerMod
{
    internal static class MenuChanger
    {
        public static void EditUI()
        {
            // Reset settings
            RandomizerMod.Instance.Settings = new SaveSettings();

            // Fetch data from vanilla screen
            MenuScreen playScreen = UIManager.instance.playModeMenuScreen;

            playScreen.title.gameObject.transform.localPosition = new Vector3(0, 520.56f);

            Object.Destroy(playScreen.topFleur.gameObject);

            MenuButton classic = (MenuButton)playScreen.defaultHighlight;
            MenuButton steel = (MenuButton)classic.FindSelectableOnDown();
            MenuButton back = (MenuButton)steel.FindSelectableOnDown();

            GameObject parent = steel.transform.parent.gameObject;

            Object.Destroy(parent.GetComponent<VerticalLayoutGroup>());

            // Create new buttons
            MenuButton startRandoBtn = classic.Clone("StartRando", MenuButton.MenuButtonType.Proceed, new Vector2(650, -480), "Start Game", "Randomizer v2", RandomizerMod.GetSprite("UI.logo"));
            MenuButton startNormalBtn = classic.Clone("StartNormal", MenuButton.MenuButtonType.Proceed, new Vector2(-650, -480), "Start Game", "Non-Randomizer");
            MenuButton startSteelRandoBtn = steel.Clone("StartSteelRando", MenuButton.MenuButtonType.Proceed, new Vector2(10000, 10000), "Steel Soul", "Randomizer v2", RandomizerMod.GetSprite("UI.logo2"));
            MenuButton startSteelNormalBtn = steel.Clone("StartSteelNormal", MenuButton.MenuButtonType.Proceed, new Vector2(10000, 10000), "Steel Soul", "Non-Randomizer");

            startNormalBtn.transform.localScale = startRandoBtn.transform.localScale = startSteelNormalBtn.transform.localScale = startSteelRandoBtn.transform.localScale = new Vector2(0.75f, 0.75f);

            MenuButton backBtn = back.Clone("Back", MenuButton.MenuButtonType.Proceed, new Vector2(0, -100), "Back");

            RandoMenuItem<bool> allBossesBtn = new RandoMenuItem<bool>(back, new Vector2(0, 850), "All Bosses", false, true);
            RandoMenuItem<bool> allSkillsBtn = new RandoMenuItem<bool>(back, new Vector2(0, 760), "All Skills", false, true);
            RandoMenuItem<bool> allCharmsBtn = new RandoMenuItem<bool>(back, new Vector2(0, 670), "All Charms", false, true);

            RandoMenuItem<string> gameTypeBtn = new RandoMenuItem<string>(back, new Vector2(0, 400), "Game Type", "Normal", "Steel Soul");

            RandoMenuItem<bool> charmNotchBtn = new RandoMenuItem<bool>(back, new Vector2(900, 850), "Salubra Notches", true, false);
            RandoMenuItem<bool> lemmBtn = new RandoMenuItem<bool>(back, new Vector2(900, 760), "Lemm Sell All", true, false);

            RandoMenuItem<string> presetBtn = new RandoMenuItem<string>(back, new Vector2(-900, 850), "Preset", "Easy", "Hard", "Moglar", "Custom");
            RandoMenuItem<bool> shadeSkipsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 760), "Shade Skips", false, true);
            RandoMenuItem<bool> acidSkipsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 670), "Acid Skips", false, true);
            RandoMenuItem<bool> spikeTunnelsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 580), "Spike Tunnels", false, true);
            RandoMenuItem<bool> miscSkipsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 490), "Misc Skips", false, true);
            RandoMenuItem<bool> fireballSkipsBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 400), "Fireball Skips", false, true);
            RandoMenuItem<bool> magolorBtn = new RandoMenuItem<bool>(back, new Vector2(-900, 310), "Mag Skips", false, true);

            RandoMenuItem<string> modeBtn = new RandoMenuItem<string>(back, new Vector2(0, 1040), "Mode", "Standard", "No Claw");

            // Create seed entry field
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

            RandomizerMod.Instance.Settings.Seed = new System.Random().Next(999999999);
            customSeedInput.text = RandomizerMod.Instance.Settings.Seed.ToString();

            customSeedInput.caretColor = Color.white;
            customSeedInput.contentType = InputField.ContentType.IntegerNumber;
            customSeedInput.onEndEdit.AddListener(ParseSeedInput);
            customSeedInput.navigation = Navigation.defaultNavigation;
            customSeedInput.caretWidth = 8;
            customSeedInput.characterLimit = 9;

            customSeedInput.colors = new ColorBlock
            {
                highlightedColor = Color.yellow,
                pressedColor = Color.red,
                disabledColor = Color.black,
                normalColor = Color.white,
                colorMultiplier = 2f
            };

            // Create some labels
            CreateLabel(back, new Vector2(-900, 960), "Required Skips");
            CreateLabel(back, new Vector2(0, 960), "Restrictions");
            CreateLabel(back, new Vector2(900, 960), "Quality of Life");
            CreateLabel(back, new Vector2(0, 1300), "Seed:");

            // We don't need these old buttons anymore
            Object.Destroy(classic.gameObject);
            Object.Destroy(steel.gameObject);
            Object.Destroy(parent.FindGameObjectInChildren("GGButton"));
            Object.Destroy(back.gameObject);

            // Gotta put something here, we destroyed the old default
            playScreen.defaultHighlight = startRandoBtn;

            // Apply navigation info (up, right, down, left)
            startNormalBtn.SetNavigation(magolorBtn.Button, startRandoBtn, backBtn, startRandoBtn);
            startRandoBtn.SetNavigation(lemmBtn.Button, startNormalBtn, backBtn, startNormalBtn);
            startSteelNormalBtn.SetNavigation(magolorBtn.Button, startSteelRandoBtn, backBtn, startSteelRandoBtn);
            startSteelRandoBtn.SetNavigation(lemmBtn.Button, startSteelNormalBtn, backBtn, startSteelNormalBtn);
            backBtn.SetNavigation(startNormalBtn, startNormalBtn, modeBtn.Button, startRandoBtn);
            allBossesBtn.Button.SetNavigation(modeBtn.Button, charmNotchBtn.Button, allSkillsBtn.Button, presetBtn.Button);
            allSkillsBtn.Button.SetNavigation(allBossesBtn.Button, lemmBtn.Button, allCharmsBtn.Button, shadeSkipsBtn.Button);
            allCharmsBtn.Button.SetNavigation(allSkillsBtn.Button, lemmBtn.Button, gameTypeBtn.Button, acidSkipsBtn.Button);
            charmNotchBtn.Button.SetNavigation(modeBtn.Button, presetBtn.Button, lemmBtn.Button, allBossesBtn.Button);
            lemmBtn.Button.SetNavigation(charmNotchBtn.Button, shadeSkipsBtn.Button, startRandoBtn, allSkillsBtn.Button);
            presetBtn.Button.SetNavigation(modeBtn.Button, allBossesBtn.Button, shadeSkipsBtn.Button, charmNotchBtn.Button);
            shadeSkipsBtn.Button.SetNavigation(presetBtn.Button, allSkillsBtn.Button, acidSkipsBtn.Button, lemmBtn.Button);
            acidSkipsBtn.Button.SetNavigation(shadeSkipsBtn.Button, allCharmsBtn.Button, spikeTunnelsBtn.Button, lemmBtn.Button);
            spikeTunnelsBtn.Button.SetNavigation(acidSkipsBtn.Button, allCharmsBtn.Button, miscSkipsBtn.Button, lemmBtn.Button);
            miscSkipsBtn.Button.SetNavigation(spikeTunnelsBtn.Button, gameTypeBtn.Button, fireballSkipsBtn.Button, lemmBtn.Button);
            fireballSkipsBtn.Button.SetNavigation(miscSkipsBtn.Button, gameTypeBtn.Button, magolorBtn.Button, lemmBtn.Button);
            magolorBtn.Button.SetNavigation(fireballSkipsBtn.Button, gameTypeBtn.Button, startNormalBtn, lemmBtn.Button);
            modeBtn.Button.SetNavigation(backBtn, modeBtn.Button, allBossesBtn.Button, modeBtn.Button);
            gameTypeBtn.Button.SetNavigation(allCharmsBtn.Button, lemmBtn.Button, backBtn, fireballSkipsBtn.Button);

            // Setup event for changing difficulty settings buttons
            void UpdateButtons(RandoMenuItem<string> item)
            {
                switch (item.CurrentSelection)
                {
                    case "Easy":
                        SetShadeSkips(false);
                        acidSkipsBtn.SetSelection(false);
                        spikeTunnelsBtn.SetSelection(false);
                        miscSkipsBtn.SetSelection(false);
                        fireballSkipsBtn.SetSelection(false);
                        magolorBtn.SetSelection(false);
                        break;
                    case "Hard":
                        SetShadeSkips(true);
                        acidSkipsBtn.SetSelection(true);
                        spikeTunnelsBtn.SetSelection(true);
                        miscSkipsBtn.SetSelection(true);
                        fireballSkipsBtn.SetSelection(true);
                        magolorBtn.SetSelection(false);
                        break;
                    case "Moglar":
                        SetShadeSkips(true);
                        acidSkipsBtn.SetSelection(true);
                        spikeTunnelsBtn.SetSelection(true);
                        miscSkipsBtn.SetSelection(true);
                        fireballSkipsBtn.SetSelection(true);
                        magolorBtn.SetSelection(true);
                        break;
                    case "Custom":
                        item.SetSelection("Easy");
                        goto case "Easy";
                    default:
                        RandomizerMod.Instance.LogWarn("Unknown value in preset button: " + item.CurrentSelection);
                        break;
                }
            }

            // Event for setting stuff to hard mode on no claw selected
            void SetNoClaw(RandoMenuItem<string> item)
            {
                if (item.CurrentSelection == "No Claw")
                {
                    SetShadeSkips(true);
                    acidSkipsBtn.SetSelection(true);
                    spikeTunnelsBtn.SetSelection(true);
                    miscSkipsBtn.SetSelection(true);
                    fireballSkipsBtn.SetSelection(true);

                    if (magolorBtn.CurrentSelection)
                    {
                        presetBtn.SetSelection("Moglar");
                    }
                    else
                    {
                        presetBtn.SetSelection("Hard");
                    }
                }
            }

            void SetShadeSkips(bool enabled)
            {
                if (enabled)
                {
                    gameTypeBtn.SetSelection("Normal");
                    SwitchGameType(false);
                }
                else
                {
                    modeBtn.SetSelection("Standard");
                }

                shadeSkipsBtn.SetSelection(enabled);
            }

            void SettingChanged(RandoMenuItem<bool> item)
            {
                presetBtn.SetSelection("Custom");

                if (!item.CurrentSelection && item.Name != "Mag Skips")
                {
                    modeBtn.SetSelection("Standard");
                }
            }

            presetBtn.Changed += UpdateButtons;
            modeBtn.Changed += SetNoClaw;

            shadeSkipsBtn.Changed += SettingChanged;
            shadeSkipsBtn.Changed += SaveShadeVal;
            acidSkipsBtn.Changed += SettingChanged;
            spikeTunnelsBtn.Changed += SettingChanged;
            miscSkipsBtn.Changed += SettingChanged;
            fireballSkipsBtn.Changed += SettingChanged;
            magolorBtn.Changed += SettingChanged;

            // Setup game type button changes
            void SaveShadeVal(RandoMenuItem<bool> item)
            {
                SetShadeSkips(shadeSkipsBtn.CurrentSelection);
            }

            void SwitchGameType(bool steelMode)
            {
                if (!steelMode)
                {
                    // Normal mode
                    startRandoBtn.transform.localPosition = new Vector2(650, -480);
                    startNormalBtn.transform.localPosition = new Vector2(-650, -480);
                    startSteelRandoBtn.transform.localPosition = new Vector2(10000, 10000);
                    startSteelNormalBtn.transform.localPosition = new Vector2(10000, 10000);

                    backBtn.SetNavigation(startNormalBtn, startNormalBtn, modeBtn.Button, startRandoBtn);
                    magolorBtn.Button.SetNavigation(fireballSkipsBtn.Button, gameTypeBtn.Button, startNormalBtn, lemmBtn.Button);
                    lemmBtn.Button.SetNavigation(charmNotchBtn.Button, shadeSkipsBtn.Button, startRandoBtn, allSkillsBtn.Button);
                }
                else
                {
                    // Steel Soul mode
                    startRandoBtn.transform.localPosition = new Vector2(10000, 10000);
                    startNormalBtn.transform.localPosition = new Vector2(10000, 10000);
                    startSteelRandoBtn.transform.localPosition = new Vector2(650, -480);
                    startSteelNormalBtn.transform.localPosition = new Vector2(-650, -480);

                    SetShadeSkips(false);

                    backBtn.SetNavigation(startSteelNormalBtn, startSteelNormalBtn, modeBtn.Button, startSteelRandoBtn);
                    magolorBtn.Button.SetNavigation(fireballSkipsBtn.Button, gameTypeBtn.Button, startSteelNormalBtn, lemmBtn.Button);
                    lemmBtn.Button.SetNavigation(charmNotchBtn.Button, shadeSkipsBtn.Button, startSteelRandoBtn, allSkillsBtn.Button);
                }
            }

            gameTypeBtn.Button.AddEvent(EventTriggerType.Submit, garbage => SwitchGameType(gameTypeBtn.CurrentSelection != "Normal"));

            // Setup start game button events
            void StartGame(bool rando)
            {
                RandomizerMod.Instance.Settings.CharmNotch = charmNotchBtn.CurrentSelection;
                RandomizerMod.Instance.Settings.Lemm = lemmBtn.CurrentSelection;

                RandomizerMod.Instance.Settings.AllBosses = allBossesBtn.CurrentSelection;
                RandomizerMod.Instance.Settings.AllCharms = allCharmsBtn.CurrentSelection;
                RandomizerMod.Instance.Settings.AllSkills = allSkillsBtn.CurrentSelection;

                RandomizerMod.Instance.Settings.Randomizer = rando;

                if (RandomizerMod.Instance.Settings.Randomizer)
                {
                    RandomizerMod.Instance.Settings.NoClaw = modeBtn.CurrentSelection == "No Claw";

                    if (gameTypeBtn.CurrentSelection != "Normal")
                    {
                        // This check may not be needed.
                        RandomizerMod.Instance.Settings.ShadeSkips = false;
                    }
                    else
                    {
                        RandomizerMod.Instance.Settings.ShadeSkips = shadeSkipsBtn.CurrentSelection;
                    }

                    RandomizerMod.Instance.Settings.AcidSkips = acidSkipsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.SpikeTunnels = spikeTunnelsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.MiscSkips = miscSkipsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.FireballSkips = fireballSkipsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.MagSkips = magolorBtn.CurrentSelection;
                }

                RandomizerMod.Instance.StartNewGame();
            }

            startNormalBtn.AddEvent(EventTriggerType.Submit, garbage => StartGame(false));
            startRandoBtn.AddEvent(EventTriggerType.Submit, garbage => StartGame(true));
            startSteelNormalBtn.AddEvent(EventTriggerType.Submit, garbage => StartGame(false));
            startSteelRandoBtn.AddEvent(EventTriggerType.Submit, garbage => StartGame(true));
        }

        private static void ParseSeedInput(string input)
        {
            if (int.TryParse(input, out int newSeed))
            {
                RandomizerMod.Instance.Settings.Seed = newSeed;
            }
            else
            {
                RandomizerMod.Instance.LogWarn($"Seed input \"{input}\" could not be parsed to an integer");
            }
        }

        private static void CreateLabel(MenuButton baseObj, Vector2 position, string text)
        {
            GameObject label = baseObj.Clone(text + "Label", MenuButton.MenuButtonType.Activate, position, text).gameObject;
            Object.Destroy(label.GetComponent<EventTrigger>());
            Object.Destroy(label.GetComponent<MenuButton>());
        }

        private class RandoMenuItem<T> where T : IEquatable<T>
        {
            private T[] selections;
            private int currentSelection;
            private Text text;
            private FixVerticalAlign align;

            public RandoMenuItem(MenuButton baseObj, Vector2 position, string name, params T[] values)
            {
                if (string.IsNullOrEmpty(name) || baseObj == null || values == null || values.Length == 0)
                {
                    throw new ArgumentNullException("Null parameters in BoolMenuButton");
                }

                selections = values;
                Name = name;

                Button = baseObj.Clone(name + "Button", MenuButton.MenuButtonType.Activate, position, string.Empty);

                text = Button.transform.Find("Text").GetComponent<Text>();
                align = Button.gameObject.GetComponentInChildren<FixVerticalAlign>(true);

                Button.ClearEvents();
                Button.AddEvent(EventTriggerType.Submit, GotoNext);

                RefreshText();
            }

            public delegate void RandoMenuItemChanged(RandoMenuItem<T> item);

            public event RandoMenuItemChanged Changed
            {
                add => ChangedInternal += value;
                remove => ChangedInternal -= value;
            }

            private event RandoMenuItemChanged ChangedInternal;

            public T CurrentSelection => selections[currentSelection];

            public MenuButton Button { get; private set; }

            public string Name { get; private set; }

            public void SetSelection(T obj)
            {
                for (int i = 0; i < selections.Length; i++)
                {
                    if (selections[i].Equals(obj))
                    {
                        currentSelection = i;
                        break;
                    }
                }

                RefreshText(false);
            }

            private void GotoNext(BaseEventData data = null)
            {
                currentSelection++;
                if (currentSelection >= selections.Length)
                {
                    currentSelection = 0;
                }

                RefreshText();
            }

            private void RefreshText(bool invokeEvent = true)
            {
                text.text = Name + ": " + selections[currentSelection].ToString();
                align.AlignText();

                if (invokeEvent)
                {
                    ChangedInternal?.Invoke(this);
                }
            }
        }
    }
}
