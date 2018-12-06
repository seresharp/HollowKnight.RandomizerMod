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

            startNormalBtn.transform.localScale = startRandoBtn.transform.localScale = new Vector2(0.75f, 0.75f);

            MenuButton backBtn = back.Clone("Back", MenuButton.MenuButtonType.Proceed, new Vector2(0, -100), "Back");
            
            RandoMenuItem allBossesBtn = new RandoMenuItem(back, new Vector2(0, 850), "All Bosses", false, true);
            RandoMenuItem allSkillsBtn = new RandoMenuItem(back, new Vector2(0, 760), "All Skills", false, true);
            RandoMenuItem allCharmsBtn = new RandoMenuItem(back, new Vector2(0, 670), "All Charms", false, true);

            RandoMenuItem charmNotchBtn = new RandoMenuItem(back, new Vector2(900, 850), "Salubra Notches", true, false);
            RandoMenuItem lemmBtn = new RandoMenuItem(back, new Vector2(900, 760), "Lemm Sell All", true, false);

            RandoMenuItem presetBtn = new RandoMenuItem(back, new Vector2(-900, 850), "Preset", "Easy", "Hard", "Moglar");
            RandoMenuItem shadeSkipsBtn = new RandoMenuItem(back, new Vector2(-900, 760), "Shade Skips", false, true);
            RandoMenuItem acidSkipsBtn = new RandoMenuItem(back, new Vector2(-900, 670), "Acid Skips", false, true);
            RandoMenuItem spikeTunnelsBtn = new RandoMenuItem(back, new Vector2(-900, 580), "Spike Tunnels", false, true);
            RandoMenuItem miscSkipsBtn = new RandoMenuItem(back, new Vector2(-900, 490), "Misc Skips", false, true);
            RandoMenuItem fireballSkipsBtn = new RandoMenuItem(back, new Vector2(-900, 400), "Fireball Skips", false, true);
            RandoMenuItem magolorBtn = new RandoMenuItem(back, new Vector2(-900, 310), "Mag Skips", false, true);

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
            UIManager.instance.playModeMenuScreen.defaultHighlight = startRandoBtn;

            // Apply navigation info (up, right, down, left)
            startNormalBtn.SetNavigation(magolorBtn.Button, startRandoBtn, backBtn, startRandoBtn);
            startRandoBtn.SetNavigation(lemmBtn.Button, startNormalBtn, backBtn, startNormalBtn);
            backBtn.SetNavigation(startNormalBtn, backBtn, allBossesBtn.Button, backBtn);
            allBossesBtn.Button.SetNavigation(backBtn, charmNotchBtn.Button, allSkillsBtn.Button, presetBtn.Button);
            allSkillsBtn.Button.SetNavigation(allBossesBtn.Button, lemmBtn.Button, allCharmsBtn.Button, shadeSkipsBtn.Button);
            allCharmsBtn.Button.SetNavigation(allSkillsBtn.Button, lemmBtn.Button, startNormalBtn, acidSkipsBtn.Button);
            charmNotchBtn.Button.SetNavigation(backBtn, presetBtn.Button, lemmBtn.Button, allBossesBtn.Button);
            lemmBtn.Button.SetNavigation(charmNotchBtn.Button, shadeSkipsBtn.Button, startRandoBtn, allSkillsBtn.Button);
            presetBtn.Button.SetNavigation(backBtn, allBossesBtn.Button, shadeSkipsBtn.Button, charmNotchBtn.Button);
            shadeSkipsBtn.Button.SetNavigation(presetBtn.Button, allSkillsBtn.Button, acidSkipsBtn.Button, lemmBtn.Button);
            acidSkipsBtn.Button.SetNavigation(shadeSkipsBtn.Button, allCharmsBtn.Button, spikeTunnelsBtn.Button, lemmBtn.Button);
            spikeTunnelsBtn.Button.SetNavigation(acidSkipsBtn.Button, allCharmsBtn.Button, miscSkipsBtn.Button, lemmBtn.Button);
            miscSkipsBtn.Button.SetNavigation(spikeTunnelsBtn.Button, allCharmsBtn.Button, fireballSkipsBtn.Button, lemmBtn.Button);
            fireballSkipsBtn.Button.SetNavigation(miscSkipsBtn.Button, allCharmsBtn.Button, magolorBtn.Button, lemmBtn.Button);
            magolorBtn.Button.SetNavigation(fireballSkipsBtn.Button, allCharmsBtn.Button, startNormalBtn, lemmBtn.Button);

            // Setup event for changing difficulty settings buttons
            void UpdateButtons(RandoMenuItem item)
            {
                if (item.CurrentSelection.Equals("Easy"))
                {
                    shadeSkipsBtn.SetSelection(false);
                    acidSkipsBtn.SetSelection(false);
                    spikeTunnelsBtn.SetSelection(false);
                    miscSkipsBtn.SetSelection(false);
                    fireballSkipsBtn.SetSelection(false);
                    magolorBtn.SetSelection(false);
                }
                else if (item.CurrentSelection.Equals("Hard"))
                {
                    shadeSkipsBtn.SetSelection(true);
                    acidSkipsBtn.SetSelection(true);
                    spikeTunnelsBtn.SetSelection(true);
                    miscSkipsBtn.SetSelection(true);
                    fireballSkipsBtn.SetSelection(true);
                    magolorBtn.SetSelection(false);
                }
                else if (item.CurrentSelection.Equals("Moglar"))
                {
                    shadeSkipsBtn.SetSelection(true);
                    acidSkipsBtn.SetSelection(true);
                    spikeTunnelsBtn.SetSelection(true);
                    miscSkipsBtn.SetSelection(true);
                    fireballSkipsBtn.SetSelection(true);
                    magolorBtn.SetSelection(true);
                }
            }

            presetBtn.Changed += UpdateButtons;

            // Setup start game button events
            void StartGame(bool rando)
            {
                RandomizerMod.Instance.Settings.CharmNotch = (bool)charmNotchBtn.CurrentSelection;
                RandomizerMod.Instance.Settings.Lemm = (bool)lemmBtn.CurrentSelection;

                RandomizerMod.Instance.Settings.AllBosses = (bool)allBossesBtn.CurrentSelection;
                RandomizerMod.Instance.Settings.AllCharms = (bool)allCharmsBtn.CurrentSelection;
                RandomizerMod.Instance.Settings.AllSkills = (bool)allSkillsBtn.CurrentSelection;

                RandomizerMod.Instance.Settings.Randomizer = rando;

                if (RandomizerMod.Instance.Settings.Randomizer)
                {
                    RandomizerMod.Instance.Settings.ShadeSkips = (bool)shadeSkipsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.AcidSkips = (bool)acidSkipsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.SpikeTunnels = (bool)spikeTunnelsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.MiscSkips = (bool)miscSkipsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.FireballSkips = (bool)fireballSkipsBtn.CurrentSelection;
                    RandomizerMod.Instance.Settings.MagSkips = (bool)magolorBtn.CurrentSelection;
                }

                RandomizerMod.Instance.StartNewGame();
            }

            startNormalBtn.AddEvent(EventTriggerType.Submit, garbage => StartGame(false));
            startRandoBtn.AddEvent(EventTriggerType.Submit, garbage => StartGame(true));
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

        private class RandoMenuItem
        {
            private object[] selections;
            private int currentSelection;
            private Text text;
            private FixVerticalAlign align;

            public RandoMenuItem(MenuButton baseObj, Vector2 position, string name, params object[] values)
            {
                if (string.IsNullOrEmpty(name) || baseObj == null || values.Length == 0)
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

            public delegate void RandoMenuItemChanged(RandoMenuItem item);

            public event RandoMenuItemChanged Changed
            {
                add => ChangedInternal += value;
                remove => ChangedInternal -= value;
            }

            private event RandoMenuItemChanged ChangedInternal;

            public object CurrentSelection => selections[currentSelection];

            public MenuButton Button { get; private set; }

            public string Name { get; private set; }

            public void SetSelection(object obj)
            {
                for (int i = 0; i < selections.Length; i++)
                {
                    if (selections[i].Equals(obj))
                    {
                        currentSelection = i;
                        break;
                    }
                }

                RefreshText();
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

            private void RefreshText()
            {
                text.text = Name + ": " + selections[currentSelection].ToString();
                align.AlignText();
                ChangedInternal?.Invoke(this);
            }
        }
    }
}
