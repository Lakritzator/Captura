using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Tests.Fixtures;
using TestStack.White.UIItems.TabItems;
using Xunit;

namespace Tests
{
    [Collection(nameof(Tests))]
    public class ScreenShotTests : IClassFixture<AppRunnerFixture>
    {
        readonly AppRunnerFixture _appRunner;

        public ScreenShotTests(AppRunnerFixture appRunner)
        {
            _appRunner = appRunner;
        }

        static void Shot(string fileName, IntPtr window)
        {
            Thread.Sleep(500);

            var startInfo = new ProcessStartInfo
            {
                FileName = TestManagerFixture.GetCliPath(),
                Arguments = $"shot --source win:{window} -f {fileName}",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            process?.WaitForExit();

            Assert.False(process == null || process.ExitCode != 0,
                $"Error occurred when taking ScreenShot, hWnd: {window}, FileName: {fileName}, ExitCode: {process?.ExitCode}");

            Assert.True(File.Exists(fileName), $"ScreenShot was not saved: {fileName}");
        }

        /// <summary>
        /// Take ScreenShot of all Tabs.
        /// </summary>
        [Fact]
        public void ScreenShotTabs()
        {
            Directory.CreateDirectory("Tabs");

            var tab = _appRunner.MainWindow.Get<Tab>();
            
            var i = 0;

            foreach (var tabPage in tab.Pages)
            {
                tabPage.Select();

                Shot($"Tabs/{i++}.png", _appRunner.App.Process.MainWindowHandle);
            }
        }
    }
}
