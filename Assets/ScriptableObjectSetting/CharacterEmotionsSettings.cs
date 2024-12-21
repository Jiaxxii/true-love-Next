using UnityEngine;
using Xiyu.DataStructure;

namespace ScriptableObjectSetting
{
    [CreateAssetMenu(fileName = "EmotionsSettings", menuName = "ScriptableObject/角色表情配置信息", order = 0)]
    public class CharacterEmotionsSettings : CharacterInfoSettings<EmotionInfoItem>
    {
    }
}