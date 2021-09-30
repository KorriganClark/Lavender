using System.Runtime.InteropServices;
using System;
using UnityEngine;
using Lavender.Cardinal;

namespace Lavender
{
    public interface ISkillState
    {
        //event Handler MoveAction;
        ISkillState HandleSwitch(LavenderCharacterControl controller, LavenderAction action);
        void Update(LavenderCharacterControl controller);
        void Enter(LavenderCharacterControl controller);
        void Exit(LavenderCharacterControl controller);
    }
    public class DefaultState : ISkillState
    {
        public ISkillState HandleSwitch(LavenderCharacterControl controller, LavenderAction action)
        {
            if (action != null && action.type == ActionType.Skill)
            {
                if (action.GetSkillAction == true)
                {
                    var newState = new SkillState();
                    newState.skill = action;
                    return newState;
                }
            }
            return this;
        }
        public void Update(LavenderCharacterControl controller)
        {

        }
        public void Enter(LavenderCharacterControl controller)
        {
            //Debug.Log("Idle");
            //controller.TryMoveAction(LavenderAction.MoveAtion(MoveState.Idle));
        }
        public void Exit(LavenderCharacterControl controller)
        {
            var moveState = controller.stateMachine;
            //移动状态机切至技能态
        }
    }

    public class SkillState : ISkillState
    {
        public float speed;
        public LavenderAction skill;
        public ISkillState HandleSwitch(LavenderCharacterControl controller, LavenderAction action)
        {
            if (action != null && action.type == ActionType.Skill)
            {
                if (action.GetSkillAction == false)
                {
                    return new DefaultState();
                }
            }
            return this;
        }
        public void Update(LavenderCharacterControl controller)
        {

        }
        public void Enter(LavenderCharacterControl controller)
        {
            controller.isOnSkill = true;
            //controller.TryReleaseAction(skill);
            controller.SkillAction(skill);
            var cardinal = controller.GetComponentInParent<CardinalSystem>();
            if (cardinal)
            {
                var skillSystem = cardinal.skillSystem;
                skillSystem.ExecuteNewSkill(((SkillPlayableAsset)skill.GetValue()).script);
            }
            controller.stateMachine.HandleSwitch(controller, LavenderAction.SpeedAction(0));
        }
        public void Exit(LavenderCharacterControl controller)
        {
            controller.isOnSkill = false;
        }
    }
}