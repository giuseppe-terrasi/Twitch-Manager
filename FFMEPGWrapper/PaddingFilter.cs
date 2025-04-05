namespace FFMEPGWrapper
{
    public record PaddingFilter : IFilter
    {
        public int X { get; init; }
        public int Y { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }
        public string Color { get; init; }

        public PaddingFilter(int x, int y, int width, int height, string color)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Color = color;
        }

        public static PaddingFilter HorizontalCenter(int y, int width, int height, string color)
            => new(-1, y, width, height, color);

        public static PaddingFilter VerticalCenter(int x, int height, int width, string color)
            => new(x, -1, width, height, color);

        public static PaddingFilter Center(int width, int height, string color)
            => new(-1, -1, width, height, color);

        override public string ToString()
        {
            var s = $"pad={Width}:{Height}";

            if (X >= 0)
            {
                s += $":{X}";
            }
            else
            {
                s += ":(ow-iw)/2";
            }

            if (Y >= 0)
            {
                s += $":{Y}";
            }
            else
            {
                s += ":(oh-ih)/2";
            }

            if (!string.IsNullOrEmpty(Color))
            {
                s += $":{Color}";
            }

            return s;
        }
    }
}
