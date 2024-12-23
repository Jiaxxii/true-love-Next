namespace Xiyu.DataStructure
{
    public readonly struct EmotionInfoItem
    {
        public EmotionInfoItem(string addressableName, LightweightRectTransform transform)
        {
            AddressableName = addressableName;
            Transform = transform;
        }

        public string AddressableName { get; }
        public LightweightRectTransform Transform { get; }


        public static EmotionInfoItem None => new(string.Empty, LightweightRectTransform.Zero);

        public static bool IsNone(EmotionInfoItem item) => !string.IsNullOrEmpty(item.AddressableName) && LightweightRectTransform.IsZero(item.Transform);
    }
}