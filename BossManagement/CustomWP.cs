﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Modding;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using UnityEngine.UI;
using Object = System.Object;
using SFCore.Utils;

namespace FiveKnights
{
    public class  CustomWP : MonoBehaviour
    {
        public static bool isFromGodhome;
        public static Boss boss;
        public static CustomWP Instance;
        public static bool wonLastFight;
        public static int lev;
        public enum Boss { Ogrim, Dryya, Isma, Hegemol, All, None, Mystic, Ze };

        private void Start()
        {
            Instance = this;
            On.GameManager.EnterHero += GameManager_EnterHero;
            On.BossChallengeUI.LoadBoss_int_bool += BossChallengeUI_LoadBoss_int_bool;
            ModHooks.TakeHealthHook += Instance_TakeHealthHook;
            boss = Boss.None;

            
            FiveKnights.preloadedGO["HubRoot"] = ABManager.AssetBundles[ABManager.Bundle.GArenaHub].LoadAsset<GameObject>("pale court gg throne aditions");
            GameObject root = Instantiate(FiveKnights.preloadedGO["HubRoot"]);
            
            
            root.SetActive(true);
            foreach (var i in root.transform.GetComponentsInChildren<SpriteRenderer>(true))
            {
                i.material = new Material(Shader.Find("Sprites/Default"));
            }

            foreach (var go in FindObjectsOfType<GameObject>().Where(x => x.name.Contains("wp_rib_back")))
            {
                if (go.name != "wp_rib_back(3)" && go.name != "wp_rib_back(4)")
                {
                    Destroy(go);
                }
            }

            var del = GameObject.Find("core_extras_0024_wp(14)");
            if (del != null)
            {
                Log("Found del, deleting");
                Destroy(del);
            }
            
            foreach (var i in FindObjectsOfType<GameObject>()
                .Where(x => x.name.Contains("new_cloud") 
                            && x.transform.position.x <= 25f))
            {
                Destroy(i);
            }
            
            Material[] blurPlaneMaterials = new Material[1];
            blurPlaneMaterials[0] = new Material(Shader.Find("UI/Blur/UIBlur"));
            blurPlaneMaterials[0].SetColor(Shader.PropertyToID("_TintColor"), new Color(1.0f, 1.0f, 1.0f, 0.0f));
            blurPlaneMaterials[0].SetFloat(Shader.PropertyToID("_Size"), 53.7f);
            blurPlaneMaterials[0].SetFloat(Shader.PropertyToID("_Vibrancy"), 0.2f);
            blurPlaneMaterials[0].SetInt(Shader.PropertyToID("_StencilComp"), 8);
            blurPlaneMaterials[0].SetInt(Shader.PropertyToID("_Stencil"), 0);
            blurPlaneMaterials[0].SetInt(Shader.PropertyToID("_StencilOp"), 0);
            blurPlaneMaterials[0].SetInt(Shader.PropertyToID("_StencilWriteMask"), 255);
            blurPlaneMaterials[0].SetInt(Shader.PropertyToID("_StencilReadMask"), 255);
            Log("Look for blur!");
            foreach (var i in FindObjectsOfType<GameObject>()
                .Where(x => x.name == "BlurPlane"))
            {
                Log("Found blur!");
                i.SetActive(true);
                i.GetComponent<MeshRenderer>().materials = blurPlaneMaterials;
            }
            
            var cLock = GameObject.Find("CameraLockArea (2)");
            if (cLock != null)
            {
                var bc = cLock.GetComponent<BoxCollider2D>();
                bc.size = new Vector2(50f, bc.size.y);
                bc.offset = new Vector2(-10, bc.offset.y);
                Log("Fixed WP_09 camera at edges");
            }
        }

        private int Instance_TakeHealthHook(int damage)
        {
            return damage;
        }

        private void GameManager_EnterHero(On.GameManager.orig_EnterHero orig, GameManager self, bool additiveGateSearch)
        {
            if (self.sceneName == "White_Palace_09")
            {
                foreach (var i in FindObjectsOfType<GameObject>().Where(x => x.name == "GG_extra_walls_0000_2_2_crack"))
                {
                    Destroy(i);
                }
                
                // Create extra floor
                GameObject go = Instantiate(FiveKnights.preloadedGO["hubfloor"]);
                GameObject go2 = GameObject.Find("Chunk 2 0");
                go.transform.Find("Chunk 2 0").GetComponent<MeshRenderer>().material =
                    go2.GetComponent<MeshRenderer>().material;
                
                GameObject crack = Instantiate(FiveKnights.preloadedGO["StartDoor"]);
                crack.SetActive(true);
                crack.transform.position = new Vector3(13.8f, 95.93f, 4.21f);
                crack.transform.localScale = new Vector3(1.33f, 1.02f, 0.87f);
                GameObject secret = crack.transform.Find("GG_secret_door").gameObject;
                TransitionPoint tp = secret.transform.Find("door_Land_of_Storms").GetComponent<TransitionPoint>();
                tp.targetScene = "GG_Workshop";
                tp.entryPoint = "door_Land_of_Storms_return";
                crack.transform.Find("door_Land_of_Storms_return").gameObject.SetActive(true);
                secret.transform.Find("door_Land_of_Storms").gameObject.LocateMyFSM("Door Control")
                    .FsmVariables.FindFsmString("New Scene").Value = "GG_Workshop";
                secret.transform.Find("door_Land_of_Storms").gameObject.LocateMyFSM("Door Control")
                    .FsmVariables.FindFsmString("Entry Gate").Value = "door_Land_of_Storms_return";
                secret.LocateMyFSM("Deactivate").enabled = false;
                secret.SetActive(true);
                
                CreateStatues();
                HubRemove();
                AddLift();
                CreateGateway("door_dreamReturnGGTestingIt", new Vector2(60.5f, 98.4f), Vector2.zero, 
                    null, null, false, true, true, 
                    GameManager.SceneLoadVisualizations.Default);
                orig(self, false);
                //SetupHub();
                SetupThrone();
                Log("MADE CUSTOM WP");
                return;
            }
            orig(self, false);
        }

        private void SetupThrone()
        {
            GameObject go = Instantiate(FiveKnights.preloadedGO["throne"]);
            go.SetActive(true);
            go.transform.position = new Vector3(60.5f, 97.7f, 0.2f);
            PlayMakerFSM fsm = go.LocateMyFSM("Sit");
            FiveKnights.preloadedGO["Statue"].transform.position =
                new Vector3(48.2f, 98.4f, HeroController.instance.transform.position.z);
            for (int i = 0; i < 3; i++)
            {
                GameObject s = Instantiate(FiveKnights.preloadedGO["Statue"]);
                float y = s.transform.position.y;
                s.transform.position = new Vector3(50.2f + i * 5f, y,
                    HeroController.instance.transform.GetPositionZ());
            }
            
            IEnumerator Throne()
            {
                while (gameObject)
                {
                    yield return new WaitWhile(() => fsm.ActiveStateName != "Resting");
                    fsm.enabled = false;
                    PlayerData.instance.disablePause = true;
                    GameObject.Find("DialogueManager").LocateMyFSM("Box Open YN").SendEvent("BOX UP YN");
                    GameObject.Find("DialogueManager").SetActive(true);
                    GameObject.Find("Text YN").SetActive(true);
                    GameObject.Find("Text YN").GetComponent<DialogueBox>().StartConversation("YN_THRONE", "YN_THRONE");
                    PlayMakerFSM textYN = GameObject.Find("Text YN").LocateMyFSM("Dialogue Page Control");
                    textYN.FsmVariables.FindFsmInt("Toll Cost").Value = 0;
                    textYN.InsertCoroutine("Yes", 1, SaidYes);
                    textYN.InsertCoroutine("No", 1, SaidNo);
                    textYN.enabled = true;
                    while (textYN.ActiveStateName != "Ready for Input") yield return new WaitForEndOfFrame();
                    while (textYN.ActiveStateName == "Ready for Input") yield return new WaitForEndOfFrame();
                    yield return new WaitForSeconds(0.1f);
                }
            }

            IEnumerator SaidNo()
            {
                yield return null;
                PlayerData.instance.disablePause = false;
                PlayMakerFSM textYN = GameObject.Find("Text YN").LocateMyFSM("Dialogue Page Control");
                GameObject.Find("DialogueManager").LocateMyFSM("Box Open YN").SendEvent("BOX DOWN YN"); 
                fsm.enabled = true;
                textYN.enabled = true;
                fsm.SetState("Get Up");
                textYN.RemoveAction("No", 1);
                textYN.RemoveAction("Yes", 1);
            }
            
            IEnumerator SaidYes()
            {
                PlayerData.instance.disablePause = false;
                PlayMakerFSM textYN = GameObject.Find("Text YN").LocateMyFSM("Dialogue Page Control");
                GameObject.Find("DialogueManager").LocateMyFSM("Box Open YN").SendEvent("BOX DOWN YN");
                PlayMakerFSM pm = GameCameras.instance.tk2dCam.gameObject.LocateMyFSM("CameraFade");
                pm.SendEvent("FADE OUT");
                yield return new WaitForSeconds(0.5f);
                boss = Boss.All;
                ArenaFinder.defeats = PlayerData.instance.whiteDefenderDefeats;
                PlayerData.instance.whiteDefenderDefeats = 0;
                PlayerData.instance.respawnMarkerName = go.name;
                GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
                {
                    SceneName = "Dream_04_White_Defender",
                    EntryGateName = "door1",
                    Visualization = GameManager.SceneLoadVisualizations.Dream,
                    WaitForSceneTransitionCameraFade = false,

                });

                textYN.RemoveAction("Yes", 1);
                textYN.RemoveAction("No", 1);
                textYN.enabled = true;
                fsm.enabled = true;
                
            }

            StartCoroutine(Throne());
        }

        private void CreateGateway(string gateName, Vector2 pos, Vector2 size, string toScene, string entryGate,
                                  bool right, bool left, bool onlyOut, GameManager.SceneLoadVisualizations vis)
        {
            GameObject gate = new GameObject(gateName);
            gate.transform.SetPosition2D(pos);
            var tp = gate.AddComponent<TransitionPoint>();
            if (!onlyOut)
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
            rm.transform.parent = tp.transform;
            rm.transform.position = new Vector2(rm.transform.position.x - 3f, rm.transform.position.y);
            var tmp = rm.AddComponent<HazardRespawnMarker>();
            tp.respawnMarker = rm.GetComponent<HazardRespawnMarker>();
            tp.sceneLoadVisualization = vis;
        }

        private void CreateStatues()
        {
            On.BossStatueLever.OnTriggerEnter2D -= BossStatueLever_OnTriggerEnter2D;
            On.BossStatueLever.OnTriggerEnter2D += BossStatueLever_OnTriggerEnter2D;
            SetStatue(new Vector2(81.75f, 94.75f), new Vector2(0.5f, 0.1f), new Vector2(0f,-0.5f), FiveKnights.preloadedGO["Statue"],
                                        ArenaFinder.IsmaScene, FiveKnights.SPRITES["Isma"], "ISMA_NAME", "ISMA_DESC", "statueStateIsma");
            SetStatue(new Vector2(39.4f, 94.75f), new Vector2(-0.25f, -0.75f), new Vector2(-0f, -1f), FiveKnights.preloadedGO["StatueMed"],
                                        ArenaFinder.DryyaScene, FiveKnights.SPRITES["Dryya"], "DRY_NAME", "DRY_DESC", "statueStateDryya");
            SetStatue(new Vector2(73.3f, 98.75f), new Vector2(-0.13f, 2.03f), new Vector2(-0.3f, -0.8f), FiveKnights.preloadedGO["StatueMed"],
                                        ArenaFinder.ZemerScene, FiveKnights.SPRITES["Zemer"], "ZEM_NAME", "ZEM_DESC", "statueStateZemer");
            SetStatue(new Vector2(48f, 98.75f), new Vector2(-0.2f, 0.1f), new Vector2(-0.3f, -0.8f), FiveKnights.preloadedGO["StatueMed"],
                                        ArenaFinder.HegemolScene, FiveKnights.SPRITES["Hegemol"], "HEG_NAME", "HEG_DESC", "statueStateHegemol");
        }
        
        private Dictionary<string, StatueControl> StatueControls = new Dictionary<string, StatueControl>();
        private void BossStatueLever_OnTriggerEnter2D(On.BossStatueLever.orig_OnTriggerEnter2D orig, BossStatueLever self, Collider2D collision)
        {
            if (collision.tag != "Nail Attack") return;
            string namePD = self.gameObject.transform.parent.parent.GetComponent<BossStatue>().statueStatePD;
            if (namePD.Contains("Isma"))
            {
                StatueControls["Isma"].StartLever(self);
            }
            else if (namePD.Contains("Zemer"))
            {
                StatueControls["Zemer"].StartLever(self);
            }
            else
            {
                orig(self, collision);
            }
        }

        private void BossChallengeUI_LoadBoss_int_bool(On.BossChallengeUI.orig_LoadBoss_int_bool orig, BossChallengeUI self, int level, bool doHideAnim)
        {
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) GameManager.instance);
            
            string title = self.transform.Find("Panel").Find("BossName_Text").GetComponent<Text>().text;
            foreach (Boss b in Enum.GetValues(typeof(Boss)))
            {
                if (title.Contains(b.ToString()))
                {
                    boss = b;
                    if (b != Boss.Isma) break;
                    if (b != Boss.Ze) break;
                }
            }
            lev = level;
            orig(self, level, doHideAnim);
        }

        private void HubRemove()
        {
            foreach (var i in FindObjectsOfType<SpriteRenderer>().Where(x => x != null && x.name.Contains("SceneBorder"))) Destroy(i);
            string[] arr = { "Breakable Wall Waterways", "black_fader","White_Palace_throne_room_top_0000_2", "White_Palace_throne_room_top_0001_1",
                             "Glow Response floor_ring large2 (1)", "core_extras_0006_wp", "msk_station",
                             "core_extras_0028_wp (12)", "wp_additions_01",
                             "Inspect Region (1)", "core_extras_0021_wp (4)", "core_extras_0021_wp (5)","core_extras_0021_wp (1)", 
                             "core_extras_0021_wp (6)", "core_extras_0021_wp (7)","core_extras_0021_wp (2)", "Darkness Region"};
            foreach (var i in FindObjectsOfType<GameObject>().Where(x => x.activeSelf))
            {
                foreach (string j in arr)
                {
                    if (i.name.Contains(j))
                    {
                        Destroy(i);
                    }
                }
            }
            foreach (var i in FindObjectsOfType<GameObject>().Where(x => x.name.Contains("abyss") || x.name.Contains("Abyss"))) Destroy(i);
        }

        private void AddLift()
        {
            IEnumerator FixArena()
            {
                yield return null;
                string[] removes = {"white_palace_wall_set_01 (10)", "white_palace_wall_set_01 (18)",
                    "_0028_white (4)", "_0028_white (3)"};
                foreach (var i in FindObjectsOfType<GameObject>()
                    .Where(x=>removes.Contains(x.name)))
                {
                    Destroy(i);
                }
                yield return null;
            }
            
            StartCoroutine(FixArena());
        }

        private GameObject SetStatue(Vector2 pos, Vector2 offset, Vector2 nameOffset,
                                    GameObject go, string sceneName, Sprite spr, 
                                    string name, string desc, string state)
        {
            //Used 56's pale prince code here
            GameObject statue = Instantiate(go);
            statue.transform.SetPosition3D(pos.x, pos.y, 0f);
            var scene = ScriptableObject.CreateInstance<BossScene>();
            scene.sceneName = sceneName;
            var bs = statue.GetComponent<BossStatue>();
            switch (name)
            {
                case "ISMA_NAME":
                    bs.StatueState = FiveKnights.Instance._saveSettings.CompletionIsma;
                    SetStatue2(statue, "GG_White_Defender", "statueStateIsma2","DD_ISMA_NAME", "DD_ISMA_DESC");
                    bs.DreamStatueState = FiveKnights.Instance._saveSettings.CompletionIsma2;
                    bs.SetDreamVersion(FiveKnights.Instance._saveSettings.AltStatueIsma, false, false);
                    break;
                case "DRY_NAME":
                    bs.StatueState = FiveKnights.Instance._saveSettings.CompletionDryya;
                    break;
                case "ZEM_NAME":
                    bs.StatueState = FiveKnights.Instance._saveSettings.CompletionZemer;
                    SetStatue2(statue, sceneName, "statueStateZemer2","ZEM2_NAME","ZEM2_DESC");
                    bs.DreamStatueState = FiveKnights.Instance._saveSettings.CompletionZemer2;
                    bs.SetDreamVersion(FiveKnights.Instance._saveSettings.AltStatueZemer, false, false);
                    break;
                case "HEG_NAME":
                    bs.StatueState = FiveKnights.Instance._saveSettings.CompletionHegemol;
                    break;
            }
            bs.bossScene = scene;
            bs.statueStatePD = state;
            bs.SetPlaquesVisible(bs.StatueState.isUnlocked && bs.StatueState.hasBeenSeen);
            var details = new BossStatue.BossUIDetails();
            details.nameKey = details.nameSheet = name;
            details.descriptionKey = details.descriptionSheet = desc;
            bs.bossDetails = details;
            foreach (Transform i in statue.transform)
            {
                if (i.name.Contains("door"))
                {
                    i.name = "door_dreamReturnGG" + state;
                }
            }
            GameObject appearance = statue.transform.Find("Base").Find("Statue").gameObject;
            appearance.SetActive(true);
            SpriteRenderer sr = appearance.transform.Find("GG_statues_0006_5").GetComponent<SpriteRenderer>();
            sr.enabled = true;
            sr.sprite = spr;
            var scaleX = sr.transform.GetScaleX();
            var scaleY = sr.transform.GetScaleY();
            float scaler = state.Contains("Hegemol") ? 1.5f : 1.4f;
            sr.transform.localScale *= scaler;
            sr.transform.SetPosition3D(sr.transform.GetPositionX() + offset.x, sr.transform.GetPositionY() + offset.y, 2f);
            if (bs.StatueState.isUnlocked && bs.StatueState.hasBeenSeen)
            {
                Sprite sprite = spr;
                GameObject fakeStat = new GameObject("FakeStat");
                SpriteRenderer sr2 = fakeStat.AddComponent<SpriteRenderer>();
                sr2.sprite = sprite;
                fakeStat.transform.localScale = appearance.transform.Find("GG_statues_0006_5").localScale;
                fakeStat.transform.position = appearance.transform.Find("GG_statues_0006_5").position;

                if (state.Contains("Isma") || state.Contains("Zemer"))
                {
                    StatueControl sc = statue.transform.Find("Base").gameObject.AddComponent<StatueControl>();
                    sc.StatueName = state;
                    sc._bs = bs;
                    sc._sr = sr2;
                    if (state.Contains("Isma"))
                    {
                        GameObject fake2 = Instantiate(FiveKnights.preloadedGO["IsmaOgrimStatue"]);
                        fake2.transform.localScale = appearance.transform.Find("GG_statues_0006_5").localScale / 1.15f;
                        fake2.transform.position =
                            appearance.transform.Find("GG_statues_0006_5").position;
                        sc._fakeStatAlt2 = fake2;
                    }
                    sc._fakeStatAlt = fakeStat;
                    if (state.Contains("Isma")) StatueControls["Isma"] = sc;
                    else StatueControls["Zemer"] = sc;
                }
            }
            var tmp = statue.transform.Find("Inspect").Find("Prompt Marker").position;
            statue.transform.Find("Inspect").Find("Prompt Marker").position = new Vector3(tmp.x + nameOffset.x, tmp.y + nameOffset.y, tmp.z);
            statue.transform.Find("Inspect").gameObject.SetActive(true);
            statue.transform.Find("Spotlight").gameObject.SetActive(true);
            statue.SetActive(true);
            wonLastFight = false;
            return statue;
        }

        private void SetStatue2(GameObject statue, string sceneN, string stateN, string key, string desc)
        {
            BossScene scene = ScriptableObject.CreateInstance<BossScene>();
            scene.sceneName = sceneN;

            BossStatue bs = statue.GetComponent<BossStatue>();
            bs.dreamBossScene = scene;
            bs.dreamStatueStatePD = stateN;

            /* 56's code { */
            Destroy(statue.FindGameObjectInChildren("StatueAlt"));
            GameObject displayStatue = bs.statueDisplay;
            GameObject alt = Instantiate
            (
                displayStatue,
                displayStatue.transform.parent,
                true
            );
            alt.SetActive(bs.UsingDreamVersion);
            alt.GetComponentInChildren<SpriteRenderer>(true).flipX = true;
            alt.name = "StatueAlt";
            bs.statueDisplayAlt = alt;
            /* } 56's code */
            BossStatue.BossUIDetails details = new BossStatue.BossUIDetails();
            details.nameKey = details.nameSheet = key;
            details.descriptionKey = details.descriptionSheet = desc;
            bs.dreamBossDetails = details;

            GameObject altLever = statue.FindGameObjectInChildren("alt_lever");
            altLever.SetActive(true);
            GameObject switchBracket = altLever.FindGameObjectInChildren("GG_statue_switch_bracket");
            switchBracket.SetActive(true);

            GameObject switchLever = altLever.FindGameObjectInChildren("GG_statue_switch_lever");
            switchLever.SetActive(true);

            BossStatueLever toggle = statue.GetComponentInChildren<BossStatueLever>();
            toggle.SetOwner(bs);
            toggle.SetState(true);
        }

        private void OnDestroy()
        {
            On.BossStatueLever.OnTriggerEnter2D -= BossStatueLever_OnTriggerEnter2D;
            On.GameManager.EnterHero -= GameManager_EnterHero;
            On.BossChallengeUI.LoadBoss_int_bool -= BossChallengeUI_LoadBoss_int_bool;
            ModHooks.TakeHealthHook -= Instance_TakeHealthHook;
            
        }
        
        private static void Log(object o)
        {
            Modding.Logger.Log("[WP] " + o);
        }
    }
}
