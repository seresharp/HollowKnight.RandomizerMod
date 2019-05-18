using HutongGames.PlayMaker;
using SeanprCore;
using UnityEngine;
using Random = System.Random;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerAddGeo : FsmStateAction
    {
        private const int GEO_VALUE_LARGE = 25;
        private const int GEO_VALUE_MEDIUM = 5;

        private readonly GameObject gameObject;
        private readonly bool minimize;
        private int count;

        public RandomizerAddGeo(GameObject baseObj, int amount, bool minimizeObjects = false)
        {
            count = amount;
            gameObject = baseObj;
            minimize = minimizeObjects;
        }

        public void SetGeo(int geo)
        {
            count = geo;
        }

        public override void OnEnter()
        {
            // Special case for pickups where you don't have an opportunity to pick up the geo
            string sceneName = Ref.GM.GetSceneNameString();
            if (sceneName == SceneNames.Dream_Nailcollection || sceneName == SceneNames.Room_Sly_Storeroom)
            {
                Ref.Hero.AddGeo(count);
                Finish();
                return;
            }

            int smallNum = 0;
            int medNum = 0;
            int largeNum = 0;

            if (!minimize)
            {
                Random random = new Random();

                smallNum = random.Next(0, count / 10);
                count -= smallNum;
                largeNum = random.Next(count / (GEO_VALUE_LARGE * 2), count / GEO_VALUE_LARGE + 1);
                count -= largeNum * GEO_VALUE_LARGE;
                medNum = count / GEO_VALUE_MEDIUM;
                count -= medNum * 5;
                smallNum += count;
            }
            else
            {
                largeNum = count / GEO_VALUE_LARGE;
                count -= largeNum * GEO_VALUE_LARGE;
                medNum = count / GEO_VALUE_MEDIUM;
                count -= medNum * GEO_VALUE_MEDIUM;
                smallNum = count;
            }

            GameObject smallPrefab = ObjectCache.SmallGeo;
            GameObject mediumPrefab = ObjectCache.MediumGeo;
            GameObject largePrefab = ObjectCache.LargeGeo;

            // Workaround because Spawn extension is slightly broken
            Object.Destroy(smallPrefab.Spawn());
            Object.Destroy(mediumPrefab.Spawn());
            Object.Destroy(largePrefab.Spawn());

            smallPrefab.SetActive(true);
            mediumPrefab.SetActive(true);
            largePrefab.SetActive(true);

            FlingUtils.Config flingConfig = new FlingUtils.Config
            {
                Prefab = smallPrefab,
                AmountMin = smallNum,
                AmountMax = smallNum,
                SpeedMin = 15f,
                SpeedMax = 30f,
                AngleMin = 80f,
                AngleMax = 115f
            };

            // Special case for thorns of agony, spore shroom, flukenest to stop geo from flying into unreachable spots
            if (sceneName == SceneNames.Fungus1_14 || sceneName == SceneNames.Fungus2_20 ||
                sceneName == SceneNames.Waterways_12)
            {
                flingConfig.AngleMin = 90;
                flingConfig.AngleMax = 90;
            }

            if (smallNum > 0)
            {
                FlingUtils.SpawnAndFling(flingConfig, gameObject.transform, new Vector3(0f, 0f, 0f));
            }

            if (medNum > 0)
            {
                flingConfig.Prefab = mediumPrefab;
                flingConfig.AmountMin = flingConfig.AmountMax = medNum;
                FlingUtils.SpawnAndFling(flingConfig, gameObject.transform, new Vector3(0f, 0f, 0f));
            }

            if (largeNum > 0)
            {
                flingConfig.Prefab = largePrefab;
                flingConfig.AmountMin = flingConfig.AmountMax = largeNum;
                FlingUtils.SpawnAndFling(flingConfig, gameObject.transform, new Vector3(0f, 0f, 0f));
            }

            smallPrefab.SetActive(false);
            mediumPrefab.SetActive(false);
            largePrefab.SetActive(false);

            Finish();
        }
    }
}