using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Components;
using RandomizerMod.Extensions;
using RandomizerMod.FsmStateActions;
using UnityEngine;

namespace RandomizerMod.Actions
{
    [Serializable]
    public struct BigItemDef
    {
        [SerializeField] public string BoolName;
        [SerializeField] public string SpriteKey;
        [SerializeField] public string TakeKey;
        [SerializeField] public string NameKey;
        [SerializeField] public string ButtonKey;
        [SerializeField] public string DescOneKey;
        [SerializeField] public string DescTwoKey;
    }

    [Serializable]
    public class ChangeShinyIntoBigItem : RandomizerAction, ISerializationCallbackReceiver
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string objectName;
        [SerializeField] private string fsmName;
        [SerializeField] private string boolName;
        [SerializeField] private bool playerdata;

        // Serialization hack
        [SerializeField] private List<string> itemDefStrings;

        private BigItemDef[] itemDefs;

        // BigItemDef array is meant to be for additive items
        // For example, items[0] could be vengeful spirit and items[1] would be shade soul
        public ChangeShinyIntoBigItem(string sceneName, string objectName, string fsmName, BigItemDef[] items, string boolName, bool playerdata = false)
        {
            this.sceneName = sceneName;
            this.objectName = objectName;
            this.fsmName = fsmName;
            this.boolName = boolName;
            this.playerdata = playerdata;
            itemDefs = items;
        }

        public override void Process()
        {
            if (GameManager.instance.GetSceneNameString() == sceneName)
            {
                foreach (PlayMakerFSM fsm in FsmList)
                {
                    if (fsm.FsmName == fsmName && fsm.gameObject.name == objectName)
                    {
                        FsmState pdBool = fsm.GetState("PD Bool?");
                        FsmState charm = fsm.GetState("Charm?");
                        FsmState bigGetFlash = fsm.GetState("Big Get Flash");

                        // Remove actions that stop shiny from spawning
                        pdBool.RemoveActionsOfType<StringCompare>();

                        // Change pd bool test to our new bool
                        PlayerDataBoolTest boolTest = pdBool.GetActionsOfType<PlayerDataBoolTest>()[0];
                        if (playerdata)
                        {
                            boolTest.boolName = boolName;
                        }
                        else
                        {
                            RandomizerBoolTest randBoolTest = new RandomizerBoolTest(boolName, boolTest.isFalse, boolTest.isTrue);
                            pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
                            pdBool.AddFirstAction(randBoolTest);
                        }

                        // Force the FSM to show the big item flash
                        charm.ClearTransitions();
                        charm.AddTransition("FINISHED", "Big Get Flash");

                        // Set bool and show the popup after the flash
                        bigGetFlash.AddAction(new RandomizerCallStaticMethod(
                            typeof(BigItemPopup),
                            "ShowAdditive",
                            itemDefs,
                            fsm.gameObject,
                            "GET ITEM MSG END"));

                        // Don't actually need to set the skill here, that happens in BigItemPopup
                        // Maybe change that at some point, it's not where it should happen
                        if (!playerdata)
                        {
                            bigGetFlash.AddAction(new RandomizerSetBool(boolName, true));
                        }

                        // Exit the fsm after the popup
                        bigGetFlash.ClearTransitions();
                        bigGetFlash.AddTransition("GET ITEM MSG END", "Hero Up");

                        // Changes have been made, stop looping
                        break;
                    }
                }
            }
        }

        public void OnBeforeSerialize()
        {
            itemDefStrings = new List<string>();
            foreach (BigItemDef item in itemDefs)
            {
                itemDefStrings.Add(JsonUtility.ToJson(item));
            }
        }

        public void OnAfterDeserialize()
        {
            List<BigItemDef> itemDefList = new List<BigItemDef>();

            foreach (string item in itemDefStrings)
            {
                itemDefList.Add(JsonUtility.FromJson<BigItemDef>(item));
            }

            itemDefs = itemDefList.ToArray();
        }
    }
}
