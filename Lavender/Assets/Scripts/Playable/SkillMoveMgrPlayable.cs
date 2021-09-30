using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Lavender
{

    // A behaviour that is attached to a playable
    public class SkillMoveMgrPlayable : PlayableBehaviour
    {
        Playable mixer;
        List<LavenderClip> clips;
        List<ScriptPlayable<UnitMoveControlPlayable>> playables;
        PlayableDirector director;
        public static ScriptPlayable<SkillMoveMgrPlayable> CreatePlayable(PlayableGraph graph, List<LavenderClip> clips, ScriptPlayable<SkillPlayable> parentPlayable, PlayableDirector director)
        {
            var playable = ScriptPlayable<SkillMoveMgrPlayable>.Create(graph, 1);
            var behaviour = playable.GetBehaviour();
            playable.SetPropagateSetTime(true);
            behaviour.Init(graph, clips, playable, director);
            return playable;
        }
        public void Init(PlayableGraph graph, List<LavenderClip> newClips, ScriptPlayable<SkillMoveMgrPlayable> parentPlayable, PlayableDirector dir)
        {
            clips = newClips;
            director = dir;
            mixer = Playable.Create(graph, clips.Count);
            mixer.SetPropagateSetTime(true);
            graph.Connect(mixer, 0, parentPlayable, 0);
            parentPlayable.SetInputWeight(0, 1f);
            if (graph.GetOutputCountByType<ScriptPlayableOutput>() == 0)
            {
                var output = ScriptPlayableOutput.Create(graph, "Move");
                output.SetSourcePlayable(parentPlayable);
                output.SetWeight(1f);
            }
            playables = new List<ScriptPlayable<UnitMoveControlPlayable>>();
            foreach (var clip in clips)
            {
                var playable = UnitMoveControlPlayable.CreatePlayable(graph, clip);
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

        float lastTime;
        // Called each frame while the state is set to Play
        public override void PrepareFrame(Playable playable, FrameData info)
        {

            var currentTime = playable.GetTime();
            if (director.state == PlayState.Paused)
            {
                SkillEditor.ResetModelPos();
            }
            bool noMove = true;
            if (Application.isPlaying)
            {
                noMove = false;
            }
            foreach (var clip in clips)
            {
                var index = clips.IndexOf(clip);
                var movePlayable = (UnitMoveControlPlayable)playables[index].GetBehaviour();
                if (director.state == PlayState.Paused)
                {
                    if (clip.ContainsTime(currentTime))
                    {
                        movePlayable.Pause((float)(currentTime));
                        noMove = false;
                    }
                    else
                    {
                        if (clip.ScaledValidEnd <= currentTime)
                        {
                            if (!movePlayable.paused)
                            {
                                movePlayable.control.CompleteCurrentMove();
                            }
                            else
                            {
                                movePlayable.MoveInstant();
                            }
                            movePlayable.paused = true;
                            noMove = false;
                        }
                    }
                }
                else
                {
                    if (clip.ContainsTime(currentTime))
                    {
                        noMove = false;
                        if (movePlayable.paused)
                        {
                            //Debug.Log(movePlayable.paused);
                            playables[index].SetTime(clip.ToLocalTime(currentTime));
                            movePlayable.Start((float)(currentTime));
                            //particlePlayable.ModifyEmission(true);
                        }
                    }
                    if (clip.ScaledValidEnd < currentTime)
                    {
                        noMove = false;
                        movePlayable.paused = true;
                    }
                }
            }
            if (noMove)
            {
                SkillEditor.ResetModelPos();
            }
        }
    }
}