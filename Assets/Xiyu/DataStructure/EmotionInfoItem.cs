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
    }
}