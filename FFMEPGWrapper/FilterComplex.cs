
namespace FFMEPGWrapper
{
    public record FilterComplex
    {
        public List<FilterGroup> FilterGroups { get; init; } = [];

        public FilterComplex AddFilterGroup(FilterGroup filterGroup)
        {
            FilterGroups.Add(filterGroup);
            return this;
        }

        public FilterComplex AddFilterGroups(IEnumerable<FilterGroup> filterGroups)
        {
            FilterGroups.AddRange(filterGroups);
            return this;
        }

        public FilterComplex ClearFilterGroups()
        {
            FilterGroups.Clear();
            return this;
        }

        override public string ToString()
        {
            if (FilterGroups.Count == 0)
            {
                return "";
            }

            return $"-filter_complex \"{string.Join("; ", FilterGroups)}\"";
        }
    }
}
