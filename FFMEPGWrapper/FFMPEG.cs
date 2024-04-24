using Microsoft.Extensions.Logging;

using System.Diagnostics;

namespace FFMEPGWrapper
{
    public class FFMPEG
    {
        private readonly List<byte[]> _inputs = [];

        private string _outputName;

        private readonly FilterComplex _complexFilter = new();

        private readonly ILogger _logger;

        public FFMPEG()
        {
            
        }

        public FFMPEG(ILogger logger)
        {
            _logger = logger;   
        }

        public FFMPEG Input(byte[] input)
        {
            _inputs.Add(input);
            return this;
        }

        public FFMPEG FirstInput(byte[] input)
        {
            _inputs.Insert(0, input);
            return this;
        }

        public FFMPEG Output(string name)
        {
            _outputName = $"-c:a copy {name}";

            return this;
        }

        public FFMPEG ClearInputs()
        {
            _inputs.Clear();
            return this;
        }

        public FFMPEG ClearFilters()
        {
            _complexFilter.ClearFilterGroups();
            return this;
        }

        public FFMPEG AddFilterGroup(FilterGroup filterGroup)
        {
            _complexFilter.AddFilterGroup(filterGroup);
            return this;
        }

        public async Task RunAsync()
        {
            var filter = _complexFilter.ToString();
            var inputs = string.Join(" ", _inputs.Select(i => $"-i -"));

            _logger?.LogInformation("Running ffmpeg with inputs: {inputs} and filter: {filter} and {outputname}", inputs, filter, _outputName);

            var process = new Process()
            {
                StartInfo =
                {
                    FileName = @"ffmpeg",
                    Arguments = $"{inputs} {filter} {_outputName}",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                }
            };


            process.Start();
            process.BeginOutputReadLine();

            foreach (var input in _inputs)
            {
                await process.StandardInput.BaseStream.WriteAsync(input);
            }

            process.StandardInput.Close();

            await process.WaitForExitAsync();
        }
    }
}
