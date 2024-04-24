
namespace FFMEPGWrapper
{
    public record FilterGroup
    {
        public string Name { get; init; }

        public string StreamName { get; init; }  

        public List<IFilter> Filters { get; init; } = [];

        public FilterGroup()
        {
            Name = "";
            StreamName = "";
        }

        public FilterGroup(string name)
        {
            Name = name;
        }

        public FilterGroup(string name, string rectangle)
        {
            Name = name;
            StreamName = rectangle;
        }

        public FilterGroup AddFilter(IFilter filter)
        {
            Filters.Add(filter);
            return this;
        }

        public override string ToString()
        {
            return $"{StreamName}{string.Join(", ", Filters.Select(f => f.ToString()))}{(string.IsNullOrEmpty(Name) ? "" : $" [{Name}]")}";
        }
    }
}
