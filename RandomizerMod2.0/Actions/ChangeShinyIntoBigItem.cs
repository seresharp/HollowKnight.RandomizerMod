using System;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using RandomizerMod.FsmStateActions;

namespace RandomizerMod.Actions
{
    [Serializable]
    public class ChangeShinyIntoBigItem : RandomizerAction
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string objectName;
        [SerializeField] private string fsmName;
        [SerializeField] private string boolName;
        [SerializeField] private string spriteName;
        [SerializeField] private string itemStateName;

        public ChangeShinyIntoBigItem(string sceneName, string objectName, string fsmName, string boolName)
        {
            this.sceneName = sceneName;
            this.objectName = objectName;
            this.fsmName = fsmName;
            this.boolName = boolName;

            switch (boolName)
            {
                case "hasDash":
                    spriteName = "Prompts.Dash.png";
                    itemStateName = "Dash";
                    break;
                case "hasWalljump":
                    spriteName = "Prompts.Walljump.png";
                    itemStateName = "Walljump";
                    break;
                case "hasSuperDash":
                    spriteName = "Prompts.Superdash.png";
                    itemStateName = "Super Dash";
                    break;
                case "hasAcidArmour":
                    spriteName = "Prompts.Isma.png";
                    itemStateName = "Pure Seed";
                    break;
                case "hasKingsBrand":
                    spriteName = "Prompts.Kingsbrand.png";
                    itemStateName = "King's Brand";
                    break;
                default:
                    throw new ArgumentException(boolName + " is not a big item, or is not implemented yet.");
            }
        }

        public override void Process()
        {
            if (GameManager.instance.GetSceneNameString() == sceneName)
            {
                foreach (PlayMakerFSM fsm in fsmList)
                {
                    if (fsm.FsmName == fsmName && fsm.gameObject.name == objectName)
                    {
                        FsmState pdBool = fsm.GetState("PD Bool?");
                        FsmState charm = fsm.GetState("Charm?");
                        FsmState bigGetFlash = fsm.GetState("Big Get Flash");

                        //Remove actions that stop shiny from spawning
                        pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
                        pdBool.RemoveActionsOfType<StringCompare>();

                        //Add action to potentially despawn the object
                        pdBool.AddAction(new RandomizerBoolTest(boolName, null, "COLLECTED", true));

                        //Force the FSM into the correct item state
                        charm.ClearTransitions();
                        charm.AddTransition("FINISHED", "Big Get Flash");
                        bigGetFlash.ClearTransitions();
                        bigGetFlash.AddTransition("FINISHED", itemStateName);

                        //Apply the correct sprite to the popup
                        //TODO: Create my own popup functionality, this sucks
                        CreateUIMsgGetItem createMsg = fsm.GetState(itemStateName).GetActionsOfType<CreateUIMsgGetItem>()[0];
                        PlayMakerFSM msgControl = FSMUtility.LocateFSM(createMsg.gameObject.Value, "Msg Control");
                        FsmState topUp = msgControl.GetState("Top Up");
                        SendEventByName sendEvent = topUp.GetActionsOfType<SendEventByName>()[1];
                        SpriteRenderer renderer = sendEvent.eventTarget.gameObject.GameObject.Value.GetComponent<SpriteRenderer>();
                        renderer.sprite = RandomizerMod.sprites[spriteName];

                        //Changes have been made, stop looping
                        break;
                    }
                }
            }
        }
    }
}
