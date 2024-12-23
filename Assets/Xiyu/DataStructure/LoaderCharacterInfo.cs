using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Xiyu.DataStructure
{
    [System.Serializable]
    public struct LoaderCharacterInfo
    {
        [SerializeField] private string characterCode;
        [SerializeField] private AssetLabelReference characterLabelReference;

        [Space(5)]
        [SerializeField] private LoaderSpriteInfo bodySpriteInfo;
        [Space(5)]
        [SerializeField] private LoaderSpriteInfo faceSpriteInfo;

        public string CharacterCode => characterCode;
        
        public string CharacterLabel => characterLabelReference.labelString;
        public LoaderSpriteInfo BodySpriteInfo => bodySpriteInfo;
        public LoaderSpriteInfo FaceSpriteInfo => faceSpriteInfo;
    }
}