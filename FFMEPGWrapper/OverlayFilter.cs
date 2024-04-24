namespace FFMEPGWrapper
{
    public record OverlayFilter : IFilter
    {
        public string StreamName { get; set; }

        public string Input { get; init; }

        public int X { get; init; }

        public int Y { get; init; }


        public OverlayFilter(string streamName, string input, int x, int y)
        {
            StreamName = streamName;
            Input = input;
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"[{Input}][{StreamName}]overlay={X}:{Y}";
        }
    }
}
