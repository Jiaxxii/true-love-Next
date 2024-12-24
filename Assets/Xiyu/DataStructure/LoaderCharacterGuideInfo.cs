namespace Xiyu.DataStructure
{
    public readonly struct LoaderCharacterGuideInfo
    {
        public LoaderCharacterGuideInfo(string characterCode, string bodyCode, string emotionCode, LightweightRectTransform transform,
            LoaderOptionSelectedInfo optionSelected)
        {
            CharacterCode = characterCode;
            BodyCode = bodyCode;
            EmotionCode = emotionCode;
            Transform = transform;
            OptionSelected = optionSelected;
        }

        public string CharacterCode { get; }
        public string BodyCode { get; }
        public string EmotionCode { get; }
        public LightweightRectTransform Transform { get; }
        public LoaderOptionSelectedInfo OptionSelected { get; }


        public static LoaderCharacterGuideInfo None => new(string.Empty, string.Empty, string.Empty, LightweightRectTransform.Zero, LoaderOptionSelectedInfo.None);
        
        public static bool IsNone(LoaderCharacterGuideInfo loaderCharacterGuideInfo)
        {
            return string.IsNullOrEmpty(loaderCharacterGuideInfo.BodyCode) ||
                   string.IsNullOrEmpty(loaderCharacterGuideInfo.CharacterCode) ||
                   string.IsNullOrEmpty(loaderCharacterGuideInfo.EmotionCode) ||
                   LightweightRectTransform.IsZero(loaderCharacterGuideInfo.Transform) ||
                   LoaderOptionSelectedInfo.IsNone(loaderCharacterGuideInfo.OptionSelected);
        }
    }
}