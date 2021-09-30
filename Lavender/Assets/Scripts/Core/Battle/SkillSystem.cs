using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System;

namespace Lavender.Cardinal
{
    public class SkillSystem
    {
        private List<ISkillScript> ownedSkills;
        private ISkillScript currentSkill;
        private ISkillScript preSkill;
        private ISkillScript nextSkill;
        private CardinalSystem ownerSystem;
        private event Action SkillExcute;
        private event Action SkillStage;
        public static SkillSystem CreateSkillSystem(CardinalSystem system)
        {
            var res = new SkillSystem();
            res.Init(system);
            return res;
        }
        public void Init(CardinalSystem system)
        {
            ownerSystem = system;
            SkillBegin += () =>
            {
                var control = ownerSystem.GetComponentInParent<LavenderCharacterControl>();
                var skillAction = new LavenderAction();
                skillAction.GetValue = () =>
                {
                    return currentSkill == null ? null : currentSkill.SkillAsset;
                };
                control.SkillAction(skillAction);
            };
        }
        public void AddSkill(ISkillScript skill)
        {
            ownedSkills.Add(skill);
        }
        public List<ISkillScript> OwnedSkills
        {
            set
            {
                ownedSkills = value;
            }
            get
            {
                return ownedSkills;
            }
        }

        public void ExecuteNewSkill(ISkillScript skill, bool force = false)
        {
            if (!ownedSkills.Contains(skill) && !force)
            {
                Debug.Log("该角色未拥有此技能！");
                return;
            }
            if (currentSkill != null && currentSkill != skill)
            {
                SkillBegin -= currentSkill.OnSkillBegin;
                SkillStop -= currentSkill.OnSkillStop;
                SkillSucceed -= currentSkill.OnSkillSucceed;
            }
            currentSkill = skill;
            SkillBegin += skill.OnSkillBegin;
            SkillStop += skill.OnSkillStop;
            SkillSucceed += skill.OnSkillSucceed;
            ExecuteSkill();
        }
        public event Action SkillBegin = () => { };
        public event Action SkillStop = () => { };
        public event Action SkillSucceed = () => { };
        public List<Action> skillChannels = new List<Action>();
        public void ExecuteSkill()
        {
            SkillBegin();
            float delay = 0;
            if (currentSkill != null)
            {
                delay = currentSkill.SkillTime;
            }
            LavenderTimer.Instance.AddEvent(SkillStop, delay);
        }
    }
}