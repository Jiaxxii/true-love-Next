using System;

namespace Xiyu.DataStructure
{
    [Serializable]
    public struct LoaderRuleMapInfo
    {
        [UnityEngine.SerializeField] private LoaderMapItem[] loaderMapInfos;

        public LoaderMapItem[] LoaderMapInfos => loaderMapInfos;
    }
}