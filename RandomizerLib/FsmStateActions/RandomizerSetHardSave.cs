using HutongGames.PlayMaker;
using JetBrains.Annotations;
using SeanprCore;
using UnityEngine;

namespace RandomizerLib.FsmStateActions
{
    [PublicAPI]
    public class RandomizerSetHardSave : FsmStateAction
    {
        public override void OnEnter()
        {
            GameManager gm = Ref.GM;

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
                LogHelper.LogWarn(
                    "RandomizerSetHardSave action present in scene with no respawn points: " +
                    Ref.GM.GetSceneNameString());
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
                LogHelper.LogWarn(
                    "RandomizerSetHardSave could not identify type of RespawnPoint object in scene " +
                    Ref.GM.GetSceneNameString());
                Finish();
                return;
            }

            gm.SaveGame();
            Finish();
        }
    }
}