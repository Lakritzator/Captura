using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Ninject;

namespace Captura.Base.Services
{
    public static class ServiceProvider
    {
        static string _settingsDir;

        public static string SettingsDir
        {
            get
            {
                if (_settingsDir == null)
                    _settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Captura");

                if (!Directory.Exists(_settingsDir))
                    Directory.CreateDirectory(_settingsDir);

                return _settingsDir;
            }
            set
            {
                _settingsDir = value;

                if (!Directory.Exists(_settingsDir))
                    Directory.CreateDirectory(_settingsDir);
            }
        }

        static IKernel Kernel { get; } = new StandardKernel();

        static readonly List<IModule> LoadedModules = new List<IModule>();

        public static void LoadModule(IModule module)
        {
            Kernel.Load(new Binder(module));

            LoadedModules.Add(module);
        }

        /// <summary>
        /// To be called on App Exit
        /// </summary>
        public static void Dispose()
        {
            LoadedModules.ForEach(module => module.Dispose());

            // Singleton objects will be disposed by Kernel
            Kernel.Dispose();
        }

        public static T Get<T>() => Kernel.Get<T>();
        
        public static void LaunchFile(ProcessStartInfo startInfo)
        {
            try { Process.Start(startInfo.FileName); }
            catch (Win32Exception e) when (e.NativeErrorCode == 2)
            {
                MessageProvider.ShowError($"Could not find file: {startInfo.FileName}");
            }
            catch (Exception e)
            {
                MessageProvider.ShowException(e, $"Could not open file: {startInfo.FileName}");
            }
        }

        public static IMessageProvider MessageProvider => Get<IMessageProvider>();
        
        public static bool FileExists(string fileName)
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(location))
            {
                return false;
            }
            return File.Exists(Path.Combine(location, fileName));
        }
    }
}
