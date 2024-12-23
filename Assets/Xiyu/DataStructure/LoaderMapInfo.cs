using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Xiyu.DataStructure
{
    [System.Serializable]
    public struct LoaderMapInfo
    {
        [SerializeField] private bool isAllMap;
        [SerializeField] private AssetLabelReference mapLabelReference;
        [SerializeField] private LoaderMapItem[] loaderMapItems;

        public bool IsAllMap => isAllMap;
        public string MapLabel => mapLabelReference.labelString;
        public LoaderMapItem[] LoaderMapItems => loaderMapItems;
    }
}