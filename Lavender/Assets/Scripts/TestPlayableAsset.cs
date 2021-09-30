// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Animations;
// using UnityEngine.Playables;
// using Lavender;
// [System.Serializable]
// [CreateAssetMenu(fileName = "TestPlayableAsset", menuName = "TestSkill", order = 0)]
// public class TestPlayableAsset : PlayableAsset
// {
//     
//     public AnimationClip clip;
//     public List<LavenderClip> lclip;
//     // Factory method that generates a playable based on this asset
//     public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
//     {
//         //AnimationClip clip = (AnimationClip)UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets / Action / CLazyRunnerActionAnimPack / Animations / P1_CLazyMovement / Mvm_5way_PwDash / Mvm_PowerDash_Front.anim");
//         //var lclip = new LavenderClip(clip);
//         //var clips = new List<LavenderClip>();
//         //clips.Add(lclip);
//         var skillable = SkillPlayable.Create(graph, lclip, null, null, go);
//         var animmixer = LavenderAnimMixerPlayable.Create(graph, lclip);
//         var playable = AnimationClipPlayable.Create(graph, clip);
//         var output = AnimationPlayableOutput.Create(graph, "anim",go.GetComponent<Animator>());
//         output.SetSourcePlayable(skillable);
//         output.SetWeight(1);
//         return playable;
//     }
// }
