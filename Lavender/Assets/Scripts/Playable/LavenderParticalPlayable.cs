using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using static UnityEngine.ParticleSystem;

namespace Lavender
{
    public class LavenderParticalPlayable : UnityEngine.Timeline.ParticleControlPlayable
    {
        ParticleSystem ps;
        LavenderClip particalClip;
        public bool enabled = false;
        public static ScriptPlayable<LavenderParticalPlayable> LavenderCreate(PlayableGraph graph, LavenderClip clip, uint randomSeed)
        {
            if (clip.prefabClip == null || !graph.IsValid())
                return ScriptPlayable<LavenderParticalPlayable>.Null;

            var playable = ScriptPlayable<LavenderParticalPlayable>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.Init(graph, clip, randomSeed);

            //ps.sim
            //ps.Stop();
            playable.GetBehaviour().Initialize(behaviour.ps, randomSeed);
            return playable;
        }
        public void Init(PlayableGraph graph, LavenderClip clip, uint randomSeed)
        {
            var component = clip.prefabClip;
            particalClip = clip;
            if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(component))
            {
                ps = component;
            }
            else
            {
                ps = ParticleSystem.Instantiate(component);
                ps.transform.SetParent(clip.lavenderTransForm);
                if (clip.lavenderTransForm)
                {
                    ps.transform.position = clip.lavenderTransForm.position;
                }
            }
            StopLoop(ps);
            ps.simulationSpace = ParticleSystemSimulationSpace.World;
        }
        public static void StopLoop(ParticleSystem ps)
        {
            MainModule pm = ps.main;
            pm.loop = false;
            for (int i = 0; i < ps.transform.childCount; i++)
            {
                StopLoop(ps.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>());
            }
        }
        public void ModifyEmission(bool enable, ParticleSystem ps = null)
        {
//             if (enabled == enable)
//             {
//                 return;
//             }
            enabled = enable;
            if (ps == null)
            {
                ps = particleSystem;
            }
            EmissionModule em = ps.emission;
            em.enabled = enable;
            for (int i = 0; i < ps.transform.childCount; i++)
            {
                ModifyEmission(enable, ps.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>());
            }
        }
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
        }
        public override void PrepareFrame(Playable playable, FrameData data)
        {
            //var currentTime = playable.GetTime();
            //if(!particalClip.ContainsTime(currentTime))
            base.PrepareFrame(playable, data);
        }
        public override void OnPlayableDestroy(Playable playable)
        {
            if (ps)
            {
                ps.Clear();
                ps.enableEmission = false;
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(ps);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(ps);
                }
            }
        }
    }
}

