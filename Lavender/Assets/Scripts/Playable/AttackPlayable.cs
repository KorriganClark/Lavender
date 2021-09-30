using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Lavender.Cardinal
{
    // A behaviour that is attached to a playable
    public class AttackPlayable : PlayableBehaviour
    {
        LavenderClip clip;
        AttackField attackField;
        public bool abled;
        public static ScriptPlayable<AttackPlayable> CreatePlayable(PlayableGraph graph, LavenderClip clip)
        {
            var playable = ScriptPlayable<AttackPlayable>.Create(graph, 1);
            var behaviour = playable.GetBehaviour();
            behaviour.Init(graph, clip);
            return playable;
        }
        public void Init(PlayableGraph graph, LavenderClip newClip)
        {
            clip = newClip;
            attackField = newClip.attackClip;
            //director = dir;

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
            //attackField.OnDisable();
        }

        public void OnEnable()
        {
            attackField.OnEnable();
            abled = true;
        }
        public void OnDisable()
        {
            attackField.OnDisable();
            abled = false;
        }
        // Called each frame while the state is set to Play
        public override void PrepareFrame(Playable playable, FrameData info)
        {

        }
    }
}