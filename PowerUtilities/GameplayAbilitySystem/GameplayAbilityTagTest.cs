#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public class GameplayAbilityTagTest : MonoBehaviour
    {
        public string curTags;
        public GameplayAbilityTag abilityTag = new();

        public string addTag;
        [EditorButton(onClickCall = "AddTag")]
        public bool isAddTag;

        public string removeTag;
        [EditorButton(onClickCall = "RemoveTag")]
        public bool isRemoveTag;

        [EditorButton(onClickCall = "OpenSetting")]
        public bool isOpenSetting;

        void AddTag()
        {
            abilityTag.AddTag(addTag);

            curTags = abilityTag.ToString();
        }

        void RemoveTag()
        {
            abilityTag.RemoveTag(removeTag);
            curTags = abilityTag.ToString();
        }

        void OpenSetting()
        {
            SettingsService.OpenProjectSettings(ProjectSettingGroupAttribute.POWER_UTILS + "/Gameplay/GameplayAbilityTag");
        }
    }
}
#endif