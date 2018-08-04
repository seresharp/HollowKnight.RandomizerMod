using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Modding;
using RandomizerMod.Actions;

namespace RandomizerMod.Components
{
    internal class Shop : MonoBehaviour
    {
        //float[] can't be const and this is mostly the same thing (Not really but whatever)
        private static readonly float[] defaultPositions = new float[] { 0.8f, 0.7f, 0.5825f, 0.465f, 0.365f, 0.265f };

        private static Sprite blackPixel = CanvasUtil.NullSprite(new byte[] { 0x00, 0x00, 0x00, 0xAA });

        private static Font perpetua;

        public ShopItemDef[] items;
        public ShopType type;

        private Sprite geoSprite;

        private int[] validItems;
        private int selected = 0;

        private GameObject[,] itemImages;

        static Shop()
        {
            CanvasUtil.CreateFonts();
            foreach (Font f in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (f.name == "Perpetua")
                {
                    perpetua = f;
                    break;
                }
            }
        }

        public static void Show()
        {
            //Create base canvas
            GameObject canvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));

            //Add shop component, set values
            Shop shop = canvas.AddComponent<Shop>();
            shop.items = new ShopItemDef[]
            {
                new ShopItemDef()
                {
                    playerDataBoolName = "gotCharm_1",
                    nameConvo = "CHARM_NAME_1",
                    descConvo = "RANDOMIZER_CHARM_DESC_1",
                    requiredPlayerDataBool = "",
                    removalPlayerDataBool = "",
                    dungDiscount = false,
                    notchCostBool = "charmCost_1",
                    cost = 420,
                    spriteName = "Charms.1.png"
                },
                new ShopItemDef()
                {
                    playerDataBoolName = "gotCharm_2",
                    nameConvo = "CHARM_NAME_2",
                    descConvo = "RANDOMIZER_CHARM_DESC_2",
                    requiredPlayerDataBool = "",
                    removalPlayerDataBool = "",
                    dungDiscount = false,
                    notchCostBool = "charmCost_2",
                    cost = 420,
                    spriteName = "Charms.2.png"
                },
                new ShopItemDef()
                {
                    playerDataBoolName = "gotCharm_3",
                    nameConvo = "CHARM_NAME_3",
                    descConvo = "RANDOMIZER_CHARM_DESC_3",
                    requiredPlayerDataBool = "",
                    removalPlayerDataBool = "",
                    dungDiscount = false,
                    notchCostBool = "charmCost_3",
                    cost = 420,
                    spriteName = "Charms.3.png"
                },
                new ShopItemDef()
                {
                    playerDataBoolName = "gotCharm_4",
                    nameConvo = "CHARM_NAME_4",
                    descConvo = "RANDOMIZER_CHARM_DESC_4",
                    requiredPlayerDataBool = "",
                    removalPlayerDataBool = "",
                    dungDiscount = false,
                    notchCostBool = "charmCost_4",
                    cost = 420,
                    spriteName = "Charms.4.png"
                },
                new ShopItemDef()
                {
                    playerDataBoolName = "gotCharm_5",
                    nameConvo = "CHARM_NAME_5",
                    descConvo = "RANDOMIZER_CHARM_DESC_5",
                    requiredPlayerDataBool = "",
                    removalPlayerDataBool = "",
                    dungDiscount = false,
                    notchCostBool = "charmCost_5",
                    cost = 420,
                    spriteName = "Charms.5.png"
                },
                new ShopItemDef()
                {
                    playerDataBoolName = "gotCharm_6",
                    nameConvo = "CHARM_NAME_6",
                    descConvo = "RANDOMIZER_CHARM_DESC_6",
                    requiredPlayerDataBool = "",
                    removalPlayerDataBool = "",
                    dungDiscount = false,
                    notchCostBool = "charmCost_6",
                    cost = 420,
                    spriteName = "Charms.6.png"
                },
                new ShopItemDef()
                {
                    playerDataBoolName = "gotCharm_7",
                    nameConvo = "CHARM_NAME_7",
                    descConvo = "RANDOMIZER_CHARM_DESC_7",
                    requiredPlayerDataBool = "",
                    removalPlayerDataBool = "",
                    dungDiscount = false,
                    notchCostBool = "charmCost_7",
                    cost = 420,
                    spriteName = "Charms.7.png"
                },
                new ShopItemDef()
                {
                    playerDataBoolName = "gotCharm_8",
                    nameConvo = "CHARM_NAME_8",
                    descConvo = "RANDOMIZER_CHARM_DESC_8",
                    requiredPlayerDataBool = "",
                    removalPlayerDataBool = "",
                    dungDiscount = false,
                    notchCostBool = "charmCost_8",
                    cost = 420,
                    spriteName = "Charms.8.png"
                },
                new ShopItemDef()
                {
                    playerDataBoolName = "gotCharm_9",
                    nameConvo = "CHARM_NAME_9",
                    descConvo = "RANDOMIZER_CHARM_DESC_9",
                    requiredPlayerDataBool = "",
                    removalPlayerDataBool = "",
                    dungDiscount = false,
                    notchCostBool = "charmCost_9",
                    cost = 420,
                    spriteName = "Charms.9.png"
                },
                new ShopItemDef()
                {
                    playerDataBoolName = "gotCharm_10",
                    nameConvo = "CHARM_NAME_10",
                    descConvo = "RANDOMIZER_CHARM_DESC_10",
                    requiredPlayerDataBool = "",
                    removalPlayerDataBool = "",
                    dungDiscount = false,
                    notchCostBool = "charmCost_10",
                    cost = 420,
                    spriteName = "Charms.10.png"
                }
            };
            shop.type = ShopType.Geo;
        }

        public void Start()
        {
            geoSprite = type == ShopType.Geo ? RandomizerMod.sprites["UI.Shop.Geo.png"] : RandomizerMod.sprites["UI.Shop.Essence.png"];

            StartCoroutine(ShowShop());
        }

        private void UpdateValidItems()
        {
            List<int> validItemsList = new List<int>();
            for (int i = 0; i < items.Length; i++)
            {
                if (IsValid(items[i])) validItemsList.Add(i);
            }
            validItems = validItemsList.ToArray();
        }

        private bool IsValid(ShopItemDef item)
        {
            PlayerData pd = PlayerData.instance;

            //These ones can't be empty
            if (string.IsNullOrEmpty(item.playerDataBoolName) || string.IsNullOrEmpty(item.nameConvo) || string.IsNullOrEmpty(item.descConvo) || string.IsNullOrEmpty(item.spriteName))
            {
                return false;
            }

            if (!RandomizerMod.sprites.ContainsKey(item.spriteName))
            {
                return false;
            }

            //These ones are fine to be empty, replacing null with empty since they're mostly the same thing in this context
            //No harm changing structs around since they're value types
            if (item.requiredPlayerDataBool == null) item.requiredPlayerDataBool = "";
            if (item.removalPlayerDataBool == null) item.removalPlayerDataBool = "";
            if (item.notchCostBool == null) item.notchCostBool = "";

            if (pd.GetBool(item.playerDataBoolName) || pd.GetBool(item.removalPlayerDataBool) || (item.requiredPlayerDataBool != "" && !pd.GetBool(item.requiredPlayerDataBool)))
            {
                return false;
            }

            if (item.cost < 0)
            {
                return false;
            }
            
            return true;
        }

        public bool HasItems()
        {
            UpdateValidItems();

            return validItems.Length > 0;
        }

        private void BuildItemImages()
        {
            if (itemImages != null && itemImages.Length > 0)
            {
                foreach (GameObject obj in itemImages)
                {
                    Destroy(obj);
                }
            }

            itemImages = new GameObject[validItems.Length, 4];

            for (int i = 0; i < itemImages.GetLength(0); i++)
            {
                itemImages[i, 0] = CanvasUtil.CreateImagePanel(gameObject, RandomizerMod.sprites[items[validItems[i]].spriteName], new CanvasUtil.RectData(new Vector2(90, 90), Vector2.zero, new Vector2(0.525f, 0f), new Vector2(0.525f, 0f)));
                itemImages[i, 1] = CanvasUtil.CreateImagePanel(gameObject, geoSprite, new CanvasUtil.RectData(new Vector2(50, 50), Vector2.zero, new Vector2(0.57f, 0f), new Vector2(0.57f, 0f)));

                int cost = (int)(items[validItems[i]].cost * (items[validItems[i]].dungDiscount ? 0.75f : 1));
                itemImages[i, 2] = CanvasUtil.CreateTextPanel(gameObject, cost.ToString(), 34, TextAnchor.MiddleCenter, new CanvasUtil.RectData(new Vector2(1920, 1080), Vector2.zero, new Vector2(0.61f, 0f), new Vector2(0.61f, 0f)), perpetua);

                if ((type == ShopType.Geo && cost > PlayerData.instance.geo) || (type == ShopType.Essence && cost > PlayerData.instance.dreamOrbs))
                {
                    itemImages[i, 3] = CanvasUtil.CreateImagePanel(gameObject, blackPixel, new CanvasUtil.RectData(new Vector2(300, 100), Vector2.zero, new Vector2(0.57f, 0f), new Vector2(0.57f, 0f)));
                    itemImages[i, 3].GetComponent<Image>().preserveAspect = false;
                }
            }

            foreach (GameObject obj in itemImages)
            {
                obj.SetActive(false);
            }
        }

        private void UpdatePositions()
        {
            if (itemImages == null) return;

            int pos = 2 - selected;

            for (int i = 0; i < itemImages.GetLength(0); i++)
            {
                for (int j = 0; j < itemImages.GetLength(1); j++)
                {
                    RandomizerMod.instance.Log(i + " " + j);
                    if (itemImages[i, j] != null)
                    {
                        if (pos >= 0 && pos < defaultPositions.Length)
                        {
                            itemImages[i, j].SetActive(true);
                            RectTransform rect = itemImages[i, j].GetComponent<RectTransform>();
                            if (rect != null)
                            {
                                rect.anchorMin = new Vector2(rect.anchorMin.x, defaultPositions[pos]);
                                rect.anchorMax = new Vector2(rect.anchorMax.x, defaultPositions[pos]);
                            }
                        }
                        else
                        {
                            itemImages[i, j].SetActive(false);
                        }
                    }
                }
                pos++;
            }
        }

        private void ResetItems()
        {
            selected = 0;
            UpdateValidItems();
            BuildItemImages();
            UpdatePositions();
        }

        private IEnumerator ShowShop()
        {
            GameObject background = CanvasUtil.CreateImagePanel(gameObject, RandomizerMod.sprites["UI.Shop.Background.png"], new CanvasUtil.RectData(new Vector2(810, 813), Vector2.zero, new Vector2(0.675f, 0.525f), new Vector2(0.675f, 0.525f)));

            GameObject bottomFleur = CanvasUtil.CreateImagePanel(gameObject, RandomizerMod.sprites["Anim.Shop.BottomFleur.0.png"], new CanvasUtil.RectData(new Vector2(811, 241), Vector2.zero, new Vector2(0.675f, 0.3f), new Vector2(0.675f, 0.3f)));
            StartCoroutine(AnimateImage(bottomFleur, new Sprite[] {
                //RandomizerMod.sprites["Anim.Shop.BottomFleur.0.png"],
                //RandomizerMod.sprites["Anim.Shop.BottomFleur.1.png"],
                RandomizerMod.sprites["Anim.Shop.BottomFleur.2.png"],
                RandomizerMod.sprites["Anim.Shop.BottomFleur.3.png"],
                RandomizerMod.sprites["Anim.Shop.BottomFleur.4.png"],
                RandomizerMod.sprites["Anim.Shop.BottomFleur.5.png"],
                RandomizerMod.sprites["Anim.Shop.BottomFleur.6.png"],
                RandomizerMod.sprites["Anim.Shop.BottomFleur.7.png"],
                RandomizerMod.sprites["Anim.Shop.BottomFleur.8.png"]
            }, 12));
            StartCoroutine(TweenY(bottomFleur, 0.3f, 0.2f, 60, 15));

            GameObject topFleur = CanvasUtil.CreateImagePanel(gameObject, RandomizerMod.sprites["Anim.Shop.TopFleur.0.png"], new CanvasUtil.RectData(new Vector2(808, 198), Vector2.zero, new Vector2(0.675f, 0.6f), new Vector2(0.675f, 0.6f)));
            StartCoroutine(AnimateImage(topFleur, new Sprite[]
            {
                RandomizerMod.sprites["Anim.Shop.TopFleur.0.png"],
                RandomizerMod.sprites["Anim.Shop.TopFleur.1.png"],
                RandomizerMod.sprites["Anim.Shop.TopFleur.2.png"],
                RandomizerMod.sprites["Anim.Shop.TopFleur.3.png"],
                RandomizerMod.sprites["Anim.Shop.TopFleur.4.png"],
                RandomizerMod.sprites["Anim.Shop.TopFleur.5.png"],
                RandomizerMod.sprites["Anim.Shop.TopFleur.6.png"],
                RandomizerMod.sprites["Anim.Shop.TopFleur.7.png"],
                RandomizerMod.sprites["Anim.Shop.TopFleur.8.png"]
            }, 12));
            StartCoroutine(TweenY(topFleur, 0.6f, 0.85f, 60, 15));

            yield return StartCoroutine(CanvasUtil.FadeInCanvasGroup(background.AddComponent<CanvasGroup>()));

            CanvasUtil.CreateImagePanel(gameObject, RandomizerMod.sprites["UI.Shop.Selector.png"], new CanvasUtil.RectData(new Vector2(340, 113), Vector2.zero, new Vector2(0.57f, 0.5825f), new Vector2(0.57f, 0.5825f)));
            CanvasUtil.CreateImagePanel(gameObject, RandomizerMod.sprites["UI.Shop.Shitpost.png"], new CanvasUtil.RectData(new Vector2(112, 112), Vector2.zero, new Vector2(0.6775f, 0.92f), new Vector2(0.6775f, 0.92f)));

            ResetItems();

            StartCoroutine(ListenForInput());
        }

        private IEnumerator ListenForInput()
        {
            HeroActions buttons = GameManager.instance.inputHandler.inputActions;

            while (true)
            {
                if (selected > 0 && buttons.up.WasPressed)
                {
                    selected--;
                    UpdatePositions();
                }
                else if (selected < (validItems.Length - 1) && buttons.down.WasPressed)
                {
                    selected++;
                    UpdatePositions();
                }
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator AnimateImage(GameObject fleur, Sprite[] frames, float fps)
        {
            Image img = fleur.GetComponent<Image>();
            int spriteNum = 0;

            while (spriteNum < frames.Length)
            {
                img.sprite = frames[spriteNum];
                spriteNum++;
                yield return new WaitForSeconds(1 / fps);
            }
        }

        private IEnumerator TweenY(GameObject obj, float start, float end, float fps, int updateCount)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(rect.anchorMin.x, start);
            rect.anchorMax = new Vector2(rect.anchorMax.x, start);

            float updateAmount = (end - start) / updateCount;

            while (updateCount > 0)
            {
                yield return new WaitForSeconds(1 / fps);
                rect.anchorMin = new Vector2(rect.anchorMin.x, rect.anchorMin.y + updateAmount);
                rect.anchorMax = new Vector2(rect.anchorMax.x, rect.anchorMax.y + updateAmount);
                updateCount--;
            }

            rect.anchorMin = new Vector2(rect.anchorMin.x, end);
            rect.anchorMax = new Vector2(rect.anchorMax.x, end);
        }

        public enum ShopType
        {
            Geo,
            Essence
        }
    }
}
