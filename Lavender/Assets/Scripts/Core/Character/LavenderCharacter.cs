using System;
using System.Collections;
using System.Collections.Generic;
using Lavender;
using Lavender.Cardinal;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
[CreateAssetMenu(fileName = "CharacterAsset", menuName = "CreateCharacter", order = 0)]
public class LavenderCharacter : SerializedScriptableObject
{
    [BoxGroup("属性")]
    public GameObject model;
    [BoxGroup("属性")]
    public string characterName;
    [BoxGroup("属性")]
    [Button("重命名")]
    private void Rename()
    {
        DestroyImmediate(SkillEditor.showModel);
        string path = AssetDatabase.GetAssetPath(this);
        AssetDatabase.RenameAsset(path, characterName);
    }
    [BoxGroup("属性")]
    public float health;
    [BoxGroup("属性")]
    public float attack;
    [BoxGroup("属性")]
    public AnimationClip attackedAnim;
    [BoxGroup("技能")]
    [OnValueChanged("UpdateData", includeChildren: true)]
    public Dictionary<KeyCode, SkillPlayableAsset> skills;

    [BoxGroup("控制组件")]
    [InlineEditor(Expanded = true)]
    [OnValueChanged("UpdateData", includeChildren: true)]
    public CharacterPlayableAsset characterPlayableAsset;

    [BoxGroup("控制组件")]
    [InlineEditor(Expanded = true)]
    [HideInInspector]
    public CharacterMovePlayableAsset moveAsset;
    [BoxGroup("MoveAnim")]
    [HideInInspector]
    public List<AnimationClip> moveAnim;
    [BoxGroup("控制组件")]
    [OnValueChanged("UpdateMoveAsset", includeChildren: true)]
    public Dictionary<MoveState, AnimationClip> moveAnimClip = new Dictionary<MoveState, AnimationClip>();
    [HideInInspector]
    public CharacterEditorControl control;
    public LavenderCharacter()
    {
        skills = new Dictionary<KeyCode, SkillPlayableAsset>();
    }
    public void UpdateMoveAsset()
    {
        moveAsset = characterPlayableAsset.movePlayableAsset;
        var animclip = moveAsset.moveClips;
        animclip.Clear();
        foreach (var pair in moveAnimClip)
        {
            var key = (int)pair.Key;
            while (key >= animclip.Count)
            {
                animclip.Add(null);
            }
            animclip[key] = pair.Value;
        }
        var path = AssetDatabase.GetAssetPath(characterPlayableAsset);
        var target = AssetDatabase.LoadAssetAtPath<CharacterPlayableAsset>(path);
        EditorUtility.SetDirty(target);
        EditorUtility.SetDirty(moveAsset);
        Debug.Log("setdirty");
        AssetDatabase.SaveAssets();
        UpdateData();

    }
    public void UpdateData()
    {
        if (control == null)
        {
            return;
        }
        control.ImportSkill(skills);
        control.Refresh();
        //EditorUtility.SetDirty(this);
    }
    public GameObject CreateCharacterInWorld(Vector3 position, bool isPlayer)
    {
        var res = Instantiate(model, position, new Quaternion());
        var camObj = new GameObject();
        var cam = camObj.AddComponent<Camera>();
        cam.depth = -1;
        cam.targetDisplay = 0;
        cam.transform.SetParent(res.transform);
        cam.transform.localPosition = new Vector3(0.05f, 1.21f, -2.7f);
        var cMove = camObj.AddComponent<CameraMove>();
        cMove.Target = res.transform;
        var cControl = camObj.AddComponent<CameraController>();
        cControl.Target = res.transform;
        camObj.AddComponent<AudioListener>();
        res.AddComponent<AudioSource>();
        if (isPlayer)
        {
            //var chaController = res.AddComponent<CharacterController>();
            //chaController.center = new Vector3(0, 1.03f, 0);
        }
        else
        {
            var collider = res.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0, 1.03f, 0);
            collider.height = 2;
            res.AddComponent<Rigidbody>();
        }
        var box = res.AddComponent<Box>();
        box.center = new Vector3(1.03f, 0, 0);
        box.size = new Vector3(2, 1, 1);
        LavenderPhysicSystem.AddShape(box);
        var director = res.AddComponent<PlayableDirector>();
        director.playableAsset = characterPlayableAsset;
        var controller = res.AddComponent<LavenderCharacterControl>();
        controller.skills = skills;
        controller.isPlayer = isPlayer;
        controller.camObj = camObj;
        var cardinal = res.AddComponent<CardinalSystem>();
        cardinal.beAttackAnim = attackedAnim;
        var unitMove = res.AddComponent<LavenderUnitMoveControl>();
        var animator = res.GetComponent<Animator>();
        if (animator)
        {
            animator.applyRootMotion = false;
        }
        return res;
    }
}

