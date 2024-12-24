using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Xiyu.DataStructure
{
    [System.Serializable]
    public struct LoaderCharacterInfo
    {
        public LoaderCharacterInfo(string characterCode, string characterLabelReference, LoaderSpriteInfo bodySpriteInfo, LoaderSpriteInfo faceSpriteInfo)
        {
            this.characterCode = characterCode;
            this.characterLabelReference = new AssetLabelReference
            {
                labelString = characterLabelReference
            };
            this.bodySpriteInfo = bodySpriteInfo;
            this.faceSpriteInfo = faceSpriteInfo;
        }

        [SerializeField] private string characterCode;
        [SerializeField] private AssetLabelReference characterLabelReference;

        [Space(5)] [SerializeField] private LoaderSpriteInfo bodySpriteInfo;
        [Space(5)] [SerializeField] private LoaderSpriteInfo faceSpriteInfo;

        public string CharacterCode => characterCode;

        public string CharacterLabel => characterLabelReference.labelString;
        public LoaderSpriteInfo BodySpriteInfo => bodySpriteInfo;
        public LoaderSpriteInfo FaceSpriteInfo => faceSpriteInfo;
    }
}