using System;
using System.Reflection;
using RandomizerMod.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace RandomizerMod
{
    internal static class ObjectCache
    {
        private static GameObject shinyItem;

        private static GameObject smallGeo;
        private static GameObject mediumGeo;
        private static GameObject largeGeo;

        private static GameObject tinkEffect;

        public static GameObject ShinyItem => Object.Instantiate(shinyItem);

        public static GameObject SmallGeo => Object.Instantiate(smallGeo);

        public static GameObject MediumGeo => Object.Instantiate(mediumGeo);

        public static GameObject LargeGeo => Object.Instantiate(largeGeo);

        public static GameObject TinkEffect => Object.Instantiate(tinkEffect);

        public static void GetPrefabs()
        {
            FieldInfo smallGeoPrefabField = typeof(HealthManager).GetField("smallGeoPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo mediumGeoPrefabField = typeof(HealthManager).GetField("mediumGeoPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo largeGeoPrefabField = typeof(HealthManager).GetField("largeGeoPrefab", BindingFlags.NonPublic | BindingFlags.Instance);

            Scene kp = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Tutorial_01");

            GameObject shiny = kp.FindGameObject("Shiny Item (1)");

            HealthManager health = kp.FindGameObject("Crawler 1").GetComponent<HealthManager>();
            GameObject smallBuggyBean = (GameObject)smallGeoPrefabField.GetValue(health);
            GameObject mediumBuggyBean = (GameObject)mediumGeoPrefabField.GetValue(health);
            GameObject largeBuggyBean = (GameObject)largeGeoPrefabField.GetValue(health);

            GameObject tink = kp.FindGameObject("Cave Spikes (1)").GetComponent<TinkEffect>().blockEffect;

            shinyItem = Object.Instantiate(shiny);
            shinyItem.SetActive(false);
            shinyItem.name = "Randomizer Shiny";
            Object.DontDestroyOnLoad(shinyItem);

            smallGeo = Object.Instantiate(smallBuggyBean);
            mediumGeo = Object.Instantiate(mediumBuggyBean);
            largeGeo = Object.Instantiate(largeBuggyBean);
            smallGeo.SetActive(false);
            mediumGeo.SetActive(false);
            largeGeo.SetActive(false);
            Object.DontDestroyOnLoad(smallGeo);
            Object.DontDestroyOnLoad(mediumGeo);
            Object.DontDestroyOnLoad(largeGeo);

            tinkEffect = Object.Instantiate(tink);
            tinkEffect.SetActive(false);
            Object.DontDestroyOnLoad(tinkEffect);

            if (shinyItem == null || smallGeo == null || mediumGeo == null || largeGeo == null || tinkEffect == null)
            {
                RandomizerMod.Instance.LogWarn("One or more ObjectCache items are null");
            }
        }
    }
}
