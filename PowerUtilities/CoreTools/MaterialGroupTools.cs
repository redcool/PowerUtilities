using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Graphs;

namespace PowerUtilities
{
    public static class MaterialGroupTools
    {
        public class GroupInfo
        {
            public bool isOn; // this group unfold?
            public bool hasCheckedMark; // isChecked work when hasCheckedMark is true
            public bool isChecked; //this group enabled?
        }

        public const string DEFAULT_GROUP_NAME = "_";
        public const float BASE_LABLE_WIDTH = 162.5f;

        public readonly static Dictionary<string, GroupInfo> groupInfoDict = new Dictionary<string,GroupInfo>();

        public static bool IsDefaultGroup(string groupName) => string.IsNullOrEmpty(groupName) ||
            groupName == DEFAULT_GROUP_NAME ||
            !groupInfoDict.ContainsKey(groupName);

        public static int GroupIndentLevel(string groupName) => IsDefaultGroup(groupName) ? 0 : 1;


        public static bool IsGroupOn(string groupName)
        {
            // default Group or not Group open always
            if (IsDefaultGroup(groupName))
                return true;

            var info = GetGroupInfo(groupName);
            return info.isOn;
        }

        public static bool IsGroupDisabled(string groupName)
        {
            var info = GetGroupInfo(groupName);
            return info.hasCheckedMark ? !info.isChecked : false;
        }

        public static void SetState(string groupName, bool isOn,bool hasCheckedMark = false,bool isChecked=false)
        {
            var info = GetGroupInfo(groupName);
            info.isChecked = isChecked;
            info.hasCheckedMark = hasCheckedMark;
            info.isOn= isOn;
        }

        public static void SetStateAll(bool isOn,bool hasCheckedMark=false,bool isChecked=false)
        {
            var keys = groupInfoDict.Keys.ToArray();
            foreach (var groupName in keys)
            {
                SetState(groupName,isOn, hasCheckedMark, isChecked);
            }
        }

        public static GroupInfo GetGroupInfo(string groupName)
        {
            if (!groupInfoDict.TryGetValue(groupName, out var groupInfo))
            {
                groupInfo = groupInfoDict[groupName] = new GroupInfo();
            }
            return groupInfo;
        }
    }
}
