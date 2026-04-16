using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    //[Flags]
    //public enum GameplayAbilityTag
    //{
    //    Health = 0,
    //    Weak = 1 << 0,
    //    Fired = 1 << 1,
    //    Frozen = 1 << 2,
    //    Blooded = 1 << 3,
    //    SpeedDown = 1 << 4,
    //    SpeedUp = 1 << 5,
    //    Chaos = 1 << 6,
    //    Sleep = 1 << 7, 
    //}

    /// <summary>
    /// GameplayAbilityTag can be used for multiple states, such as Health tag can be used for both health state and shield state, so use string instead of enum
    /// 
    /// edit tag use GameplayAbilityTagSO, 
    /// then use string to add/remove tag in GameplayAbilityTag, and use HasTag to check tag in GameplayAbilitySystem
    /// </summary>
    public class GameplayAbilityTag
    {
        public Action<GameplayTagInfo> onAddTag;
        public Action<GameplayTagInfo> onRemoveTag;
        public Action<GameplayTagInfo> onTagChange;

        private static GameplayAbilityTagSO tagSO;
        public static GameplayAbilityTagSO TagSO
        {
            get {
                if (tagSO == null)
                {
                    tagSO = Resources.Load<GameplayAbilityTagSO>("Gameplay/GameplayAbilityTag");
                }
                return tagSO;
            }
            //set => tagSO = value;
        }

        /// <summary>
        /// Get preset tag dict, edit in GameplayAbilityTagSO
        /// </summary>
        public Dictionary<string, GameplayTagInfo> PresetTagDict => TagSO.GetPresetTagDict();
        /// <summary>
        /// character current tag dict
        /// key is tag, value is tag state, such as timeStamp, can be used for tag duration or cooldown
        /// </summary>
        public readonly Dictionary<string, GameplayTagInfo> curTagDict = new();

        List<GameplayTagInfo> removeTagList = new();

        public void CheckTagValid(string tag)
        {
            if (!PresetTagDict.ContainsKey(tag))
                throw new InvalidOperationException($"{tag} not exists,edit in ProjectSettings/Gameplay/GameplayAbilityTagSO");
        }

        public bool HasTag(string tag)
        {
            CheckTagValid(tag);
            return curTagDict.ContainsKey(tag);
        }
        public void AddTag(string tag)
        {
            CheckTagValid(tag);
            curTagDict.Add(tag, PresetTagDict[tag].Clone());
            onAddTag?.Invoke(curTagDict[tag]);
        }

        public void RemoveTag(string tag)
        {
            CheckTagValid(tag);
            if (curTagDict.ContainsKey(tag))
            {
                var tagState = curTagDict[tag];
                curTagDict.Remove(tag);
                onRemoveTag?.Invoke(tagState);
            }
        }
        /// <summary>
        /// Update tag state, ifLifeTime is over, remove tag
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <remarks>removed tag info</remarks>
        public GameplayTagInfo UpdateTagState(string tag, bool isRemoveTagWhenLifeTimeOver)
        {
            CheckTagValid(tag);
            if (!curTagDict.ContainsKey(tag) || !curTagDict[tag].IsCooldownTag)
                return null;

            curTagDict[tag].life -= Time.deltaTime;
            if (curTagDict[tag].life <= 0)
            {
                if (isRemoveTagWhenLifeTimeOver)
                    RemoveTag(tag);
                return curTagDict[tag];
            }
            else
            {
                onTagChange?.Invoke(curTagDict[tag]);
            }
            return null;
        }
        /// <summary>
        /// call in Mono.Update, 
        /// so can make sure tag state is always up to date, and tag will be automatically removed when duration end.
        /// </summary>
        public void UpdateAllTagState()
        {
            removeTagList.Clear();
            foreach (var tag in curTagDict.Keys)
            {
                var removedTagInfo = UpdateTagState(tag, false);
                if (removedTagInfo != null)
                {
                    removeTagList.Add(removedTagInfo);
                }
            }
            
            foreach (var tagInfo in removeTagList)
            {
                RemoveTag(tagInfo.tagName);
            }
        }

        public void Dispose()
        {
            curTagDict.Clear();
            onAddTag = null;
            onRemoveTag = null;
            onTagChange = null;
        }

        public override string ToString()
        {
            return string.Join(", ", curTagDict.Values);
        }
    }
}
