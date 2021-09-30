using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public class SoundPlayable : PlayableBehaviour
{
    public LavenderClip clip;
    public AudioClip audio;
    public AudioSource audioSource;
    public GameObject audioObj;
    public bool isPlaying=false;
    public PlayableDirector director;
    public static ScriptPlayable<SoundPlayable> CreatePlayable(PlayableGraph graph, LavenderClip audioClip, GameObject parentObj)
    {
        var playable = ScriptPlayable<SoundPlayable>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.Init(audioClip, parentObj);
        return playable;
    }
    public void Init(LavenderClip audioClip, GameObject parent)
    {
        clip = audioClip;
        audio = clip.GetAudioClip();
        audioObj = new GameObject("audio Obj");
        audioObj.transform.SetParent(parent.transform);
        audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.clip = audio;
        audioSource.playOnAwake = false;
        director = parent.GetComponent<PlayableDirector>();
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
        if (!clip.ContainsTime(playable.GetTime()))
        {
            return;
        }
        if (director.state == PlayState.Paused)
        {
            return;
        }
        audioSource.time = (float)clip.ToLocalTime(playable.GetTime());

        audioSource.Play();
        isPlaying = true;
    }

    // Called when the state of the playable is set to Paused
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (audioSource == null)
        {
            return;
        }
        audioSource.Stop();
        isPlaying = false;
    }

    // Called each frame while the state is set to Play
    public override void PrepareFrame(Playable playable, FrameData info)
    {
        //Debug.Log(isPlaying);
        var time = playable.GetTime();
        var localTime = clip.ToLocalTime(playable.GetTime());
        if (clip.ContainsTime(time) && !isPlaying)
        {
            OnBehaviourPlay(playable, info);
        }
        else if (!clip.ContainsTime(time) && isPlaying)
        {
            OnBehaviourPause(playable, info);
        }
        
    }
    public override void OnPlayableDestroy(Playable playable)
    {
        if (Application.isPlaying)
        {
            Object.Destroy(audioObj);
        }
        else
        {
            Object.DestroyImmediate(audioObj);
        }
    }
}
