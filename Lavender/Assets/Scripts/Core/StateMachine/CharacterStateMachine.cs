using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lavender
{
    public class CharacterStateMachine
    {
        private List<IMoveState> currentStates;
        public IMoveState CurrentState
        {
            get
            {
                if (currentStates == null || currentStates.Count == 0)
                {
                    currentStates = new List<IMoveState>();
                    currentStates.Add(new IdleState());
                }
                return currentStates[0];
            }
            set
            {
                if (currentStates == null)
                {
                    currentStates = new List<IMoveState>();
                }
                currentStates.Insert(0, value);
                while (currentStates.Count > 5)
                {
                    currentStates.RemoveAt(5);
                }
            }
        }
        public void HandleSwitch(LavenderCharacterControl control, LavenderAction action)
        {
            var newState = CurrentState.HandleSwitch(control, action);
            if (newState != CurrentState)
            {
                CurrentState = newState;
                CurrentState.Enter(control);
            }
            if (currentStates.Count == 1)
            {
                CurrentState.Enter(control);
            }
        }
        public void Update(LavenderCharacterControl control)
        {
            CurrentState.Update(control);
        }
    }
}