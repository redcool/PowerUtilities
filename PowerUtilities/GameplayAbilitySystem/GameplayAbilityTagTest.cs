#if UNITY_EDITOR
using UnityEditor;
#endif
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

        [EditorButton(onClickCall = "Update")]
        public bool isUpdate;

        [Header("Editor only")]
        [EditorButton(onClickCall = "OpenSetting")]
        public bool isOpenSetting;

        void AddTag()
        {
            OnEnable();
            if(string.IsNullOrEmpty(addTag))
            {
                Debug.LogError("addTag is null or empty");
                return;
            }
            abilityTag.AddTag(addTag);

            curTags = abilityTag.ToString();
        }

        public void Update()
        {
            abilityTag.UpdateAllTagState();
            curTags = abilityTag.ToString();

        }
        public void OnEnable()
        {
            abilityTag.onAddTag = (tagInfo) => Debug.Log($"add tag {tagInfo}");
            abilityTag.onRemoveTag = (tagInfo) => Debug.Log($"remove tag {tagInfo}");
            abilityTag.onTagChange = (tagInfo) => Debug.Log($"tag change {tagInfo}");

        }

        void RemoveTag()
        {
            if (string.IsNullOrEmpty(addTag))
            {
                Debug.LogError("addTag is null or empty");
                return;
            }
            abilityTag.RemoveTag(removeTag);
            curTags = abilityTag.ToString();
        }
#if UNITY_EDITOR
        void OpenSetting()
        {
            SettingsService.OpenProjectSettings(GameplayAbilityTagSO.PROJECT_SETTING_GROUP_PATH);
        }
#endif
    }
}