using System.Reflection;
using Lavender;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System;

namespace Lavender
{
    public delegate void Handler(LavenderAction action);
    [ExecuteInEditMode]
    public class LavenderCharacterControl : MonoBehaviour
    {
        public CharacterStateMachine stateMachine;
        public SkillStateMachine skillStateMachine;
        public bool isPlayer = true;
        //public SkillPlayableAsset nextSkill;
        public Dictionary<KeyCode, SkillPlayableAsset> skills;
        public bool isOnSkill = false;
        public float walkSpeed = 2;
        public float runSpeed = 5;
        public float nowSpeed;
        public CharacterEditorControl editorControl;
        private CharacterController controller;
        private LavenderUnitMoveControl moveControl;
        private Vector3 playerVelocity;
        private bool isOnGround;
        private float playerSpeed = 2.0f;
        private float maxSpeed = 5f;
        private float jumpHeight = 1.0f;
        private float gravityValue = -9.81f;
        private PlayableDirector director;
        public GameObject camObj;
        // Start is called before the first frame update
        void Start()
        {
            director = GetComponent<PlayableDirector>();
            director.timeUpdateMode = DirectorUpdateMode.DSPClock;
            if (!Application.isPlaying)
            {
                director.timeUpdateMode = DirectorUpdateMode.Manual;
            }
            director.RebuildGraph();
            director.Play();
            controller = GetComponent<CharacterController>();
            moveControl = GetComponent<LavenderUnitMoveControl>();
            stateMachine = new CharacterStateMachine();
            skillStateMachine = new SkillStateMachine();
            stateMachine.HandleSwitch(this, LavenderAction.SpeedAction(0));
        }

        // Update is called once per frame
        void Update()
        {
            if (editorControl != null || !isPlayer)
            {
                //TryToMove();
                stateMachine.Update(this);
                director.Evaluate();
                director.playableGraph.Evaluate(Time.deltaTime);
                return;
            }
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                if (maxSpeed == 2.0f)
                {
                    maxSpeed = 5.0f;
                }
                else
                {
                    maxSpeed = 2f;
                }
            }
            var skillAction = new LavenderAction();
            SkillPlayableAsset skill = null;
            skillAction.CreateAction = () =>
            {
                var action = new LavenderAction();
                action.type = ActionType.Skill;
                action.content = true;
                action.contents = new List<object>();
                action.contents.Add(skill);
                action.DetailUtility = new Dictionary<string, Func<dynamic>>();
                action.GetValue = () =>
                {
                    return skill;
                };
                action.DetailUtility.Add(
                    "SkillOn",
                    () =>
                    {
                        return true;
                    }
                );
                return action;
            };
            //int skillNum = 0;
            Func<KeyCode, int> SetSkill = (skillNum) =>
             {
                 //if (skillNum < skills.Count)
                 {
                     skill = skills[skillNum];
                 }
                 return 0;
             };
            foreach (var pair in skills)
            {
                if (Input.GetKeyDown(pair.Key))
                {
                    SetSkill(pair.Key);//多此一举QWQ
                }
            }
            if (skill != null)
            {
                skillStateMachine.HandleSwitch(this, skillAction.CreateAction());
            }
            TryToMove();
            CameraMove();

            stateMachine.Update(this);
        }
        public Handler MoveAction;
        public Handler SkillAction;
        public Handler AnimAction;
        public Vector3 dir;
        void TryToMove()
        {
            if (!CanMove())
            {
                playerSpeed = 0;
                return;
            }
            if (camObj == null)
            {
                return;
            }
            //Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            Vector3 move = camObj.transform.forward * Input.GetAxis("Vertical") + camObj.transform.right * Input.GetAxis("Horizontal");
            move.y = 0;
            if (move != Vector3.zero)
            {
                //playerSpeed += 2f;
                playerSpeed = Mathf.Max(playerSpeed, maxSpeed);
                dir = move.normalized;
                gameObject.transform.forward = move;
            }

            playerSpeed = Mathf.Max(playerSpeed, 0);
            //controller.Move(dir * Time.deltaTime * playerSpeed);
            if (playerSpeed != 0)
            {
                moveControl.MoveRelative(dir * Time.deltaTime * playerSpeed);
            }
            nowSpeed = playerSpeed;
            playerSpeed -= 2f;
            stateMachine.HandleSwitch(this, LavenderAction.SpeedAction(nowSpeed));

            if (Input.GetKey(KeyCode.LeftShift))
            {
                moveControl.MoveRelative(gameObject.transform.forward * 2);
            }
            if (Input.GetKey(KeyCode.RightShift))
            {
                moveControl.MoveRelative(gameObject.transform.forward * 2, 1);
            }

            if (Input.GetButtonDown("Jump") && isOnGround)
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * -1.0f * gravityValue);
                stateMachine.HandleSwitch(this, LavenderAction.InputKeyAction(KeyCode.Space));
            }
            stateMachine.HandleSwitch(this, LavenderAction.YSpeedAction(playerVelocity.y));
            playerVelocity.y += gravityValue * Time.deltaTime;
            isOnGround = Physics.Raycast(gameObject.transform.position, Vector3.down, 0.1f);
            if (isOnGround && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;
            }
            if (playerVelocity.y != 0)
            {
                moveControl.MoveRelative(playerVelocity * Time.deltaTime);
            }
        }
        bool CanMove()
        {
            if (skillStateMachine.CurrentState.GetType() == typeof(SkillState))
            {
                return false;
            }
            if (stateMachine.CurrentState.GetType() == typeof(HitedState))
            {
                return false;
            }
            return true;
        }
        void CameraMove()
        {
            var camTransform = GetComponentInParent(typeof(Transform));
            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        }
    }
}