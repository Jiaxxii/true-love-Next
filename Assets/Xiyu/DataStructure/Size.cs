namespace Xiyu.DataStructure
{
    public readonly struct Size
    {
        public Size(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public float Width { get; }
        public float Height { get; }

        public static implicit operator UnityEngine.Vector2(Size size) => new(size.Width, size.Height);
        public static implicit operator UnityEngine.Vector3(Size size) => new(size.Width, size.Height, 0);

        public static Size Zero => new(0, 0);

        public static bool IsZero(Size size) => (int)size.Width == 0 && (int)size.Height == 0;
    }
}