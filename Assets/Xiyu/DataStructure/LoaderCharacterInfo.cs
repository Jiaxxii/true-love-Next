using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Xiyu.DataStructure
{
    [System.Serializable]
    public struct LoaderCharacterInfo
    {
        public LoaderCharacterInfo(string characterCode, string characterLabelReference, bool isLoadGuide, LoaderSpriteInfo bodySpriteInfo, LoaderSpriteInfo faceSpriteInfo)
        {
            this.characterCode = characterCode;
            this.characterLabelReference = new AssetLabelReference
            {
                labelString = characterLabelReference
            };
            this.isLoadGuide = isLoadGuide;
            this.bodySpriteInfo = bodySpriteInfo;
            this.faceSpriteInfo = faceSpriteInfo;
        }

        public LoaderCharacterInfo(string characterCode, string characterLabelReference, LoaderSpriteInfo bodySpriteInfo,
            LoaderSpriteInfo faceSpriteInfo)
        {
            this.characterCode = characterCode;
            this.characterLabelReference = new AssetLabelReference
            {
                labelString = characterLabelReference
            };
            isLoadGuide = false;
            this.bodySpriteInfo = bodySpriteInfo;
            this.faceSpriteInfo = faceSpriteInfo;
        }

        [SerializeField] private string characterCode;
        [SerializeField] private AssetLabelReference characterLabelReference;
        [SerializeField] private bool isLoadGuide;


        [Space(5)] [SerializeField] private LoaderSpriteInfo bodySpriteInfo;
        [Space(5)] [SerializeField] private LoaderSpriteInfo faceSpriteInfo;

        public string CharacterCode => characterCode;

        public string CharacterLabel => characterLabelReference.labelString;

        public bool IsLoadGuide => isLoadGuide;

        public LoaderSpriteInfo BodySpriteInfo => bodySpriteInfo;
        public LoaderSpriteInfo FaceSpriteInfo => faceSpriteInfo;
    }
}