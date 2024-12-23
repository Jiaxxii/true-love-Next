using UnityEngine;

namespace Xiyu.DataStructure
{
    public readonly struct LightweightRectTransform
    {
        public LightweightRectTransform(Vector2 position, Size size, Vector2 pivot, Vector3 eulerAngle, Vector3 scale)
        {
            Position = position;
            Size = size;
            Pivot = pivot;
            EulerAngle = eulerAngle;
            Scale = scale;
        }

        public Vector2 Position { get; }
        public Size Size { get; }
        public Vector2 Pivot { get; }
        public Vector3 EulerAngle { get; }
        public Vector3 Scale { get; }


        public static LightweightRectTransform Zero => new(Vector2.zero, Size.Zero, Vector2.zero, Vector3.zero, Vector3.one);

        public static bool IsZero(LightweightRectTransform lrt) => lrt.Position == Vector2.zero && Size.IsZero(lrt.Size) && lrt.Pivot == Vector2.zero &&
                                                                   lrt.EulerAngle == Vector3.zero && lrt.Scale == Vector3.zero;
    }

    public static class LightweightRectTransformExtensions
    {
        public static void Apply(this RectTransform rectTransform, LightweightRectTransform lrt)
        {
            rectTransform.anchoredPosition = lrt.Position;
            rectTransform.sizeDelta = lrt.Size;
            rectTransform.pivot = lrt.Pivot;
            rectTransform.localEulerAngles = lrt.EulerAngle;
            rectTransform.localScale = lrt.Scale;
        }
    }
}