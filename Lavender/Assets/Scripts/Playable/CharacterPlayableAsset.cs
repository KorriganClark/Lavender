using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Playables;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
namespace Lavender
{
    [CreateAssetMenu(fileName = "SkillPlayableAsset", menuName = "CreateChraControl", order = 0)]
    [Serializable]
    public class CharacterPlayableAsset : PlayableAsset
    {
        [InlineEditor(Expanded = true)]
        public CharacterMovePlayableAsset movePlayableAsset;
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            //bool shouldCreateOutput = graph.GetPlayableCount() == 0;
            var Character = CharacterPlayable.Create(graph, movePlayableAsset, go);
            //skill.SetPropagateSetTime(true);
            return Character.IsValid() ? Character : Playable.Null;
        }
    }
}

