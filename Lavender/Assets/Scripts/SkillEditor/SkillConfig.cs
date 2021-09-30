using Lavender;
using Lavender.Cardinal;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[Serializable]
public class SkillConfig : SerializedScriptableObject
{
    #region Odin相关变量
    protected const string VERTICAL_GROUP_LEFT = "Split/Left";
    protected const string VERTICAL_GROUP_RIGHT = "Split/Right";

    [HorizontalGroup("Split", 0.3f, LabelWidth = 100)]

    /// <summary>
    /// 当前选择的Prefab
    /// </summary>
    [BoxGroup(VERTICAL_GROUP_LEFT)]
    [ShowInInspector]
    [AssetsOnly]
    [ValidateInput("IsValidPrefab", "请选择正确格式的Prefab文件,且其中包含了AnimClips")]
    [OnValueChanged("Reload")]
    [PreviewField(0.0f, Alignment = ObjectFieldAlignment.Center)]
    public GameObject Model;

    [BoxGroup(VERTICAL_GROUP_LEFT)]
    public string SkillName;

    [BoxGroup(VERTICAL_GROUP_LEFT)]
    [Button("重命名")]
    public void ChangeSkillName()
    {
        Rename();
    }

    [BoxGroup(VERTICAL_GROUP_LEFT)]
    [Button("生成PlayableAsset")]
    public void CreatePlayableAsset()
    {
        var skillAsset = new SkillPlayableAsset();// create instance
        skillAsset.config = this;
        skillAsset.ImportConfig();
        UnityEditor.AssetDatabase.CreateAsset(skillAsset, "Assets/Editor/SkillEditor/Skills/" + SkillName + ".playable");
        return;
    }

    [BoxGroup(VERTICAL_GROUP_LEFT)]
    public bool stopMoveWhenSkill;

    [BoxGroup(VERTICAL_GROUP_LEFT)]
    public ISkillScript script;

    [BoxGroup(VERTICAL_GROUP_RIGHT)]
    [OnValueChanged("OnDataChanged", includeChildren: true)]
    public List<AnimationClip> AnimEntrepot;

    [BoxGroup(VERTICAL_GROUP_RIGHT)]
    public List<AudioClip> AudioEntrepot;

    [BoxGroup(VERTICAL_GROUP_RIGHT)]
    [OnValueChanged("SetChosedVal", includeChildren: true)]
    public LavenderClip Detail;

    //[BoxGroup(VERTICAL_GROUP_RIGHT)]
    //[FoldoutGroup("AudioList", expanded: true)]
    //[AssetList(AutoPopulate = true)]
    //[ReadOnly]
    //[HideInInspector]
    //public List<AudioClip> AudioList;

    #endregion

    #region Odin相关函数

    private void Rename()
    {
        DestroyImmediate(SkillEditor.showModel);
        string path = AssetDatabase.GetAssetPath(this);
        AssetDatabase.RenameAsset(path, SkillName);
    }
    /// <summary>
    /// 判断选择的GameObject是否合理
    /// 
    /// 增加预制体中AnimClips的判断
    /// </summary>
    /// <param name="ChosedModel"></param>
    /// <returns></returns>
    private bool IsValidPrefab(GameObject ChosedModel)
    {
        Regex regex = new Regex("^Assets");
        if (ChosedModel != null)
        {
            if (!PrefabUtility.IsPartOfPrefabAsset(ChosedModel) || !regex.IsMatch(AssetDatabase.GetAssetPath(ChosedModel)))
            {
                return false;
            }
            if (!Model.GetComponent<AnimClips>())
            {
                Model.AddComponent<AnimClips>();
                return true;
            }
        }
        return true;
    }


    public void Reload()
    {
        if (ownerProcessor && ReferenceEquals(ownerProcessor.skillConfig, this) && IsValidPrefab(Model))
        {
            ownerProcessor.LoadSkillConfig();
        }
    }

    /// <summary>
    /// 加载Prefab中保存的Animations，如果Asset中保存有动画片段，弹出询问是否保留原动画片段
    /// </summary>
    public void ImportAnimations()
    {
        if (Model == null)
        {
            return;
        }
        if (Model.GetComponent<AnimClips>())
        {
            AnimEntrepot = Model.GetComponent<AnimClips>().clips;
        }
        else
        {
            AnimEntrepot = new List<AnimationClip>();
        }
    }
    #endregion

    //    动画片段仓库修改，已丢弃
    //    public void CheckEntrepot()
    //     {
    //         if (AnimEntrepot.Count > Model.GetComponent<AnimClips>().lavenderClips.Count)
    //         {
    //             Model.GetComponent<AnimClips>().lavenderClips.Clear();
    //             foreach(var clip in AnimEntrepot)
    //             {
    //                 Model.GetComponent<AnimClips>().lavenderClips.Add(clip);
    //             }
    //         }
    //         else
    //         {
    //             ImportAnimations();
    //         }
    //     }
    //     public void ChangeEntrepot()
    //     {
    // 
    //     }
    [HideInInspector]
    public SkillEditor ownerProcessor;
    [OnValueChanged("OnDataChanged")]
    //[HideInInspector]
    public List<List<LavenderClip>> LavenderClips = new List<List<LavenderClip>>();
    [OnValueChanged("OnDataChanged")]
    [ReadOnly]
    [HideInInspector]
    public List<TrackType> trackTypes = new List<TrackType>();
    [ReadOnly]
    public Dictionary<string, Transform> joints;

    public SkillConfig()
    {
        if (LavenderClips != null)
        {
            LavenderClips.Add(new List<LavenderClip>());
        }
        if (trackTypes != null)
        {
            trackTypes.Add(TrackType.Anim);
        }
        //AudioList = new List<AudioClip>();
        AudioEntrepot = new List<AudioClip>();
        joints = new Dictionary<string, Transform>();
    }
    public void LoadJoints(GameObject showModel)
    {
        if (joints == null || showModel == null)
        {
            return;
        }
        joints.Clear();
        LoadJointsFromObj(showModel.transform);
        foreach (var clips in LavenderClips)
        {
            foreach (var clip in clips)
            {
                if (clip.prefabClip)
                {
                    clip.joints = joints;
                    clip.GetTransformByName();
                }
                if (clip.isMove)
                {
                    clip.baseTransforms = ownerProcessor.monsterControl.monsterTransforms;
                    clip.baseNames = ownerProcessor.monsterControl.monsterNames;
                }
            }
        }
    }
    public void LoadJointsFromObj(Transform parent)
    {
        if (joints.ContainsKey(parent.name))
        {
            return;
        }
        joints.Add(parent.name, parent);
        for (int i = 0; i < parent.childCount; i++)
        {
            LoadJointsFromObj(parent.GetChild(i));
        }
    }
    public bool CheckValid()
    {
        if (LavenderClips == null || trackTypes == null)
        {
            return false;
        }
        if (LavenderClips.Count != trackTypes.Count)
        {
            return false;
        }
        if (LavenderClips.Count <= 0)
        {
            return false;
        }
        return true;
    }
    public void OnDataChanged()
    {
        ownerProcessor.DataSetDirty();
    }
    public void SaveCurve()
    {

    }
    public void InitCurve()
    {
        foreach (var clips in LavenderClips)
        {
            foreach (var clip in clips)
            {
                if (clip.moveConfig != null)
                {
                    clip.moveConfig.InitCurve();
                }
            }
        }
    }
    public void SetChosedVal()
    {
        ownerProcessor.SaveChosedVal();
    }
    //public List<LavenderClip> AnimClips;

    //     /// 测试用获取动画片段，已丢弃 
    //     /// <summary>
    //     /// 以AnimationClip的形式获取动画片段
    //     /// </summary>
    //     /// <returns></returns>
    //     public List<AnimationClip> GetClipAsAnimations()
    //     {
    //         List<AnimationClip> Res = new List<AnimationClip>();
    //         foreach(var clip in LavenderClips)
    //         {
    //             Res.Add(clip.GetAnimClip());
    //         }
    //         return Res;
    //     }
    /// <summary>
    /// 根据传入的动画片段按自动衔接的方式生成LavenderClip
    /// </summary>
    /// <param name="clips"></param>
    //     public void SetClipsByAnimations(List<AnimationClip> lavenderClips)
    //     {
    //         LavenderClips.Clear();
    //         float lastEndTime = 0;
    //         foreach(var clip in lavenderClips)
    //         {
    //             LavenderClip lClip = new LavenderClip(clip);
    //             lClip.startTime = lastEndTime;
    //             lClip.endTime += lastEndTime;
    //             lastEndTime = lClip.endTime;
    //             LavenderClips.Add(lClip);
    //         }
    //         Reload();
    //     }
}
