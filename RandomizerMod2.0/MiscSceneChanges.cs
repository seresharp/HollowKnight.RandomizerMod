using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using RandomizerMod.Extensions;
using RandomizerMod.FsmStateActions;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;
using Random = System.Random;

namespace RandomizerMod
{
    internal static class MiscSceneChanges
    {
        private static Random rnd = new Random();
        private static int rndNum;

        public static void Hook()
        {
            UnHook();

            ModHooks.Instance.ObjectPoolSpawnHook += FixExplosionPogo;
            On.EnemyHitEffectsArmoured.RecieveHitEffect += FalseKnightNoises;
        }

        public static void UnHook()
        {
            ModHooks.Instance.ObjectPoolSpawnHook -= FixExplosionPogo;
            On.EnemyHitEffectsArmoured.RecieveHitEffect -= FalseKnightNoises;
        }

        public static void SceneChanged(Scene newScene)
        {
            RecalculateRandom();

            ApplyGeneralChanges(newScene);

            if (RandomizerMod.Instance.Settings.Randomizer)
            {
                ApplyRandomizerChanges(newScene);
            }
        }

        private static void RecalculateRandom()
        {
            rndNum = rnd.Next(25);
        }

        private static void ApplyRandomizerChanges(Scene newScene)
        {
            string sceneName = newScene.name;

            // Remove quake floors in Soul Sanctum to prevent soft locks
            if (PlayerData.instance.quakeLevel <= 0 && PlayerData.instance.killedMageLord && (sceneName == SceneNames.Ruins1_23 || sceneName == SceneNames.Ruins1_30 || sceneName == SceneNames.Ruins1_32))
            {
                foreach (GameObject obj in newScene.GetRootGameObjects())
                {
                    if (obj.name.Contains("Quake Floor") || obj.name.Contains("Quake Window"))
                    {
                        Object.Destroy(obj);
                    }
                }
            }

            // Make baldurs always able to spit rollers
            if (sceneName == SceneNames.Crossroads_11_alt || sceneName == SceneNames.Crossroads_ShamanTemple || sceneName == SceneNames.Fungus1_28)
            {
                foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
                {
                    if (obj.name.Contains("Blocker"))
                    {
                        PlayMakerFSM fsm = FSMUtility.LocateFSM(obj, "Blocker Control");
                        if (fsm != null)
                        {
                            fsm.GetState("Can Roller?").RemoveActionsOfType<IntCompare>();
                        }
                    }
                }
            }

            switch (sceneName)
            {
                case SceneNames.Abyss_12:
                    // Destroy shriek pickup if the player doesn't have wraiths
                    if (PlayerData.instance.screamLevel == 0)
                    {
                        Object.Destroy(GameObject.Find("Randomizer Shiny"));
                    }

                    break;
                case SceneNames.Crossroads_ShamanTemple:
                    // Remove gate in shaman hut
                    Object.Destroy(GameObject.Find("Bone Gate"));

                    // Add hard save to shaman shiny
                    FSMUtility.LocateFSM(GameObject.Find("Randomizer Shiny"), "Shiny Control").GetState("Finish").AddAction(new RandomizerSetHardSave());

                    break;
                case SceneNames.Dream_Nailcollection:
                    // Make picking up shiny load new scene
                    FSMUtility.LocateFSM(GameObject.Find("Randomizer Shiny"), "Shiny Control").GetState("Finish").AddAction(new RandomizerChangeScene("RestingGrounds_07", "right1"));
                    break;
                case SceneNames.Fungus2_21 when PlayerData.instance.hasCityKey:
                    // Remove city crest gate
                    Object.Destroy(GameObject.Find("City Gate Control"));
                    Object.Destroy(GameObject.Find("Ruins_front_gate"));

                    break;
                case SceneNames.Fungus2_26:
                    // Prevent leg eater from doing anything but opening the shop
                    PlayMakerFSM legEater = FSMUtility.LocateFSM(GameObject.Find("Leg Eater"), "Conversation Control");
                    FsmState legEaterChoice = legEater.GetState("Convo Choice");
                    legEaterChoice.RemoveTransitionsTo("Convo 1");
                    legEaterChoice.RemoveTransitionsTo("Convo 2");
                    legEaterChoice.RemoveTransitionsTo("Convo 3");
                    legEaterChoice.RemoveTransitionsTo("Infected Crossroad");
                    legEaterChoice.RemoveTransitionsTo("Bought Charm");
                    legEaterChoice.RemoveTransitionsTo("Gold Convo");
                    legEaterChoice.RemoveTransitionsTo("All Gold");
                    legEaterChoice.RemoveTransitionsTo("Ready To Leave");
                    legEater.GetState("All Gold?").RemoveTransitionsTo("No Shop");

                    // Just in case something other than the "Ready To Leave" state controls this
                    PlayerData.instance.legEaterLeft = false;
                    break;
                case SceneNames.Mines_33:
                    // Make tolls always interactable
                    GameObject[] tolls = new GameObject[] { GameObject.Find("Toll Gate Machine"), GameObject.Find("Toll Gate Machine (1)") };
                    foreach (GameObject toll in tolls)
                    {
                        Object.Destroy(FSMUtility.LocateFSM(toll, "Disable if No Lantern"));
                    }

                    break;
                case SceneNames.Room_Sly_Storeroom:
                    // Make Sly pickup send Sly back upstairs
                    FsmState slyFinish = FSMUtility.LocateFSM(GameObject.Find("Randomizer Shiny"), "Shiny Control").GetState("Finish");
                    slyFinish.AddAction(new RandomizerSetBool("SlyCharm", true));

                    // The game breaks if you leave the storeroom after this, so just send the player out of the shop completely
                    // People will think it's an intentional feature to cut out pointless walking anyway
                    slyFinish.AddAction(new RandomizerChangeScene("Town", "door_sly"));
                    break;
                case SceneNames.Ruins1_01 when !PlayerData.instance.hasWalljump:
                    // Add platform to stop quirrel bench soft lock
                    GameObject plat2 = Object.Instantiate(GameObject.Find("ruind_int_plat_float_01"));
                    plat2.SetActive(true);
                    plat2.transform.position = new Vector2(116, 14);

                    break;
                case SceneNames.Ruins1_02 when !PlayerData.instance.hasWalljump:
                    // Add platform to stop quirrel bench soft lock
                    GameObject plat3 = Object.Instantiate(GameObject.Find("ruind_int_plat_float_01"));
                    plat3.SetActive(true);
                    plat3.transform.position = new Vector2(2, 61.5f);

                    break;
                case SceneNames.Ruins1_05:
                    // Slight adjustment to breakable so wings is enough to progress, just like on old patches
                    GameObject chandelier = GameObject.Find("ruind_dressing_light_02 (10)");
                    chandelier.transform.SetPositionX(chandelier.transform.position.x - 2);
                    chandelier.GetComponent<NonBouncer>().active = false;

                    break;
                case SceneNames.Ruins1_24:
                    // Pickup (Quake Pickup) -> Idle -> GetPlayerDataInt (quakeLevel)
                    // Quake (Quake Item) -> Get -> SetPlayerDataInt (quakeLevel)
                    // Stop spell container from destroying itself
                    PlayMakerFSM quakePickup = FSMUtility.LocateFSM(GameObject.Find("Quake Pickup"), "Pickup");
                    quakePickup.GetState("Idle").RemoveActionsOfType<IntCompare>();

                    foreach (PlayMakerFSM childFSM in quakePickup.gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
                    {
                        if (childFSM.FsmName == "Shiny Control")
                        {
                            // Make spell container spawn shiny instead
                            quakePickup.GetState("Appear").GetActionsOfType<ActivateGameObject>()[1].gameObject.GameObject.Value = childFSM.gameObject;

                            // Make shiny open gates on pickup/destroy
                            SendEvent openGate = new SendEvent
                            {
                                eventTarget = new FsmEventTarget()
                                {
                                    target = FsmEventTarget.EventTarget.BroadcastAll,
                                    excludeSelf = true
                                },
                                sendEvent = FsmEvent.FindEvent("BG OPEN"),
                                delay = 0,
                                everyFrame = false
                            };
                            childFSM.GetState("Destroy").AddFirstAction(openGate);
                            childFSM.GetState("Finish").AddFirstAction(openGate);

                            // Add hard save after picking up item
                            childFSM.GetState("Finish").AddFirstAction(new RandomizerSetHardSave());

                            break;
                        }

                        // Stop the weird invisible floor from appearing if dive has been obtained
                        // I don't think it really serves any purpose, so destroying it should be fine
                        if (PlayerData.instance.quakeLevel > 0)
                        {
                            Object.Destroy(GameObject.Find("Roof Collider Battle"));
                        }
                    }

                    break;
                case SceneNames.Ruins1_32 when !PlayerData.instance.hasWalljump:
                    // Platform after soul master
                    GameObject plat = Object.Instantiate(GameObject.Find("ruind_int_plat_float_02 (3)"));
                    plat.SetActive(true);
                    plat.transform.position = new Vector2(40.5f, 72f);
                    break;
                case SceneNames.Ruins2_04:
                    // Shield husk doesn't walk as far as on old patches, making something pogoable to make up for this
                    GameObject.Find("Direction Pole White Palace").GetComponent<NonBouncer>().active = false;
                    break;
            }
        }

        private static void ApplyGeneralChanges(Scene newScene)
        {
            switch (newScene.name)
            {
                case SceneNames.Crossroads_38:
                    // Faster daddy
                    PlayMakerFSM grubDaddy = FSMUtility.LocateFSM(GameObject.Find("Grub King"), "King Control");
                    grubDaddy.GetState("Final Reward?").RemoveTransitionsTo("Recover");
                    grubDaddy.GetState("Final Reward?").AddTransition("FINISHED", "Recheck");
                    grubDaddy.GetState("Recheck").RemoveTransitionsTo("Gift Anim");
                    grubDaddy.GetState("Recheck").AddTransition("FINISHED", "Activate Reward");

                    // Terrible code to make a terrible fsm work not terribly
                    int geoTotal = 0;
                    grubDaddy.GetState("All Given").AddAction(new RandomizerAddGeo(grubDaddy.gameObject, 0, true));
                    grubDaddy.GetState("Recheck").AddAction(new RandomizerExecuteLambda(() => grubDaddy.GetState("All Given").GetActionsOfType<RandomizerAddGeo>()[0].SetGeo(geoTotal)));

                    foreach (PlayMakerFSM grubFsm in grubDaddy.gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
                    {
                        if (grubFsm.FsmName == "grub_reward_geo")
                        {
                            FsmState grubGeoState = grubFsm.GetState("Remaining?");
                            int geo = grubGeoState.GetActionsOfType<IntCompare>()[0].integer1.Value;

                            grubGeoState.RemoveActionsOfType<FsmStateAction>();
                            grubGeoState.AddAction(new RandomizerExecuteLambda(() => geoTotal += geo));
                            grubGeoState.AddTransition("FINISHED", "End");
                        }
                    }

                    break;
                case SceneNames.Fungus1_04:
                    // Open gates after Hornet fight
                    foreach (PlayMakerFSM childFSM in GameObject.Find("Cloak Corpse").GetComponentsInChildren<PlayMakerFSM>(true))
                    {
                        if (childFSM.FsmName == "Shiny Control")
                        {
                            SendEvent openGate = new SendEvent
                            {
                                eventTarget = new FsmEventTarget()
                                {
                                    target = FsmEventTarget.EventTarget.BroadcastAll,
                                    excludeSelf = true
                                },
                                sendEvent = FsmEvent.FindEvent("BG OPEN"),
                                delay = 0,
                                everyFrame = false
                            };
                            childFSM.GetState("Destroy").AddFirstAction(openGate);
                            childFSM.GetState("Finish").AddFirstAction(openGate);

                            break;
                        }
                    }

                    // Destroy everything relating to the dreamer cutscene
                    // This stuff is in another scene and doesn't exist immediately, so I can't use Object.Destroy
                    Components.ObjectDestroyer.Destroy("Dreamer Scene 1");
                    Components.ObjectDestroyer.Destroy("Hornet Saver");
                    Components.ObjectDestroyer.Destroy("Cutscene Dreamer");
                    Components.ObjectDestroyer.Destroy("Dream Scene Activate");

                    // Fix the camera lock zone by removing the FSM that destroys it
                    if (!PlayerData.instance.hornet1Defeated)
                    {
                        Object.Destroy(FSMUtility.LocateFSM(GameObject.Find("Camera Locks Boss"), "FSM"));
                    }

                    break;
                case SceneNames.Mines_35:
                    // Make descending dark spikes pogoable like on old patches
                    foreach (NonBouncer nonBounce in Object.FindObjectsOfType<NonBouncer>())
                    {
                        if (nonBounce.gameObject.name.StartsWith("Spike Collider"))
                        {
                            nonBounce.active = false;
                            nonBounce.gameObject.AddComponent<Components.RandomizerTinkEffect>();
                        }
                    }

                    break;
                case SceneNames.Ruins1_05b when RandomizerMod.Instance.Settings.Lemm:
                    // Lemm sell all
                    PlayMakerFSM lemm = FSMUtility.LocateFSM(GameObject.Find("Relic Dealer"), "npc_control");
                    lemm.GetState("Convo End").AddAction(new RandomizerSellRelics());
                    break;
            }
        }

        private static void FalseKnightNoises(On.EnemyHitEffectsArmoured.orig_RecieveHitEffect orig, EnemyHitEffectsArmoured self, float dir)
        {
            orig(self, dir);

            if (rndNum == 17 && self.gameObject.name == "False Knight New")
            {
                AudioPlayerOneShot hitPlayer = FSMUtility.LocateFSM(self.gameObject, "FalseyControl").GetState("Hit").GetActionsOfType<AudioPlayerOneShot>()[0];
                AudioClip clip = hitPlayer.audioClips[rnd.Next(hitPlayer.audioClips.Length)];

                AudioClip temp = self.enemyDamage.Clip;
                self.enemyDamage.Clip = clip;
                self.enemyDamage.SpawnAndPlayOneShot(self.audioPlayerPrefab, self.transform.position);
                self.enemyDamage.Clip = temp;
            }
        }

        private static GameObject FixExplosionPogo(GameObject go)
        {
            if (go.name.StartsWith("Gas Explosion Recycle M"))
            {
                go.layer = (int)GlobalEnums.PhysLayers.ENEMIES;
                NonBouncer noFun = go.GetComponent<NonBouncer>();
                if (noFun)
                {
                    noFun.active = false;
                }
            }

            return go;
        }
    }
}
