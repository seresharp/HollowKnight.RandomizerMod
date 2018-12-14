using System;
using System.Collections;
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
        public const int TYPE_GEO = 0;
        public const int TYPE_ESSENCE = 1;

        [SerializeField] private string sceneName;
        [SerializeField] private string objectName;
        [SerializeField] private string fsmName;
        [SerializeField] private string itemName;
        [SerializeField] private int cost;
        [SerializeField] private int type;

        public AddYNDialogueToShiny(string sceneName, string objectName, string fsmName, string itemName, int cost, int type = TYPE_GEO)
        {
            if (cost < 0)
            {
                RandomizerMod.Instance.LogWarn("AddYNDialogueToShiny created with negative cost, setting to 0 instead");
                cost = 0;
            }

            this.sceneName = sceneName;
            this.objectName = objectName;
            this.fsmName = fsmName;
            this.itemName = itemName;
            this.cost = cost;
            this.type = type;
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

            charm.AddTransition("HERO DAMAGED", noState.Name);
            charm.AddTransition("NO", noState.Name);
            charm.AddTransition("YES", yesState);

            fsm.GetState(yesState).AddAction(new RandomizerCallStaticMethod(GetType(), nameof(CloseYNDialogue)));

            charm.AddFirstAction(new RandomizerCallStaticMethod(GetType(), nameof(OpenYNDialogue), fsm.gameObject, itemName, cost, type));
        }

        private static void OpenYNDialogue(GameObject shiny, string itemName, int cost, int type)
        {
            FSMUtility.LocateFSM(GameObject.Find("DialogueManager"), "Box Open YN").SendEvent("BOX UP YN");
            FSMUtility.LocateFSM(GameObject.Find("Text YN"), "Dialogue Page Control").FsmVariables.GetFsmGameObject("Requester").Value = shiny;

            if (type == TYPE_ESSENCE)
            {
                LanguageStringManager.SetString("UI", "RANDOMIZER_YN_DIALOGUE", cost + " Essence: " + LanguageStringManager.GetLanguageString(itemName, "UI"));

                if (PlayerData.instance.dreamOrbs < cost)
                {
                    FSMUtility.LocateFSM(GameObject.Find("Text YN"), "Dialogue Page Control").StartCoroutine(KillGeoText());
                }

                cost = 0;
            }
            else
            {
                LanguageStringManager.SetString("UI", "RANDOMIZER_YN_DIALOGUE", LanguageStringManager.GetLanguageString(itemName, "UI"));
            }

            FSMUtility.LocateFSM(GameObject.Find("Text YN"), "Dialogue Page Control").FsmVariables.GetFsmInt("Toll Cost").Value = cost;

            GameObject.Find("Text YN").GetComponent<DialogueBox>().StartConversation("RANDOMIZER_YN_DIALOGUE", "UI");
        }

        private static void CloseYNDialogue()
        {
            FSMUtility.LocateFSM(GameObject.Find("DialogueManager"), "Box Open YN").SendEvent("BOX DOWN YN");
        }

        private static IEnumerator KillGeoText()
        {
            PlayMakerFSM ynFsm = FSMUtility.LocateFSM(GameObject.Find("Text YN"), "Dialogue Page Control");
            while (ynFsm.ActiveStateName != "Ready for Input")
            {
                yield return new WaitForEndOfFrame();
            }

            ynFsm.FsmVariables.GetFsmGameObject("Geo Text").Value.SetActive(false);
            ynFsm.FsmVariables.GetFsmInt("Toll Cost").Value = int.MaxValue;
            PlayMakerFSM.BroadcastEvent("NOT ENOUGH");
        }
    }
}
