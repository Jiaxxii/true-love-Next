using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Xiyu.DataStructure
{
    [System.Serializable]
    public struct LoaderSpriteInfo
    {
        [SerializeField] private bool allLoader;
        [SerializeField] private string[] resourceCode;

        public bool AllLoader => allLoader;
        public string[] ResourceCode => resourceCode;
    }
}