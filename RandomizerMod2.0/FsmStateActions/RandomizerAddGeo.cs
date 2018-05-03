using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HutongGames.PlayMaker;

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
            if (RandomizerMod.smallGeoPrefab != null && RandomizerMod.mediumGeoPrefab != null && RandomizerMod.largeGeoPrefab != null)
            {
                System.Random random = new System.Random();

                int smallNum = random.Next(0, count / 10);
                count -= smallNum;
                int largeNum = random.Next(count / (GEO_VALUE_LARGE * 2), count / GEO_VALUE_LARGE + 1);
                count -= largeNum * GEO_VALUE_LARGE;
                int medNum = count / GEO_VALUE_MEDIUM;
                count -= medNum * 5;
                smallNum += count;

                FlingUtils.SpawnAndFling(new FlingUtils.Config
                {
                    Prefab = RandomizerMod.smallGeoPrefab,
                    AmountMin = smallNum,
                    AmountMax = smallNum,
                    SpeedMin = 15f,
                    SpeedMax = 30f,
                    AngleMin = 80f,
                    AngleMax = 115f
                }, gameObject.transform, new Vector3(0f, 0f, 0f));
                FlingUtils.SpawnAndFling(new FlingUtils.Config
                {
                    Prefab = RandomizerMod.mediumGeoPrefab,
                    AmountMin = medNum,
                    AmountMax = medNum,
                    SpeedMin = 15f,
                    SpeedMax = 30f,
                    AngleMin = 80f,
                    AngleMax = 115f
                }, gameObject.transform, new Vector3(0f, 0f, 0f));
                FlingUtils.SpawnAndFling(new FlingUtils.Config
                {
                    Prefab = RandomizerMod.largeGeoPrefab,
                    AmountMin = largeNum,
                    AmountMax = largeNum,
                    SpeedMin = 15f,
                    SpeedMax = 30f,
                    AngleMin = 80f,
                    AngleMax = 115f
                }, gameObject.transform, new Vector3(0f, 0f, 0f));
            }
            else
            {
                HeroController.instance.AddGeo(count);
            }

            Finish();
        }
    }
}
