using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using UnityEngine;

namespace RandomizerMod.FsmStateActions
{
    internal class RandomizerSetHardSave : FsmStateAction
    {
        public override void OnEnter()
        {
            GameManager gm = GameManager.instance;
            PlayerData pd = gm?.playerData;

            if (gm != null && pd != null)
            {
                GameObject spawnPoint = GameObject.FindGameObjectWithTag("RespawnPoint");
                if (spawnPoint != null)
                {
                    PlayMakerFSM bench = FSMUtility.LocateFSM(spawnPoint, "Bench Control");
                    if (bench != null)
                    {
                        pd.SetBenchRespawn(spawnPoint.name, gm.GetSceneNameString(), 1, true);
                    }
                    else
                    {
                        RespawnMarker marker = spawnPoint.GetComponent<RespawnMarker>();
                        if (marker != null)
                        {
                            pd.SetBenchRespawn(marker, gm.GetSceneNameString(), 2);
                        }
                    }
                }
            }

            Finish();
        }
    }
}
