﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FiveKnights.Misc;
using FrogCore;
using Modding;
using SFCore.Utils;
using UnityEngine;
using UnityEngine.Audio;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using Logger = Modding.Logger;

namespace FiveKnights
{
    public static class RewardRoom
    {
        private static LanguageCtrl langCtrl;
        private static Animator dryyaAnim;
        private static Animator ismaAnim;
        private static Animator ogrimAnim;
        private static Animator hegemolAnim;
        private static Animator zemerAnim;

        public static void Hook()
        {
            langCtrl = new LanguageCtrl();

            ModHooks.LanguageGetHook += LangGet;
			On.GameManager.GetCurrentMapZone += GameManagerGetCurrentMapZone;
            On.GameManager.EnterHero += GameManagerEnterHero;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ActiveSceneChanged;

        }

		public static void UnHook()
        {
            ModHooks.LanguageGetHook -= LangGet;
            On.GameManager.GetCurrentMapZone -= GameManagerGetCurrentMapZone;
            On.GameManager.EnterHero -= GameManagerEnterHero;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= ActiveSceneChanged;
        }

        private static string LangGet(string key, string sheet, string orig)
        {
            if(key.StartsWith("RR_"))
            {
                sheet = "Reward Room";
            }
            return langCtrl.ContainsKey(key, sheet) ? langCtrl.Get(key, sheet) : orig;
        }

        private static string GameManagerGetCurrentMapZone(On.GameManager.orig_GetCurrentMapZone orig, GameManager self)
        {
            if(self.sceneName == "hidden_reward_room") return MapZone.DREAM_WORLD.ToString();
            return orig(self);
        }

        private static void GameManagerEnterHero(On.GameManager.orig_EnterHero orig, GameManager self, bool additiveGateSearch)
        {
            if(self.sceneName == "hidden_reward_room")
            {
                self.tilemap.width = 500;
                self.tilemap.height = 200;
                CreateGateway("door1", new Vector2(266f, 131f), Vector2.zero,
                    null, null, true, false, true,
                    GameManager.SceneLoadVisualizations.Dream);
                GameCameras.instance.hudCamera.gameObject.transform.Find("Blanker White").gameObject.LocateMyFSM("Blanker Control").SendEvent("FADE OUT");
            }
            orig(self, false);
        }
        
        private static void ActiveSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            if (arg1.name == "White_Palace_09")
            {
                DialogueNPC entrance = DialogueNPC.CreateInstance();
                entrance.transform.position = new Vector3(65f, 98.4f, 0f);
                entrance.DialogueSelector = EntranceDialogue;
                entrance.SetTitle("RR_ENTER_TITLE");
                entrance.SetDreamKey("RR_ENTER_TITLE_SUB");
                entrance.SetUp();
            }
            if (arg1.name == "hidden_reward_room")
            {
                foreach (var g in UnityEngine.Object.FindObjectsOfType<CameraLockArea>(true))
                {
                    UnityEngine.Object.Destroy(g.gameObject);
                }
                float yLvl = 132.5f;
                CreateCameraLock("CameraLockOuter", new Vector2(301f, yLvl), new Vector2(3f, 1f),
                    new Vector2(37f, 23f), new Vector2(0f, 0f),
                    new Vector2(263f, yLvl), new Vector2(340f, yLvl), true);

                CreateCameraLock("CameraLockMid", new Vector2(302f, yLvl), new Vector2(1f, 1f),
                    new Vector2(24f, 23f), new Vector2(0f, 0f),
                    new Vector2(302f, yLvl), new Vector2(302f, yLvl), true, true);
                FixBlur();
                DreamEntry();

                PlayerData.instance.dreamReturnScene = "White_Palace_09";

                GameObject dreamBase = GameObject.Instantiate(FiveKnights.preloadedGO["Dream Base"], new Vector3(337.71f, 127.58f, -0.5f), Quaternion.identity);
                GameObject dreamBeam = GameObject.Instantiate(FiveKnights.preloadedGO["Dream Beam"], new Vector3(337.71f, 127.58f), Quaternion.identity);
                GameObject doorWarp = GameObject.Instantiate(FiveKnights.preloadedGO["Dream Door Warp"], new Vector3(337.61f, 128.18f, 0.2f), Quaternion.identity);
                PlayMakerFSM warpFSM = doorWarp.LocateMyFSM("Door Control");
                warpFSM.RemoveAction("Set PD Bool", 1);
                warpFSM.GetFsmStringVariable("Entry Gate").Value = "door_dreamReturn";
                warpFSM.GetFsmStringVariable("New Scene").Value = "White_Palace_09";
                warpFSM.GetAction<BeginSceneTransition>("Change Scene", 3).preventCameraFadeOut = true;
                dreamBase.SetActive(true);
                dreamBeam.SetActive(true);
                doorWarp.SetActive(true);

                DialogueNPC dryya = DialogueNPC.CreateInstance();
                dryya.transform.position = new Vector3(293.38f, 129.67f, 0f);
                dryya.DialogueSelector = DryyaDialogue;
                dryya.GetComponent<MeshRenderer>().enabled = false;
                dryya.SetTitle("RR_DRYYA_TITLE");
                dryya.SetDreamKey("RR_DRYYA_TITLE_SUB");
                dryya.SetUp();

                DialogueNPC isma = DialogueNPC.CreateInstance();
                isma.transform.position = new Vector3(300.79f, 129.0865f, 0f);
                isma.DialogueSelector = IsmaDialogue;
                isma.GetComponent<MeshRenderer>().enabled = false;
                isma.SetTitle("RR_ISMA_TITLE");
                isma.SetDreamKey("RR_ISMA_TITLE_SUB");
                isma.SetUp();

                DialogueNPC ogrim = DialogueNPC.CreateInstance();
                ogrim.transform.position = new Vector3(297.35f, 129.0865f, 0f);
                ogrim.DialogueSelector = OgrimDialogue;
                ogrim.GetComponent<MeshRenderer>().enabled = false;
                ogrim.SetTitle("RR_OGRIM_TITLE");
                ogrim.SetDreamKey("RR_OGRIM_TITLE_SUB");
                ogrim.gameObject.LocateMyFSM("npc_control").GetFsmBoolVariable("Hero Always Left").Value = true;
                ogrim.gameObject.LocateMyFSM("npc_control").GetFsmBoolVariable("Hero Always Right").Value = false;
                ogrim.SetUp();

                DialogueNPC hegemol = DialogueNPC.CreateInstance();
                hegemol.transform.position = new Vector3(305.08f, 129.38f, 0f);
                hegemol.DialogueSelector = HegemolDialogue;
                hegemol.GetComponent<MeshRenderer>().enabled = false;
                hegemol.SetTitle("RR_HEGEMOL_TITLE");
                hegemol.SetDreamKey("RR_HEGEMOL_TITLE_SUB");
                hegemol.gameObject.LocateMyFSM("npc_control").GetFsmBoolVariable("Hero Always Left").Value = true;
                hegemol.gameObject.LocateMyFSM("npc_control").GetFsmBoolVariable("Hero Always Right").Value = false;
                hegemol.SetUp();

                DialogueNPC zemer = DialogueNPC.CreateInstance();
                zemer.transform.position = new Vector3(311.03f, 129.0576f, 0f);
                zemer.DialogueSelector = ZemerDialogue;
                zemer.GetComponent<MeshRenderer>().enabled = false;
                zemer.SetTitle("RR_ZEMER_TITLE");
                zemer.SetDreamKey("RR_ZEMER_TITLE_SUB");
                zemer.gameObject.LocateMyFSM("npc_control").GetFsmBoolVariable("Hero Always Left").Value = true;
                zemer.gameObject.LocateMyFSM("npc_control").GetFsmBoolVariable("Hero Always Right").Value = false;
                zemer.SetUp();

                dryyaAnim = GameObject.Find("Dryya").Find("Head").GetComponent<Animator>();
                ismaAnim = GameObject.Find("Isma").GetComponent<Animator>();
                ogrimAnim = GameObject.Find("Ogrim").GetComponent<Animator>();
                hegemolAnim = GameObject.Find("Hegemol").GetComponent<Animator>();
                zemerAnim = GameObject.Find("Zemer").GetComponent<Animator>();
            }
        }

        private static void CreateGateway(string gateName, Vector2 pos, Vector2 size, string toScene, string entryGate,
                                  bool right, bool left, bool onlyOut, GameManager.SceneLoadVisualizations vis)
        {
            GameObject gate = new GameObject(gateName);
            gate.transform.SetPosition2D(pos);
            var tp = gate.AddComponent<TransitionPoint>();
            if(!onlyOut)
            {
                var bc = gate.AddComponent<BoxCollider2D>();
                bc.size = size;
                bc.isTrigger = true;
                tp.targetScene = toScene;
                tp.entryPoint = entryGate;
            }
            tp.alwaysEnterLeft = left;
            tp.alwaysEnterRight = right;
            GameObject rm = new GameObject("Hazard Respawn Marker");
            rm.transform.parent = gate.transform;
            rm.tag = "RespawnPoint";
            rm.transform.SetPosition2D(pos);
            tp.respawnMarker = rm.AddComponent<HazardRespawnMarker>();
            tp.sceneLoadVisualization = vis;
        }

        private static void CreateCameraLock(string n, Vector2 pos, Vector2 scl, Vector2 cSize, Vector2 cOff,
                                      Vector2 min, Vector2 max, bool preventLookDown = false, bool maxPriority = false)
        {
            GameObject parentlock = new GameObject(n);
            BoxCollider2D lockCol = parentlock.AddComponent<BoxCollider2D>();
            CameraLockArea cla = parentlock.AddComponent<CameraLockArea>();
            parentlock.transform.position = pos;
            parentlock.transform.localScale = scl;
            lockCol.isTrigger = true;
            lockCol.size = cSize;
            lockCol.offset = cOff;
            cla.cameraXMin = min.x;
            cla.cameraXMax = max.x;
            cla.cameraYMin = cla.cameraYMax = min.y;
            cla.preventLookDown = preventLookDown;
            cla.maxPriority = maxPriority;
            parentlock.SetActive(true);
            lockCol.enabled = cla.enabled = true;
        }

        private static void FixBlur()
        {
            GameObject pref = null;
            foreach(var i in UnityEngine.Object.FindObjectsOfType<SceneManager>())
            {
                var j = i.borderPrefab;
                pref = j;
                UnityEngine.Object.Destroy(i.gameObject);
            }
            GameObject o = UnityEngine.Object.Instantiate(FiveKnights.preloadedGO["SMTest"]);
            if(pref != null)
            {
                o.GetComponent<SceneManager>().borderPrefab = pref;
            }
            o.GetComponent<SceneManager>().noLantern = true;
            o.GetComponent<SceneManager>().darknessLevel = -1;
            o.SetActive(true);
        }

        private static void DreamEntry()
        {
            foreach(var i in GameObject.FindObjectsOfType<GameObject>()
                .Where(x => x.name == "Dream Entry"))
            {
                HeroController.instance.isHeroInPosition = true;
                GameObject de = GameObject.Instantiate(FiveKnights.preloadedGO["DreamEntry"]);
                de.transform.position = i.transform.position;
                GameObject.Destroy(i);
                de.SetActive(true);
                de.name = "Dream Entry";
                HeroController.instance.FaceRight();
            }
        }

        public static void PlayMusic(AudioClip clip)
        {
            MusicCue musicCue = ScriptableObject.CreateInstance<MusicCue>();
            MusicCue.MusicChannelInfo channelInfo = new MusicCue.MusicChannelInfo();
            Vasi.Mirror.SetField(channelInfo, "clip", clip);
            MusicCue.MusicChannelInfo[] channelInfos = new MusicCue.MusicChannelInfo[]
            {
                channelInfo, null, null, null, null, null
            };
            Vasi.Mirror.SetField(musicCue, "channelInfos", channelInfos);
            var yoursnapshot = Resources.FindObjectsOfTypeAll<AudioMixer>().First(x => x.name == "Music").FindSnapshot("Main Only");
            yoursnapshot.TransitionTo(0);
            GameManager.instance.AudioManager.ApplyMusicCue(musicCue, 0, 0, false);
        }

        private static IEnumerator DebugLoadRR()
        {
            yield return new WaitForSeconds(1f);
            HeroController.instance.EnterWithoutInput(true);
            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
            {
                SceneName = "hidden_reward_room",
                EntryGateName = "door1",
                Visualization = GameManager.SceneLoadVisualizations.Default,
                WaitForSceneTransitionCameraFade = false,
                PreventCameraFadeOut = true,
                EntryDelay = 0,
                HeroLeaveDirection = GatePosition.door
            });
        }

        private static DialogueOptions EntranceDialogue(DialogueCallbackOptions prev)
        {
            if (prev.Continue == false)
                return new() { Key = "RR_ENTER", Sheet = "Reward Room", Cost = 0, Type = DialogueType.YesNo, Continue = true };
            else
            {
                if (prev.Response == DialogueResponse.Yes)
                    GameManager.instance.StartCoroutine(DebugLoadRR());
                return new() { Continue = false };
            }
        }

        private static DialogueOptions DryyaDialogue(DialogueCallbackOptions prev)
        {
            if(!prev.Continue)
            {
                string key;
                if(FiveKnights.Instance.SaveSettings.ChampionsCallClears == 1)
                {
                    if(!FiveKnights.Instance.SaveSettings.DryyaFirstConvo1)
                    {
                        key = "RR_DRYYA_FIRST_1_1";
                        FiveKnights.Instance.SaveSettings.DryyaFirstConvo1 = true;
                    }
                    else if(!FiveKnights.Instance.SaveSettings.DryyaFirstConvo2)
                    {
                        key = "RR_DRYYA_FIRST_2_1";
                        FiveKnights.Instance.SaveSettings.DryyaFirstConvo2 = true;
                    }
                    else
					{
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.DryyaCharmConvo)
                        {
                            key = "RR_DRYYA_CHARM_1";
                            FiveKnights.Instance.SaveSettings.DryyaCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.DryyaOldNailConvo)
                        {
                            key = "RR_DRYYA_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.DryyaOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_DRYYA_FIRST_REPEAT";
                        }
                    }
                }
                else if(FiveKnights.Instance.SaveSettings.ChampionsCallClears == 2)
                {
                    if(!FiveKnights.Instance.SaveSettings.DryyaSecondConvo1)
                    {
                        key = "RR_DRYYA_SECOND_1_1";
                        FiveKnights.Instance.SaveSettings.DryyaSecondConvo1 = true;
                    }
                    else if(!FiveKnights.Instance.SaveSettings.DryyaSecondConvo2)
                    {
                        key = "RR_DRYYA_SECOND_2_1";
                        FiveKnights.Instance.SaveSettings.DryyaSecondConvo2 = true;
                    }
                    else
                    {
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.DryyaCharmConvo)
                        {
                            key = "RR_DRYYA_CHARM_1";
                            FiveKnights.Instance.SaveSettings.DryyaCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.DryyaOldNailConvo)
                        {
                            key = "RR_DRYYA_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.DryyaOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_DRYYA_SECOND_REPEAT";
                        }
                    }
                }
                else if(FiveKnights.Instance.SaveSettings.ChampionsCallClears >= 3)
                {
                    if(!FiveKnights.Instance.SaveSettings.DryyaThirdConvo1)
                    {
                        key = "RR_DRYYA_THIRD_1_1";
                        FiveKnights.Instance.SaveSettings.DryyaThirdConvo1 = true;
                    }
                    else
                    {
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.DryyaCharmConvo)
                        {
                            key = "RR_DRYYA_CHARM_1";
                            FiveKnights.Instance.SaveSettings.DryyaCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.DryyaOldNailConvo)
                        {
                            key = "RR_DRYYA_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.DryyaOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_DRYYA_THIRD_REPEAT";
                        }
                    }
                }
                else
				{
                    key = "RR_DRYYA_CHEATER";
                }
                return new()
                {
                    Key = key,
                    Sheet = "Reward Room",
                    Type = DialogueType.Normal,
                    Wait = PlayAnimDryya(),
                    Continue = true
                };
            }
            switch(prev.Key)
            {
                case "RR_DRYYA_FIRST_1_1":
                    return new() { Key = "RR_DRYYA_FIRST_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_DRYYA_FIRST_1_2":
                    return new() { Key = "RR_DRYYA_FIRST_1_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_DRYYA_FIRST_1_3":
                    return new() { Key = "RR_DRYYA_FIRST_1_4", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_DRYYA_FIRST_2_1":
                    return new() { Key = "RR_DRYYA_FIRST_2_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_DRYYA_FIRST_2_2":
                    return new() { Key = "RR_DRYYA_FIRST_2_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_DRYYA_SECOND_1_1":
                    return new() { Key = "RR_DRYYA_SECOND_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_DRYYA_SECOND_1_2":
                    return new() { Key = "RR_DRYYA_SECOND_1_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_DRYYA_SECOND_2_1":
                    if(PlayerData.instance.nailSmithUpgrades == 0) return new() { Key = "RR_DRYYA_SECOND_2_2_ALT", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                    return new() { Key = "RR_DRYYA_SECOND_2_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_DRYYA_SECOND_2_2":
                    return new() { Key = "RR_DRYYA_SECOND_2_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_DRYYA_SECOND_2_2_ALT":
                    return new() { Key = "RR_DRYYA_SECOND_2_3_ALT", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_DRYYA_THIRD_1_1":
                    return new() { Key = "RR_DRYYA_THIRD_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_DRYYA_CHARM_1":
                    return new() { Key = "RR_DRYYA_CHARM_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_DRYYA_CHARM_2":
                    return new() { Key = "RR_DRYYA_CHARM_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_DRYYA_OLDNAIL_1":
                    return new() { Key = "RR_DRYYA_OLDNAIL_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_DRYYA_OLDNAIL_2":
                    return new() { Key = "RR_DRYYA_OLDNAIL_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                default:
                    return new() { Continue = false, Wait = StopAnimDryya() };
            }

            IEnumerator PlayAnimDryya()
            {
                if(HeroController.instance.transform.position.x < dryyaAnim.gameObject.transform.position.x)
                {
                    dryyaAnim.Play("TalkLeft");
                }
                else
				{
                    yield return dryyaAnim.PlayBlocking("TurnRight");
                    dryyaAnim.Play("TalkRight");
                }
                yield break;
            }

            IEnumerator StopAnimDryya()
            {
                if(HeroController.instance.transform.position.x > dryyaAnim.gameObject.transform.position.x)
                {
                    yield return dryyaAnim.PlayBlocking("TurnLeft");
                }
                dryyaAnim.Play("Idle");
                yield break;
            }
        }

        private static DialogueOptions IsmaDialogue(DialogueCallbackOptions prev)
        {
            if(!prev.Continue)
            {
                string key;
                if(FiveKnights.Instance.SaveSettings.ChampionsCallClears == 1)
                {
                    if(!FiveKnights.Instance.SaveSettings.IsmaFirstConvo1)
                    {
                        key = "RR_ISMA_FIRST_1_1";
                        FiveKnights.Instance.SaveSettings.IsmaFirstConvo1 = true;
                    }
                    else if(!FiveKnights.Instance.SaveSettings.IsmaFirstConvo2)
                    {
                        key = "RR_ISMA_FIRST_2_1";
                        FiveKnights.Instance.SaveSettings.IsmaFirstConvo2 = true;
                    }
                    else
                    {
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.IsmaCharmConvo)
                        {
                            key = "RR_ISMA_CHARM_1";
                            FiveKnights.Instance.SaveSettings.IsmaCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.IsmaOldNailConvo)
                        {
                            key = "RR_ISMA_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.IsmaOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_ISMA_FIRST_REPEAT";
                        }
                    }
                }
                else if(FiveKnights.Instance.SaveSettings.ChampionsCallClears == 2)
                {
                    if(!FiveKnights.Instance.SaveSettings.IsmaSecondConvo1)
                    {
                        key = "RR_ISMA_SECOND_1_1";
                        FiveKnights.Instance.SaveSettings.IsmaSecondConvo1 = true;
                    }
                    else if(!FiveKnights.Instance.SaveSettings.IsmaSecondConvo2)
                    {
                        key = "RR_ISMA_SECOND_2_1";
                        FiveKnights.Instance.SaveSettings.IsmaSecondConvo2 = true;
                    }
                    else
                    {
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.IsmaCharmConvo)
                        {
                            key = "RR_ISMA_CHARM_1";
                            FiveKnights.Instance.SaveSettings.IsmaCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.IsmaOldNailConvo)
                        {
                            key = "RR_ISMA_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.IsmaOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_ISMA_SECOND_REPEAT";
                        }
                    }
                }
                else if(FiveKnights.Instance.SaveSettings.ChampionsCallClears >= 3)
                {
                    if(!FiveKnights.Instance.SaveSettings.IsmaThirdConvo1)
                    {
                        key = "RR_ISMA_THIRD_1_1";
                        FiveKnights.Instance.SaveSettings.IsmaThirdConvo1 = true;
                    }
                    else
                    {
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.IsmaCharmConvo)
                        {
                            key = "RR_ISMA_CHARM_1";
                            FiveKnights.Instance.SaveSettings.IsmaCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.IsmaOldNailConvo)
                        {
                            key = "RR_ISMA_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.IsmaOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_ISMA_THIRD_REPEAT";
                        }
                    }
                }
                else
                {
                    key = "RR_ISMA_CHEATER";
                }
                return new()
                {
                    Key = key,
                    Sheet = "Reward Room",
                    Type = DialogueType.Normal,
                    Wait = PlayAnimIsma(),
                    Continue = true
                };
            }
            switch(prev.Key)
            {
                case "RR_ISMA_FIRST_1_1":
                    return new() { Key = "RR_ISMA_FIRST_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ISMA_FIRST_1_2":
                    return new() { Key = "RR_ISMA_FIRST_1_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ISMA_FIRST_2_1":
                    return new() { Key = "RR_ISMA_FIRST_2_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ISMA_FIRST_2_2":
                    return new() { Key = "RR_ISMA_FIRST_2_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ISMA_SECOND_1_1":
                    return new() { Key = "RR_ISMA_SECOND_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ISMA_SECOND_1_2":
                    return new() { Key = "RR_ISMA_SECOND_1_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ISMA_SECOND_2_1":
                    return new() { Key = "RR_ISMA_SECOND_2_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ISMA_SECOND_2_2":
                    return new() { Key = "RR_ISMA_SECOND_2_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ISMA_THIRD_1_1":
                    return new() { Key = "RR_ISMA_THIRD_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ISMA_CHARM_1":
                    return new() { Key = "RR_ISMA_CHARM_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ISMA_CHARM_2":
                    return new() { Key = "RR_ISMA_CHARM_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ISMA_OLDNAIL_1":
                    return new() { Key = "RR_ISMA_OLDNAIL_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ISMA_OLDNAIL_2":
                    return new() { Key = "RR_ISMA_OLDNAIL_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                default:
                    return new() { Continue = false, Wait = StopAnimIsma() };
            }

            IEnumerator PlayAnimIsma()
            {
                yield return ismaAnim.PlayBlocking("TurnRight");
                ismaAnim.Play("TalkRight");
            }

            IEnumerator StopAnimIsma()
            {
                yield return ismaAnim.PlayBlocking("TurnLeft");
                ismaAnim.Play("Idle");
            }
        }

        private static DialogueOptions OgrimDialogue(DialogueCallbackOptions prev)
        {
            if(!prev.Continue)
            {
                string key;
                if(FiveKnights.Instance.SaveSettings.ChampionsCallClears == 1)
                {
                    if(!FiveKnights.Instance.SaveSettings.OgrimFirstConvo1)
                    {
                        key = "RR_OGRIM_FIRST_1_1";
                        FiveKnights.Instance.SaveSettings.OgrimFirstConvo1 = true;
                    }
                    else if(!FiveKnights.Instance.SaveSettings.OgrimFirstConvo2)
                    {
                        key = "RR_OGRIM_FIRST_2_1";
                        FiveKnights.Instance.SaveSettings.OgrimFirstConvo2 = true;
                    }
                    else
                    {
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.OgrimCharmConvo)
                        {
                            key = "RR_OGRIM_CHARM_1";
                            FiveKnights.Instance.SaveSettings.OgrimCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.OgrimOldNailConvo)
                        {
                            key = "RR_OGRIM_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.OgrimOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_OGRIM_FIRST_REPEAT";
                        }
                    }
                }
                else if(FiveKnights.Instance.SaveSettings.ChampionsCallClears == 2)
                {
                    if(!FiveKnights.Instance.SaveSettings.OgrimSecondConvo1)
                    {
                        key = "RR_OGRIM_SECOND_1_1";
                        FiveKnights.Instance.SaveSettings.OgrimSecondConvo1 = true;
                    }
                    else if(!FiveKnights.Instance.SaveSettings.OgrimSecondConvo2)
                    {
                        key = "RR_OGRIM_SECOND_2_1";
                        FiveKnights.Instance.SaveSettings.OgrimSecondConvo2 = true;
                    }
                    else
                    {
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.OgrimCharmConvo)
                        {
                            key = "RR_OGRIM_CHARM_1";
                            FiveKnights.Instance.SaveSettings.OgrimCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.OgrimOldNailConvo)
                        {
                            key = "RR_OGRIM_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.OgrimOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_OGRIM_SECOND_REPEAT";
                        }
                    }
                }
                else if(FiveKnights.Instance.SaveSettings.ChampionsCallClears >= 3)
                {
                    if(!FiveKnights.Instance.SaveSettings.OgrimThirdConvo1)
                    {
                        key = "RR_OGRIM_THIRD_1_1";
                        FiveKnights.Instance.SaveSettings.OgrimThirdConvo1 = true;
                    }
                    else
                    {
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.OgrimCharmConvo)
                        {
                            key = "RR_OGRIM_CHARM_1";
                            FiveKnights.Instance.SaveSettings.OgrimCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.OgrimOldNailConvo)
                        {
                            key = "RR_OGRIM_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.OgrimOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_OGRIM_THIRD_REPEAT";
                        }
                    }
                }
                else
                {
                    key = "RR_OGRIM_CHEATER";
                }
                return new()
                {
                    Key = key,
                    Sheet = "Reward Room",
                    Type = DialogueType.Normal,
                    Wait = PlayAnimOgrim(),
                    Continue = true
                };
            }
            switch(prev.Key)
            {
                case "RR_OGRIM_FIRST_1_1":
                    return new() { Key = "RR_OGRIM_FIRST_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_OGRIM_FIRST_1_2":
                    return new() { Key = "RR_OGRIM_FIRST_1_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_OGRIM_FIRST_2_1":
                    return new() { Key = "RR_OGRIM_FIRST_2_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_OGRIM_FIRST_2_2":
                    return new() { Key = "RR_OGRIM_FIRST_2_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_OGRIM_SECOND_1_1":
                    return new() { Key = "RR_OGRIM_SECOND_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_OGRIM_SECOND_1_2":
                    return new() { Key = "RR_OGRIM_SECOND_1_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_OGRIM_SECOND_2_1":
                    return new() { Key = "RR_OGRIM_SECOND_2_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_OGRIM_SECOND_2_2":
                    return new() { Key = "RR_OGRIM_SECOND_2_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_OGRIM_THIRD_1_1":
                    return new() { Key = "RR_OGRIM_THIRD_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_OGRIM_CHARM_1":
                    return new() { Key = "RR_OGRIM_CHARM_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_OGRIM_CHARM_2":
                    return new() { Key = "RR_OGRIM_CHARM_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_OGRIM_OLDNAIL_1":
                    return new() { Key = "RR_OGRIM_OLDNAIL_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_OGRIM_OLDNAIL_2":
                    return new() { Key = "RR_OGRIM_OLDNAIL_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                default:
                    return new() { Continue = false, Wait = StopAnimOgrim() };
            }

            IEnumerator PlayAnimOgrim()
            {
                yield return ogrimAnim.PlayBlocking("TurnLeft");
                ogrimAnim.Play("TalkLeft");
            }

            IEnumerator StopAnimOgrim()
            {
                yield return ogrimAnim.PlayBlocking("TurnRight");
                ogrimAnim.Play("TalkRight");
            }
        }

        private static DialogueOptions HegemolDialogue(DialogueCallbackOptions prev)
        {
            if(!prev.Continue)
            {
                string key;
                if(FiveKnights.Instance.SaveSettings.ChampionsCallClears == 1)
                {
                    if(!FiveKnights.Instance.SaveSettings.HegemolFirstConvo1)
                    {
                        key = "RR_HEGEMOL_FIRST_1_1";
                        FiveKnights.Instance.SaveSettings.HegemolFirstConvo1 = true;
                    }
                    else if(!FiveKnights.Instance.SaveSettings.HegemolFirstConvo2)
                    {
                        key = "RR_HEGEMOL_FIRST_2_1";
                        FiveKnights.Instance.SaveSettings.HegemolFirstConvo2 = true;
                    }
                    else
                    {
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.HegemolCharmConvo)
                        {
                            key = "RR_HEGEMOL_CHARM_1";
                            FiveKnights.Instance.SaveSettings.HegemolCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.HegemolOldNailConvo)
                        {
                            key = "RR_HEGEMOL_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.HegemolOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_HEGEMOL_FIRST_REPEAT";
                        }
                    }
                }
                else if(FiveKnights.Instance.SaveSettings.ChampionsCallClears == 2)
                {
                    if(!FiveKnights.Instance.SaveSettings.HegemolSecondConvo1)
                    {
                        key = "RR_HEGEMOL_SECOND_1_1";
                        FiveKnights.Instance.SaveSettings.HegemolSecondConvo1 = true;
                    }
                    else if(!FiveKnights.Instance.SaveSettings.HegemolSecondConvo2)
                    {
                        key = "RR_HEGEMOL_SECOND_2_1";
                        FiveKnights.Instance.SaveSettings.HegemolSecondConvo2 = true;
                    }
                    else
                    {
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.HegemolCharmConvo)
                        {
                            key = "RR_HEGEMOL_CHARM_1";
                            FiveKnights.Instance.SaveSettings.HegemolCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.HegemolOldNailConvo)
                        {
                            key = "RR_HEGEMOL_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.HegemolOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_HEGEMOL_SECOND_REPEAT";
                        }
                    }
                }
                else if(FiveKnights.Instance.SaveSettings.ChampionsCallClears >= 3)
                {
                    if(!FiveKnights.Instance.SaveSettings.HegemolThirdConvo1)
                    {
                        key = "RR_HEGEMOL_THIRD_1_1";
                        FiveKnights.Instance.SaveSettings.HegemolThirdConvo1 = true;
                    }
                    else
                    {
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.HegemolCharmConvo)
                        {
                            key = "RR_HEGEMOL_CHARM_1";
                            FiveKnights.Instance.SaveSettings.HegemolCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.HegemolOldNailConvo)
                        {
                            key = "RR_HEGEMOL_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.HegemolOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_HEGEMOL_THIRD_REPEAT";
                        }
                    }
                }
                else
                {
                    key = "RR_HEGEMOL_CHEATER";
                }
                return new()
                {
                    Key = key,
                    Sheet = "Reward Room",
                    Type = DialogueType.Normal,
                    Wait = PlayAnimHegemol(),
                    Continue = true
                };
            }
            switch(prev.Key)
            {
                case "RR_HEGEMOL_FIRST_1_1":
                    return new() { Key = "RR_HEGEMOL_FIRST_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_HEGEMOL_FIRST_1_2":
                    return new() { Key = "RR_HEGEMOL_FIRST_1_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_HEGEMOL_FIRST_2_1":
                    return new() { Key = "RR_HEGEMOL_FIRST_2_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_HEGEMOL_FIRST_2_2":
                    return new() { Key = "RR_HEGEMOL_FIRST_2_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_HEGEMOL_SECOND_1_1":
                    return new() { Key = "RR_HEGEMOL_SECOND_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_HEGEMOL_SECOND_1_2":
                    return new() { Key = "RR_HEGEMOL_SECOND_1_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_HEGEMOL_SECOND_2_1":
                    return new() { Key = "RR_HEGEMOL_SECOND_2_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_HEGEMOL_SECOND_2_2":
                    return new() { Key = "RR_HEGEMOL_SECOND_2_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_HEGEMOL_THIRD_1_1":
                    return new() { Key = "RR_HEGEMOL_THIRD_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_HEGEMOL_CHARM_1":
                    return new() { Key = "RR_HEGEMOL_CHARM_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_HEGEMOL_CHARM_2":
                    return new() { Key = "RR_HEGEMOL_CHARM_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_HEGEMOL_OLDNAIL_1":
                    return new() { Key = "RR_HEGEMOL_OLDNAIL_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_HEGEMOL_OLDNAIL_2":
                    return new() { Key = "RR_HEGEMOL_OLDNAIL_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                default:
                    return new() { Continue = false, Wait = StopAnimHegemol() };
            }

            IEnumerator PlayAnimHegemol()
            {
                yield return hegemolAnim.PlayBlocking("TurnLeft");
                hegemolAnim.Play("Talk");
            }

            IEnumerator StopAnimHegemol()
            {
                yield return hegemolAnim.PlayBlocking("TurnRight");
                hegemolAnim.Play("Idle");
            }
        }

        private static DialogueOptions ZemerDialogue(DialogueCallbackOptions prev)
        {
            if(!prev.Continue)
            {
                string key;
                if(FiveKnights.Instance.SaveSettings.ChampionsCallClears == 1)
                {
                    if(!FiveKnights.Instance.SaveSettings.ZemerFirstConvo1)
                    {
                        key = "RR_ZEMER_FIRST_1_1";
                        FiveKnights.Instance.SaveSettings.ZemerFirstConvo1 = true;
                    }
                    else if(!FiveKnights.Instance.SaveSettings.ZemerFirstConvo2)
                    {
                        key = "RR_ZEMER_FIRST_2_1";
                        FiveKnights.Instance.SaveSettings.ZemerFirstConvo2 = true;
                    }
                    else
                    {
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.ZemerCharmConvo)
                        {
                            key = "RR_ZEMER_CHARM_1";
                            FiveKnights.Instance.SaveSettings.ZemerCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.ZemerOldNailConvo)
                        {
                            key = "RR_ZEMER_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.ZemerOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_ZEMER_FIRST_REPEAT";
                        }
                    }
                }
                else if(FiveKnights.Instance.SaveSettings.ChampionsCallClears == 2)
                {
                    if(!FiveKnights.Instance.SaveSettings.ZemerSecondConvo1)
                    {
                        key = "RR_ZEMER_SECOND_1_1";
                        FiveKnights.Instance.SaveSettings.ZemerSecondConvo1 = true;
                    }
                    else if(!FiveKnights.Instance.SaveSettings.ZemerSecondConvo2)
                    {
                        key = "RR_ZEMER_SECOND_2_1";
                        FiveKnights.Instance.SaveSettings.ZemerSecondConvo2 = true;
                    }
                    else
                    {
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.ZemerCharmConvo)
                        {
                            key = "RR_ZEMER_CHARM_1";
                            FiveKnights.Instance.SaveSettings.ZemerCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.ZemerOldNailConvo)
                        {
                            key = "RR_ZEMER_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.ZemerOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_ZEMER_SECOND_REPEAT";
                        }
                    }
                }
                else if(FiveKnights.Instance.SaveSettings.ChampionsCallClears >= 3)
                {
                    if(!FiveKnights.Instance.SaveSettings.ZemerThirdConvo1)
                    {
                        key = "RR_ZEMER_THIRD_1_1";
                        FiveKnights.Instance.SaveSettings.ZemerThirdConvo1 = true;
                    }
                    else
                    {
                        if(FiveKnights.Instance.SaveSettings.equippedCharms[1] &&
                            !FiveKnights.Instance.SaveSettings.ZemerCharmConvo)
                        {
                            key = "RR_ZEMER_CHARM_1";
                            FiveKnights.Instance.SaveSettings.ZemerCharmConvo = true;
                        }
                        else if(PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades)) == 0 &&
                            !FiveKnights.Instance.SaveSettings.ZemerOldNailConvo)
                        {
                            key = "RR_ZEMER_OLDNAIL_1";
                            FiveKnights.Instance.SaveSettings.ZemerOldNailConvo = true;
                        }
                        else
                        {
                            key = "RR_ZEMER_THIRD_REPEAT";
                        }
                    }
                }
                else
                {
                    key = "RR_ZEMER_CHEATER";
                }
                return new()
                {
                    Key = key,
                    Sheet = "Reward Room",
                    Type = DialogueType.Normal,
                    Wait = PlayAnimZemer(),
                    Continue = true
                };
            }
            switch(prev.Key)
            {
                case "RR_ZEMER_FIRST_1_1":
                    return new() { Key = "RR_ZEMER_FIRST_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ZEMER_FIRST_1_2":
                    return new() { Key = "RR_ZEMER_FIRST_1_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ZEMER_FIRST_2_1":
                    return new() { Key = "RR_ZEMER_FIRST_2_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ZEMER_FIRST_2_2":
                    return new() { Key = "RR_ZEMER_FIRST_2_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ZEMER_SECOND_1_1":
                    return new() { Key = "RR_ZEMER_SECOND_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ZEMER_SECOND_1_2":
                    return new() { Key = "RR_ZEMER_SECOND_1_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ZEMER_SECOND_2_1":
                    return new() { Key = "RR_ZEMER_SECOND_2_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ZEMER_SECOND_2_2":
                    return new() { Key = "RR_ZEMER_SECOND_2_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ZEMER_THIRD_1_1":
                    return new() { Key = "RR_ZEMER_THIRD_1_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ZEMER_CHARM_1":
                    return new() { Key = "RR_ZEMER_CHARM_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ZEMER_CHARM_2":
                    return new() { Key = "RR_ZEMER_CHARM_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ZEMER_OLDNAIL_1":
                    return new() { Key = "RR_ZEMER_OLDNAIL_2", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                case "RR_ZEMER_OLDNAIL_2":
                    return new() { Key = "RR_ZEMER_OLDNAIL_3", Sheet = "Reward Room", Type = DialogueType.Normal, Continue = true };
                default:
                    return new() { Continue = false, Wait = StopAnimZemer() };
            }

            IEnumerator PlayAnimZemer()
            {
                // Nothing for now, need turn right and talk animation
                yield break;
            }

            IEnumerator StopAnimZemer()
            {
                // Nothing for now, need turn right and talk animation
                yield break;
            }
        }
    }
}
