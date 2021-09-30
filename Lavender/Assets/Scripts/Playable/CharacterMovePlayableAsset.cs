using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Lavender
{
    //[System.Serializable]
    [Serializable]
    [CreateAssetMenu(fileName = "CharacterMovePlayableAsset", menuName = "CreateMove", order = 0)]
    public class CharacterMovePlayableAsset : PlayableAsset
    {
        public List<AnimationClip> moveClips;
        // Factory method that generates a playable based on this asset
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var movePlayable = CharacterMovePlayable.Create(graph, moveClips, go);
            return movePlayable;
        }
    }
}