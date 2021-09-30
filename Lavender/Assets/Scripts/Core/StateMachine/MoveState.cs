using System;
using UnityEngine;

namespace Lavender
{
    public interface IMoveState
    {
        //event Handler MoveAction;
        IMoveState HandleSwitch(LavenderCharacterControl controller, LavenderAction action);
        void Update(LavenderCharacterControl controller);
        void Enter(LavenderCharacterControl controller);
        void Exit(LavenderCharacterControl controller);
    }
    public class IdleState : IMoveState
    {
        public IMoveState HandleSwitch(LavenderCharacterControl controller, LavenderAction action)
        {
            if (action == null)
            {
                return this;
            }
            if (action.type == ActionType.Speed)
            {
                var speed = action.GetSpeed;
                if (speed <= 0)
                {
                    return this;
                }
                else if (speed <= controller.walkSpeed)
                {
                    return new WalkState();
                }
                else
                {
                    return new RunState();
                }
            }
            if (action.type == ActionType.InputKey && action.GetInputKey == KeyCode.Space)
            {
                return new JumpState();
            }
            if (action.GetValue != null && (string)action.GetValue() == "Hited")
            {
                //Enter(controller);
                return new HitedState();
            }
            return this;
        }
        public void Update(LavenderCharacterControl controller)
        {

        }
        public void Enter(LavenderCharacterControl controller)
        {
            //Debug.Log("Idle");
            //Move(controller.MoveAction);
            controller.MoveAction(LavenderAction.MoveAtion(MoveState.Idle));
        }
        public void Move(Handler action)
        {
            action(LavenderAction.MoveAtion(MoveState.Idle));
        }
        public void Exit(LavenderCharacterControl controller)
        {

        }
    }

    public class WalkState : IMoveState
    {
        public float speed;
        public IMoveState HandleSwitch(LavenderCharacterControl controller, LavenderAction action)
        {
            if (action == null)
            {
                return this;
            }
            if (action.type == ActionType.Speed)
            {
                speed = action.GetSpeed;
                if (speed <= 0)
                {
                    return new IdleState();
                }
                else if (speed <= controller.walkSpeed)
                {
                    return this;
                }
                else
                {
                    return new RunState();
                }
            }
            if (action.type == ActionType.InputKey && action.GetInputKey == KeyCode.Space)
            {
                return new JumpState();
            }
            return this;
        }
        public void Update(LavenderCharacterControl controller)
        {
            speed = controller.nowSpeed;
            float weight = (speed + 1) / (controller.walkSpeed + 1);
            controller.MoveAction(LavenderAction.MoveAtion(MoveState.Walk, weight, MoveState.Idle, 1 - weight));
        }
        public void Enter(LavenderCharacterControl controller)
        {
            //Debug.Log("Walk");
        }
        public void Exit(LavenderCharacterControl controller)
        {

        }
    }
    public class RunState : IMoveState
    {
        public float speed;
        public IMoveState HandleSwitch(LavenderCharacterControl controller, LavenderAction action)
        {
            if (action == null)
            {
                return this;
            }
            if (action.type == ActionType.Speed)
            {
                speed = action.GetSpeed;
                if (speed <= 0)
                {
                    return new IdleState();
                }
                else if (speed <= controller.walkSpeed)
                {
                    return new WalkState();
                }
                else
                {
                    return this;
                }
            }
            if (action.type == ActionType.InputKey && action.GetInputKey == KeyCode.Space)
            {
                return new JumpState();
            }
            return this;
        }
        public void Update(LavenderCharacterControl controller)
        {
            speed = controller.nowSpeed;
            float weight = (speed - 3 / controller.runSpeed - 3);
            //controller.MoveAction(LavenderAction.MoveAtion(MoveState.Run, weight, MoveState.Walk, 1 - weight));
            controller.MoveAction(LavenderAction.MoveAtion(MoveState.Run));
        }
        public void Enter(LavenderCharacterControl controller)
        {
            //Debug.Log("Run");
        }
        public void Exit(LavenderCharacterControl controller)
        {

        }
    }
    public class JumpState : IMoveState
    {
        public IMoveState HandleSwitch(LavenderCharacterControl controller, LavenderAction action)
        {
            if (action == null)
            {
                return this;
            }
            if (action.type == ActionType.YSpeed)
            {
                var speed = action.GetYSpeed;
                if (speed == 0)
                {
                    return new IdleState();
                }
            }
            return this;
        }
        public void Update(LavenderCharacterControl controller)
        {
            //controller.TryMoveAction(LavenderAction.MoveAtion(MoveState.Jump));
        }
        public void Enter(LavenderCharacterControl controller)
        {
            controller.MoveAction(LavenderAction.MoveAtion(MoveState.Jump));
        }
        public void Exit(LavenderCharacterControl controller)
        {

        }
    }
    public class HitedState : IMoveState
    {
        public float HitedTime = 0.667f;
        public float restTime;
        public IMoveState HandleSwitch(LavenderCharacterControl controller, LavenderAction action)
        {
            if (action == null)
            {
                return this;
            }
            if (action.GetValue != null && (string)action.GetValue() == "Hited")
            {
                Enter(controller);
                return this;
            }
            if (restTime < 0 && action.type == ActionType.Speed)
            {
                var speed = action.GetSpeed;
                //if (speed == 0)
                {
                    return new IdleState();
                }
            }
            return this;
        }
        public void Update(LavenderCharacterControl controller)
        {
            restTime -= Time.deltaTime;
            if (restTime < 0)
            {
                controller.stateMachine.HandleSwitch(controller, LavenderAction.SpeedAction(0));
            }
        }
        public void Enter(LavenderCharacterControl controller)
        {
            controller.MoveAction(LavenderAction.MoveAtion(MoveState.Hited));
            restTime = HitedTime;
        }
        public void Exit(LavenderCharacterControl controller)
        {

        }
    }
}