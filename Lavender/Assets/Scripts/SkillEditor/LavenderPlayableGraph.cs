using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using static UnityEngine.ParticleSystem;

namespace Lavender
{

    [Serializable]
    [Obsolete]
    public class LavenderPlayableGraph : UnityEngine.ScriptableObject
    {
        //private PlayableDirector director;
        private PlayableGraph playableGraph;
        private bool isPlaying;
        private float currentTime;
        private float totalTime;
        private List<LavenderClip> lavenderClips;
        private List<Playable> clipsPlayablePtr;
        private AnimationMixerPlayable animPlayableMixer;
        private AudioMixerPlayable audioPlayableMixer;
        private Playable scriptPlayableController;
        private Animator targetAnimator;
        private AnimationPlayableOutput animOutPut;
        private AudioPlayableOutput audioOutPut;
        private ScriptPlayableOutput scriptOutput;
        private List<LavenderClip> nowAnimClip;
        private List<LavenderClip> nowAudioClip;
        private List<LavenderClip> nowPrefabClip;
        private List<GameObject> playingAudio;
        public List<ParticleSystem> controlledParticles;
        public bool closeParticleImmediately = false;
        public void Init(Animator animator, AudioSource targetAudio, SkillConfig config, GameObject timeLineObj)
        {
            if (playableGraph.IsValid())
            {
                playableGraph.Destroy();
            }
            if (!timeLineObj.GetComponent<PlayableDirector>())
            {
                timeLineObj.AddComponent<PlayableDirector>();
            }
            playingAudio = new List<GameObject>();
            this.playableGraph = PlayableGraph.Create();
            this.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.DSPClock);
            this.isPlaying = false;
            this.currentTime = 0;
            this.totalTime = 0;
            nowAnimClip = new List<LavenderClip>();
            nowAudioClip = new List<LavenderClip>();
            nowPrefabClip = new List<LavenderClip>();
            controlledParticles = new List<ParticleSystem>();
            if (lavenderClips != null)
            {
                lavenderClips.Clear();
            }
            else
            {
                lavenderClips = new List<LavenderClip>();
            }
            foreach (var listClip in config.LavenderClips)
            {
                foreach (var clip in listClip)
                {
                    lavenderClips.Add(clip);
                }
            }
            this.targetAnimator = animator;
            if (animOutPut.IsOutputValid())
            {
                animOutPut.SetTarget(targetAnimator);
            }
            else
            {
                this.animOutPut = AnimationPlayableOutput.Create(playableGraph, "Animation", targetAnimator);
            }
            if (audioOutPut.IsOutputValid())
            {
                audioOutPut.SetTarget(targetAudio);
            }
            else
            {
                this.audioOutPut = AudioPlayableOutput.Create(playableGraph, "Audio", targetAudio);
            }
            scriptOutput = ScriptPlayableOutput.Create(playableGraph, "Script");
            scriptPlayableController = Playable.Create(playableGraph, GetPrefabClipsNum());
            scriptOutput.SetSourcePlayable(scriptPlayableController);
            animPlayableMixer = AnimationMixerPlayable.Create(playableGraph, GetAnimClipsNum());
            animOutPut.SetSourcePlayable(animPlayableMixer);
            audioPlayableMixer = AudioMixerPlayable.Create(playableGraph, GetAudioClipsNum());
            audioOutPut.SetSourcePlayable(audioPlayableMixer);
            if (clipsPlayablePtr != null)
            {
                clipsPlayablePtr.Clear();
            }
            else
            {
                clipsPlayablePtr = new List<Playable>();
            }
            foreach (var clip in lavenderClips)
            {
                if (clip.animClip)
                {
                    AnimationClipPlayable newClip = AnimationClipPlayable.Create(playableGraph, clip.GetAnimClip());
                    newClip.SetDuration(clip.length);
                    newClip.Pause();
                    clipsPlayablePtr.Add(newClip);
                    playableGraph.Connect(newClip, 0, animPlayableMixer, GetAnimIdInClips(lavenderClips.IndexOf(clip)));
                    totalTime = Math.Max(totalTime, clip.ScaledValidEnd);
                }
                else if (clip.audioClip)
                {
                    AudioClipPlayable newClip = AudioClipPlayable.Create(playableGraph, clip.GetAudioClip(), false);
                    newClip.SetDuration(clip.length);
                    newClip.Pause();
                    clipsPlayablePtr.Add(newClip);
                    playableGraph.Connect(newClip, 0, audioPlayableMixer, GetAudioIdInClips(lavenderClips.IndexOf(clip)));
                    totalTime = Math.Max(totalTime, clip.ScaledValidEnd);
                }
                /*
                else
                {
                    var newClip = LavenderParticalPlayable.LavenderCreate(playableGraph, clip.prefabClip, clip.lavenderTransForm, 13546);
                    //var bh = newClip.GetBehaviour();
                    //var m_Instance = bh.Initialize(clip.prefabClip, timeLineObj.transform);//(GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(clip.prefabClip, timeLineObj.transform);
                    //UnityEditor.PrefabUtility.prefabInstanceUpdated += OnPrefabUpdated;
                    //Debug.Log(newClip);
                    //if (newClip == ScriptPlayable<LavenderParticalPlayable>.Null)
                    newClip.Pause();
                    clipsPlayablePtr.Add(newClip);
                    //scriptOutput.SetSourcePlayable(newClip);
                    playableGraph.Connect(newClip, 0, scriptPlayableController, GetPrefabIdInClips(lavenderClips.IndexOf(clip)));
                    scriptPlayableController.SetInputWeight(newClip, 1.0f);
                    totalTime = Math.Max(totalTime, clip.ScaledValidEnd);
                    controlledParticles.Add(newClip.GetBehaviour().particleSystem);
                }*/
            }
        }
        public static LavenderPlayableGraph Create(Animator animator, AudioSource targetAudio, SkillConfig config, GameObject timeLineObj)
        {
            LavenderPlayableGraph tempLPG = CreateInstance<LavenderPlayableGraph>();
            tempLPG.Init(animator, targetAudio, config, timeLineObj);
            return tempLPG;
        }
        public void Destory()
        {
            if (playableGraph.IsValid())
            {
                playableGraph.Destroy();
            }
            foreach (var ps in controlledParticles)
            {
                DestroyImmediate(ps);
            }
            ClearAudio();
        }
        private void OnDestroy()
        {
            Destory();
        }
        public bool IsValid()
        {
            return playableGraph.IsValid();
        }
        /// <summary>
        /// 播放器正常播放，将从当前时间点继续播放
        /// </summary>
        public void Play()
        {
            isPlaying = true;
            preTime = AudioSettings.dspTime;
            //playableGraph.Play();
        }
        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            isPlaying = false;
            ClearAudio();
            //playableGraph.Stop();
        }
        /// <summary>
        /// 是否播放中
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            return isPlaying;
        }
        /// <summary>
        /// 设置当前播放的时间，如果暂停中，继续暂停，播放中继续播放
        /// </summary>
        public void SetCurrentTime(float time)
        {
            currentTime = time;
        }
        /// <summary>
        /// 获取当前时间
        /// </summary>
        /// <returns></returns>
        public float GetCurrentTime()
        {
            return currentTime;
        }
        /// <summary>
        /// 加入动画片段
        /// </summary>
        /// <param name="clip"></param>
        public void AddClip(LavenderClip clip)
        {
            if (!lavenderClips.Contains(clip))
            {
                lavenderClips.Add(clip);
                AnimationClipPlayable newClip = AnimationClipPlayable.Create(playableGraph, clip.GetAnimClip());
                clipsPlayablePtr.Add(newClip);
            }
            totalTime = Math.Max(clip.endTime, totalTime);

        }
        /// <summary>
        /// 往尾端加入Clip
        /// </summary>
        public void AddClipAtEnd(LavenderClip clip)
        {
            clip.startTime = totalTime;
            clip.endTime += totalTime;
            totalTime += clip.length;
            AddClip(clip);
        }
        /// <summary>
        /// 根据传入的LavenderClip找到图中的AnimationClipPlayable
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public Playable GetClipPtr(LavenderClip clip)
        {
            if (lavenderClips.IndexOf(clip) < lavenderClips.Count && lavenderClips.IndexOf(clip) >= 0)
            {
                return clipsPlayablePtr[lavenderClips.IndexOf(clip)];
            }
            else
            {
                return new AnimationClipPlayable();
            }
        }
        double preTime;
        public GameObject updateGameObj;
        public void Update()
        {
            double delta = 0;// = Time.fixedDeltaTime;
            //ttt = Time.realtimeSinceStartup;
            if (isPlaying)
            {
                var nowT = AudioSettings.dspTime;
                delta = nowT - preTime;
                preTime = nowT;
                //Debug.Log(" ? ? " + Time.deltaTime + " .. " + delta + " ...."  + updateID);
                if (delta > 100)
                {
                    return;
                }
                currentTime += (float)delta;//TODO 编写自己的计时器，Unity原生的可能有bug
                //currentTime += mydt;
                if (currentTime - totalTime >= -1e-6)
                {
                    isPlaying = false;
                }
            }
            RefreshCurrentClip();
            //动画片段动态权重计算
            float totalAnimWeight = 0;
            while (nowAnimClip.Count > 2)
            {
                nowAnimClip.RemoveAt(nowAnimClip.Count - 1);
            }
            if (nowAnimClip.Count == 2)
            {
                float clipStartTime0 = nowAnimClip[0].ScaledValidStart; //nowAnimClip[0].startTime + nowAnimClip[0].cutStartTime;
                float clipStartTime1 = nowAnimClip[1].ScaledValidStart;//nowAnimClip[1].startTime + nowAnimClip[1].cutStartTime;
                float clipEndTime0 = nowAnimClip[0].ScaledValidEnd;//nowAnimClip[0].endTime - nowAnimClip[0].cutEndTime;
                float clipEndTime1 = nowAnimClip[1].ScaledValidEnd;//nowAnimClip[1].endTime - nowAnimClip[1].cutEndTime;

                if (clipStartTime0 > clipStartTime1 + 0.01 || (clipStartTime0 > clipStartTime1 - 0.1 && clipEndTime0 > clipEndTime1 - 0.1))
                {
                    nowAnimClip[0].dynamicWeight = Math.Max(0.001f, nowAnimClip[0].weight * 1000 * (currentTime - clipStartTime0));
                    nowAnimClip[1].dynamicWeight = Math.Max(0.001f, nowAnimClip[1].weight * 1000 * (clipEndTime1 - currentTime));
                    totalAnimWeight = nowAnimClip[0].dynamicWeight + nowAnimClip[1].dynamicWeight;
                    //Debug.Log(nowAnimClip[0].dynamicWeight / totalAnimWeight);
                }
                else
                {
                    nowAnimClip[1].dynamicWeight = Math.Max(0.001f, nowAnimClip[1].weight * 1000 * (currentTime - clipStartTime1));
                    nowAnimClip[0].dynamicWeight = Math.Max(0.001f, nowAnimClip[0].weight * 1000 * (clipEndTime0 - currentTime));
                    totalAnimWeight = nowAnimClip[0].dynamicWeight + nowAnimClip[1].dynamicWeight;
                }
                if (totalAnimWeight <= 0)
                {
                    //                     Debug.Log(clipStartTime0);
                    //                     Debug.Log(clipStartTime1);
                    //                     Debug.Log(clipEndTime0);
                    //                     Debug.Log(clipEndTime1);

                }
            }
            else if (nowAnimClip.Count == 1)
            {
                float clipStartTime0 = nowAnimClip[0].ScaledValidStart;
                float clipEndTime0 = nowAnimClip[0].ScaledValidEnd;
                nowAnimClip[0].dynamicWeight = (currentTime >= clipStartTime0 - 1e-3 && currentTime <= clipEndTime0 + 1e-3) ? nowAnimClip[0].weight : 0;
                totalAnimWeight = nowAnimClip[0].dynamicWeight;
            }
            float totalAudioWeight = 0;
            foreach (var clip in nowAudioClip)
            {
                float clipStartTime = clip.startTime + clip.cutStartTime;
                float clipEndTime = clip.endTime - clip.cutEndTime;
                if (currentTime > clipStartTime && currentTime < clipEndTime)
                {
                    totalAudioWeight += clip.weight;
                }
            }
            for (int i = 0; i < lavenderClips.Count; i++)
            {
                Playable playable = GetClipPtr(lavenderClips[i]);
                if (lavenderClips[i].animClip && totalAnimWeight > 0)
                {
                    int inputId = GetAnimIdInClips(i);
                    animPlayableMixer.SetInputWeight(inputId, nowAnimClip.Contains(lavenderClips[i]) ? lavenderClips[i].dynamicWeight / totalAnimWeight : 0);
                }
                if (lavenderClips[i].animClip && totalAnimWeight <= 0)
                {
                    int inputId = GetAnimIdInClips(i);
                    animPlayableMixer.SetInputWeight(inputId, 0);
                }
                if (lavenderClips[i].prefabClip && nowPrefabClip.Contains(lavenderClips[i]))
                {
                    int inputId = GetPrefabIdInClips(i);
                    //scriptPlayableController.SetInputWeight(inputId, 1);
                }
                else if (lavenderClips[i].prefabClip && !nowPrefabClip.Contains(lavenderClips[i]))
                {
                    int inputId = GetPrefabIdInClips(i);
                    var playAble = ((ScriptPlayable<LavenderParticalPlayable>)playable);
                    if (closeParticleImmediately)
                    {
                        playable.SetTime((currentTime - lavenderClips[i].startTime) * lavenderClips[i].rate);
                        //playAble.GetBehaviour().PrepareFrame(playable, new FrameData());
                        playAble.GetBehaviour().ModifyEmission(false);
                        //playAble.GetBehaviour().particleSystem.di
                        playAble.GetBehaviour().particleSystem.Stop();
                        //playAble.GetBehaviour().particleSystem.sim
                    }
                    else
                    {
                        if (playable.GetTime() != lavenderClips[i].length)
                        {
                            playable.SetTime((currentTime - lavenderClips[i].startTime) * lavenderClips[i].rate);
                            //playable.SetTime(lavenderClips[i].length);
                            //if (!isPlaying)
                            {
                                playAble.GetBehaviour().ModifyEmission(false);
                            }
                            playAble.GetBehaviour().PrepareFrame(playable, new FrameData());

                            //playAble.GetBehaviour().particleSystem.Play();
                            //playAble.GetBehaviour().particleSystem.playbackSpeed = lavenderClips[i].rate;
                            //MainModule pm = playAble.GetBehaviour().particleSystem.main;
                            //pm.simulationSpeed = lavenderClips[i].rate;
                        }
                    }
                    //playable.Play();
                    //Debug.Log(playable.GetPlayState());

                    //
                    //scriptPlayableController.SetInputWeight(inputId, 0);
                }
                if (lavenderClips[i].audioClip)
                {
                    ///暂时屏蔽playablegraph的音频输出，使用AudioSource输出
                    int inputId = GetAudioIdInClips(i);
                    //audioPlayableMixer.SetInputWeight(inputId, nowAudioClip.Contains(lavenderClips[i]) ? lavenderClips[i].dynamicWeight / totalAudioWeight : 0);
                    //AudioSource.PlayClipAtPoint();
                    //audioPlayableMixer.SetInputWeight(inputId, 1);
                }
                if (lavenderClips[i].audioClip && totalAudioWeight <= 0)
                {
                    int inputId = GetAudioIdInClips(i);
                    audioPlayableMixer.SetInputWeight(inputId, 0);
                }
                if (!nowAnimClip.Contains(lavenderClips[i]) && !nowAudioClip.Contains(lavenderClips[i]) && !nowPrefabClip.Contains(lavenderClips[i]))
                {
                    //Playable playable = GetClipPtr(lavenderClips[i]);
                    //playable.Pause();
                    continue;
                }

                playable.SetTime((currentTime - lavenderClips[i].startTime) * lavenderClips[i].rate);
                if (isPlaying)
                {
                    if (playable.GetPlayState() == PlayState.Paused && playable.GetPlayableType() == typeof(AudioClipPlayable))
                    {
                        playable.Play();
                        double startTime = AudioSettings.dspTime;
                        //double endTime = lavenderClips[i].endTime - lavenderClips[i].cutEndTime - currentTime + startTime;
                        double endTime = lavenderClips[i].ScaledValidEnd - currentTime + startTime;

                        PlayNewAudio(((AudioClipPlayable)playable).GetClip(), currentTime - lavenderClips[i].ScaledStart, endTime);
                    }

                    if (playable.GetPlayableType() == typeof(LavenderParticalPlayable))
                    {
                        var playAble = ((ScriptPlayable<LavenderParticalPlayable>)playable);
                        if (playAble.GetBehaviour().particleSystem.enableEmission == false)
                        {
                            playable.Play();
                            playAble.GetBehaviour().ModifyEmission(true);

                        }
                        //playAble.GetBehaviour().PrepareFrame(playable, new FrameData());
                    }
                }
                else
                {

                    if (playable.GetPlayableType() == typeof(LavenderParticalPlayable))
                    {
                        var playAble = ((ScriptPlayable<LavenderParticalPlayable>)playable);
                        playAble.GetBehaviour().ModifyEmission(true);
                        playAble.GetBehaviour().PrepareFrame(playable, new FrameData());
                    }
                    else
                    {
                        playable.Pause();
                    }
                }
                playableGraph.Evaluate();
            }
        }
        /// <summary>
        /// 检查连接的片段，并选择正确的片段 TODO改为计时器
        /// </summary>
        public void RefreshCurrentClip()
        {

            nowAudioClip.Clear();
            LavenderClip tmpClip = null;
            if (nowAnimClip.Count > 0)
            {
                tmpClip = nowAnimClip[0];
            }
            nowAnimClip.Clear();
            nowPrefabClip.Clear();
            foreach (var clip in lavenderClips)
            {
                if (clip.ScaledValidStart < currentTime + 1e-5 && clip.ScaledValidEnd > currentTime - 1e-5)
                {
                    if (clip.animClip)
                    {
                        nowAnimClip.Add(clip);
                    }
                    else if (clip.audioClip)
                    {
                        nowAudioClip.Add(clip);
                    }
                    else
                    {
                        nowPrefabClip.Add(clip);
                    }
                }
            }
            if (nowAnimClip.Count == 0 && tmpClip != null)
            {
                nowAnimClip.Add(tmpClip);
            }
        }
        public void RefreshTotalTime()
        {
            totalTime = 0;
            foreach (var clip in lavenderClips)
            {
                totalTime = Math.Max(totalTime, clip.ScaledValidEnd);
            }
        }
        /// <summary>
        /// 动画总时间，以最后一个动画片段的结束时间为准
        /// </summary>
        /// <returns></returns>
        public float GetTotalTime()
        {
            return totalTime;
        }
        /// <summary>
        /// 当前动画片段的个数
        /// </summary>
        /// <returns></returns>
        public int GetAnimClipsNum()
        {
            int res = 0;
            foreach (var clip in lavenderClips)
            {
                if (clip.animClip != null)
                {
                    res++;
                }
            }
            return res;
        }
        /// <summary>
        /// 音频片段的个数
        /// </summary>
        /// <returns></returns>
        public int GetAudioClipsNum()
        {
            int res = 0;
            foreach (var clip in lavenderClips)
            {
                if (clip.audioClip != null)
                {
                    res++;
                }
            }
            return res;
        }
        public int GetPrefabClipsNum()
        {
            int res = 0;
            foreach (var clip in lavenderClips)
            {
                if (clip.prefabClip != null)
                {
                    res++;
                }
            }
            return res;
        }
        /// <summary>
        /// 查询第几个动画片段
        /// </summary>
        /// <param name="clipId"></param>
        /// <returns></returns>
        public int GetAnimIdInClips(int clipId)
        {
            int res = 0;
            for (int i = 0; i < clipId; i++)
            {
                if (lavenderClips[i].animClip)
                {
                    res++;
                }
            }
            return res;
        }
        /// <summary>
        /// 查询第几个音频片段
        /// </summary>
        /// <param name="clipId"></param>
        /// <returns></returns>
        public int GetAudioIdInClips(int clipId)
        {
            int res = 0;
            for (int i = 0; i < clipId; i++)
            {
                if (lavenderClips[i].audioClip)
                {
                    res++;
                }
            }
            return res;
        }
        public int GetPrefabIdInClips(int clipId)
        {
            int res = 0;
            for (int i = 0; i < clipId; i++)
            {
                if (lavenderClips[i].prefabClip)
                {
                    res++;
                }
            }
            return res;
        }

        /// <summary>
        /// 播放音乐，starttime是相对位置，endTime是绝对位置
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public GameObject PlayNewAudio(AudioClip clip, double startTime, double endTime)
        {
            GameObject audioObj = new GameObject("Timeline Audio");
            audioObj.transform.position = targetAnimator.transform.position;
            playingAudio.Add(audioObj);
            AudioSource audioSource = audioObj.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.time = (float)startTime;
            audioSource.PlayScheduled(AudioSettings.dspTime);
            audioSource.SetScheduledEndTime(endTime);
            return audioObj;
        }
        public void ClearAudio()
        {
            foreach (var audioObj in playingAudio)
            {
                var audio = audioObj.GetComponent<AudioSource>();
                audio.Pause();
                DestroyImmediate(audioObj);
            }
            playingAudio.Clear();
        }
        public ParticleSystem GetParticleByClip(LavenderClip target)
        {
            var index = lavenderClips.IndexOf(target);
            if (index >= 0 && GetPrefabIdInClips(index) < controlledParticles.Count)
            {
                return controlledParticles[GetPrefabIdInClips(index)];
            }
            return null;
        }
        public void ChangeParticleTransform(LavenderClip target, Transform transform)
        {
            var ps = GetParticleByClip(target);
            if (ps)
            {
                ps.transform.SetParent(transform);
                ps.transform.position = transform.position;
            }
            //             Playable targetPlayable = GetClipPtr(target);
            //             var playAble = ((ScriptPlayable<LavenderParticalPlayable>)targetPlayable);
            //             //if ((playAble.GetBehaviour())!=PlayableBehaviour. )
            //             {
            //                 ParticleSystem ps = playAble.GetBehaviour().particleSystem;
            //                 ps.transform.SetParent(transform);
            //                 ps.transform.position = transform.position;
            //             }
        }
        public void ModifyParticle(LavenderClip target, ParticleConfig config)
        {
            var ps = GetParticleByClip(target);
            if (ps)
            {
                ps.transform.localPosition = config.relativePosition;
                ps.transform.localRotation = config.relativeRotation;
                ps.Simulate((currentTime - target.startTime) * target.rate);
            }
            //Update();
        }
    }
}

