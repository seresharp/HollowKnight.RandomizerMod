using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandomizerMod.Components
{
    internal class Preloader : MonoBehaviour
    {
        public static void Preload()
        {
            GameObject obj = new GameObject();
            DontDestroyOnLoad(obj);
            obj.AddComponent<Preloader>();
        }

        public void Start()
        {
            StartCoroutine(PreloadCoroutine());
        }

        private IEnumerator PreloadCoroutine()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.Tutorial_01, LoadSceneMode.Additive);

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            ObjectCache.GetPrefabs();

            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetSceneByName(SceneNames.Tutorial_01));
            Destroy(gameObject);
        }
    }
}
