using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using RandomizerMod.FsmStateActions;
using UnityEngine;

using Object = UnityEngine.Object;

namespace RandomizerMod.Actions
{
    internal class AddYNDialogueToShiny : RandomizerAction
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string objectName;
        [SerializeField] private string fsmName;
        [SerializeField] private string itemName;
        [SerializeField] private int cost;

        public AddYNDialogueToShiny(string sceneName, string objectName, string fsmName, string itemName, int cost)
        {
            if (cost <= 0)
            {
                RandomizerMod.Instance.LogWarn("AddYNDialogueToShiny created with 0 or negative cost, setting to 1 instead");
                cost = 1;
            }

            this.sceneName = sceneName;
            this.objectName = objectName;
            this.fsmName = fsmName;
            this.itemName = itemName;
            this.cost = cost;
        }

        public override ActionType Type => ActionType.PlayMakerFSM;

        public override void Process(string scene, Object changeObj)
        {
            if (scene != sceneName || !(changeObj is PlayMakerFSM fsm) || fsm.FsmName != fsmName || fsm.gameObject.name != objectName)
            {
                return;
            }

            FsmState noState = new FsmState(fsm.GetState("Idle"));
            noState.Name = "YN No";
            noState.ClearTransitions();
            noState.RemoveActionsOfType<FsmStateAction>();

            noState.AddTransition("FINISHED", "Give Control");

            Tk2dPlayAnimationWithEvents heroUp = new Tk2dPlayAnimationWithEvents()
            {
                gameObject = new FsmOwnerDefault()
                {
                    OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                    GameObject = HeroController.instance.gameObject
                },
                clipName = "Collect Normal 3",
                animationTriggerEvent = null,
                animationCompleteEvent = FsmEvent.GetFsmEvent("FINISHED")
            };

            noState.AddAction(new RandomizerCallStaticMethod(GetType(), nameof(CloseYNDialogue)));
            noState.AddAction(heroUp);

            FsmState giveControl = new FsmState(fsm.GetState("Idle"));
            giveControl.Name = "Give Control";
            giveControl.ClearTransitions();
            giveControl.RemoveActionsOfType<FsmStateAction>();

            giveControl.AddTransition("FINISHED", "Idle");

            giveControl.AddAction(new RandomizerExecuteLambda(() => PlayMakerFSM.BroadcastEvent("END INSPECT")));

            fsm.AddState(noState);
            fsm.AddState(giveControl);

            FsmState charm = fsm.GetState("Charm?");
            string yesState = charm.Transitions[0].ToState;
            charm.ClearTransitions();

            charm.AddTransition("NO", noState.Name);
            charm.AddTransition("YES", yesState);

            fsm.GetState(yesState).AddAction(new RandomizerCallStaticMethod(GetType(), nameof(CloseYNDialogue)));

            charm.AddFirstAction(new RandomizerCallStaticMethod(GetType(), nameof(OpenYNDialogue), fsm.gameObject, itemName, cost));
        }

        private static void OpenYNDialogue(GameObject shiny, string itemName, int cost)
        {
            FSMUtility.LocateFSM(GameObject.Find("DialogueManager"), "Box Open YN").SendEvent("BOX UP YN");
            FSMUtility.LocateFSM(GameObject.Find("Text YN"), "Dialogue Page Control").FsmVariables.GetFsmInt("Toll Cost").Value = cost;
            FSMUtility.LocateFSM(GameObject.Find("Text YN"), "Dialogue Page Control").FsmVariables.GetFsmGameObject("Requester").Value = shiny;

            GameObject.Find("Text YN").GetComponent<DialogueBox>().StartConversation(itemName, "UI");
        }

        private static void CloseYNDialogue()
        {
            FSMUtility.LocateFSM(GameObject.Find("DialogueManager"), "Box Open YN").SendEvent("BOX DOWN YN");
        }
    }
}
