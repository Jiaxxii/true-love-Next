using UnityEngine;
using UnityEngine.AddressableAssets;
using Xiyu.DataStructure;

namespace ScriptableObjectSettings
{
    [CreateAssetMenu(fileName = "SceneAssetsSettings", menuName = "ScriptableObject/场景加载资源信息", order = 0)]
    public sealed class SceneAssetsSettings : ScriptableObject
    {
        [SerializeField] private LoaderMapInfo loaderMapInfo;

        [Header("标签引用")] [SerializeField] private AssetLabelReference bodyLabelReference;
        [SerializeField] private AssetLabelReference faceLabelReference;

        [Header("角色信息")] [SerializeField] private LoaderCharacterInfo[] loaderCharacterInfo;

        public LoaderMapInfo LoaderMapInfo => loaderMapInfo;

        public string BodyLabel => bodyLabelReference.labelString;
        public string FaceLabel => faceLabelReference.labelString;
        public LoaderCharacterInfo[] LoaderCharacterInfo => loaderCharacterInfo;
    }
}