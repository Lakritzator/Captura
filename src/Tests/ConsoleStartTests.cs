using System.Diagnostics;
using System.Threading;
using Tests.Fixtures;
using Xunit;

namespace Tests
{
    [Collection(nameof(Tests))]
    public class ConsoleStartTests
    {
        static Process Start(string arguments)
        {
            var path = TestManagerFixture.GetCliPath();

            var process = new Process
            {
                StartInfo =
                {
                    FileName = path,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            process.Start();

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            void Write(string data, string prefix)
            {
                if (string.IsNullOrWhiteSpace(data))
                    return;

                Trace.WriteLine($"{prefix}: {data}");
            }

            process.ErrorDataReceived += (s, e) => Write(e.Data, "Err");
            process.OutputDataReceived += (s, e) => Write(e.Data, "Out");

            return process;
        }
        
        [Fact]
        public void StartGif()
        {
            var process = Start("start --encoder gif");

            Thread.Sleep(1000);

            process.StandardInput.WriteLine('q');

            process.WaitForExit();

            Assert.Equal(0, process.ExitCode);
        }

        [Fact]
        public void StartGifFixedDuration()
        {
            var process = Start("start --encoder gif --length 1");

            process.WaitForExit();

            Assert.Equal(0, process.ExitCode);
        }
    }
}
