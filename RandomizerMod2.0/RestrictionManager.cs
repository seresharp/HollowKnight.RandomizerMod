using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using RandomizerMod.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace RandomizerMod
{
    internal static class RestrictionManager
    {
        private const string HORNET_SHEET = "Hornet";
        private const string HORNET_DOOR_KEY = "HORNET_DOOR_UNOPENED";

        private static Dictionary<string, string> skills;
        private static Dictionary<string, string> bosses;

        static RestrictionManager()
        {
            // Set up dictionaries for restriction checking
            skills = new Dictionary<string, string>();
            skills.Add("hasDash", "Mothwing Cloak");
            skills.Add("hasShadowDash", "Shade Cloak");
            skills.Add("hasWalljump", "Mantis Claw");
            skills.Add("hasDoubleJump", "Monarch Wings");
            skills.Add("hasAcidArmour", "Isma's Tear");
            skills.Add("hasDashSlash", "Great Slash");
            skills.Add("hasUpwardSlash", "Dash Slash");
            skills.Add("hasCyclone", "Cyclone Slash");

            bosses = new Dictionary<string, string>();
            bosses.Add("killedInfectedKnight", "Broken Vessel");
            bosses.Add("killedMawlek", "Brooding Mawlek");
            bosses.Add("collectorDefeated", "The Collector");
            bosses.Add("defeatedMegaBeamMiner", "Crystal Guardian 1");
            bosses.Add("killedDungDefender", "Dung Defender");
            bosses.Add("killedGhostHu", "Elder Hu");
            bosses.Add("falseKnightDreamDefeated", "Failed Champion");
            bosses.Add("killedFalseKnight", "False Knight");
            bosses.Add("killedFlukeMother", "Flukemarm");
            bosses.Add("killedGhostGalien", "Galien");
            bosses.Add("killedLobsterLancer", "God Tamer");
            bosses.Add("killedGhostAladar", "Gorb");
            bosses.Add("killedGreyPrince", "Grey Prince Zote");
            bosses.Add("killedBigFly", "Gruz Mother");
            bosses.Add("killedHiveKnight", "Hive Knight");
            bosses.Add("killedHornet", "Hornet 1");
            bosses.Add("hornetOutskirtsDefeated", "Hornet 2");
            bosses.Add("infectedKnightDreamDefeated", "Lost Kin");
            bosses.Add("defeatedMantisLords", "Mantis Lords");
            bosses.Add("killedGhostMarkoth", "Markoth");
            bosses.Add("killedGhostMarmu", "Marmu");
            bosses.Add("killedNightmareGrimm", "Nightmare King Grimm");
            bosses.Add("killedGhostNoEyes", "No Eyes");
            bosses.Add("killedMimicSpider", "Nosk");
            bosses.Add("killedMageLord", "Soul Master");
            bosses.Add("mageLordDreamDefeated", "Soul Tyrant");
            bosses.Add("killedTraitorLord", "Traitor Lord");
            bosses.Add("killedGrimm", "Troupe Master Grimm");
            bosses.Add("killedMegaJellyfish", "Uumuu");
            bosses.Add("killedBlackKnight", "Watcher Knights");
            bosses.Add("killedWhiteDefender", "White Defender");
            bosses.Add("killedGhostXero", "Xero");
            bosses.Add("killedZote", "Zote");
        }

        public static void ChangeScene(Scene newScene)
        {
            switch (newScene.name)
            {
                case SceneNames.Room_temple:
                    // Handle completion restrictions
                    ProcessRestrictions();
                    break;
                case SceneNames.Room_Final_Boss_Core when RandomizerMod.Instance.Settings.AllBosses:
                    // Trigger Radiance fight without requiring dream nail hit
                    // Prevents skipping the fight in all bosses mode
                    PlayMakerFSM dreamFSM = FSMUtility.LocateFSM(newScene.FindGameObject("Dream Enter"), "Control");
                    SendEvent enterRadiance = new SendEvent
                    {
                        eventTarget = new FsmEventTarget()
                        {
                            target = FsmEventTarget.EventTarget.FSMComponent,
                            fsmComponent = dreamFSM
                        },
                        sendEvent = FsmEvent.FindEvent("NAIL HIT"),
                        delay = 0,
                        everyFrame = false
                    };

                    PlayMakerFSM bossFSM = FSMUtility.LocateFSM(newScene.FindGameObject("Hollow Knight Boss"), "Control");
                    bossFSM.GetState("H Collapsed").AddAction(enterRadiance);

                    break;
                case SceneNames.Cliffs_06 when RandomizerMod.Instance.Settings.AllBosses:
                    // Prevent banish ending in all bosses
                    Object.Destroy(GameObject.Find("Brumm Lantern NPV"));
                    break;
            }
        }

        private static void ProcessRestrictions()
        {
            if (RandomizerMod.Instance.Settings.AllBosses || RandomizerMod.Instance.Settings.AllCharms || RandomizerMod.Instance.Settings.AllSkills)
            {
                // Close the door and get rid of Quirrel
                PlayerData.instance.openedBlackEggDoor = false;
                PlayerData.instance.quirrelLeftEggTemple = true;

                // Prevent the game from opening the door
                GameObject door = GameObject.Find("Final Boss Door");
                PlayMakerFSM doorFSM = FSMUtility.LocateFSM(door, "Control");
                doorFSM.SetState("Idle");

                // The door is cosmetic, gotta get rid of the actual TransitionPoint too
                TransitionPoint doorTransitionPoint = door.GetComponentInChildren<TransitionPoint>(true);
                doorTransitionPoint.gameObject.SetActive(false);

                // Make Hornet appear
                GameObject hornet = GameObject.Find("Hornet Black Egg NPC");
                hornet.SetActive(true);
                FsmState activeCheck = FSMUtility.LocateFSM(hornet, "Conversation Control").GetState("Active?");
                activeCheck.RemoveActionsOfType<IntCompare>();
                activeCheck.RemoveActionsOfType<PlayerDataBoolTest>();

                // Reset Hornet dialog to default
                LanguageStringManager.ResetString(HORNET_SHEET, HORNET_DOOR_KEY);

                // Check dreamers
                if (!PlayerData.instance.lurienDefeated || !PlayerData.instance.monomonDefeated || !PlayerData.instance.hegemolDefeated)
                {
                    LanguageStringManager.SetString(HORNET_SHEET, HORNET_DOOR_KEY, "What kind of idiot comes here without even killing the dreamers?");
                    return;
                }

                // Check all charms
                if (RandomizerMod.Instance.Settings.AllCharms)
                {
                    PlayerData.instance.CountCharms();
                    if (PlayerData.instance.charmsOwned < 40)
                    {
                        LanguageStringManager.SetString(HORNET_SHEET, HORNET_DOOR_KEY, "What are you doing here? Go get the rest of the charms.");
                        return;
                    }
                    else if (PlayerData.instance.royalCharmState < 3)
                    {
                        LanguageStringManager.SetString(HORNET_SHEET, HORNET_DOOR_KEY, "Nice try, but half of a charm doesn't count. Go get the rest of the kingsoul.");
                        return;
                    }
                }

                // Check all skills
                if (RandomizerMod.Instance.Settings.AllSkills)
                {
                    List<string> missingSkills = new List<string>();

                    foreach (KeyValuePair<string, string> kvp in skills)
                    {
                        if (!PlayerData.instance.GetBool(kvp.Key))
                        {
                            missingSkills.Add(kvp.Value);
                        }
                    }

                    // These aren't as easy to check in a loop, so I'm just gonna check them manually
                    if (PlayerData.instance.fireballLevel == 0)
                    {
                        missingSkills.Add("Vengeful Spirit");
                    }

                    if (PlayerData.instance.fireballLevel < 2)
                    {
                        missingSkills.Add("Shade Soul");
                    }

                    if (PlayerData.instance.quakeLevel == 0)
                    {
                        missingSkills.Add("Desolate Dive");
                    }

                    if (PlayerData.instance.quakeLevel < 2)
                    {
                        missingSkills.Add("Descending Dark");
                    }

                    if (PlayerData.instance.screamLevel == 0)
                    {
                        missingSkills.Add("Howling Wraiths");
                    }

                    if (PlayerData.instance.screamLevel < 2)
                    {
                        missingSkills.Add("Abyss Shriek");
                    }

                    if (missingSkills.Count > 0)
                    {
                        string hornetStr = "You are still missing ";
                        for (int i = 0; i < missingSkills.Count; i++)
                        {
                            if (i != 0 && i == missingSkills.Count - 1)
                            {
                                hornetStr += " and ";
                            }

                            hornetStr += missingSkills[i];

                            if (i != missingSkills.Count - 1)
                            {
                                hornetStr += ", ";
                            }
                        }

                        hornetStr += ".";

                        LanguageStringManager.SetString(HORNET_SHEET, HORNET_DOOR_KEY, hornetStr);
                        return;
                    }
                }

                // Check all bosses
                if (RandomizerMod.Instance.Settings.AllBosses)
                {
                    List<string> missingBosses = new List<string>();

                    foreach (KeyValuePair<string, string> kvp in bosses)
                    {
                        if (!PlayerData.instance.GetBool(kvp.Key))
                        {
                            missingBosses.Add(kvp.Value);
                        }
                    }

                    // CG2 has no bool
                    if (PlayerData.instance.killsMegaBeamMiner > 0)
                    {
                        missingBosses.Add("Crystal Guardian 2");
                    }

                    if (missingBosses.Count > 0)
                    {
                        if (missingBosses.Count >= 10)
                        {
                            LanguageStringManager.SetString(HORNET_SHEET, HORNET_DOOR_KEY, $"You haven't killed {missingBosses.Count} bosses.");
                            return;
                        }

                        string hornetStr = "You haven't killed ";
                        for (int i = 0; i < missingBosses.Count; i++)
                        {
                            if (i != 0 && i == missingBosses.Count - 1)
                            {
                                hornetStr += " and ";
                            }

                            hornetStr += missingBosses[i];

                            if (i != missingBosses.Count - 1)
                            {
                                hornetStr += ", ";
                            }
                        }

                        hornetStr += ".";

                        LanguageStringManager.SetString(HORNET_SHEET, HORNET_DOOR_KEY, hornetStr);
                        return;
                    }

                    if (PlayerData.instance.royalCharmState != 4)
                    {
                        LanguageStringManager.SetString(HORNET_SHEET, HORNET_DOOR_KEY, "You chose all bosses, go get void heart ya dip.");
                        return;
                    }
                }

                // All checks passed, time to open up
                PlayerData.instance.openedBlackEggDoor = true;
                doorFSM.SetState("Opened");
                doorTransitionPoint.gameObject.SetActive(true);
            }
        }
    }
}
