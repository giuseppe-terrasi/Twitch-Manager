namespace FFMEPGWrapper
{
    public record CropFilter : IFilter
    {
        public int X { get; init; }
        public int Y { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }


        public CropFilter(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }



        override public string ToString()
        {
            return $"crop={Width}:{Height}:{X}:{Y}";
        }
    }
}
