using System.Collections;
using System.Linq;
using RandomizerMod.Actions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandomizerMod.Components
{
    internal class ShinyPreloader : MonoBehaviour
    {
        public static void Preload()
        {
            GameObject obj = new GameObject();
            DontDestroyOnLoad(obj);
            obj.AddComponent<ShinyPreloader>();
        }

        public void Start()
        {
            StartCoroutine(PreloadCoroutine());
        }

        private IEnumerator PreloadCoroutine()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Tutorial_01", LoadSceneMode.Additive);

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Scene kp = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Tutorial_01");
            bool found = false;
            foreach (GameObject obj in kp.GetRootGameObjects())
            {
                foreach (PlayMakerFSM fsm in obj.GetComponentsInChildren<PlayMakerFSM>(true))
                {
                    if (fsm.FsmName == "Shiny Control")
                    {
                        RandomizerAction.SetShinyPrefab(fsm.gameObject);
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    break;
                }
            }

            kp.GetRootGameObjects().ToList().ForEach(kill => Destroy(kill));
            Destroy(gameObject);
        }
    }
}
