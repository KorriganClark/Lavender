using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using UnityEngine.Playables;

namespace Lavender
{
    public class CharacterEditorControl : OdinEditorWindow // SerializedObject
    {

        public enum moveState
        {
            静止,
            行走,
            跑步,
            跳跃,
            受击,
            起身,
            死亡,
            坠落
        }
        [EnumToggleButtons, HideLabel]
        [OnValueChanged("ChangeState")]
        public moveState currentMoveState;
        [HideInInspector]
        public Handler MoveAction;
        [HideInInspector]
        public Handler SkillAction;
        [TableList(AlwaysExpanded = true, DrawScrollView = false)]
        public List<SkillItem> skills;
        public void ReleaseSkill(SkillPlayableAsset skill)
        {
            var action = new LavenderAction();
            action.GetValue += () =>
            {
                return skill;
            };
            action.DetailUtility = new Dictionary<string, Func<object>>();
            action.DetailUtility.Add(
                "SkillOn",
                () =>
                {
                    return true;
                }
            );
            SkillAction(action);
        }
        public class SkillItem
        {
            [ReadOnly]
            public SkillPlayableAsset skill;
            [ReadOnly]
            public string SkillName;
            [ReadOnly]
            public KeyCode input;
            [HideInInspector]
            public CharacterEditorControl control;
            [Button("展示")]
            public void ReleaseSkill()
            {
                control.ReleaseSkill(skill);
            }
            public SkillItem(SkillPlayableAsset skillAsset, KeyCode key, CharacterEditorControl editorControl)
            {
                skill = skillAsset;
                input = key;
                SkillName = skill.name;
                control = editorControl;
            }
        }
        //[ValueDropdown("skills")]
        //public SkillPlayableAsset skill;
        PlayableDirector director;
        LavenderCharacter characterConfig;
        public void Init(GameObject model, LavenderCharacter character)
        {
            characterConfig = character;
            director = model.GetComponent<PlayableDirector>();
            if (director == null)
            {
                director = model.AddComponent<PlayableDirector>();
                //director.playableAsset=
            }
            director.playableAsset = characterConfig.characterPlayableAsset;
            director.RebuildGraph();
            director.Play();
            Idle();
            skills = new List<SkillItem>();
        }
        public void Refresh()
        {
            if (director)
            {
                director.playableAsset = characterConfig.characterPlayableAsset;
                director.RebuildGraph();
                director.Play();
                Idle();
            }
        }
        public void ImportSkill(Dictionary<KeyCode, SkillPlayableAsset> skillsDic)
        {
            if (skillsDic == null)
            {
                return;
            }
            skills.Clear();
            foreach (var skill in skillsDic)
            {
                skills.Add(new SkillItem(skill.Value, skill.Key, this));
            }
        }
        public void ChangeState()
        {
            switch (currentMoveState)
            {
                case moveState.静止: Idle(); break;
                case moveState.行走: Walk(); break;
                case moveState.跑步: Run(); break;
                case moveState.跳跃: Jump(); break;
                case moveState.受击: Hited(); break;
                case moveState.起身: GetUp(); break;
                case moveState.死亡: Die(); break;
                case moveState.坠落: FallDown(); break;
            }
        }
        public void Idle()
        {
            MoveAction(LavenderAction.MoveAtion(MoveState.Idle));
        }
        public void Walk()
        {
            MoveAction(LavenderAction.MoveAtion(MoveState.Walk));
        }
        public void Run()
        {
            MoveAction(LavenderAction.MoveAtion(MoveState.Run));
        }
        public void Jump()
        {
            MoveAction(LavenderAction.MoveAtion(MoveState.Jump));
            new LavenderTimer.Builder().SetStartTime(0.9f).CreateTimer(() =>
            {
                currentMoveState = moveState.静止;
                Idle();
            });
        }
        public void Hited()
        {
            MoveAction(LavenderAction.MoveAtion(MoveState.Hited));
            new LavenderTimer.Builder().SetStartTime(0.9f).CreateTimer(() =>
            {
                currentMoveState = moveState.静止;
                Idle();
            });
        }
        public void GetUp()
        {
            MoveAction(LavenderAction.MoveAtion(MoveState.GetUp));
            new LavenderTimer.Builder().SetStartTime(0.9f).CreateTimer(() =>
            {
                currentMoveState = moveState.静止;
                Idle();
            });
        }
        public void Die()
        {
            MoveAction(LavenderAction.MoveAtion(MoveState.Die));
            new LavenderTimer.Builder().SetStartTime(2f).CreateTimer(() =>
            {
                currentMoveState = moveState.起身;
                GetUp();
            });
        }
        public void FallDown()
        {
            MoveAction(LavenderAction.MoveAtion(MoveState.FallDown));
            new LavenderTimer.Builder().SetStartTime(2f).CreateTimer(() =>
            {
                currentMoveState = moveState.起身;
                GetUp();
            });
        }

        public void Draw()
        {
            OnGUI();
        }
        protected override void OnEndDrawEditors()
        {
            base.OnEndDrawEditors();
        }
    }
}