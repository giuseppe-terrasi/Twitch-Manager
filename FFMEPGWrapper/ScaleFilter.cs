namespace FFMEPGWrapper
{

    public record ScaleFilter : IFilter
    {
        public int Width { get; init; }

        public int Height { get; init; }

        public ForceOriginalAspectRatio ForceOriginalAspectRatio { get; init; }

        public ScaleFilter(int width, int height, ForceOriginalAspectRatio forceOriginalAspectRatio)
        {
            Width = width;
            Height = height;
            ForceOriginalAspectRatio = forceOriginalAspectRatio;
        }

        override public string ToString()
        {
            var s = $"scale={Width}:{Height}";

            switch (ForceOriginalAspectRatio)
            {
                case ForceOriginalAspectRatio.Disable:
                    break;
                case ForceOriginalAspectRatio.Decrease:
                    s += ":force_original_aspect_ratio=decrease";
                    break;
                case ForceOriginalAspectRatio.Increase:
                    s += ":force_original_aspect_ratio=increase";
                    break;
            }

            return s;
        }

    }
}
