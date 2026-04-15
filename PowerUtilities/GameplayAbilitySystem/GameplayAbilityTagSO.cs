using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Gameplay/GameplayAbilityTag", isUseUIElment = false)]
    [SOAssetPath("Assets/PowerUtilities/Resources/Gameplay/GameplayAbilityTag.asset")]
    public class GameplayAbilityTagSO : ScriptableObject
    {
        [Serializable]
        public class TagInfo
        {
            public string tagName;
            public string desc;
        }

        [ListItemDraw("tagName:,tagName,desc:,desc", "50,200,50,")]
        public List<TagInfo> tagInfos = new List<TagInfo>()
        {
            new TagInfo(){ tagName = "Health", desc = "nothing"},
        };

        [Header("text ")]
        [Multiline(6)]
        public string abilityTagText = @"
            Health,nothing
            Weak,weakstate
            Fired,firedstate
            Frozen,frozenstate
            Blooded,bloodingstate
            Chaos,chaosstate
            Sleep,sleepstate";

        [EditorButton(onClickCall = nameof(ReadTagFromText))]
        public bool isReadTagFromText = false;

        HashSet<string> tagNameSet = new();
        public HashSet<string> GetTagSet()
        {
            if (tagNameSet.Count != tagInfos.Count)
                tagNameSet.Clear();

            for (int i = 0; i < tagInfos.Count; i++)
            {
                tagNameSet.Add(tagInfos[i].tagName);
            }

            return tagNameSet;
        }

        public void ReadTagFromText() => ReadTagFromText(abilityTagText,",");

        public void ReadTagFromText(string abilityTagText, string splitChar=",")
        {
            if (string.IsNullOrEmpty(abilityTagText))
                return;

            var lines = abilityTagText.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            tagInfos.Clear();
            foreach (var line in lines)
            {
                var kv = line.Split(splitChar);
                if (kv.Length != 2)
                    continue;
                tagInfos.Add(new TagInfo() { tagName = kv[0].Trim(), desc = kv[1].Trim() });
            }
        }
    }
}
