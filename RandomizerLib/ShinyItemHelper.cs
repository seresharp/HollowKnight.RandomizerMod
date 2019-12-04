using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using Modding;
using RandomizerLib.Components;
using RandomizerLib.FsmStateActions;
using SeanprCore;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RandomizerLib
{
    [PublicAPI]
    public static class ShinyItemHelper
    {
        public static GameObject ReplaceObjectWithShiny(GameObject obj, string shinyName)
        {
            GameObject shiny = ObjectCache.ShinyItem;
            shiny.name = shinyName;

            if (shiny.transform.parent != null)
            {
                shiny.transform.SetParent(obj.transform.parent);
            }

            shiny.transform.position = obj.transform.position;
            shiny.transform.localPosition = obj.transform.localPosition;
            shiny.SetActive(obj.activeSelf);

            // Force the new shiny to fall straight downwards
            RemoveFling(shiny);

            // Destroy the original object
            Object.DestroyImmediate(obj);

            return shiny;
        }

        public static GameObject CreateNewShiny(float x, float y, string shinyName)
        {
            // Put a shiny in the same location as the original
            GameObject shiny = ObjectCache.ShinyItem;
            shiny.name = shinyName;

            shiny.transform.position = new Vector3(x, y, shiny.transform.position.z);
            shiny.SetActive(true);

            // Force the new shiny to fall straight downwards
            RemoveFling(shiny);

            return shiny;
        }

        // TODO: Bool for trinket pickups
        public static void ChangeIntoTrinket(GameObject obj, int trinkNum)
        {
            PlayMakerFSM fsm = obj.LocateFSM("Shiny Control");

            FsmState pdBool = fsm.GetState("PD Bool?");
            FsmState charm = fsm.GetState("Charm?");
            FsmState trinkFlash = fsm.GetState("Trink Flash");

            // Remove actions that stop shiny from spawning
            pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
            pdBool.RemoveActionsOfType<StringCompare>();

            // Force the FSM to follow the path for the correct trinket
            charm.ClearTransitions();
            charm.AddTransition("FINISHED", "Trink Flash");
            trinkFlash.ClearTransitions();
            trinkFlash.AddTransition("FINISHED", $"Trink {trinkNum}");
        }

        public static void ChangeIntoGeo(GameObject obj, Mod mod, string boolName, int geo)
        {
            PlayMakerFSM fsm = obj.LocateFSM("Shiny Control");

            FsmState pdBool = fsm.GetState("PD Bool?");
            FsmState charm = fsm.GetState("Charm?");

            // Remove actions that stop shiny from spawning
            pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
            pdBool.RemoveActionsOfType<StringCompare>();

            // Add our own check to stop the shiny from being grabbed twice
            pdBool.AddAction(new RandomizerBoolTest(mod, boolName, null, "COLLECTED"));

            // The "Charm?" state is a good entry point for our geo spawning
            charm.AddAction(new RandomizerSetBool(mod, boolName, true, true));
            charm.AddAction(new RandomizerAddGeo(fsm.gameObject, geo));

            // Skip all the other type checks
            charm.ClearTransitions();
            charm.AddTransition("FINISHED", "Flash");
        }

        public static void ChangeIntoCharm(GameObject obj, Mod mod, string boolName, string charmBool)
        {
            ChangeIntoCharm(obj, mod, boolName, int.Parse(charmBool.Substring(9)));
        }

        public static void ChangeIntoCharm(GameObject obj, Mod mod, string boolName, int charmNum)
        {
            PlayMakerFSM fsm = obj.LocateFSM("Shiny Control");

            FsmState pdBool = fsm.GetState("PD Bool?");
            FsmState charm = fsm.GetState("Charm?");
            FsmState getCharm = fsm.GetState("Get Charm");

            // Remove actions that stop shiny from spawning
            pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
            pdBool.RemoveActionsOfType<StringCompare>();

            // Add action to potentially despawn the object
            pdBool.AddAction(new RandomizerBoolTest(mod, boolName, null, "COLLECTED", true));

            // Force the FSM into the charm state, set it to the correct charm
            charm.ClearTransitions();
            charm.AddTransition("FINISHED", "Get Charm");
            getCharm.RemoveActionsOfType<SetPlayerDataBool>();
            getCharm.AddAction(new RandomizerSetBool(mod, boolName, true, true));
            fsm.GetState("Normal Msg").GetActionsOfType<SetFsmInt>()[0].setValue = charmNum;
        }

        public static void ChangeIntoBigItem(GameObject obj, Mod mod, string boolName, BigItemDef[] itemDefs)
        {
            PlayMakerFSM fsm = obj.LocateFSM("Shiny Control");

            FsmState pdBool = fsm.GetState("PD Bool?");
            FsmState charm = fsm.GetState("Charm?");
            FsmState bigGetFlash = fsm.GetState("Big Get Flash");

            // Remove actions that stop shiny from spawning
            pdBool.RemoveActionsOfType<StringCompare>();

            // Change pd bool test to our new bool
            PlayerDataBoolTest boolTest = pdBool.GetActionsOfType<PlayerDataBoolTest>()[0];
            RandomizerBoolTest randBoolTest = new RandomizerBoolTest(mod, boolName, boolTest.isFalse, boolTest.isTrue);
            pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
            pdBool.AddFirstAction(randBoolTest);

                // Force the FSM to show the big item flash
            charm.ClearTransitions();
            charm.AddTransition("FINISHED", "Big Get Flash");

            // Set bool and show the popup after the flash
            bigGetFlash.AddAction(new RandomizerCallStaticMethod(
                typeof(BigItemPopup),
                nameof(BigItemPopup.ShowAdditive),
                itemDefs,
                fsm.gameObject,
                "GET ITEM MSG END"));

            // Don't actually need to set the skill here, that happens in BigItemPopup
            // Maybe change that at some point, it's not where it should happen
            bigGetFlash.AddAction(new RandomizerSetBool(mod, boolName, true, true));

            // Exit the fsm after the popup
            bigGetFlash.ClearTransitions();
            bigGetFlash.AddTransition("GET ITEM MSG END", "Hero Up");
            bigGetFlash.AddTransition("HERO DAMAGED", "Finish");
        }

        public static void ChangeIntoSimple(GameObject obj, Mod mod, string boolName)
        {
            PlayMakerFSM fsm = obj.LocateFSM("Shiny Control");

            FsmState pdBool = fsm.GetState("PD Bool?");
            FsmState charm = fsm.GetState("Charm?");
            FsmState bigGetFlash = fsm.GetState("Big Get Flash");

            // Remove actions that stop shiny from spawning
            pdBool.RemoveActionsOfType<StringCompare>();

            // Change pd bool test to our new bool
            PlayerDataBoolTest boolTest = pdBool.GetActionsOfType<PlayerDataBoolTest>()[0];
            RandomizerBoolTest randBoolTest = new RandomizerBoolTest(mod, boolName, boolTest.isFalse, boolTest.isTrue);
            pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
            pdBool.AddFirstAction(randBoolTest);

            // Force the FSM to show the big item flash
            charm.ClearTransitions();
            charm.AddTransition("FINISHED", "Big Get Flash");

            // Set the given bool
            bigGetFlash.AddAction(new RandomizerSetBool(mod, boolName, true, true));

            // Exit the fsm after giving the item
            bigGetFlash.ClearTransitions();
            bigGetFlash.AddTransition("FINISHED", "Hero Up");
            bigGetFlash.AddTransition("HERO DAMAGED", "Finish");
        }

        public static void AddYNDialogueToShiny(GameObject obj, string itemName, int cost)
        {
            PlayMakerFSM fsm = obj.LocateFSM("Shiny Control");

            // Build new state for if the user selects no
            FsmState noState = new FsmState(fsm.GetState("Idle"))
            {
                Name = "YN No"
            };

            noState.ClearTransitions();
            noState.RemoveActionsOfType<FsmStateAction>();

            // "Give Control" state defined below
            noState.AddTransition("FINISHED", "Give Control");

            // Create action for animating the hero getting up from the kneeling position
            // Animation of going into the kneel animation is handled by existing states
            Tk2dPlayAnimationWithEvents heroUp = new Tk2dPlayAnimationWithEvents
            {
                gameObject = new FsmOwnerDefault
                {
                    OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                    GameObject = Ref.Hero.gameObject
                },
                clipName = "Collect Normal 3",
                animationTriggerEvent = null,
                animationCompleteEvent = FsmEvent.GetFsmEvent("FINISHED")
            };

            // Close the dialogue and animate hero up when no is selected
            noState.AddAction(new RandomizerCallStaticMethod(typeof(ShinyItemHelper), nameof(CloseYNDialogue)));
            noState.AddAction(heroUp);

            // New state to give hero control back after
            FsmState giveControl = new FsmState(fsm.GetState("Idle"))
            {
                Name = "Give Control"
            };

            giveControl.ClearTransitions();
            giveControl.RemoveActionsOfType<FsmStateAction>();

            // Return back to idle after giving control
            giveControl.AddTransition("FINISHED", "Idle");

            // Broadcast "END INSPECT" to allow other FSMs to handle giving back control
            giveControl.AddAction(new RandomizerExecuteLambda(() => PlayMakerFSM.BroadcastEvent("END INSPECT")));

            // Add the new states into the fsm
            fsm.AddState(noState);
            fsm.AddState(giveControl);

            // "Charm?" state is the standard entry point for these helper methods.
            // Assuming this method is ran after another of the helpers, the first transition
            // in this state will lead to the whichever item needs to be given, rather than
            // the default "Get Charm" state
            FsmState charm = fsm.GetState("Charm?");
            string yesState = charm.Transitions[0].ToState;
            charm.ClearTransitions();

            // Add transitions for y/n
            charm.AddTransition("HERO DAMAGED", noState.Name);
            charm.AddTransition("NO", noState.Name);
            charm.AddTransition("YES", yesState);

            // Close the dialogue after yes is pressed
            fsm.GetState(yesState).AddAction(new RandomizerCallStaticMethod(typeof(ShinyItemHelper), nameof(CloseYNDialogue)));

            // Open the dialogue when the entry state is reached
            charm.AddFirstAction(new RandomizerCallStaticMethod(typeof(ShinyItemHelper), nameof(OpenYNDialogue),
                fsm.gameObject, itemName, cost,
                Ref.GM.GetSceneNameString() == SceneNames.RestingGrounds_07
                    ? YNDialogueType.Essence
                    : YNDialogueType.Geo));
        }

        public static void AddToChest(GameObject obj, string shinyName)
        {
            PlayMakerFSM fsm = obj.LocateFSM("Shiny Control");
            FsmState spawnItems = fsm.GetState("Spawn Items");

            // Remove geo from chest
            foreach (FlingObjectsFromGlobalPool fling in spawnItems.GetActionsOfType<FlingObjectsFromGlobalPool>())
            {
                fling.spawnMin = 0;
                fling.spawnMax = 0;
            }

            // Need to check SpawnFromPool action too because of Mantis Lords chest
            foreach (SpawnFromPool spawn in spawnItems.GetActionsOfType<SpawnFromPool>())
            {
                spawn.spawnMin = 0;
                spawn.spawnMax = 0;
            }

            // Instantiate a new shiny and set the chest as its parent
            GameObject item = fsm.gameObject.transform.Find("Item").gameObject;
            GameObject shiny = ObjectCache.ShinyItem;
            shiny.SetActive(false);
            shiny.transform.SetParent(item.transform);
            shiny.transform.position = item.transform.position;
            shiny.name = shinyName;

            // Force the new shiny to fling out of the chest
            PlayMakerFSM shinyControl = FSMUtility.LocateFSM(shiny, "Shiny Control");
            FsmState shinyFling = shinyControl.GetState("Fling?");
            shinyFling.ClearTransitions();
            shinyFling.AddTransition("FINISHED", "Fling R");
        }

        // TODO: ChangeChestGeo and ChangeShopContents somewhere (Not here? Not sure)

        private static void RemoveFling(GameObject obj)
        {
            PlayMakerFSM fsm = obj.LocateFSM("Shiny Control");
            FsmState fling = fsm.GetState("Fling?");
            fling.ClearTransitions();
            fling.AddTransition("FINISHED", "Fling R");
            FlingObject flingObj = fsm.GetState("Fling R").GetActionOfType<FlingObject>();
            flingObj.angleMin = flingObj.angleMax = 270;
            flingObj.speedMin = flingObj.speedMax = 0.1f;
        }

        private static void OpenYNDialogue(GameObject shiny, string itemName, int cost, YNDialogueType type)
        {
            FSMUtility.LocateFSM(GameObject.Find("DialogueManager"), "Box Open YN").SendEvent("BOX UP YN");
            FSMUtility.LocateFSM(GameObject.Find("Text YN"), "Dialogue Page Control").FsmVariables
                .GetFsmGameObject("Requester").Value = shiny;

            switch (type)
            {
                case YNDialogueType.Essence:
                    LanguageStringManager.SetString("UI", "RANDOMIZER_YN_DIALOGUE",
                        cost + " Essence: " + LanguageStringManager.GetLanguageString(itemName, "UI"));

                    if (Ref.PD.dreamOrbs < cost)
                    {
                        FSMUtility.LocateFSM(GameObject.Find("Text YN"), "Dialogue Page Control")
                            .StartCoroutine(KillGeoText());
                    }

                    cost = 0;
                    break;
                case YNDialogueType.Geo:
                    LanguageStringManager.SetString("UI", "RANDOMIZER_YN_DIALOGUE",
                        LanguageStringManager.GetLanguageString(itemName, "UI"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            FSMUtility.LocateFSM(GameObject.Find("Text YN"), "Dialogue Page Control").FsmVariables
                .GetFsmInt("Toll Cost").Value = cost;

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

        private enum YNDialogueType
        {
            Geo,
            Essence
        }
    }
}
