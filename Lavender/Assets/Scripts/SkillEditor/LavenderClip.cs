using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Sirenix.OdinInspector;
using Lavender.Cardinal;

[Serializable]
public class LavenderClip : IComparable<LavenderClip>
{
    [SerializeField]
    [ShowIf("isAnim")]
    public AnimationClip animClip;
    [ShowIf("isAudio")]
    public AudioClip audioClip;
    [ShowIf("isPartical")]
    public ParticleSystem prefabClip;
    [ShowIf("isAttack")]
    [OnValueChanged("SetDirty", includeChildren: true)]
    //[InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    public AttackField attackClip;
    public Lavender.LavenderUnitMoveControl.MoveUnit moveClip;
    [ShowIf("isMove")]
    public MoveConfig moveConfig;
    [HideInInspector]
    public bool isAnim = false;
    [HideInInspector]
    public bool isAudio = false;
    [HideInInspector]
    public bool isPartical = false;

    [HideInInspector]
    public bool isAttack = false;
    [HideInInspector]
    public bool isMove = false;

    [SerializeField]
    public float startTime, length, endTime;
    [ValueDropdown("jointsNames")]
    [OnValueChanged("GetTransformByName")]
    [ShowIf("@isPartical||isAttack||isMove")]
    public string transformName;
    public IEnumerable<String> jointsNames;

    [ShowIf("@isPartical||isAttack||isMove")]
    public Dictionary<string, Transform> joints;
    [ShowIf("@isPartical||isAttack||isMove")]
    [OnValueChanged("ChangeTransFormToField", includeChildren: true)]
    public Transform lavenderTransForm;

    public IEnumerable<String> baseNames;
    [ValueDropdown("baseNames")]
    [OnValueChanged("GetTransformByName")]
    [ShowIf("isMove")]
    public string baseTransformName;
    /// <summary>
    /// 目标transform
    /// </summary>
    [ShowIf("isMove")]
    public Dictionary<string, Transform> baseTransforms = new Dictionary<string, Transform>();
    [ShowIf("isMove")]
    //[OnValueChanged("ChangeTransFormToField", includeChildren: true)]
    public Transform baseTransForm;

    [OnValueChanged("ModifyParticle")]
    [ShowIf("isPartical")]
    public ParticleConfig ParticleDetail;
    [HideInInspector]
    public SkillConfig ownerConfig;
    //[ShowDrawerChain]
    [LabelWidth(60f)]
    [SerializeField, Range(0.1f, 10)]
    public float rate;
    [HideInInspector]
    public float weight, dynamicWeight;
    public float cutStartTime, cutEndTime;//剪辑时间相对于单个动画片段而言，为偏移量
    public float ScaledStart
    {
        get
        {
            return startTime;
        }
        set
        {
            startTime = value;
            endTime = value + length;
            //ModifyParticle();
        }
    }
    public float ScaledEnd
    {
        get
        {
            return startTime + length / rate;
        }
        set
        {
            startTime = value - length / rate;
            endTime = startTime + length;
        }
    }
    public float ScaledValidStart
    {
        get
        {
            return startTime + cutStartTime / rate;
        }
        set
        {
            startTime = value - cutStartTime / rate;
            endTime = value + length;
            //moveConfig.time = ScaledValidEnd - ScaledValidStart;
        }
    }
    public float ScaledValidEnd
    {
        get
        {
            return ScaledEnd - cutEndTime / rate;
        }
        set
        {
            ScaledEnd = value + cutEndTime / rate;
            //moveConfig.time = ScaledValidEnd - ScaledValidStart;
        }
    }
    public LavenderClip(AnimationClip animClp)
    {
        this.animClip = animClp;
        this.rate = 1;
        this.startTime = 0;
        this.cutStartTime = 0;
        this.length = animClp.length;
        this.endTime = animClp.length;
        this.cutEndTime = 0;
        this.weight = 1;
        this.isAnim = true;
    }
    public LavenderClip(AudioClip audioClp)
    {
        this.audioClip = audioClp;
        this.rate = 1;
        this.startTime = 0;
        this.cutStartTime = 0;
        this.length = audioClp.length;
        this.endTime = audioClp.length;
        this.cutEndTime = 0;
        this.weight = 1;
        this.isAudio = true;
    }
    public LavenderClip(ParticleSystem prefabClp)
    {
        this.prefabClip = prefabClp;
        this.rate = 1;
        this.startTime = 0;
        this.cutStartTime = 0;
        this.length = prefabClp.main.duration;
        this.endTime = this.length;
        this.cutEndTime = 0;
        this.weight = 1;
        this.isPartical = true;
        GetTransformByName();
        InitParticleConfig();
    }

    public LavenderClip(AttackField attack)
    {
        this.attackClip = attack;
        this.rate = 1;
        this.startTime = 0;
        this.cutStartTime = 0;
        this.length = 1;
        this.endTime = 1;
        this.cutEndTime = 0;
        this.weight = 1;
        this.isAttack = true;
        GetTransformByName();
    }

    public LavenderClip(Lavender.LavenderUnitMoveControl.MoveUnit move)
    {
        this.moveClip = move;
        this.rate = 1;
        this.startTime = 0;
        this.cutStartTime = 0;
        this.length = 1;
        this.endTime = 1;
        this.cutEndTime = 0;
        this.weight = 1;
        this.isMove = true;
        GetTransformByName();
    }

    public AnimationClip GetAnimClip()
    {
        return animClip;
    }

    public AudioClip GetAudioClip()
    {
        return audioClip;
    }
    public string GetClipName()
    {
        if (animClip)
        {
            return animClip.name;
        }
        if (audioClip)
        {
            return audioClip.name;
        }
        return null;
    }
    public void GetTransformByName()
    {
        if (joints == null)
        {
            if (ownerConfig != null)
            {
                ownerConfig.LoadJoints(SkillEditor.showModel);
            }
        }
        if (transformName == null && joints != null)
        {
            foreach (var fir in joints)
            {
                lavenderTransForm = fir.Value;
                ChangeTransFormToField();
                break;
            }
            //lavenderTransForm = Model.GetComponent<Transform>();
        }
        else if (joints != null&&transformName.Length>0)
        {
            if(joints.TryGetValue(transformName,out lavenderTransForm))
            {
                ChangeTransFormToField();
            }
        }
        if (baseTransforms != null && baseTransforms.Count != 0 && baseTransformName != null)
        {
            baseTransforms.TryGetValue(baseTransformName, out baseTransForm);
            //baseTransForm = baseTransforms[baseTransformName];
        }
        //         if (ownerConfig.ownerProcessor.Director != null)
        //         {
        //             ownerConfig.ownerProcessor.Director.Refresh();//.ModifyParticle(this, ParticleDetail);
        //         }
        //if (ownerConfig && ownerConfig.ownerProcessor && ownerConfig.ownerProcessor.PlayGraph)
        ///if (ownerConfig.ownerProcessor.PlayGraph)
        {
            //    ownerConfig.ownerProcessor.Director.ChangeParticleTransform(this, lavenderTransForm);
        }
    }
    public void ModifyParticle()
    {
        //         if (ParticleDetail != null)
        //         {
        //             if (ownerConfig.ownerProcessor.Director != null)
        //             {
        //                 ownerConfig.ownerProcessor.Director.Refresh();//.ModifyParticle(this, ParticleDetail);
        //             }
        //         }
    }
    public void ChangeTransFormToField()
    {
        if (attackClip != null)
        {
            attackClip.Init(lavenderTransForm);
        }
    }
    public void SetDirty()
    {
        ownerConfig.OnDataChanged();
    }
    public void InitParticleConfig()
    {
        if (ParticleDetail == null)
        {
            ParticleDetail = new ParticleConfig();
        }
        ParticleDetail.relativePosition = new Vector3(0, 0, 0);
        ParticleDetail.relativeRotation = new Quaternion(0, 0, 0, 0);
        ParticleDetail.owner = this;
    }
    /// <summary>
    /// 外部时间是否落在有效区间，时间为缩放后
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool ContainsTime(double time)
    {
        return time > ScaledValidStart - float.Epsilon && time < ScaledValidEnd + float.Epsilon;
    }
    /// <summary>
    /// 去除时间缩放后的本地时间
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public double ToLocalTime(double time)
    {
        return (time - ScaledStart) / rate;
    }
    public int CompareTo(LavenderClip p)
    {
        if (ScaledStart < p.ScaledStart)
        {
            return -1;
        }
        else if (ScaledStart == p.ScaledStart)
        {
            if (ScaledEnd < p.ScaledEnd)
            {
                return -1;
            }
            else if (ScaledEnd == p.ScaledEnd)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
        else
        {
            return 1;
        }
    }
    [Serializable]
    public class MoveConfig
    {
        public bool followBase = false;
        public bool useWorldPos = false;
        public bool setOriPosition = false;
        [ShowIf("setOriPosition")]
        public Vector3 originPosition;
        public Vector3 targetPosition;
        public bool editRotation;
        public bool useCurve;
        [ShowIf("useCurve")]
        //[OnValueChanged("UpdateKey")]
        
        public AnimationCurve animationCurve;
        public Keyframe[] keys;
        public void UpdateKey()
        {
            if (animationCurve != null)
            {
                keys = animationCurve.keys;
                //owner.SetDirty();
            }
        }
        public void InitCurve()
        {
            if (keys != null)
            {
                //Debug.Log(keys);
                //animationCurve = new AnimationCurve(keys);
            }
        }
    }
}

