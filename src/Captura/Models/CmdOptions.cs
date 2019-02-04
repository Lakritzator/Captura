using CommandLine;

namespace Captura.Models
{
    /// <summary>
    /// Command-line options for the WPF app.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CmdOptions
    {
        [Option("reset", HelpText = "Reset all setting values to default.")]
        public bool Reset { get; set; }

        [Option("tray", HelpText = "Start minimized into the system tray.")]
        public bool Tray { get; set; }
        
        [Option("no-HotKey", HelpText = "Do not Register HotKeys.")]
        public bool NoHotKeys { get; set; }

        [Option("no-persist", HelpText = "Do not save any changes in settings.")]
        public bool NoPersist { get; set; }

        [Option("settings", HelpText = "Settings Directory")]
        public string Settings { get; set; }
    }
}
