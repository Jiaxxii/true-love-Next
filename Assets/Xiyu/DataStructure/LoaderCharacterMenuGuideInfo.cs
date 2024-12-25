namespace Xiyu.DataStructure
{
    public readonly struct LoaderCharacterMenuGuideInfo
    {
        public LoaderCharacterMenuGuideInfo(string characterCode, string bodyCode, string emotionCode, LightweightRectTransform transform,
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


        public static LoaderCharacterMenuGuideInfo None => new(string.Empty, string.Empty, string.Empty, LightweightRectTransform.Zero, LoaderOptionSelectedInfo.None);
        
        public static bool IsNone(LoaderCharacterMenuGuideInfo loaderCharacterMenuGuideInfo)
        {
            return string.IsNullOrEmpty(loaderCharacterMenuGuideInfo.BodyCode) ||
                   string.IsNullOrEmpty(loaderCharacterMenuGuideInfo.CharacterCode) ||
                   string.IsNullOrEmpty(loaderCharacterMenuGuideInfo.EmotionCode) ||
                   LightweightRectTransform.IsZero(loaderCharacterMenuGuideInfo.Transform) ||
                   LoaderOptionSelectedInfo.IsNone(loaderCharacterMenuGuideInfo.OptionSelected);
        }
    }
}