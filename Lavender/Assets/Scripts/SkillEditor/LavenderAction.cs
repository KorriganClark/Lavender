using System;
using System.Collections.Generic;
using UnityEngine;
namespace Lavender
{

    public enum ActionType
    {
        Move,//用于Playable
        Speed,//用于状态机
        YSpeed,
        Skill,
        InputKey
    }
    public class LavenderAction
    {

        public object content;
        public List<object> contents;
        public float blendInTime = 0;
        /// <summary>
        /// 操作总时间，小于0为无限
        /// </summary>
        public float totalTime = 0;
        public ActionType type;
        public bool canBeInterrupt = true;
        public static LavenderAction MoveAtion(MoveState moveState, float blendIn = 0.1f, float total = -1)
        {
            var newAction = new LavenderAction();
            newAction.type = ActionType.Move;
            newAction.content = moveState;
            newAction.blendInTime = blendIn;
            newAction.totalTime = total;
            return newAction;
        }
        public static LavenderAction MoveAtion(MoveState mainMoveState, float firstWeight, MoveState secondMoveState, float secondWeight, float blendIn = 0.1f, float total = -1)
        {
            var action = new LavenderAction();
            action.type = ActionType.Move;
            action.content = mainMoveState;
            action.contents = new List<object>();
            action.contents.Add(mainMoveState);
            action.contents.Add(firstWeight);
            action.contents.Add(secondMoveState);
            action.contents.Add(secondWeight);
            action.blendInTime = blendIn;
            action.totalTime = total;
            return action;
        }

        public Dictionary<MoveState, float> GetMoveConfig
        {
            get
            {
                Dictionary<MoveState, float> res = new Dictionary<MoveState, float>();
                if (contents == null)
                {
                    res.Add((MoveState)content, 1);
                    return res;
                }
                res.Add((MoveState)contents[0], (float)contents[1]);
                res.Add((MoveState)contents[2], (float)contents[3]);
                return res;
            }
        }

        public static LavenderAction SpeedAction(float speed)
        {
            var newAction = new LavenderAction();
            newAction.type = ActionType.Speed;
            newAction.content = speed;
            return newAction;
        }
        public static LavenderAction YSpeedAction(float speed)
        {
            var newAction = new LavenderAction();
            newAction.type = ActionType.YSpeed;
            newAction.content = speed;
            return newAction;
        }
        public float GetSpeed
        {
            get
            {
                return (float)content;
            }
        }
        public float GetYSpeed
        {
            get
            {
                return (float)content;
            }
        }
        public static LavenderAction InputKeyAction(KeyCode key)
        {
            var newAction = new LavenderAction();
            newAction.type = ActionType.InputKey;
            newAction.content = key;
            return newAction;
        }
        public KeyCode GetInputKey
        {
            get { return (KeyCode)content; }
        }
        public Func<LavenderAction> CreateAction;
        //public delegate T test<T>();
        public Func<object> GetValue;
        public Dictionary<string, Func<object>> DetailUtility;
        public static LavenderAction SkillAction(bool SkillOn, SkillPlayableAsset skill = null)
        {
            var action = new LavenderAction();
            action.type = ActionType.Skill;
            action.content = SkillOn;
            action.contents = new List<object>();
            action.contents.Add(skill);
            return action;
        }
        public bool GetSkillAction
        {
            get { return (bool)content; }
        }
        public SkillPlayableAsset GetSkill
        {
            get { return (SkillPlayableAsset)contents[0]; }
        }
    }
}