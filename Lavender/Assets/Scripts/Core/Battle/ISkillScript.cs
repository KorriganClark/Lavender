using System;
using System.Collections.Generic;
namespace Lavender.Cardinal
{
    public interface ISkillScript
    {
        string SkillName
        {
            get;
        }
        SkillPlayableAsset SkillAsset
        {
            get;
            set;
        }
        float SkillTime
        {
            get;
            set;
        }
        int ChannelNum
        {
            get;
        }
        List<string> Channels
        {
            get;
        }
        void OnSkillBegin();
        void OnSkillStop();
        void OnSkillSucceed();

    }
}