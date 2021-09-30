using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lavender;
namespace Lavender.Cardinal
{
    public class CardinalSystem : MonoBehaviour
    {
        private float health;
        private float attack;
        public SkillSystem skillSystem;
        public AnimationClip beAttackAnim;
        void Start()
        {
            skillSystem = SkillSystem.CreateSkillSystem(this);
        }


        // Update is called once per frame
        void Update()
        {

        }

        public void Init(LavenderCharacter config)
        {
            health = config.health;
            attack = config.attack;
        }

        public void ApplyAttackTo(GameObject target)
        {
            if (target == null || target.GetComponent<CardinalSystem>() == null)
            {
                return;
            }
            var targetCardinal = target.GetComponent<CardinalSystem>();
            if (targetCardinal == this)
            {
                return;
            }
            targetCardinal.BeAttacked(attack);
        }
        public void BeAttacked(float damage)
        {
            health -= damage;
            BeAttackedAnim();
            if (health < 0)
            {
                Die();
            }

        }
        public void BeAttackedAnim()
        {
            var control = gameObject.GetComponent<LavenderCharacterControl>();
            var action = new LavenderAction();
            action.GetValue = () =>
            {
                return "Hited";
            };
            control.stateMachine.HandleSwitch(control, action);
        }
        public void Die()
        {

        }
    }
}