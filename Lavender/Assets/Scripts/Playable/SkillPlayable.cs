using System;
using System.Collections.Generic;
using Lavender.Cardinal;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;
namespace Lavender
{
    public class SkillPlayable : PlayableBehaviour
    {
        public float totalTime = 1000000;
        /// <summary>
        /// 创建SkillPlayable的一个实例
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="animClips"></param>
        /// <param name="audioClips"></param>
        /// <param name="particleClips"></param>
        /// <param name="Model"></param>
        /// <param name="createOutput"></param>
        /// <returns></returns>
        public static ScriptPlayable<SkillPlayable> Create(PlayableGraph graph, SkillPlayableAsset asset, float totalTime, GameObject Model)
        {
            if (Model == null || UnityEditor.PrefabUtility.IsPartOfAnyPrefab(Model))
            {
                throw new ArgumentException("Target is null");
            }
            var playable = ScriptPlayable<SkillPlayable>.Create(graph, 4);//创建出来时就会调用一次prepareframe
            var behaviour = playable.GetBehaviour();
            playable.SetDuration(totalTime);

            behaviour.totalTime = totalTime;
            behaviour.Compile(graph, playable, asset, Model);
            return playable;
        }
        /// <summary>
        /// 构造动画、音频、特效Playable
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="parentPlayable"></param>
        /// <param name="animClips"></param>
        /// <param name="audioClips"></param>
        /// <param name="particleClips"></param>
        /// <param name="Model"></param>
        public void Compile(PlayableGraph graph, ScriptPlayable<SkillPlayable> parentPlayable, SkillPlayableAsset asset, GameObject Model)
        {
            if (Model == null)
            {
                throw new ArgumentException("Target is null");
            }
            parentPlayable.SetPropagateSetTime(true);
            CompileAnim(graph, parentPlayable, asset.lavenderAnimClips, Model);
            CompileAudio(graph, parentPlayable, asset.lavenderAudioClips, Model);
            CompileParticle(graph, parentPlayable, asset.lavenderParticleClips, Model);
            CompileAttack(graph, parentPlayable, asset.lavenderAttackClips, Model);
            CompileMove(graph, parentPlayable, asset.lavenderMoveClips, Model);
        }
        public void CompileAnim(PlayableGraph graph, ScriptPlayable<SkillPlayable> parentPlayable, List<LavenderClip> animClips, GameObject Model)
        {
            var mixer = LavenderAnimMixerPlayable.Create(graph, animClips);
            graph.Connect(mixer, 0, parentPlayable, 0);
            parentPlayable.SetInputWeight(0, 1.0f);
            //parentPlayable.SetOutputCount(1);
            if (graph.GetOutputCountByType<AnimationPlayableOutput>() == 0)
            {
                var output = AnimationPlayableOutput.Create(graph, "Anim", Model.GetComponent<Animator>());
                output.SetSourcePlayable(parentPlayable);
                output.SetWeight(1.0f);
            }
        }
        public void CompileAudio(PlayableGraph graph, ScriptPlayable<SkillPlayable> parentPlayable, List<LavenderClip> audioClips, GameObject Model)
        {
            var mixer = Playable.Create(graph);//AudioMixerPlayable.Create(graph, 0);
            mixer.SetPropagateSetTime(true);
            graph.Connect(mixer, 0, parentPlayable, 1);
            parentPlayable.SetInputWeight(1, 1.0f);
            if (graph.GetOutputCountByType<AudioPlayableOutput>() == 0)
            {
                //var output = AudioPlayableOutput.Create(graph, "Audio", Model.GetComponent<AudioSource>());
                //output.SetSourcePlayable(parentPlayable);
                //output.SetWeight(1.0f);
            }
            foreach (var clip in audioClips)
            {
                //new LavenderTimer.Builder().SetStartTime(clip.ScaledValidStart).CreateTimer(() => PlaySound(mixer, clip, Model));
                PlaySound(mixer, clip, Model);
            }
        }
        public void CompileParticle(PlayableGraph graph, ScriptPlayable<SkillPlayable> parentPlayable, List<LavenderClip> particleClip, GameObject Model)
        {
            var joints = new Dictionary<string, Transform>();
            LoadJointsFromObj(Model.transform, joints);
            foreach (var clip in particleClip)
            {
                clip.joints = joints;
                clip.GetTransformByName();
                //new LavenderTimer.Builder().SetStartTime(clip.ScaledValidStart).CreateTimer(() => PlayParticle(graph, mixer, clip, particleClip.IndexOf(clip)));
                //PlayParticle(graph, mixer, clip, particleClip.IndexOf(clip));
            }
            var director = Model.GetComponent<PlayableDirector>();
            var particleControl = ParticleControlPlayable.CreatePlayable(graph, particleClip, parentPlayable, director);
            //var mixer = Playable.Create(graph, particleClip.Count);
            graph.Connect(particleControl, 0, parentPlayable, 2);
            parentPlayable.SetInputWeight(2, 1f);
        }
        public void CompileAttack(PlayableGraph graph, ScriptPlayable<SkillPlayable> parentPlayable, List<LavenderClip> attackClip, GameObject Model)
        {
            var joints = new Dictionary<string, Transform>();
            LoadJointsFromObj(Model.transform, joints);
            foreach (var clip in attackClip)
            {
                clip.joints = joints;
                clip.GetTransformByName();
                clip.attackClip.Init(clip.lavenderTransForm);
            }
            var director = Model.GetComponent<PlayableDirector>();
            var attackControl = AttackMgrPlayable.CreatePlayable(graph, attackClip, parentPlayable, director);
            //var mixer = Playable.Create(graph, particleClip.Count);
            graph.Connect(attackControl, 0, parentPlayable, 3);
            parentPlayable.SetInputWeight(3, 1f);
        }
        public void CompileMove(PlayableGraph graph, ScriptPlayable<SkillPlayable> parentPlayable, List<LavenderClip> moveClip, GameObject Model)
        {
            var joints = new Dictionary<string, Transform>();
            LoadJointsFromObj(Model.transform, joints);
            foreach (var clip in moveClip)
            {
                clip.joints = joints;
                clip.GetTransformByName();
            }
            var director = Model.GetComponent<PlayableDirector>();
            var moveControl = SkillMoveMgrPlayable.CreatePlayable(graph, moveClip, parentPlayable, director);
            //var mixer = Playable.Create(graph, particleClip.Count);
            parentPlayable.SetInputCount(5);
            graph.Connect(moveControl, 0, parentPlayable, 4);
            parentPlayable.SetInputWeight(4, 1f);
        }
        public void PlayParticle(PlayableGraph graph, Playable mixer, LavenderClip clip, int index)
        {
            var playable = LavenderParticalPlayable.LavenderCreate(graph, clip, 1111);
            //playable.SetTime(clip.cutStartTime);
            playable.SetSpeed(clip.rate);
            graph.Connect(playable, 0, mixer, index);
            mixer.SetInputWeight(index, 1);
            //new LavenderTimer.Builder().SetStartTime(clip.ScaledValidEnd).CreateTimer(() => EndParticle(playable));
        }
        public void EndParticle(Playable playable)
        {
            if (playable.IsValid())
            {
                playable.Destroy();
            }
        }
        public void LoadJointsFromObj(Transform parent, Dictionary<string, Transform> joints)
        {
            if (joints.ContainsKey(parent.name))
            {
                return;
            }
            joints.Add(parent.name, parent);
            for (int i = 0; i < parent.childCount; i++)
            {
                LoadJointsFromObj(parent.GetChild(i), joints);
            }
        }
        public void PlaySound(Playable mixer, LavenderClip clip, GameObject Model)
        {
            if (mixer.IsNull())
            {
                return;
            }
            var graph = mixer.GetGraph();
            var playable = SoundPlayable.CreatePlayable(graph, clip, Model);
            mixer.SetInputCount(mixer.GetInputCount() + 1);
            graph.Connect(playable, 0, mixer, mixer.GetInputCount() - 1);
            mixer.SetInputWeight(mixer.GetInputCount() - 1, 1);
            //playable.SetTime(clip.cutStartTime);
            //new LavenderTimer.Builder().SetStartTime(clip.ScaledValidEnd).CreateTimer(() => EndSound(playable));
        }
        public void EndSound(Playable playable)
        {
            //             var graph = playable.GetGraph();
            if (playable.IsValid())
            {
                playable.Destroy();
            }
            //             graph.DestroyOutput(output);
        }
        public override void OnPlayableCreate(Playable playable)
        {

        }
        public override void OnGraphStart(Playable playable)
        {
            playable.SetDuration(totalTime);
        }
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (playable.GetOutputCount() == 0)
            {
                return;
            }

            //Debug.Log(playable.GetDuration());
            if (playable.GetTime() > totalTime - 0.4)
            {
                if (playable.GetOutput(0).IsNull())
                {
                    return;
                }
                var parent = playable.GetOutput(0).GetOutput(0);
                var mgr = (ScriptPlayable<SkillMgrPlayable>)parent;
                if (mgr.IsNull())
                {
                    return;
                }
                var behaviour = mgr.GetBehaviour();
                behaviour.BeginBlend();
            }
            if (playable.GetTime() >= totalTime)
            {
                var parent = playable.GetOutput(0).GetOutput(0);
                var mgr = (ScriptPlayable<SkillMgrPlayable>)parent;
                var behaviour = mgr.GetBehaviour();
                behaviour.mixer.SetInputWeight(0, 0);
            }
        }
    }
}