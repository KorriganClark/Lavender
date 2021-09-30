using System.Collections;
using System.Collections.Generic;
using Lavender;
using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public class UnitMoveControlPlayable : PlayableBehaviour
{
    LavenderClip clip;
    LavenderClip.MoveConfig config;
    public LavenderUnitMoveControl control;
    public bool paused = true;
    public static ScriptPlayable<UnitMoveControlPlayable> CreatePlayable(PlayableGraph graph, LavenderClip clip)
    {
        var playable = ScriptPlayable<UnitMoveControlPlayable>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.Init(graph, clip);
        return playable;
    }
    public void Init(PlayableGraph graph, LavenderClip newClip)
    {
        clip = newClip;
        control = clip.lavenderTransForm.GetComponentInParent<LavenderUnitMoveControl>();
        if (control == null)
        {
            Debug.LogError("No MoveControl");
            return;
        }
        config = clip.moveConfig;
    }

    public void Start(float currentTime)
    {
        paused = false;
        currentTime -= clip.ScaledValidStart;
        //Debug.Log(paused);
        var moveUnit = new LavenderUnitMoveControl.MoveUnit(clip.ScaledValidEnd - clip.ScaledValidStart, clip.lavenderTransForm, config.targetPosition, config.useWorldPos, config.originPosition, clip.baseTransForm, !config.setOriPosition, config.followBase, config.editRotation, false, currentTime, config.useCurve ? config.animationCurve : null);
        control.MoveByMoveUnit(moveUnit);
    }
    public void MoveInstant()
    {
        var moveUnit = new LavenderUnitMoveControl.MoveUnit(0, clip.lavenderTransForm, config.targetPosition, config.useWorldPos, config.originPosition, clip.baseTransForm, !config.setOriPosition, config.followBase, config.editRotation, false, 0, config.useCurve ? config.animationCurve : null);
        control.MoveByMoveUnit(moveUnit);
    }
    public void Pause(float currentTime)
    {
        //Debug.Log("Pause" + currentTime);
        currentTime -= clip.ScaledValidStart;
        control.EditMoveToPosPaused(clip.ScaledValidEnd - clip.ScaledValidStart, clip.lavenderTransForm, config.targetPosition, config.useWorldPos, config.originPosition, clip.baseTransForm, !config.setOriPosition, config.followBase, config.editRotation, currentTime, config.useCurve ? config.animationCurve : null);
        paused = true;
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
        //         var currentTime = playable.GetTime();
        //         if (!clip.ContainsTime(currentTime))
        //         {
        //             paused = true;
        //         }
    }
}
