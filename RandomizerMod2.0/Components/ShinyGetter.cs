using System.Collections;
using UnityEngine;
using RandomizerMod.Actions;
using GlobalEnums;

namespace RandomizerMod.Components
{
    internal class ShinyGetter : MonoBehaviour
    {
        public void GetShinyPrefab()
        {
            RandomizerMod.instance.Log("Getting shiny prefab from tutorial");
            StartCoroutine(LoadShiny());
        }

        private IEnumerator LoadShiny()
        {
            string currentScene = GameManager.instance.GetSceneNameString();
            string lastGate = GameManager.instance.GetEntryGateName();

            //Non-ideal failsafe
            if (string.IsNullOrEmpty(lastGate))
            {
                foreach (WorldNavigation.SceneItem item in WorldNavigation.Scenes)
                {
                    if (item.Name == currentScene)
                    {
                        lastGate = item.Transitions[0].Name;
                    }
                }
            }

            //Load the tutorial
            GameManager.instance.ChangeToScene("Tutorial_01", "right1", 0);

            //Two frames is enough to guarantee the shiny has loaded
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //Grab the shiny
            RandomizerAction.FetchFSMList(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            //Head back to wherever we were
            GameManager.instance.ChangeToScene(currentScene, lastGate, 0);

            //No need for clutter
            Destroy(gameObject);
        }
    }
}
