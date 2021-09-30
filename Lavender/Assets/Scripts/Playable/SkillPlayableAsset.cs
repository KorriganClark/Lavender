using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Playables;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using Lavender.Cardinal;

namespace Lavender
{
    [CreateAssetMenu(fileName = "SkillPlayableAsset", menuName = "CreateSkill", order = 0)]
    [Serializable]
    public class SkillPlayableAsset : PlayableAsset
    {
        /// <summary>
        /// 技能的config文件
        /// </summary>
        [OnValueChanged("ImportConfig")]
        public SkillConfig config;
        public ISkillScript script;
        /// <summary>
        /// 动画片段
        /// </summary>
        public List<LavenderClip> lavenderAnimClips;
        /// <summary>
        /// 音频片段
        /// </summary>
        public List<LavenderClip> lavenderAudioClips;
        /// <summary>
        /// 特效片段
        /// </summary>
        public List<LavenderClip> lavenderParticleClips;
        /// <summary>
        /// 攻击片段
        /// </summary>
        public List<LavenderClip> lavenderAttackClips;

        public List<LavenderClip> lavenderMoveClips;

        public float totalTime;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            //bool shouldCreateOutput = graph.GetPlayableCount() == 0;
            var skill = SkillPlayable.Create(graph, this, totalTime, go);
            //skill.SetPropagateSetTime(true);
            return skill.IsValid() ? skill : Playable.Null;
        }

        public void ImportConfig()
        {
            if (config == null)
            {
                return;
            }
            script = config.script;
            totalTime = 0;
            lavenderAnimClips = new List<LavenderClip>();
            lavenderAudioClips = new List<LavenderClip>();
            lavenderParticleClips = new List<LavenderClip>();
            lavenderAttackClips = new List<LavenderClip>();
            lavenderMoveClips = new List<LavenderClip>();
            foreach (var clips in config.LavenderClips)
            {
                foreach (var clip in clips)
                {
                    if (clip.isAnim)
                    {
                        lavenderAnimClips.Add(clip);
                    }
                    else if (clip.isAudio)
                    {
                        lavenderAudioClips.Add(clip);
                    }
                    else if (clip.isPartical)
                    {
                        lavenderParticleClips.Add(clip);
                    }
                    else if (clip.isAttack)
                    {
                        lavenderAttackClips.Add(clip);
                    }
                    else if (clip.isMove)
                    {
                        lavenderMoveClips.Add(clip);
                    }
                    totalTime = Mathf.Max(totalTime, clip.ScaledValidEnd);
                }
            }

        }
    }
}

