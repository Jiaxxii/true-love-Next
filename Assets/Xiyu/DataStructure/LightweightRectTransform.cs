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
    }

    public readonly struct Size
    {
        public Size(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public float Width { get; }
        public float Height { get; }

        public static implicit operator Vector2(Size size) => new(size.Width, size.Height);
        public static implicit operator Vector3(Size size) => new(size.Width, size.Height, 0);
    }
}