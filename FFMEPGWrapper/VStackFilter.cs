
namespace FFMEPGWrapper
{
    public record VStackFilter : IFilter
    {
        public IEnumerable<string> FilterNames { get; init; }

        public VStackFilter(params string[] filterNames)
        {
            FilterNames = filterNames;
        }

        public override string ToString()
        {
            return $"{string.Join("", FilterNames.Select(n => $"[{n}]"))}vstack";   
        }
    }
}
