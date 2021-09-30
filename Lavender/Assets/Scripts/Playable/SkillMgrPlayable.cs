using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace Lavender
{
    // A behaviour that is attached to a playable
    //[Serializable]
    public class SkillMgrPlayable : PlayableBehaviour
    {
        public ScriptPlayable<CharacterPlayable> characterPlayable;
        public AnimationMixerPlayable mixer;
        public GameObject character;
        public static ScriptPlayable<SkillMgrPlayable> Create(PlayableGraph graph, ScriptPlayable<CharacterPlayable> character, GameObject Model)
        {
            if (Model == null)
            {
                throw new ArgumentException("Target is null");
            }
            var playable = ScriptPlayable<SkillMgrPlayable>.Create(graph, 2);
            var behaviour = playable.GetBehaviour();
            behaviour.characterPlayable = character;
            behaviour.Compile(graph, playable, Model);
            return playable;
        }
        public void Compile(PlayableGraph graph, Playable parentPlayable, GameObject Model)
        {
            character = Model;
            mixer = AnimationMixerPlayable.Create(graph, 0);
            graph.Connect(mixer, 0, parentPlayable, 0);
            parentPlayable.SetInputWeight(0, 1);
            var controller = character.GetComponent<LavenderCharacterControl>();
            controller.SkillAction += ReleaseSkill;
            if (controller.editorControl)
            {
                controller.editorControl.SkillAction += ReleaseSkill;
            }
        }
        public void ReleaseSkill(LavenderAction action)
        {
            var skillAsset = (SkillPlayableAsset)(action.GetValue());
            if (!characterPlayable.IsValid())
            {
                return;
            }
            //var isOn = action.DetailUtility["SkillOn"]();
            //Debug.Log(isOn);
            var graph = characterPlayable.GetGraph();
            var playable = skillAsset.CreatePlayable(graph, character);
            mixer.SetInputCount(1);
            graph.Connect(playable, 0, mixer, 0);
            mixer.SetInputWeight(0, 1);
            var owner = characterPlayable.GetBehaviour();
            owner.SetWeight(0, 1, 0.2);
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
            if (character == null)
            {
                return;
            }
            var controller = character.GetComponent<LavenderCharacterControl>();

            if (mixer.GetInputCount() == 0)
            {
                var owner = characterPlayable.GetBehaviour();
                owner.SetWeight(1, 0, 0.2);

            }
            if (mixer.GetInputCount() > 0)
            {
                if (mixer.GetInputWeight(0) == 0)
                {
                    DestorySkill(mixer.GetInput(0));
                    if(controller.skillStateMachine!=null)
                    {
                        controller.skillStateMachine.HandleSwitch(controller, LavenderAction.SkillAction(false));
                    }
                }
            }
        }
        public void DestorySkill(Playable playable)
        {
            var graph = playable.GetGraph();
            mixer.DisconnectInput(0);
            graph.DestroySubgraph(playable);
            mixer.SetInputCount(0);
        }
        public void BeginBlend()
        {
            var owner = characterPlayable.GetBehaviour();
            owner.SetWeight(1, 0, 0.1);
        }
        public override void OnPlayableDestroy(Playable playable)
        {
            if (character == null)
            {
                return;
            }
            var controller = character.GetComponent<LavenderCharacterControl>();
            if (controller == null)
            {
                return;
            }
            controller.SkillAction -= ReleaseSkill;
            if (controller.editorControl)
            {
                controller.editorControl.SkillAction -= ReleaseSkill;
            }
        }
    }
}