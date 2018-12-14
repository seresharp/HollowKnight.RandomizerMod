using System;
using HutongGames.PlayMaker;
using UnityEngine;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerSetHardSave : FsmStateAction
    {
        public override void OnEnter()
        {
            GameManager gm = GameManager.instance;

            if (gm == null)
            {
                Finish();
                return;
            }

            PlayerData pd = gm.playerData;

            if (pd == null)
            {
                Finish();
                return;
            }

            GameObject spawnPoint = GameObject.FindGameObjectWithTag("RespawnPoint");

            if (spawnPoint == null)
            {
                RandomizerMod.Instance.LogWarn("RandomizerSetHardSave action present in scene with no respawn points: " + GameManager.instance.GetSceneNameString());
                Finish();
                return;
            }

            PlayMakerFSM bench = FSMUtility.LocateFSM(spawnPoint, "Bench Control");
            RespawnMarker marker = spawnPoint.GetComponent<RespawnMarker>();
            if (bench != null)
            {
                pd.SetBenchRespawn(spawnPoint.name, gm.GetSceneNameString(), 1, true);
            }
            else if (marker != null)
            {
                pd.SetBenchRespawn(marker, gm.GetSceneNameString(), 2);
            }
            else
            {
                RandomizerMod.Instance.LogWarn("RandomizerSetHardSave could not identify type of RespawnPoint object in scene " + GameManager.instance.GetSceneNameString());
                Finish();
                return;
            }

            gm.SaveGame();
            Finish();
        }
    }
}
