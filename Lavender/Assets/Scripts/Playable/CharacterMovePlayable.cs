using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System;
using System.Runtime.InteropServices;
// A behaviour that is attached to a playable
namespace Lavender
{
    public enum MoveState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Hited,
        FallDown,
        GetUp,
        Die
    }
    public class CharacterMovePlayable : PlayableBehaviour
    {
        public MoveState currentState;
        public MoveState nextState;
        public int stateNum;
        public LavenderAction nextAction;
        public Dictionary<MoveState, Playable> playables;
        private AnimationMixerPlayable mixer;
        private GameObject character;
        private List<double> weights;
        private List<double> targetWeights;
        private double blendRate;

        private bool CanChange = true;
        private double lastUpdatedTime;
        public static ScriptPlayable<CharacterMovePlayable> Create(PlayableGraph graph, List<AnimationClip> moveClips, GameObject go)
        {
            var movePlayable = ScriptPlayable<CharacterMovePlayable>.Create(graph, 1);
            var behaviour = movePlayable.GetBehaviour();
            behaviour.Initialize(graph, movePlayable, moveClips, go);
            return movePlayable;
        }
        public void Initialize(PlayableGraph graph, Playable parentPlayable, List<AnimationClip> moveClips, GameObject go)
        {
            //var count = System.Enum.GetNames(typeof(State)).Length;
            character = go;
            stateNum = System.Enum.GetNames(typeof(MoveState)).Length;
            mixer = AnimationMixerPlayable.Create(graph, stateNum);
            graph.Connect(mixer, 0, parentPlayable, 0);
            parentPlayable.SetInputWeight(0, 1);
            weights = new List<double>();
            targetWeights = new List<double>();
            playables = new Dictionary<MoveState, Playable>();
            CompileMove(graph, mixer, moveClips, go);
            if (graph.GetOutputCountByType<AnimationPlayableOutput>() == 0)
            {
                CreateOutput(graph, parentPlayable, go);
            }
            if (character == null)
            {
                return;
            }
            var controller = character.GetComponent<LavenderCharacterControl>();
            controller.MoveAction += AddAction;
            if (controller.editorControl)
            {
                controller.editorControl.MoveAction += AddAction;
            }
        }
        public void CompileMove(PlayableGraph graph, AnimationMixerPlayable mixer, List<AnimationClip> moveClips, GameObject go)
        {
            while (moveClips.Count > stateNum)
            {
                moveClips.RemoveAt(moveClips.Count - 1);
            }
            /*
                        if (moveClips.Count < stateNum)
                        {
                            throw new ArgumentException("Move Anims Not Found");
                        }*/
            //foreach (var clip in moveClips)
            for (int i = 0; i < stateNum; i++)
            {
                weights.Add(0);
                targetWeights.Add(0);
                if (i >= moveClips.Count)
                {
                    continue;
                }
                var clip = moveClips[i];
                if (clip == null)
                {
                    continue;
                }
                var playable = AnimationClipPlayable.Create(graph, clip);
                playables.Add((MoveState)i, playable);
                graph.Connect(playable, 0, mixer, moveClips.IndexOf(clip));

                if (moveClips.IndexOf(clip) == 2)
                {
                    playable.SetSpeed(0.5714285714);
                }
                //mixer.AddInput(playable, moveClips.IndexOf(clip));
            }
        }
        /// <summary>
        /// 如果没有Output就创造一个
        /// TODO：在工具类里面集成
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="parentPlayable"></param>
        /// <param name="go"></param>
        public void CreateOutput(PlayableGraph graph, Playable parentPlayable, GameObject go)
        {
            var output = AnimationPlayableOutput.Create(graph, "Anim", go.GetComponent<Animator>());
            output.SetSourcePlayable(parentPlayable);
            output.SetWeight(1.0f);
        }

        public void AddAction(LavenderAction action)
        {
            nextAction = action;
        }
        public void DealAction()
        {
            if (nextAction.blendInTime != 0)
            {
                blendRate = 1 / nextAction.blendInTime;
            }
            else
            {
                blendRate = 10000000;
            }
            var dir = nextAction.GetMoveConfig;
            for (int i = 0; i < targetWeights.Count; i++)
            {
                var state = (MoveState)i;
                float value;
                if (dir.TryGetValue(state, out value))
                {
                    targetWeights[i] = value;
                    if (state == MoveState.Jump || state == MoveState.Hited || state == MoveState.GetUp || state == MoveState.Die)
                    {
                        playables[state].SetTime(0);
                    }
                }
                else
                {
                    targetWeights[i] = 0;
                }
            }
            currentState = (MoveState)nextAction.content;
            nextAction = null;
        }
        public void AdjustWeight(Playable playable)
        {
            var deltaTime = playable.GetTime() - lastUpdatedTime;
            lastUpdatedTime = playable.GetTime();
            for (int i = 0; i < targetWeights.Count; i++)
            {
                if (blendRate * deltaTime <= deltaTime)
                {
                    weights[i] = targetWeights[i];
                    blendRate = 0;
                }
                else
                {
                    weights[i] += deltaTime * (targetWeights[i] - weights[i]) * blendRate;
                    weights[i] = Mathf.Clamp((float)weights[i], 0f, 1f);
                }
            }
        }
        // Called when the owning graph starts playing
        public override void OnGraphStart(Playable playable)
        {
            currentState = MoveState.Idle;
            //targetWeights[(int)currentState] = 1;
        }

        // Called when the owning graph stops playing
        public override void OnGraphStop(Playable playable)
        {

        }

        // Called when the state of the playable is set to Play
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {

        }

        // Called when the state of the playable is set to Paused
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {

        }

        // Called each frame while the state is set to Play
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (character == null)
            {
                return;
            }
            var controller = character.GetComponent<LavenderCharacterControl>();
            if (CanChange && nextAction != null)
            {
                DealAction();
            }
            AdjustWeight(playable);
            for (int i = 0; i < mixer.GetInputCount(); i++)
            {
                if (weights.Count <= i)
                {
                    break;
                }
                mixer.SetInputWeight(i, (float)weights[i]);
            }
        }
    }
}