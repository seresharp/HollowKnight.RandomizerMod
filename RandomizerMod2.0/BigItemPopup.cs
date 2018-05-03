using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Modding;

namespace RandomizerMod
{
    internal class BigItemPopup
    {
        public static void Show()
        {
            GameObject canvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            //CanvasUtil.CreateImagePanel(canvas, RandomizerMod.sprites["Prompts.Dash.png"], );
            canvas.GetComponent<CanvasScaler>().StartCoroutine(Test());
        }

        private static IEnumerator Test()
        {
            while (true)
            {
                yield return null;
                Modding.Logger.Log("Working");
            }
        }
    }
}
