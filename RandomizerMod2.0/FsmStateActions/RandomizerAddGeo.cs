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
            System.Random random = new System.Random();

            int smallNum = random.Next(0, count / 10);
            count -= smallNum;
            int largeNum = random.Next(count / (GEO_VALUE_LARGE * 2), (count / GEO_VALUE_LARGE) + 1);
            count -= largeNum * GEO_VALUE_LARGE;
            int medNum = count / GEO_VALUE_MEDIUM;
            count -= medNum * 5;
            smallNum += count;

            FlingUtils.Config flingConfig = new FlingUtils.Config()
            {
                Prefab = ObjectCache.SmallGeo,
                AmountMin = smallNum,
                AmountMax = smallNum,
                SpeedMin = 15f,
                SpeedMax = 30f,
                AngleMin = 80f,
                AngleMax = 115f
            };

            FlingUtils.SpawnAndFling(flingConfig, gameObject.transform, new Vector3(0f, 0f, 0f));

            flingConfig.Prefab = ObjectCache.MediumGeo;
            flingConfig.AmountMin = flingConfig.AmountMax = medNum;
            FlingUtils.SpawnAndFling(flingConfig, gameObject.transform, new Vector3(0f, 0f, 0f));

            flingConfig.Prefab = ObjectCache.LargeGeo;
            flingConfig.AmountMin = flingConfig.AmountMax = largeNum;
            FlingUtils.SpawnAndFling(flingConfig, gameObject.transform, new Vector3(0f, 0f, 0f));

            Finish();
        }
    }
}
