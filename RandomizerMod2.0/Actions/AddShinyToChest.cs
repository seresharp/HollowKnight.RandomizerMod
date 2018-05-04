using System;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using RandomizerMod.Components;

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

        public override void Process()
        {
            if (GameManager.instance.GetSceneNameString() == sceneName)
            {
                //If we somehow don't have a shiny reference, get one
                if (shinyPrefab == null)
                {
                    GameObject obj = new GameObject();
                    Object.DontDestroyOnLoad(obj);
                    obj.AddComponent<ShinyGetter>().GetShinyPrefab();

                    //This should get called again by the hooks, so we can just exit
                    return;
                }

                foreach (PlayMakerFSM fsm in fsmList)
                {
                    if (fsm.FsmName == fsmName && fsm.gameObject.name == objectName)
                    {
                        FsmState spawnItems = fsm.GetState("Spawn Items");

                        //Remove geo from chest
                        foreach (FlingObjectsFromGlobalPool fling in spawnItems.GetActionsOfType<FlingObjectsFromGlobalPool>())
                        {
                            fling.spawnMin = 0;
                            fling.spawnMax = 0;
                        }

                        //Instantiate a new shiny and set the chest as its parent
                        GameObject item = fsm.gameObject.transform.Find("Item").gameObject;
                        GameObject shiny = Object.Instantiate(shinyPrefab);
                        shiny.SetActive(false);
                        shiny.transform.SetParent(item.transform);
                        shiny.transform.position = item.transform.position;
                        shiny.name = newShinyName;

                        //Changes have been made, stop looping
                        break;
                    }
                }
            }
        }
    }
}
