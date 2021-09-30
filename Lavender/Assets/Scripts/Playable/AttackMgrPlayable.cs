using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Lavender.Cardinal
{
    // A behaviour that is attached to a playable
    public class AttackMgrPlayable : PlayableBehaviour
    {
        Playable mixer;
        List<LavenderClip> clips;
        List<ScriptPlayable<AttackPlayable>> playables;
        PlayableDirector director;
        public static ScriptPlayable<AttackMgrPlayable> CreatePlayable(PlayableGraph graph, List<LavenderClip> clips, ScriptPlayable<SkillPlayable> parentPlayable, PlayableDirector director)
        {
            var playable = ScriptPlayable<AttackMgrPlayable>.Create(graph, 1);
            var behaviour = playable.GetBehaviour();
            playable.SetPropagateSetTime(true);
            behaviour.Init(graph, clips, playable, director);
            return playable;
        }
        public void Init(PlayableGraph graph, List<LavenderClip> newClips, ScriptPlayable<AttackMgrPlayable> parentPlayable, PlayableDirector dir)
        {
            clips = newClips;
            director = dir;
            mixer = Playable.Create(graph, clips.Count);
            mixer.SetPropagateSetTime(true);
            graph.Connect(mixer, 0, parentPlayable, 0);
            parentPlayable.SetInputWeight(0, 1f);
            if (graph.GetOutputCountByType<ScriptPlayableOutput>() == 0)
            {
                var output = ScriptPlayableOutput.Create(graph, "Attack");
                output.SetSourcePlayable(parentPlayable);
                output.SetWeight(1f);
            }
            playables = new List<ScriptPlayable<AttackPlayable>>();
            foreach (var clip in clips)
            {
                var playable = AttackPlayable.CreatePlayable(graph, clip);
                playables.Add(playable);
                playable.SetSpeed(clip.rate);
                graph.Connect(playable, 0, mixer, playables.Count - 1);
                mixer.SetInputWeight(playables.Count - 1, 1);
            }
        }
        // Called when the owning graph starts playing
        public override void OnGraphStart(Playable playable)
        {

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
            var currentTime = playable.GetTime();
            foreach (var clip in clips)
            {
                var index = clips.IndexOf(clip);
                var attackPlayable = (AttackPlayable)playables[index].GetBehaviour();
                if (clip.ContainsTime(currentTime))
                {
                    if (attackPlayable.abled==false)
                    {
                        playables[index].SetTime(clip.ToLocalTime(currentTime));
                        attackPlayable.OnEnable();
                        //particlePlayable.ModifyEmission(true);
                    }
                    else// if (director.state == PlayState.Paused)
                    {
                        //particlePlayable.ModifyEmission(true);
                        playables[index].SetTime(clip.ToLocalTime(currentTime));
                        //attackPlayable.OnBehaviourPause(playables[index], new FrameData());
                        //attackPlayable.PrepareFrame(playables[index], new FrameData());
                    }
                }
                else if (attackPlayable.abled)
                {
                    attackPlayable.OnDisable();
                    //particlePlayable.ModifyEmission(false);
                    //particlePlayable.PrepareFrame(playables[index], new FrameData());
                }
            }
        }
    }
}