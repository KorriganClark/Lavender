using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;
namespace Lavender
{
    public class CharacterPlayable : PlayableBehaviour
    {
        public AnimationMixerPlayable mixer;
        public List<double> targetWeight;
        public List<double> currentWeight;
        public double blendRate;
        public double lastUpdatedTime;
        public static ScriptPlayable<CharacterPlayable> Create(PlayableGraph graph, CharacterMovePlayableAsset moveAsset, GameObject Model)
        {
            if (Model == null)
            {
                throw new ArgumentException("Target is null");
            }
            var playable = ScriptPlayable<CharacterPlayable>.Create(graph, 1);
            var behaviour = playable.GetBehaviour();
            behaviour.Compile(graph, moveAsset, playable, Model);
            return playable;
        }
        public void Compile(PlayableGraph graph, CharacterMovePlayableAsset moveAsset, Playable parentPlayable, GameObject Model)
        {
            var output = AnimationPlayableOutput.Create(graph, "anim", Model.GetComponent<Animator>());
            output.SetSourcePlayable(parentPlayable);
            output.SetWeight(1.0f);
            var audioOutPut = AudioPlayableOutput.Create(graph, "audio output", Model.GetComponent<AudioSource>());
            audioOutPut.SetSourcePlayable(parentPlayable);
            audioOutPut.SetWeight(1.0f);
            var scriptOutPut = ScriptPlayableOutput.Create(graph, "audio output");
            scriptOutPut.SetSourcePlayable(parentPlayable);
            scriptOutPut.SetWeight(1.0f);
            var movePlay = moveAsset.CreatePlayable(graph, Model);
            mixer = AnimationMixerPlayable.Create(graph, 2, true);
            graph.Connect(mixer, 0, parentPlayable, 0);
            parentPlayable.SetInputWeight(0, 1f);
            graph.Connect(movePlay, 0, mixer, 0);
            mixer.SetInputWeight(0, 1f);
            var skillPlay = SkillMgrPlayable.Create(graph, (ScriptPlayable<CharacterPlayable>)parentPlayable, Model);
            graph.Connect(skillPlay, 0, mixer, 1);
            mixer.SetInputWeight(1, 0.0f);
            targetWeight = new List<double>();
            currentWeight = new List<double>();
            targetWeight.Add(1);
            targetWeight.Add(0);
            currentWeight.Add(1);
            currentWeight.Add(0);
            var control = Model.GetComponent<LavenderCharacterControl>();
            control.AnimAction += PlayAnim;
        }
        public void SetWeight(double moveWeight, double skillWeight, double blendInTime)
        {
            double total = moveWeight + skillWeight;
            moveWeight = moveWeight / total;
            skillWeight = skillWeight / total;
            targetWeight[0] = moveWeight;
            targetWeight[1] = skillWeight;
            if (blendInTime > 0)
            {
                blendRate = 1 / blendInTime;
            }
            else
            {
                blendRate = 1000000;
            }
        }
        public void AdjustWeight(double deltaTime)
        {
            if (blendRate == 0)
            {
                return;
            }
            for (int i = 0; i < 2; i++)
            {
                if (blendRate * deltaTime * 0.1 > Math.Abs(currentWeight[i] - targetWeight[i]))
                {
                    currentWeight[i] = targetWeight[i];
                    blendRate = 0;
                }
                else
                {
                    currentWeight[i] += deltaTime * (targetWeight[i] - currentWeight[i]) * blendRate;
                    currentWeight[i] = Mathf.Clamp((float)currentWeight[i], 0f, 1f);
                }
                mixer.SetInputWeight(i, (float)currentWeight[i]);
            }
        }
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            double deltaTime = playable.GetTime() - lastUpdatedTime;
            lastUpdatedTime = playable.GetTime();
            if (deltaTime > 1)
            {
                return;
            }
            if (mixer.GetInputCount() == 3)
            {
                mixer.SetInputWeight(0, 0);
                mixer.SetInputWeight(1, 0);
                mixer.SetInputWeight(2, 1);
            }
            else
            {
                AdjustWeight(deltaTime);
            }
        }
        public void PlayAnim(LavenderAction action)
        {
            var animation = (AnimationClip)action.GetValue();
            var playable = AnimationClipPlayable.Create(mixer.GetGraph(), animation);
            mixer.SetInputCount(3);
            var graph = mixer.GetGraph();
            graph.Disconnect(mixer, 2);
            graph.Connect(playable, 0, mixer, 2);
            LavenderTimer.Instance.AddEvent(() =>
            {
                playable.Destroy();
                mixer.SetInputCount(2);
            }, animation.length);
        }
    }
}