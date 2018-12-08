using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using UnityEngine;

using Object = UnityEngine.Object;

namespace RandomizerMod.Actions
{
    [Serializable]
    public class AddShinyToChest : RandomizerAction
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string objectName;
        [SerializeField] private string fsmName;
        [SerializeField] private string newShinyName;

        public AddShinyToChest(string sceneName, string objectName, string fsmName, string newShinyName)
        {
            this.sceneName = sceneName;
            this.objectName = objectName;
            this.fsmName = fsmName;
            this.newShinyName = newShinyName;
        }

        public override ActionType Type => ActionType.PlayMakerFSM;

        public override void Process(string scene, Object changeObj)
        {
            if (scene != sceneName || !(changeObj is PlayMakerFSM fsm) || fsm.FsmName != fsmName || fsm.gameObject.name != objectName)
            {
                return;
            }

            FsmState spawnItems = fsm.GetState("Spawn Items");

            // Remove geo from chest
            foreach (FlingObjectsFromGlobalPool fling in spawnItems.GetActionsOfType<FlingObjectsFromGlobalPool>())
            {
                fling.spawnMin = 0;
                fling.spawnMax = 0;
            }

            // Instantiate a new shiny and set the chest as its parent
            GameObject item = fsm.gameObject.transform.Find("Item").gameObject;
            GameObject shiny = ObjectCache.ShinyItem;
            shiny.SetActive(false);
            shiny.transform.SetParent(item.transform);
            shiny.transform.position = item.transform.position;
            shiny.name = newShinyName;

            // Force the new shiny to fling out of the chest
            PlayMakerFSM shinyControl = FSMUtility.LocateFSM(shiny, "Shiny Control");
            FsmState shinyFling = shinyControl.GetState("Fling?");
            shinyFling.ClearTransitions();
            shinyFling.AddTransition("FINISHED", "Fling R");
        }
    }
}
