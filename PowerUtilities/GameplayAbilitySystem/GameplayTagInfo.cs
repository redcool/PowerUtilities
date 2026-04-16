using System;

namespace PowerUtilities
{
    [Serializable]
    public class GameplayTagInfo
    {
        public string tagName;
        public string desc;

        // if life > 0, tag is in cooldown state, and life is the remaining cooldown time, if life <= 0, tag is not in cooldown state
        public float life = -1;

        /// <summary>
        /// Tag is in cooldown state if life > 0, and life is the remaining cooldown time, if life <= 0, tag is not in cooldown state
        /// tag will be automatically removed when duration end.
        /// 
        /// otherwise need manual remove.
        /// </summary>
        public bool IsCooldownTag => life > 0;

        /// <summary>
        /// clone new instance, but same value, so can be used for add tag to character, and won't change the preset tag info in SO.
        /// </summary>
        /// <returns></returns>
        public GameplayTagInfo Clone()
        {
            return new GameplayTagInfo() { tagName = this.tagName, desc = this.desc, life = this.life };
        }

        public override string ToString()
        {
            return $"({tagName},({life}))";
        }
    }
}
