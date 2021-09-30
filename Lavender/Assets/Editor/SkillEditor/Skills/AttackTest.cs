using System.Collections;
using System.Collections.Generic;
using Lavender;
using Lavender.Cardinal;
using UnityEngine;

public class AttackTest : ISkillScript
{
    private string skillName = "AttackTest";
    public string SkillName
    {
        get
        {
            return skillName;
        }
    }
    private SkillPlayableAsset skillPlayable;
    public SkillPlayableAsset SkillAsset { get { return skillPlayable; } set => skillPlayable = value; }
    private float skillTime = 2;
    public float SkillTime
    {
        get
        {
            return skillTime;
        }
        set
        {
            skillTime = value;
        }
    }
    public int ChannelNum
    {
        get
        {
            return Channels.Count;
        }
    }

    public List<string> Channels
    {
        get
        {
            return new List<string>();
        }
    }

    public void OnSkillBegin()
    {

    }

    public void OnSkillStop()
    {
       
    }

    public void OnSkillSucceed()
    {
        
    }
}
