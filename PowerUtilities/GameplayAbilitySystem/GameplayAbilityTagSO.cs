using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Use ScriptableObject to edit preset tag info, such as tag name, description, and cooldown duration, so can be used in GameplayAbilitySystem, and can be easily extended by add new tag info in SO without change code.
    /// call Resources.Load("Gameplay/GameplayAbilityTag") to load the asset.
    /// </summary>
    [ProjectSettingGroup(PROJECT_SETTING_GROUP_PATH, isUseUIElment = false)]
    [SOAssetPath("Assets/PowerUtilities/Resources/Gameplay/GameplayAbilityTag.asset")]
    public class GameplayAbilityTagSO : ScriptableObject
    {
        public const string PROJECT_SETTING_GROUP_PATH = ProjectSettingGroupAttribute.POWER_UTILS + "/Gameplay/GameplayAbilityTag";

        [ListItemDraw("tagName:,tagName,desc:,desc,life:,life", "50,200,50,400,50,")]
        public List<GameplayTagInfo> tagInfos = new List<GameplayTagInfo>()
        {
            new GameplayTagInfo(){ tagName = "Health", desc = "nothing"},
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

        Dictionary<string,GameplayTagInfo> tagNameDict = new();
        /// <summary>
        /// Get tag info dict, key is tag name, value is tag info
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, GameplayTagInfo> GetPresetTagDict()
        {
            if (tagNameDict.Count == tagInfos.Count)
                return tagNameDict;

            // if count not match, clear and rebuild dict, so can make sure dict is always up to date with list.
            tagNameDict.Clear();

            for (int i = 0; i < tagInfos.Count; i++)
            {
                tagNameDict.Add(tagInfos[i].tagName, tagInfos[i]);
            }

            return tagNameDict;
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
                if (kv.Length < 1)
                    continue;

                var tag = new GameplayTagInfo() { tagName = kv[0].Trim() };
                if (kv.Length >= 2)
                    tag.desc = kv[1].Trim();
                if (kv.Length >= 3)
                    tag.life = float.TryParse(kv[2].Trim(), out var life) ? life : -1;
                tagInfos.Add(tag);
            }
        }
    }
}
