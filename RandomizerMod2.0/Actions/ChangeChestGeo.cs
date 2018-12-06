using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using RandomizerMod.FsmStateActions;
using UnityEngine;

namespace RandomizerMod.Actions
{
    [Serializable]
    public class ChangeChestGeo : RandomizerAction
    {
        private const int GEO_VALUE_LARGE = 25;
        private const int GEO_VALUE_MEDIUM = 5;

        [SerializeField] private string sceneName;
        [SerializeField] private string objectName;
        [SerializeField] private string fsmName;
        [SerializeField] private int geoAmount;

        public ChangeChestGeo(string sceneName, string objectName, string fsmName, int geoAmount)
        {
            this.sceneName = sceneName;
            this.objectName = objectName;
            this.fsmName = fsmName;
            this.geoAmount = geoAmount;
        }

        public override void Process()
        {
            if (GameManager.instance.GetSceneNameString() == sceneName)
            {
                foreach (PlayMakerFSM fsm in FsmList)
                {
                    if (fsm.FsmName == fsmName && fsm.gameObject.name == objectName)
                    {
                        // Remove actions that activate shiny item
                        FsmState spawnItems = fsm.GetState("Spawn Items");
                        spawnItems.RemoveActionsOfType<ActivateAllChildren>();
                        fsm.GetState("Activated").RemoveActionsOfType<ActivateAllChildren>();

                        // Add geo to chest
                        // Chest geo pool cannot be trusted, often spawns less than it should
                        spawnItems.AddAction(new RandomizerAddGeo(fsm.gameObject, geoAmount));

                        // Remove pre-existing geo from chest
                        foreach (FlingObjectsFromGlobalPool fling in spawnItems.GetActionsOfType<FlingObjectsFromGlobalPool>())
                        {
                            fling.spawnMin = 0;
                            fling.spawnMax = 0;
                        }

                        // Changes have been made, stop looping
                        break;
                    }
                }
            }
        }
    }
}
