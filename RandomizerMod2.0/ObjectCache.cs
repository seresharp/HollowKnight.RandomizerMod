using System;
using System.Collections.Generic;
using RandomizerMod.Extensions;
using Modding;
using UnityEngine;

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

        public static void GetPrefabs(Dictionary<string, GameObject> objects)
        {
            shinyItem = objects["_Props/Chest/Item/Shiny Item (1)"];
            shinyItem.name = "Randomizer Shiny";

            HealthManager health = objects["_Enemies/Crawler 1"].GetComponent<HealthManager>();
            smallGeo = Object.Instantiate(ReflectionHelper.GetAttr<HealthManager, GameObject>(health, "smallGeoPrefab"));
            mediumGeo = Object.Instantiate(ReflectionHelper.GetAttr<HealthManager, GameObject>(health, "mediumGeoPrefab"));
            largeGeo = Object.Instantiate(ReflectionHelper.GetAttr<HealthManager, GameObject>(health, "largeGeoPrefab"));

            smallGeo.SetActive(false);
            mediumGeo.SetActive(false);
            largeGeo.SetActive(false);
            Object.DontDestroyOnLoad(smallGeo);
            Object.DontDestroyOnLoad(mediumGeo);
            Object.DontDestroyOnLoad(largeGeo);

            tinkEffect = Object.Instantiate(objects["_Props/Cave Spikes (1)"].GetComponent<TinkEffect>().blockEffect);
            tinkEffect.SetActive(false);
            Object.DontDestroyOnLoad(tinkEffect);

            Object.Destroy(objects["_Props/Cave Spikes (1)"]);
            Object.Destroy(objects["_Enemies/Crawler 1"]);

            if (shinyItem == null || smallGeo == null || mediumGeo == null || largeGeo == null || tinkEffect == null)
            {
                RandomizerMod.Instance.LogWarn("One or more ObjectCache items are null");
            }
        }
    }
}
