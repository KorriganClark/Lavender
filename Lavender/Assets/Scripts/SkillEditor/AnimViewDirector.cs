using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Lavender
{
    public class AnimViewDirector
    {
        private PlayableDirector director;
        private SkillConfig config;
        private SkillPlayableAsset playableAsset;
        public bool isPlaying
        {
            get
            {
                if (director == null)
                {
                    return false;
                }
                if (director.time == director.duration)
                {
                    return false;
                }
                return director.state == PlayState.Playing;
            }
        }
        public double currentTime
        {
            get
            {
                if (director != null)
                {
                    return director.time;
                }
                return 0;
            }
            set
            {
                if (director != null)
                {
                    director.time = value;
                }
            }
        }
        public float totalTime
        {
            get
            {
                float res = 0;
                if (config == null)
                {
                    return 0;
                }
                foreach (var clips in config.LavenderClips)
                {
                    foreach (var clip in clips)
                    {
                        res = Mathf.Max(res, clip.ScaledValidEnd);
                    }
                }
                return res;
            }
        }
        public static AnimViewDirector Create(GameObject targetModel, SkillConfig config)
        {
            AnimViewDirector animViewDirector = new AnimViewDirector();
            animViewDirector.Init(targetModel, config);
            return animViewDirector;
        }
        public void Init(GameObject targetModel, SkillConfig newConfig)
        {
            director = targetModel.GetComponent<PlayableDirector>();
            if (director == null)
            {
                director = targetModel.AddComponent<PlayableDirector>();
            }
            playableAsset = ScriptableObject.CreateInstance<SkillPlayableAsset>();
            config = newConfig;
            playableAsset.config = config;
            playableAsset.ImportConfig();
            {
                director.playableAsset = playableAsset;
                //director.timeUpdateMode = DirectorUpdateMode.DSPClock;
                director.extrapolationMode = DirectorWrapMode.Hold;
                director.playOnAwake = false;
                director.RebuildGraph();
                //Debug.Log(director.duration);
                //director.duration = totalTime;
                //director.Play();
            }
        }
        public void Play()
        {
            director.Play();
        }
        public void Stop()
        {
            director.Pause();
        }
        public void Destory()
        {
            //director.Stop();
        }
        public void Evaluate()
        {
            if (director != null)
            {
                director.Evaluate();
                //Debug.Log(director.state);
                //director.Pause();
            }
        }
        public void Refresh()
        {
            var time = currentTime;
            if (director == null || playableAsset == null)
            {
                return;
            }
            playableAsset.ImportConfig();
            //director.RebuildGraph();
            director.time = time;
            director.Evaluate();
        }
    }
}
