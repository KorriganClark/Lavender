using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Lavender
{
    // A behaviour that is attached to a playable
    public class ParticleControlPlayable : PlayableBehaviour
    {
        Playable mixer;
        List<LavenderClip> clips;
        List<ScriptPlayable<LavenderParticalPlayable>> playables;
        PlayableDirector director;
        public static ScriptPlayable<ParticleControlPlayable> CreatePlayable(PlayableGraph graph, List<LavenderClip> clips, ScriptPlayable<SkillPlayable> parentPlayable, PlayableDirector director)
        {
            var playable = ScriptPlayable<ParticleControlPlayable>.Create(graph,1);
            var behaviour = playable.GetBehaviour();
            playable.SetPropagateSetTime(true);
            behaviour.Init(graph, clips, playable, director);
            return playable;
        }
        public void Init(PlayableGraph graph, List<LavenderClip> newClips, ScriptPlayable<ParticleControlPlayable> parentPlayable, PlayableDirector dir)
        {
            clips = newClips;
            director = dir;
            mixer = Playable.Create(graph, clips.Count);
            mixer.SetPropagateSetTime(true);
            graph.Connect(mixer, 0, parentPlayable, 0);
            parentPlayable.SetInputWeight(0, 1f);
            if (graph.GetOutputCountByType<ScriptPlayableOutput>() == 0)
            {
                var output = ScriptPlayableOutput.Create(graph, "Particle");
                output.SetSourcePlayable(parentPlayable);
                output.SetWeight(1f);
            }
            playables = new List<ScriptPlayable<LavenderParticalPlayable>>();
            foreach (var clip in clips)
            {
                var playable = LavenderParticalPlayable.LavenderCreate(graph, clip, 1111);
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
                var particlePlayable = (LavenderParticalPlayable)playables[index].GetBehaviour();
                if (clip.ContainsTime(currentTime))
                {
                    if (director.state == PlayState.Playing && playables[index].GetPlayState() == PlayState.Paused)
                    {
                        playables[index].SetTime(clip.ToLocalTime(currentTime));
                        playables[index].Play();
                        particlePlayable.ModifyEmission(true);
                    }
                    else if (director.state == PlayState.Paused)
                    {
                        particlePlayable.ModifyEmission(true);
                        playables[index].SetTime(clip.ToLocalTime(currentTime));
                        if (playables[index].GetPlayState() == PlayState.Playing)
                        {
                            playables[index].Pause();
                        }
                        particlePlayable.OnBehaviourPause(playables[index], new FrameData());
                        particlePlayable.PrepareFrame(playables[index], new FrameData());
                    }
                }
                else
                {
                    playables[index].Pause();
                    particlePlayable.ModifyEmission(false);
                    particlePlayable.PrepareFrame(playables[index], new FrameData());
                }
            }
        }
    }

}
