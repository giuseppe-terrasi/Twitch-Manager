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


        override public string ToString()
        {
            var s = $"pad={Width}:{Height}:{X}:{Y}";

            if (!string.IsNullOrEmpty(Color))
            {
                s += $":{Color}";
            }

            return s;
        }
    }
}
