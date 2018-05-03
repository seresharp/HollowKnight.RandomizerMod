using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Modding;
using RandomizerMod.Extensions;

namespace RandomizerMod
{
    internal class BigItemPopup : MonoBehaviour
    {
        private static Sprite blackPixel = CanvasUtil.NullSprite(new byte[] { 0x00, 0x00, 0x00, 0xAA });
        private static Font perpetua;
        private static Sprite[] frames;

        public Sprite imagePrompt;
        public string takeText;
        public string nameText;
        public string buttonText;
        public string descOneText;
        public string descTwoText;
        
        static BigItemPopup()
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

            frames = new Sprite[] 
            {
                RandomizerMod.sprites["Anim.BigItemFleur.0.png"],
                RandomizerMod.sprites["Anim.BigItemFleur.1.png"],
                RandomizerMod.sprites["Anim.BigItemFleur.2.png"],
                RandomizerMod.sprites["Anim.BigItemFleur.3.png"],
                RandomizerMod.sprites["Anim.BigItemFleur.4.png"],
                RandomizerMod.sprites["Anim.BigItemFleur.5.png"],
                RandomizerMod.sprites["Anim.BigItemFleur.6.png"],
                RandomizerMod.sprites["Anim.BigItemFleur.7.png"],
                RandomizerMod.sprites["Anim.BigItemFleur.8.png"],
            };
        }

        public static void Show(string spriteKey, string takeKey, string nameKey, string buttonKey, string descOneKey, string descTwoKey)
        {
            //Create base canvas
            GameObject canvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));

            //Add popup component, set values
            BigItemPopup popup = canvas.AddComponent<BigItemPopup>();
            popup.imagePrompt = RandomizerMod.sprites[spriteKey];
            popup.takeText = Language.Language.Get(takeKey, "Prompts");
            popup.nameText = Language.Language.Get(nameKey, "UI");
            popup.buttonText = Language.Language.Get(buttonKey, "Prompts");
            popup.descOneText = Language.Language.Get(descOneKey, "Prompts");
            popup.descTwoText = Language.Language.Get(descTwoKey, "Prompts");

            DontDestroyOnLoad(canvas);
        }

        public void Start()
        {
            StartCoroutine(ShowPopup());
        }

        private IEnumerator ShowPopup()
        {
            try
            {
                //Begin dimming the scene
                GameObject dimmer = CanvasUtil.CreateImagePanel(gameObject, blackPixel, new CanvasUtil.RectData(Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one));
                dimmer.GetComponent<Image>().preserveAspect = false;
                CanvasGroup dimmerCG = dimmer.AddComponent<CanvasGroup>();

                dimmerCG.blocksRaycasts = false;
                dimmerCG.interactable = false;
                dimmerCG.alpha = 0;

                StartCoroutine(CanvasUtil.FadeInCanvasGroup(dimmerCG));
            }
            catch (Exception e)
            {
                Modding.Logger.Log(e);
            }

            yield return new WaitForSecondsRealtime(0.1f);

            try
            {
                //Begin fading in the top bits of the popup
                GameObject topImage = CanvasUtil.CreateImagePanel(gameObject, imagePrompt, new CanvasUtil.RectData(imagePrompt.Size(), Vector2.zero, new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.8f)));
                GameObject topTextOne = CanvasUtil.CreateTextPanel(gameObject, takeText, 34, TextAnchor.MiddleCenter, new CanvasUtil.RectData(new Vector2(1920, 100), Vector2.zero, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f)), perpetua);
                GameObject topTextTwo = CanvasUtil.CreateTextPanel(gameObject, nameText, 76, TextAnchor.MiddleCenter, new CanvasUtil.RectData(new Vector2(1920, 300), Vector2.zero, new Vector2(0.5f, 0.49f), new Vector2(0.5f, 0.49f)));

                CanvasGroup topImageCG = topImage.AddComponent<CanvasGroup>();
                CanvasGroup topTextOneCG = topTextOne.AddComponent<CanvasGroup>();
                CanvasGroup topTextTwoCG = topTextTwo.AddComponent<CanvasGroup>();

                topImageCG.blocksRaycasts = false;
                topImageCG.interactable = false;
                topImageCG.alpha = 0;

                topTextOneCG.blocksRaycasts = false;
                topTextOneCG.interactable = false;
                topTextOneCG.alpha = 0;

                topTextTwoCG.blocksRaycasts = false;
                topTextTwoCG.interactable = false;
                topTextTwoCG.alpha = 0;

                StartCoroutine(CanvasUtil.FadeInCanvasGroup(topImageCG));
                StartCoroutine(CanvasUtil.FadeInCanvasGroup(topTextOneCG));
                StartCoroutine(CanvasUtil.FadeInCanvasGroup(topTextTwoCG));
            }
            catch (Exception e)
            {
                Modding.Logger.Log(e);
            }

            //Animate the middle fleur
            GameObject fleur = CanvasUtil.CreateImagePanel(gameObject, frames[0], new CanvasUtil.RectData(new Vector2(frames[0].texture.width / 1.6f, frames[0].texture.height / 1.6f), Vector2.zero, new Vector2(0.5f, 0.4125f), new Vector2(0.5f, 0.4125f)));
            yield return StartCoroutine(AnimateFleur(fleur, 15));
            yield return new WaitForSeconds(0.25f);

            //Fade in the remaining text
            GameObject botTextOne = CanvasUtil.CreateTextPanel(gameObject, "You already know what to press", 34, TextAnchor.MiddleCenter, new CanvasUtil.RectData(new Vector2(1920, 100), Vector2.zero, new Vector2(0.5f, 0.335f), new Vector2(0.5f, 0.335f)), perpetua);
            GameObject botTextTwo = CanvasUtil.CreateTextPanel(gameObject, descOneText, 34, TextAnchor.MiddleCenter, new CanvasUtil.RectData(new Vector2(1920, 100), Vector2.zero, new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.26f)), perpetua);
            GameObject botTextThree =  CanvasUtil.CreateTextPanel(gameObject, descTwoText, 34, TextAnchor.MiddleCenter, new CanvasUtil.RectData(new Vector2(1920, 100), Vector2.zero, new Vector2(0.5f, 0.205f), new Vector2(0.5f, 0.205f)), perpetua);

            CanvasGroup botTextOneCG = botTextOne.AddComponent<CanvasGroup>();
            CanvasGroup botTextTwoCG = botTextTwo.AddComponent<CanvasGroup>();
            CanvasGroup botTextThreeCG = botTextThree.AddComponent<CanvasGroup>();

            botTextOneCG.blocksRaycasts = false;
            botTextOneCG.interactable = false;
            botTextOneCG.alpha = 0;

            botTextTwoCG.blocksRaycasts = false;
            botTextTwoCG.interactable = false;
            botTextTwoCG.alpha = 0;

            botTextThreeCG.blocksRaycasts = false;
            botTextThreeCG.interactable = false;
            botTextThreeCG.alpha = 0;

            yield return StartCoroutine(CanvasUtil.FadeInCanvasGroup(botTextOneCG));
            StartCoroutine(CanvasUtil.FadeInCanvasGroup(botTextTwoCG));
            yield return StartCoroutine(CanvasUtil.FadeInCanvasGroup(botTextThreeCG));
            yield return new WaitForSecondsRealtime(2f);

            //Can I offer you an egg in this trying time?
            GameObject egg = CanvasUtil.CreateImagePanel(gameObject, RandomizerMod.sprites["UI.egg.png"], new CanvasUtil.RectData(new Vector2(RandomizerMod.sprites["UI.egg.png"].texture.width / 1.65f, RandomizerMod.sprites["UI.egg.png"].texture.height / 1.65f), Vector2.zero, new Vector2(0.5f, 0.1075f), new Vector2(0.5f, 0.1075f)));
            CanvasGroup eggCG = egg.AddComponent<CanvasGroup>();

            eggCG.blocksRaycasts = false;
            eggCG.interactable = false;
            eggCG.alpha = 0;

            //Wait for at least one fade in before polling input
            yield return CanvasUtil.FadeInCanvasGroup(eggCG);

            //Save the coroutine to stop it later
            Coroutine coroutine = StartCoroutine(FadeInOut(eggCG));

            //Wait for the user to cancel the menu
            while (true)
            {
                HeroActions actions = GameManager.instance.inputHandler.inputActions;
                if (actions.jump.WasPressed || actions.attack.WasPressed || actions.menuCancel.WasPressed) break;
                yield return new WaitForEndOfFrame();
            }

            //Fade out the full popup
            yield return CanvasUtil.FadeOutCanvasGroup(gameObject.GetComponent<CanvasGroup>());

            //Stop the egg routine and destroy everything
            StopCoroutine(coroutine);
            Destroy(gameObject);
        }

        private IEnumerator AnimateFleur(GameObject fleur, float fps)
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

        private IEnumerator FadeInOut(CanvasGroup cg)
        {
            while (true)
            {
                yield return CanvasUtil.FadeOutCanvasGroup(cg);
                yield return CanvasUtil.FadeInCanvasGroup(cg);
            }
        }
    }
}
