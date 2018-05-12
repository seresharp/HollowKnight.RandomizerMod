using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Modding;
using RandomizerMod.Actions;

namespace RandomizerMod.Components
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
        public GameObject fsmObj;
        public string fsmEvent;
        
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

        public static GameObject ShowAdditive(BigItemDef[] items, GameObject fsmObj = null, string eventName = null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (!PlayerData.instance.GetBool(items[i].boolName))
                {
                    return Show(items[i], fsmObj, eventName);
                }
            }

            //In case of failure to give item, prevent soft lock
            if (fsmObj != null && eventName != null)
            {
                FSMUtility.SendEventToGameObject(fsmObj, eventName);
            }

            return null;
        }

        public static GameObject Show(BigItemDef item, GameObject fsmObj = null, string eventName = null)
        {
            PlayerData.instance.SetBool(item.boolName, true);
            return Show(item.spriteKey, item.takeKey, item.nameKey, item.buttonKey, item.descOneKey, item.descTwoKey, fsmObj, eventName);
        }

        public static GameObject Show(string spriteKey, string takeKey, string nameKey, string buttonKey, string descOneKey, string descTwoKey, GameObject fsmObj = null, string eventName = null)
        {
            //Create base canvas
            GameObject canvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));

            //Add popup component, set values
            BigItemPopup popup = canvas.AddComponent<BigItemPopup>();
            popup.imagePrompt = RandomizerMod.sprites[spriteKey];
            popup.takeText = Language.Language.Get(takeKey, "Prompts").Replace("<br>", " ");
            popup.nameText = Language.Language.Get(nameKey, "UI").Replace("<br>", " ");
            popup.buttonText = Language.Language.Get(buttonKey, "Prompts").Replace("<br>", " ");
            popup.descOneText = Language.Language.Get(descOneKey, "Prompts").Replace("<br>", " ");
            popup.descTwoText = Language.Language.Get(descTwoKey, "Prompts").Replace("<br>", " ");
            popup.fsmObj = fsmObj;
            popup.fsmEvent = eventName;

            return canvas;
        }

        public void Start()
        {
            GameManager.instance.SaveGame(GameManager.instance.profileID);
            StartCoroutine(ShowPopup());
        }

        private IEnumerator ShowPopup()
        {
            //Begin dimming the scene
            GameObject dimmer = CanvasUtil.CreateImagePanel(gameObject, blackPixel, new CanvasUtil.RectData(Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one));
            dimmer.GetComponent<Image>().preserveAspect = false;
            CanvasGroup dimmerCG = dimmer.AddComponent<CanvasGroup>();

            dimmerCG.blocksRaycasts = false;
            dimmerCG.interactable = false;
            dimmerCG.alpha = 0;

            StartCoroutine(CanvasUtil.FadeInCanvasGroup(dimmerCG));

            yield return new WaitForSeconds(0.1f);

            //Aim for 400 high prompt image
            float scaler = imagePrompt.texture.height / 400f;
            Vector2 size = new Vector2(imagePrompt.texture.width / scaler, imagePrompt.texture.height / scaler);

            //Begin fading in the top bits of the popup
            GameObject topImage = CanvasUtil.CreateImagePanel(gameObject, imagePrompt, new CanvasUtil.RectData(size, Vector2.zero, new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.8f)));
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
            yield return StartCoroutine(CanvasUtil.FadeInCanvasGroup(topTextTwoCG));

            //Animate the middle fleur
            GameObject fleur = CanvasUtil.CreateImagePanel(gameObject, frames[0], new CanvasUtil.RectData(new Vector2(frames[0].texture.width / 1.6f, frames[0].texture.height / 1.6f), Vector2.zero, new Vector2(0.5f, 0.4125f), new Vector2(0.5f, 0.4125f)));
            yield return StartCoroutine(AnimateFleur(fleur, 12));
            yield return new WaitForSeconds(0.25f);

            //Fade in the remaining text
            GameObject botTextOne = CanvasUtil.CreateTextPanel(gameObject, buttonText, 34, TextAnchor.MiddleCenter, new CanvasUtil.RectData(new Vector2(1920, 100), Vector2.zero, new Vector2(0.5f, 0.335f), new Vector2(0.5f, 0.335f)), perpetua);
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
            yield return new WaitForSeconds(1.5f);

            //Can I offer you an egg in this trying time?
            GameObject egg = CanvasUtil.CreateImagePanel(gameObject, RandomizerMod.sprites["UI.egg.png"], new CanvasUtil.RectData(new Vector2(RandomizerMod.sprites["UI.egg.png"].texture.width / 1.65f, RandomizerMod.sprites["UI.egg.png"].texture.height / 1.65f), Vector2.zero, new Vector2(0.5f, 0.1075f), new Vector2(0.5f, 0.1075f)));
            CanvasGroup eggCG = egg.AddComponent<CanvasGroup>();

            eggCG.blocksRaycasts = false;
            eggCG.interactable = false;
            eggCG.alpha = 0;

            //Should wait for one fade in, don't want to poll input immediately
            yield return CanvasUtil.FadeInCanvasGroup(eggCG);

            //Save the coroutine to stop it later
            Coroutine coroutine = StartCoroutine(BlinkCanvasGroup(eggCG));

            //Wait for the user to cancel the menu
            while (true)
            {
                HeroActions actions = GameManager.instance.inputHandler.inputActions;
                if (actions.jump.WasPressed || actions.attack.WasPressed || actions.menuCancel.WasPressed) break;
                yield return new WaitForEndOfFrame();
            }

            //Fade out the full popup
            yield return FadeOutCanvasGroup(gameObject.GetComponent<CanvasGroup>());

            //Small delay before hero control
            yield return new WaitForSeconds(0.75f);

            //Optionally send FSM event after finishing
            if (fsmObj != null && fsmEvent != null)
            {
                FSMUtility.SendEventToGameObject(fsmObj, fsmEvent);
            }

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

        private IEnumerator BlinkCanvasGroup(CanvasGroup cg)
        {
            while (true)
            {
                yield return FadeOutCanvasGroup(cg);
                yield return CanvasUtil.FadeInCanvasGroup(cg);
            }
        }

        //Below functions ripped from CanvasUtil in order to change the speed
        private IEnumerator FadeInCanvasGroup(CanvasGroup cg)
        {
            float loopFailsafe = 0f;
            cg.alpha = 0f;
            cg.gameObject.SetActive(true);
            while (cg.alpha < 1f)
            {
                cg.alpha += Time.deltaTime * 2f;
                loopFailsafe += Time.deltaTime;
                if (cg.alpha >= 0.95f)
                {
                    cg.alpha = 1f;
                    break;
                }
                if (loopFailsafe >= 2f)
                {
                    break;
                }
                yield return null;
            }
            cg.alpha = 1f;
            cg.interactable = true;
            cg.gameObject.SetActive(true);
            yield return null;
        }

        //Identical to CanvasUtil version except it doesn't randomly set the canvas object inactive at the end
        private IEnumerator FadeOutCanvasGroup(CanvasGroup cg)
        {
            float loopFailsafe = 0f;
            cg.interactable = false;
            while (cg.alpha > 0.05f)
            {
                cg.alpha -= Time.deltaTime * 2f;
                loopFailsafe += Time.deltaTime;
                if (cg.alpha <= 0.05f)
                {
                    break;
                }
                if (loopFailsafe >= 2f)
                {
                    break;
                }
                yield return null;
            }
            cg.alpha = 0f;
            yield return null;
        }
    }
}
