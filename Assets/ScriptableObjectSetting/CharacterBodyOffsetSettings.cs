using UnityEngine;
using Xiyu.DataStructure;

namespace ScriptableObjectSetting
{
    [CreateAssetMenu(fileName = "BodyOffsetSettings", menuName = "ScriptableObject/角色身体偏移配置信息", order = 0)]
    public class CharacterBodyOffsetSettings : CharacterInfoSettings<LightweightRectTransform>
    {
    }
}