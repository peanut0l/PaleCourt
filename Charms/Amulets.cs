﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SFCore.Utils;
using Modding;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FiveKnights
{
    internal partial class Amulets : MonoBehaviour
    {
        private HeroController _hc;// = HeroController.instance;
        private PlayerData _pd;// = PlayerData.instance;

        private AudioSource _audio;

        private PlayMakerFSM _spellControl;

        private PlayMakerFSM _blastControl;
        private PlayMakerFSM _pvControl;
        private GameObject _audioPlayerActor;

        public void Awake()
        {
            On.HeroController.Awake += On_HeroController_Awake;
            On.CharmIconList.GetSprite += CharmIconList_GetSprite;
            ModHooks.CharmUpdateHook += CharmUpdate;
        }

        private Sprite CharmIconList_GetSprite(On.CharmIconList.orig_GetSprite orig, CharmIconList self, int id)
        {
            if (FiveKnights.Instance.SaveSettings.upgradedCharm_10)
            {
                //Log("Upgraded Defender's Crest");
                self.spriteList[10] = FiveKnights.SPRITES["Kings_Honour"];
            }
            else
            {
                self.spriteList[10] = FiveKnights.SPRITES["Defenders_Crest"];
            }
            return orig(self, id);
        }

        public void On_HeroController_Awake(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig(self);
            _hc = self;
            _pd = PlayerData.instance;

            Log("Amulets Awake");

            Log("Dash Cooldown: " + self.DASH_COOLDOWN);
            Log("Dash Cooldown Charm: " + self.DASH_COOLDOWN_CH);
            Log("Dash Speed: " + self.DASH_SPEED);
            Log("Dash Speed Sharp: " + self.DASH_SPEED_SHARP);

            //RepositionCharmsInInventory();

            _pvControl = Instantiate(FiveKnights.preloadedGO["PV"].LocateMyFSM("Control"), self.transform);
            GameObject blast = Instantiate(FiveKnights.preloadedGO["Blast"]);
            blast.SetActive(true);
            _blastControl = blast.LocateMyFSM("Control");

            //_pd.CalculateNotchesUsed();

            Log("Waiting for Audio Player Actor...");
            _spellControl = self.spellControl;
            GameObject fireballParent = _spellControl.GetAction<SpawnObjectFromGlobalPool>("Fireball 2", 3).gameObject.Value;
            PlayMakerFSM fireballCast = fireballParent.LocateMyFSM("Fireball Cast");
            _audioPlayerActor = fireballCast.GetAction<AudioPlayerOneShotSingle>("Cast Right", 3).audioPlayer.Value;
            _audio = _audioPlayerActor.GetComponent<AudioSource>();
            Log("Got Audio");
            _audio.pitch = 1.5f;

            // Mark of Purity
            self.gameObject.AddComponent<PurityTimer>().enabled = false;
            self.gameObject.AddComponent<AutoSwing>().enabled = false;

            // Boon of Hallownest
            self.gameObject.AddComponent<BoonSpells>().enabled = false;
            InsertCharmSpellEffectsInFsm();

            // Abyssal Bloom
            self.gameObject.AddComponent<ModifyBloomProps>().enabled = true;
            self.gameObject.AddComponent<AbyssalBloom>().enabled = false;
            self.gameObject.AddComponent<CheckBloomStage>().enabled = true;
            AddVoidAttacks(self);
            ModifyFuryForAbyssalBloom();

#if DEBUG
            //  FiveKnights.Instance.SaveSettings.upgradedCharm_10 = true;

            /*FiveKnights.Instance.SaveSettings.gotCharms[0] = true;
            FiveKnights.Instance.SaveSettings.gotCharms[1] = true;
            FiveKnights.Instance.SaveSettings.gotCharms[2] = true;
            FiveKnights.Instance.SaveSettings.gotCharms[3] = true;*/

            /*PureAmulets.Settings.newCharm_41 = true;
            PureAmulets.Settings.newCharm_42 = true;
            PureAmulets.Settings.newCharm_43 = true;
            PureAmulets.Settings.newCharm_44 = true;*/

            Log("Got Charm 41: " + FiveKnights.Instance.SaveSettings.gotCharms[0]);
            Log("Got Charm 42: " + FiveKnights.Instance.SaveSettings.gotCharms[1]);
            Log("Got Charm 43: " + FiveKnights.Instance.SaveSettings.gotCharms[2]);
            Log("Got Charm 44: " + FiveKnights.Instance.SaveSettings.gotCharms[3]);
            Log("New Charm 41: " + FiveKnights.Instance.SaveSettings.newCharms[0]);
            Log("New Charm 42: " + FiveKnights.Instance.SaveSettings.newCharms[1]);
            Log("New Charm 43: " + FiveKnights.Instance.SaveSettings.newCharms[2]);
            Log("New Charm 44: " + FiveKnights.Instance.SaveSettings.newCharms[3]);
            Log("Equipped Charm 41: " + FiveKnights.Instance.SaveSettings.equippedCharms[0]);
            Log("Equipped Charm 42: " + FiveKnights.Instance.SaveSettings.equippedCharms[1]);
            Log("Equipped Charm 43: " + FiveKnights.Instance.SaveSettings.equippedCharms[2]);
            Log("Equipped Charm 44: " + FiveKnights.Instance.SaveSettings.equippedCharms[3]);
            Log("Upgraded Charm 10: " + FiveKnights.Instance.SaveSettings.upgradedCharm_10);
#endif
        }

        private GameObject _royalAura;

        private void InsertCharmSpellEffectsInFsm()
        {
            _spellControl.CopyState("Fireball 1", "Fireball 1 SmallShots");
            _spellControl.CopyState("Fireball 2", "Fireball 2 SmallShots");

            _spellControl.RemoveAction<SpawnObjectFromGlobalPool>("Fireball 1 SmallShots");
            _spellControl.RemoveAction<SpawnObjectFromGlobalPool>("Fireball 2 SmallShots");
            _spellControl.InsertMethod("Fireball 1 SmallShots", 3, () => HeroController.instance.GetComponent<BoonSpells>().CastDaggers(false));
            _spellControl.InsertMethod("Fireball 2 SmallShots", 3, () => HeroController.instance.GetComponent<BoonSpells>().CastDaggers(true));

            _spellControl.CopyState("Quake1 Land", "Q1 Land Plumes");
            _spellControl.CopyState("Q2 Land", "Q2 Land Plumes");
            _spellControl.ChangeTransition("Q2 Land Plumes", "FINISHED", "Quake Finish");
            _spellControl.InsertMethod("Q1 Land Plumes", () => HeroController.instance.GetComponent<BoonSpells>().CastPlumes(false), 0);
            _spellControl.InsertMethod("Q2 Land Plumes", () => HeroController.instance.GetComponent<BoonSpells>().CastPlumes(true), 0);

            _spellControl.CopyState("Scream Antic1", "Scream Antic1 Blasts");
            _spellControl.CopyState("Scream Burst 1", "Scream Burst 1 Blasts");
            _spellControl.CopyState("Scream Antic2", "Scream Antic2 Blasts");
            _spellControl.CopyState("Scream Burst 2", "Scream Burst 2 Blasts");
            _spellControl.ChangeTransition("Scream Antic1 Blasts", "FINISHED", "Scream Burst 1 Blasts");
            _spellControl.ChangeTransition("Scream Antic2 Blasts", "FINISHED", "Scream Burst 2 Blasts");

            _spellControl.RemoveAction<AudioPlay>("Scream Antic1 Blasts");
            _spellControl.RemoveAction<CreateObject>("Scream Burst 1 Blasts");
            _spellControl.RemoveAction<ActivateGameObject>("Scream Burst 1 Blasts");
            _spellControl.RemoveAction<ActivateGameObject>("Scream Burst 1 Blasts");
            _spellControl.RemoveAction<SendEventByName>("Scream Burst 1 Blasts");
            _spellControl.RemoveAction<SendEventByName>("Scream Burst 1 Blasts");
            _spellControl.InsertMethod("Scream Burst 1 Blasts", 0, () => HeroController.instance.GetComponent<BoonSpells>().CastBlasts(false));

            _spellControl.RemoveAction<AudioPlay>("Scream Antic2 Blasts");
            _spellControl.RemoveAction<CreateObject>("Scream Burst 2 Blasts");
            _spellControl.RemoveAction<ActivateGameObject>("Scream Burst 2 Blasts");
            _spellControl.RemoveAction<ActivateGameObject>("Scream Burst 2 Blasts");
            _spellControl.RemoveAction<SendEventByName>("Scream Burst 2 Blasts");
            _spellControl.RemoveAction<SendEventByName>("Scream Burst 2 Blasts");
            _spellControl.InsertMethod("Scream Burst 2 Blasts", 0, () => HeroController.instance.GetComponent<BoonSpells>().CastBlasts(true));

            _spellControl.CopyState("Focus", "Focus Blast");
            _spellControl.CopyState("Focus Heal", "Focus Heal Blast");
            _spellControl.CopyState("Start MP Drain", "Start MP Drain Blast");
            _spellControl.CopyState("Focus Heal 2", "Focus Heal 2 Blast");
            _spellControl.InsertCoroutine("Focus Blast", 0, PureVesselBlastFadeIn);
            _spellControl.InsertCoroutine("Focus Heal Blast", 0, PureVesselBlast);
            _spellControl.InsertCoroutine("Start MP Drain Blast", 0, PureVesselBlastFadeIn);
            _spellControl.InsertCoroutine("Focus Heal 2 Blast", 0, PureVesselBlast);
            _spellControl.InsertMethod("Cancel All", 0, CancelBlast);

            _spellControl.InsertMethod("Focus Cancel", 0, CancelBlast);
            _spellControl.InsertMethod("Focus Cancel 2", 0, CancelBlast);
        }

        private void AddVoidAttacks(HeroController self)
        {
            GameObject attacks = self.gameObject.FindGameObjectInChildren("Attacks");

            Shader shader = self.GetComponent<tk2dSprite>().Collection.spriteDefinitions[0].material.shader;

            GameObject collectionPrefab = FiveKnights.preloadedGO["Bloom Sprite Prefab"];
            tk2dSpriteCollection collection = collectionPrefab.GetComponent<tk2dSpriteCollection>();
            GameObject animationPrefab = FiveKnights.preloadedGO["Bloom Anim Prefab"];
            tk2dSpriteAnimation animation = animationPrefab.GetComponent<tk2dSpriteAnimation>();

            // Knight sprites and animations
            var heroSprite = self.GetComponent<tk2dSprite>();
            var knightAnim = self.GetComponent<tk2dSpriteAnimator>();
            tk2dSpriteCollectionData collectionData = heroSprite.Collection;
            List<tk2dSpriteDefinition> knightSpriteDefs = collectionData.spriteDefinitions.ToList();
            foreach(tk2dSpriteDefinition def in collection.spriteCollection.spriteDefinitions)
            {
                def.material.shader = shader;
                knightSpriteDefs.Add(def);
            }
            heroSprite.Collection.spriteDefinitions = knightSpriteDefs.ToArray();
            List<tk2dSpriteAnimationClip> knightClips = knightAnim.Library.clips.ToList();
            foreach(tk2dSpriteAnimationClip clip in animation.clips)
            {
                knightClips.Add(clip);
            }
            knightAnim.Library.clips = knightClips.ToArray();

            GameObject cycloneSlashVoid = Instantiate(attacks.FindGameObjectInChildren("Cyclone Slash"), attacks.transform);
            cycloneSlashVoid.name = "Cyclone Slash Void";
            cycloneSlashVoid.GetComponent<tk2dSpriteAnimator>().DefaultClipId = knightAnim.GetClipIdByName("Cyclone Slash Effect Void");

            GameObject dashSlashVoid = Instantiate(attacks.FindGameObjectInChildren("Dash Slash"), attacks.transform);
            dashSlashVoid.name = "Dash Slash Void";
            dashSlashVoid.GetComponent<tk2dSpriteAnimator>().DefaultClipId = knightAnim.GetClipIdByName("Dash Slash Effect Void");

            GameObject greatSlashVoid = Instantiate(attacks.FindGameObjectInChildren("Great Slash"), attacks.transform);
            greatSlashVoid.name = "Great Slash Void";
            greatSlashVoid.GetComponent<tk2dSpriteAnimator>().DefaultClipId = knightAnim.GetClipIdByName("Great Slash Effect Void");

            // Nail Arts FSM
            PlayMakerFSM nailArts = self.gameObject.LocateMyFSM("Nail Arts");
            if(nailArts.FsmStates[0].Fsm == null)
            {
                nailArts.Preprocess();
            }

            // Create states to test for activated Abyssal Bloom
            nailArts.AddState("Bloom Activated CSlash?");
            nailArts.AddState("Bloom Activated DSlash?");
            nailArts.AddState("Bloom Activated GSlash?");

            // Clone Cyclone Slash states
            nailArts.CopyState("Cyclone Start", "Cyclone Start Void");
            nailArts.CopyState("Hover Start", "Hover Start Void");
            nailArts.CopyState("Activate Slash", "Activate Slash Void");
            nailArts.CopyState("Play Audio", "Play Audio Void");
            nailArts.CopyState("Cyclone Spin", "Cyclone Spin Void");
            nailArts.CopyState("Cyclone Extend", "Cyclone Extend Void");
            nailArts.CopyState("Cyclone End", "Cyclone End Void");

            // Clone Dash Slash states
            nailArts.CopyState("Dash Slash", "Dash Slash Void");
            nailArts.CopyState("DSlash Move End", "DSlash Move End Void");
            nailArts.CopyState("D Slash End", "D Slash End Void");

            // Clone Great Slash states
            nailArts.CopyState("G Slash", "G Slash Void");
            nailArts.CopyState("Stop Move", "Stop Move Void");
            nailArts.CopyState("G Slash End", "G Slash End Void");

            // Change transitions for Cyclone Slash Void
            nailArts.ChangeTransition("Flash", "FINISHED", "Bloom Activated CSlash?");
            nailArts.ChangeTransition("Cyclone Start Void", "FINISHED", "Activate Slash Void");
            nailArts.ChangeTransition("Cyclone Start Void", "BUTTON DOWN", "Hover Start Void");
            nailArts.ChangeTransition("Hover Start Void", "FINISHED", "Cyclone Start Void");
            nailArts.ChangeTransition("Activate Slash Void", "FINISHED", "Play Audio Void");
            nailArts.ChangeTransition("Play Audio Void", "FINISHED", "Cyclone Spin Void");
            nailArts.ChangeTransition("Cyclone Spin Void", "BUTTON DOWN", "Cyclone Extend Void");
            nailArts.ChangeTransition("Cyclone Spin Void", "END", "Cyclone End Void");
            nailArts.ChangeTransition("Cyclone Extend Void", "END", "Cyclone End Void");
            nailArts.ChangeTransition("Cyclone Extend Void", "WAIT", "Cyclone Spin Void");

            // Change transitions for Dash Slash Void
            nailArts.ChangeTransition("Left 2", "FINISHED", "Bloom Activated DSlash?");
            nailArts.ChangeTransition("Right 2", "FINISHED", "Bloom Activated DSlash?");
            nailArts.ChangeTransition("Dash Slash Void", "FINISHED", "DSlash Move End Void");
            nailArts.ChangeTransition("DSlash Move End Void", "FINISHED", "D Slash End Void");

            // Change transitions for Great Slash Void
            nailArts.ChangeTransition("Left", "FINISHED", "Bloom Activated GSlash?");
            nailArts.ChangeTransition("Right", "FINISHED", "Bloom Activated GSlash?");
            nailArts.ChangeTransition("G Slash Void", "FINISHED", "Stop Move Void");
            nailArts.ChangeTransition("Stop Move Void", "FINISHED", "G Slash End Void");

            // Make transitions for void narts
            nailArts.AddTransition("Bloom Activated CSlash?", "VOID", "Cyclone Start Void");
            nailArts.AddTransition("Bloom Activated CSlash?", "NORMAL", "Cyclone Start");
            nailArts.AddTransition("Bloom Activated DSlash?", "VOID", "Dash Slash Void");
            nailArts.AddTransition("Bloom Activated DSlash?", "NORMAL", "Dash Slash");
            nailArts.AddTransition("Bloom Activated GSlash?", "VOID", "G Slash Void");
            nailArts.AddTransition("Bloom Activated GSlash?", "NORMAL", "G Slash");
            nailArts.AddMethod("Bloom Activated CSlash?", () =>
            {
                nailArts.SetState(FiveKnights.Instance.SaveSettings.equippedCharms[3] && _pd.health <= 1 ? "Cyclone Start Void" : "Cyclone Start");
            });
            nailArts.AddMethod("Bloom Activated DSlash?", () =>
            {
                nailArts.SetState(FiveKnights.Instance.SaveSettings.equippedCharms[3] && _pd.health <= 1 ? "Dash Slash Void" : "Dash Slash");
            });
            nailArts.AddMethod("Bloom Activated GSlash?", () =>
            {
                nailArts.SetState(FiveKnights.Instance.SaveSettings.equippedCharms[3] && _pd.health <= 1 ? "G Slash Void" : "G Slash");
            });

            // Change Knight animation clips
            nailArts.GetAction<Tk2dPlayAnimationWithEvents>("Cyclone Start Void").clipName = "NA Cyclone Start Void";
            nailArts.GetAction<Tk2dPlayAnimation>("Cyclone Spin Void").clipName = "NA Cyclone Void";
            nailArts.GetAction<Tk2dPlayAnimation>("Cyclone Extend Void").clipName = "NA Cyclone Void";
            nailArts.GetAction<Tk2dPlayAnimationWithEvents>("Cyclone End Void").clipName = "NA Cyclone End Void";
            nailArts.GetAction<Tk2dPlayAnimationWithEvents>("Dash Slash Void").clipName = "NA Dash Slash Void";
            nailArts.GetAction<Tk2dPlayAnimationWithEvents>("G Slash Void").clipName = "NA Big Slash Void";

			//// Insert testing methods for testing states
			//nailArts.InsertMethod("Bloom Activated CSlash?", 0, () =>
			//{
			//	nailArts.SetState(FiveKnights.Instance.SaveSettings.equippedCharms[3] && _pd.health <= 10 ? "Cyclone Start Void" : "Cyclone Start");
			//});
			//nailArts.InsertMethod("Bloom Activated DSlash?", 0, () =>
			//{
			//	nailArts.SetState(FiveKnights.Instance.SaveSettings.equippedCharms[3] && _pd.health <= 10 ? "Dash Slash Void" : "Dash Slash");
			//});
			//nailArts.InsertMethod("Bloom Activated GSlash?", 0, () =>
			//{
			//	Log($"PureAmulets.Settings.equippedCharm_44: {FiveKnights.Instance.SaveSettings.equippedCharms[3]}, health: {_pd.health <= 10}");
			//	nailArts.SetState(FiveKnights.Instance.SaveSettings.equippedCharms[3] && _pd.health <= 10 ? "G Slash Void" : "G Slash");
			//});

			// Insert activation and deactivation of void nail arts
			nailArts.InsertMethod("Activate Slash Void", 0, () =>
            {
                cycloneSlashVoid.SetActive(true);
                cycloneSlashVoid.GetComponent<tk2dSpriteAnimator>().Play("Cyclone Slash Effect Void");
            });
            nailArts.InsertMethod("Cyclone End Void", 2, () => cycloneSlashVoid.SetActive(false));
            nailArts.AddMethod("Cancel All", () => cycloneSlashVoid.SetActive(false));
            nailArts.InsertMethod("Dash Slash Void", 0, () =>
            {
                dashSlashVoid.SetActive(true);
                dashSlashVoid.GetComponent<tk2dSpriteAnimator>().Play("Dash Slash Effect Void");
            });
            nailArts.InsertMethod("D Slash End Void", 0, () => dashSlashVoid.SetActive(false));
            nailArts.AddMethod("Cancel All", () => dashSlashVoid.SetActive(false));
            nailArts.InsertMethod("G Slash Void", 0, () =>
            {
                greatSlashVoid.SetActive(true);
                greatSlashVoid.GetComponent<tk2dSpriteAnimator>().Play("Great Slash Effect Void");
            });
            nailArts.InsertMethod("G Slash End Void", 0, () => greatSlashVoid.SetActive(false));
            nailArts.AddMethod("Cancel All", () => greatSlashVoid.SetActive(false));

            // Remove activating old nail art effects
            nailArts.RemoveAction<ActivateGameObject>("Activate Slash Void");
            nailArts.RemoveAction<ActivateGameObject>("Dash Slash Void");
            nailArts.RemoveAction<ActivateGameObject>("G Slash Void");

            nailArts.Log();
            nailArts.MakeLog(true);

            StartCoroutine(ResetVoidNarts(new GameObject[] { cycloneSlashVoid, dashSlashVoid, greatSlashVoid }));
        }

        private IEnumerator ResetVoidNarts(GameObject[] narts)
		{
            // This is necessary because otherwise the very first void nail art will always be a normal one
            foreach(GameObject nart in narts)
			{
                nart.SetActive(true);
			}
            yield return new WaitForEndOfFrame();
            foreach(GameObject nart in narts)
            {
                nart.SetActive(false);
            }
        }

        private void ModifyFuryForAbyssalBloom()
        {
            PlayMakerFSM fury = _hc.gameObject.FindGameObjectInChildren("Charm Effects").LocateMyFSM("Fury");
            Log("Fury Color: " + fury.GetAction<Tk2dSpriteSetColor>("Activate", 17).color.Value);
            Color furyColor = fury.GetAction<Tk2dSpriteSetColor>("Activate", 18).color.Value;
            fury.InsertMethod("Activate", 17, () =>
            {
                Color color = FiveKnights.Instance.SaveSettings.equippedCharms[3] ? Color.black : furyColor;
                fury.GetAction<Tk2dSpriteSetColor>("Activate", 18).color.Value = color;
                fury.GetAction<Tk2dSpriteSetColor>("Activate", 19).color.Value = color;
                fury.GetAction<Tk2dSpriteSetColor>("Activate", 20).color.Value = color;
            });
        }

        private void CharmUpdate(PlayerData playerData, HeroController hc)
        {
            Log("Charm Update");

            if (playerData.GetBool("equippedCharm_" + Charms.DefendersCrest) && FiveKnights.Instance.SaveSettings.upgradedCharm_10)
            {
                StartCoroutine(FindAndAddComponentToDung());
                /*if (_royalAura != null) Destroy(_royalAura);
                _royalAura = Instantiate(FiveKnights.preloadedGO["Royal Aura"]);
                Vector3 pos = hc.transform.position;
                Transform auraTransform = _royalAura.transform;
                auraTransform.SetPosition2D(pos);
                auraTransform.SetPositionZ(pos.z + 1.0f);
                auraTransform.parent = gameObject.transform;
                _royalAura.FindGameObjectInChildren("Smoke 0").AddComponent<RoyalAura>();*/
            }
            else
            {
                if (_royalAura != null) Destroy(_royalAura);
            }

            if (FiveKnights.Instance.SaveSettings.equippedCharms[0])
            {
                _hc.GetComponent<PurityTimer>().enabled = true;
                _hc.GetComponent<AutoSwing>().enabled = true;
            }
            else
            {
                _hc.GetComponent<PurityTimer>().enabled = false;
                _hc.GetComponent<AutoSwing>().enabled = false;
            }

            if (FiveKnights.Instance.SaveSettings.equippedCharms[1])
            {
                _spellControl.ChangeTransition("Slug?", "FINISHED", "Focus Blast");
                _spellControl.ChangeTransition("Set HP Amount", "FINISHED", "Focus Heal Blast");
                _spellControl.ChangeTransition("Speedup?", "FINISHED", "Start MP Drain Blast");
                _spellControl.ChangeTransition("Set HP Amount 2", "FINISHED", "Focus Heal 2 Blast");
            }
            else
            {
                _spellControl.ChangeTransition("Slug?", "FINISHED", "Focus");
                _spellControl.ChangeTransition("Set HP Amount", "FINISHED", "Focus Heal");
                _spellControl.ChangeTransition("Speedup?", "FINISHED", "Start MP Drain");
                _spellControl.ChangeTransition("Set HP Amount 2", "FINISHED", "Focus Heal 2");
            }

            _hc.GetComponent<BoonSpells>().enabled = FiveKnights.Instance.SaveSettings.equippedCharms[2];

            _hc.GetComponent<AbyssalBloom>().enabled = FiveKnights.Instance.SaveSettings.equippedCharms[3];
        }

        private IEnumerator FindAndAddComponentToDung()
        {
            yield return new WaitWhile(() => !GameObject.Find("Dung"));
            // Destroy(GameObject.Find("Dung"));
            GameObject dung = GameObject.Find("Dung");
            if (!dung.GetComponent<Dung>()) dung.AddComponent<Dung>();
        }

        private void Update()
        {
            //GameObject cursor = GameManager.instance.inventoryFSM.gameObject.FindGameObjectInChildren("Charms").FindGameObjectInChildren("Cursor");
            //Log("Cursor pos: " + cursor.transform.position);
            // Log("Equipped Charms: " + PureAmulets.Settings.equippedCharm_41 + " " +
            //     PureAmulets.Settings.equippedCharm_42 + " " + PureAmulets.Settings.equippedCharm_43 + " " +
            //     PureAmulets.Settings.equippedCharm_44);
        }

        private GameObject _blast;

        private IEnumerator PureVesselBlastFadeIn()
        {
            AudioPlayerOneShotSingle("Focus Charge", 1.2f, 1.5f);
            _blast = Instantiate(FiveKnights.preloadedGO["Blast"], HeroController.instance.transform);
            _blast.transform.localPosition += Vector3.up * 0.25f;
            _blast.SetActive(true);
            Destroy(_blast.FindGameObjectInChildren("hero_damager"));

            if (_pd.GetBool("equippedCharm_" + Charms.DeepFocus))
            {
                _blast.transform.localScale *= 2.5f;
            }
            else
            {
                _blast.transform.localScale *= 1.5f;
            }

            Animator anim = _blast.GetComponent<Animator>();
            anim.speed = 1;
            if (_pd.GetBool("equippedCharm_" + Charms.QuickFocus))
            {
                anim.speed *= 1.5f;
            }

            if (_pd.GetBool("equippedCharm_" + Charms.DeepFocus))
            {
                anim.speed -= anim.speed * 0.35f;
            }

            yield return null;
        }

        private IEnumerator PureVesselBlast()
        {
            Log("Pure Vessel Blast");
            _blast.layer = 17;
            Animator anim = _blast.GetComponent<Animator>();
            anim.speed = 1;
            int hash = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
            anim.PlayInFixedTime(hash, -1, 0.8f);

            Log("Adding CircleCollider2D");
            CircleCollider2D blastCollider = _blast.AddComponent<CircleCollider2D>();
            blastCollider.radius = 2.5f;
            if (_pd.GetBool("equippedCharm_" + Charms.DeepFocus))
            {
                blastCollider.radius *= 2.5f / 1.5f;
            }

            blastCollider.offset = Vector3.up;
            blastCollider.isTrigger = true;
            Log("Adding DebugColliders");
            //_blast.AddComponent<DebugColliders>();
            Log("Adding DamageEnemies");
            DamageEnemies damageEnemies = _blast.AddComponent<DamageEnemies>();
            damageEnemies.damageDealt = 50;
            damageEnemies.attackType = AttackTypes.Spell;
            damageEnemies.ignoreInvuln = false;
            damageEnemies.enabled = true;
            Log("Playing AudioClip");
            AudioPlayerOneShotSingle("Burst", 1.5f, 1.5f);
            yield return new WaitForSeconds(0.1f);
            Destroy(_blast);
        }

        private void CancelBlast()
        {
            if (_blast != null) Destroy(_blast);
            _audio.Stop();
        }

        //Old method of setting Purity nail size, caused default nail swings to be too large
        /*private void ChangeSlashScale(float scaleX, float scaleY, float scaleZ, bool mantis = false)
        {
            Vector3 slashScale = new Vector3(scaleX, scaleY, scaleZ);

            foreach (NailSlash nailSlash in _nailSlashes)
            {
                nailSlash.SetMantis(mantis);
                nailSlash.scale = slashScale;
            }
        }*/

        //Old purity effect
        /*private void SetPuritySize()
        {
            
            foreach (NailSlash nailSlash in _nailSlashes)
            {
                if (nailSlash == null) break;
                switch (nailSlash.name)
                {
                    //Set nail size to 1.75x defaultz
                    case "Slash":
                        nailSlash.scale = new Vector3(2.835f, 2.879177f, 2.2362515f);
                        break;
                    case "AltSlash":
                        nailSlash.scale = new Vector3(2.19975f, 2.4892f, 1.9664575f);
                        break;
                    case "DownSlash":
                        nailSlash.scale = new Vector3(1.96875f, 1.974f, 1.75f);
                        break;
                    case "UpSlash":
                        nailSlash.scale = new Vector3(2.0125f, 2.45f, 2.179625f);
                        break;
                }
            }
        }
        private void RemovePuritySize()
        {
            
            foreach (NailSlash nailSlash in _nailSlashes)
            {
                if (nailSlash == null) break;
                switch (nailSlash.name)
                {
                    case "Slash":
                        nailSlash.scale = new Vector3(1.62f, 1.645244f, 1.277858f);
                        break;
                    case "AltSlash":
                        nailSlash.scale = new Vector3(1.257f, 1.4224f, 1.12369f);
                        break;
                    case "DownSlash":
                        nailSlash.scale = new Vector3(1.125f, 1.28f, 1f);
                        break;
                    case "UpSlash":
                        nailSlash.scale = new Vector3(1.15f, 1.4f, 1.2455f);
                        break;
                }
            }
        }*/

        private void AudioPlayerOneShotSingle(AudioClip clip, float pitchMin = 1.0f, float pitchMax = 1.0f, float time = 1.0f, float volume = 1.0f)
        {
            GameObject actorInstance = _audioPlayerActor.Spawn(HeroController.instance.transform.position, Quaternion.Euler(Vector3.up));
            AudioSource audio = actorInstance.GetComponent<AudioSource>();
            audio.pitch = Random.Range(pitchMin, pitchMax);
            audio.volume = volume;
            audio.PlayOneShot(clip);
        }

        private void AudioPlayerOneShotSingle(string clipName, float pitchMin = 1.0f, float pitchMax = 1.0f, float time = 1.0f, float volume = 1.0f)
        {
            AudioClip GetAudioClip()
            {
                switch (clipName)
                {
                    case "Burst":
                        return (AudioClip)_pvControl.GetAction<AudioPlayerOneShotSingle>("Focus Burst", 8).audioClip.Value;
                    case "Focus Charge":
                        return (AudioClip)_pvControl.GetAction<AudioPlayerOneShotSingle>("Focus Charge", 2).audioClip.Value;
                    case "Plume Up":
                        return (AudioClip)_pvControl.GetAction<AudioPlayerOneShotSingle>("Plume Up", 1).audioClip.Value;
                    case "Small Burst":
                        return (AudioClip)_blastControl.GetAction<AudioPlayerOneShotSingle>("Sound", 1).audioClip.Value;
                    default:
                        return null;
                }
            }

            AudioPlayerOneShotSingle(GetAudioClip(), pitchMin, pitchMax, time, volume);
        }

        private void OnDestroy()
        {
            On.HeroController.Awake -= On_HeroController_Awake;
            ModHooks.CharmUpdateHook -= CharmUpdate;
        }

        private static void Log(object message) => Modding.Logger.Log("[FiveKnights][Amulets] " + message);
    }
}
