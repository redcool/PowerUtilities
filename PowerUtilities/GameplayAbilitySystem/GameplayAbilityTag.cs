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
        /// project all tag set, edit in GameplayAbilityTagSO
        /// </summary>
        public HashSet<string> GameTagSet => TagSO.GetTagSet();
        /// <summary>
        /// character current tag set
        /// </summary>
        public HashSet<string> curTagSet = new HashSet<string>();

        public void CheckTagValid(string tag)
        {
            if (!GameTagSet.Contains(tag))
                throw new InvalidOperationException($"{tag} not exists,edit in ProjectSettings/Gameplay/GameplayAbilityTagSO");
        }

        public bool HasTag(string tag)
        {
            CheckTagValid(tag);
            return curTagSet.Contains(tag);
        }
        public void AddTag(string tag)
        {
            CheckTagValid(tag);
            curTagSet.Add(tag);
        }
        public void RemoveTag(string tag)
        {
            CheckTagValid(tag);
            curTagSet.Remove(tag);
        }

        public override string ToString()
        {
            return string.Join(", ", curTagSet);
        }
    }
}
