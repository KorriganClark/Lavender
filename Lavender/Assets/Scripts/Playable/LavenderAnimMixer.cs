using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace Lavender
{
    public class LavenderAnimMixerPlayable : PlayableBehaviour
    {
        private double currentTime;
        private float totalTime = 0;
        /// <summary>
        /// 混合器
        /// </summary>
        private AnimationMixerPlayable animMixer;
        private List<LavenderClip> sortedClips;
        private List<LavenderClip> nowClips;
        private List<Playable> animPlayable;
        private Dictionary<Playable, int> playablePorts;
        private PlayableGraph playableGraph;
        public static ScriptPlayable<LavenderAnimMixerPlayable> Create(PlayableGraph graph, List<LavenderClip> animClips)
        {
            var lavenderMixer = ScriptPlayable<LavenderAnimMixerPlayable>.Create(graph, 1);
            lavenderMixer.SetPropagateSetTime(true);
            var behaviour = lavenderMixer.GetBehaviour();
            behaviour.animMixer = AnimationMixerPlayable.Create(graph, animClips.Count);
            behaviour.animMixer.SetPropagateSetTime(true);
            graph.Connect(behaviour.animMixer, 0, lavenderMixer, 0);
            lavenderMixer.SetInputWeight(0, 1.0f);
            //lavenderMixer.SetPropagateSetTime(true);
            behaviour.Init(graph, animClips);
            return lavenderMixer;
        }
        public void Init(PlayableGraph graph, List<LavenderClip> animClips)
        {
            sortedClips = new List<LavenderClip>();
            animPlayable = new List<Playable>();
            nowClips = new List<LavenderClip>();
            playablePorts = new Dictionary<Playable, int>();
            playableGraph = graph;

            foreach (var clip in animClips)
            {
                sortedClips.Add(clip);
            }
            sortedClips.Sort();
            foreach (var clip in sortedClips)
            {
                {
                    var newplayable = AnimationClipPlayable.Create(playableGraph, clip.GetAnimClip());
                    newplayable.SetSpeed(clip.rate);
                    newplayable.SetDuration(clip.length);
                    var port = sortedClips.IndexOf(clip);
                    playablePorts.Add(newplayable, port);
                    animPlayable.Add(newplayable);
                    playableGraph.Connect(newplayable, 0, animMixer, port);
                    animMixer.SetInputWeight(port, 0.0f);

                    totalTime = Mathf.Max(totalTime, clip.ScaledValidEnd);
                    animMixer.SetDuration(totalTime);
                }
            }
        }
        public float DynamicWeight(double currentTime, int index)
        {
            if (nowClips.Count > 2)
            {
                nowClips.RemoveAt(nowClips.Count - 1);
            }

            if (nowClips.Count == 1)
            {
                return 1.0f;
            }
            if (nowClips.Count == 0)
            {
                return 0;
            }
            LavenderClip firstclip = nowClips[0];
            LavenderClip secondclip = nowClips[1];
            firstclip.dynamicWeight = (float)(firstclip.weight * (firstclip.ScaledValidEnd - currentTime));
            secondclip.dynamicWeight = (float)(secondclip.weight * (currentTime - secondclip.ScaledValidStart));
            float totalWeight = firstclip.dynamicWeight + secondclip.dynamicWeight;
            if (index == 0)
            {
                return firstclip.dynamicWeight / totalWeight;
            }
            else
            {
                return secondclip.dynamicWeight / totalWeight;
            }
        }
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            currentTime = playable.GetTime();
//             if (nowClips.Count > 0)
//             {
//                 nowClips.Clear();
//             }
            for(int i = 0; i < animMixer.GetInputCount(); i++)
            {
                animMixer.SetInputWeight(i, 0);
            }
            
        }
        public override void OnGraphStart(Playable playable)
        {
            foreach (var clip in sortedClips)
            {
                totalTime = Mathf.Max(totalTime, clip.ScaledValidEnd);
            }
            playable.SetDuration(totalTime - Mathf.Epsilon);

        }
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            //playable.SetDuration(totalTime);
            currentTime = playable.GetTime();
            foreach (var clip in sortedClips)
            {

                if (clip.ContainsTime(currentTime) && !nowClips.Contains(clip))
                {
                    nowClips.Add(clip);
                    var port = sortedClips.IndexOf(clip);
                    animPlayable[port].SetTime(clip.ToLocalTime(currentTime));
                }
            }
            LavenderClip lastClip = null;
            foreach (var clip in sortedClips)
            {
                var port = sortedClips.IndexOf(clip);
                var localTime = clip.ToLocalTime(currentTime);

                if (!clip.ContainsTime(currentTime) && nowClips.Contains(clip))
                {
                    lastClip = clip;
                    nowClips.Remove(clip);
                    animMixer.SetInputWeight(port, 0.0f);
                }

                if (clip.ContainsTime(currentTime))
                {
                    var animPlayable = (AnimationClipPlayable)animMixer.GetInput(port);
                    animPlayable.SetTime(localTime);
                    animMixer.SetInputWeight(port, DynamicWeight(currentTime, nowClips.IndexOf(clip)));
                }

            }
            if (nowClips.Count == 0 && lastClip != null)
            {
                nowClips.Add(lastClip);
                var port = sortedClips.IndexOf(lastClip);
                var localTime = lastClip.ToLocalTime(currentTime);
                var animPlayable = (AnimationClipPlayable)animMixer.GetInput(port);
                animPlayable.SetTime(localTime);
                animMixer.SetInputWeight(port, DynamicWeight(currentTime, nowClips.IndexOf(lastClip)));
            }

        }
        //         public void PlayClip(PlayableGraph graph, LavenderClip clip, double beginTime, int port)
        //         {
        //             var newplayable = AnimationClipPlayable.Create(graph, clip.GetAnimClip());
        //             playablePorts.Add(newplayable, port);
        //             animPlayable.Add(newplayable);
        //             graph.Connect(newplayable, 0, animMixer, port);
        //             animMixer.SetInputWeight(port, 1.0f);
        //             newplayable.SetTime(clip.ToLocalTime(currentTime));
        //             newplayable.Play();
        //             LavenderTimer.AddEvent(() => RemovePlayable(newplayable), startTime: (float)(clip.ScaledValidEnd - beginTime), timerLength: 1f);
        //         }
        public void RemovePlayable(Playable playable)
        {
            animPlayable.Remove(playable);
            playable.Destroy();
        }
    }
}