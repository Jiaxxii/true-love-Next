namespace Xiyu.DataStructure
{
    public readonly struct LoaderCharacterGuideInfo
    {
        public LoaderCharacterGuideInfo(string defaultCharacterCode, string defaultEmotionName, LightweightRectTransform defaultTransform, LoaderOptionSelectedInfo optionSelected)
        {
            DefaultCharacterCode = defaultCharacterCode;
            DefaultEmotionName = defaultEmotionName;
            DefaultTransform = defaultTransform;
            OptionSelected = optionSelected;
        }

        public string DefaultCharacterCode { get; }
        public string DefaultEmotionName { get; }
        public LightweightRectTransform DefaultTransform { get; }
        public LoaderOptionSelectedInfo OptionSelected { get; }


        public static LoaderCharacterGuideInfo None => new(string.Empty, string.Empty, LightweightRectTransform.Zero, LoaderOptionSelectedInfo.None);

        public static bool IsNone(LoaderCharacterGuideInfo loaderCharacterGuideInfo) => !string.IsNullOrEmpty(loaderCharacterGuideInfo.DefaultCharacterCode) &&
                                                                                        !string.IsNullOrEmpty(loaderCharacterGuideInfo.DefaultEmotionName) &&
                                                                                        !LightweightRectTransform.IsZero(loaderCharacterGuideInfo.DefaultTransform) &&
                                                                                        !LoaderOptionSelectedInfo.IsNone(loaderCharacterGuideInfo.OptionSelected);
    }
}