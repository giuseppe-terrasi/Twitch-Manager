using Microsoft.Extensions.Options;

using System.Text.Json;
using System.Text.Json.Nodes;

namespace TwitchManager.Helpers
{
    public class WritableOptions<T>(
        IWebHostEnvironment environment,
        IOptionsMonitor<T> options,
        IConfigurationRoot configuration,
        string section,
        string file) : IWritableOptions<T> where T : class, new()
    {
        private readonly IWebHostEnvironment _environment = environment;
        private readonly IOptionsMonitor<T> _options = options;
        private readonly IConfigurationRoot _configuration = configuration;
        private readonly string _section = section;
        private readonly string _file = file;

        public T Value => _options.CurrentValue;
        public T Get(string name) => _options.Get(name);

        public void Update(Action<T> applyChanges)
        {
            var fileProvider = _environment.ContentRootFileProvider;
            var fileInfo = fileProvider.GetFileInfo(_file);
            var physicalPath = fileInfo.PhysicalPath;

            if (!File.Exists(physicalPath))
                File.WriteAllText(physicalPath, "{}");

            var jObject = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(physicalPath));
            var sectionObject = jObject.TryGetPropertyValue(_section, out JsonNode section) ?
                JsonSerializer.Deserialize<T>(section.ToString()) : (Value ?? new T());

            applyChanges(sectionObject);

            jObject[_section] = JsonNode.Parse(JsonSerializer.Serialize(sectionObject));
            File.WriteAllText(physicalPath, JsonSerializer.Serialize(jObject, new JsonSerializerOptions() { WriteIndented = true }));
            _configuration.Reload();
        }
    }
}
