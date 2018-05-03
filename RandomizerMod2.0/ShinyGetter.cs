using System.Collections;
using UnityEngine;
using RandomizerMod.Actions;
using GlobalEnums;

namespace RandomizerMod
{
    internal class ShinyGetter : MonoBehaviour
    {
        public void GetShinyPrefab()
        {
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

            //Load tutorial and tell RandomizerAction to grab shinies
            //For some reason an invalid gate name is least prone to breaking
            GameManager.instance.entryGateName = "";
            UnityEngine.SceneManagement.SceneManager.LoadScene("Tutorial_01");
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            RandomizerAction.FetchFSMList(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            //Head back to wherever we were
            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo()
            {
                SceneName = currentScene,
                EntryGateName = lastGate,
                HeroLeaveDirection = GetGatePosition(lastGate),
                EntryDelay = 0f,
                WaitForSceneTransitionCameraFade = true,
                Visualization = GameManager.SceneLoadVisualizations.Default,
                AlwaysUnloadUnusedAssets = false
            });

            //No need for clutter
            Destroy(gameObject);
        }

        private GatePosition GetGatePosition(string name)
        {
            if (name.Contains("top")) return GatePosition.top;
            if (name.Contains("bot")) return GatePosition.bottom;
            if (name.Contains("left")) return GatePosition.left;
            if (name.Contains("right")) return GatePosition.right;
            if (name.Contains("door")) return GatePosition.door;

            return GatePosition.unknown;
        }
    }
}
