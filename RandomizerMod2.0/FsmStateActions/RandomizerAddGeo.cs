using System;
using HutongGames.PlayMaker;
using UnityEngine;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerAddGeo : FsmStateAction
    {
        private const int GEO_VALUE_LARGE = 25;
        private const int GEO_VALUE_MEDIUM = 5;

        private GameObject gameObject;
        private int count;

        public RandomizerAddGeo(GameObject baseObj, int amount)
        {
            count = amount;
            gameObject = baseObj;
        }

        public override void OnEnter()
        {
            // Special case for pickups where you don't have an opportunity to pick up the geo
            string sceneName = GameManager.instance.GetSceneNameString();
            if (sceneName == "Dream_Nailcollection" || sceneName == "Room_Sly_Storeroom")
            {
                HeroController.instance.AddGeo(count);
                Finish();
                return;
            }

            System.Random random = new System.Random();

            int smallNum = random.Next(0, count / 10);
            count -= smallNum;
            int largeNum = random.Next(count / (GEO_VALUE_LARGE * 2), (count / GEO_VALUE_LARGE) + 1);
            count -= largeNum * GEO_VALUE_LARGE;
            int medNum = count / GEO_VALUE_MEDIUM;
            count -= medNum * 5;
            smallNum += count;

            GameObject smallPrefab = ObjectCache.SmallGeo;
            GameObject mediumPrefab = ObjectCache.MediumGeo;
            GameObject largePrefab = ObjectCache.LargeGeo;

            // Workaround because Spawn extension is slightly broken
            smallPrefab.Spawn();
            mediumPrefab.Spawn();
            largePrefab.Spawn();

            smallPrefab.SetActive(true);
            mediumPrefab.SetActive(true);
            largePrefab.SetActive(true);

            FlingUtils.Config flingConfig = new FlingUtils.Config()
            {
                Prefab = smallPrefab,
                AmountMin = smallNum,
                AmountMax = smallNum,
                SpeedMin = 15f,
                SpeedMax = 30f,
                AngleMin = 80f,
                AngleMax = 115f
            };

            // Special case for thorns of agony to stop geo from flying into unreachable spots
            if (sceneName == "Fungus1_14")
            {
                flingConfig.AngleMin = 90;
                flingConfig.AngleMax = 90;
            }

            FlingUtils.SpawnAndFling(flingConfig, gameObject.transform, new Vector3(0f, 0f, 0f));

            flingConfig.Prefab = mediumPrefab;
            flingConfig.AmountMin = flingConfig.AmountMax = medNum;
            FlingUtils.SpawnAndFling(flingConfig, gameObject.transform, new Vector3(0f, 0f, 0f));

            flingConfig.Prefab = largePrefab;
            flingConfig.AmountMin = flingConfig.AmountMax = largeNum;
            FlingUtils.SpawnAndFling(flingConfig, gameObject.transform, new Vector3(0f, 0f, 0f));

            Finish();
        }
    }
}
