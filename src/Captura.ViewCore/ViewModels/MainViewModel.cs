using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;
using Captura.Base;
using Captura.Base.Services;
using Captura.Base.Settings;
using Captura.Core.Models;
using Captura.Core.Settings;
using Captura.Core.ViewModels;
using Captura.FFmpeg;
using Captura.HotKeys;
using Captura.Loc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Captura.ViewCore.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;

        public ICommand ShowPreviewCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand OpenOutputFolderCommand { get; }
        public ICommand SelectOutputFolderCommand { get; }
        public ICommand SelectFFmpegFolderCommand { get; } = new DelegateCommand(FFmpegService.SelectFFmpegFolder);
        public ICommand ResetFFmpegFolderCommand { get; }
        public ICommand TrayLeftClickCommand { get; }

        public MainViewModel(Settings settings,
            LanguageManager languageManager,
            HotKeyManager hotKeyManager,
            IPreviewWindow previewWindow,
            IDialogService dialogService,
            RecordingModel recordingModel,
            MainModel mainModel) : base(settings, languageManager)
        {
            _dialogService = dialogService;

            ShowPreviewCommand = new DelegateCommand(previewWindow.Show);

            #region Commands
            RefreshCommand = recordingModel
                .ObserveProperty(model => model.RecorderState)
                .Select(recorderState => recorderState == RecorderState.NotRecording)
                .ToReactiveCommand()
                .WithSubscribe(() =>
                {
                    mainModel.Refresh();

                    Refreshed?.Invoke();
                });

            OpenOutputFolderCommand = new DelegateCommand(OpenOutputFolder);

            SelectOutputFolderCommand = new DelegateCommand(SelectOutputFolder);

            ResetFFmpegFolderCommand = new DelegateCommand(() => settings.FFmpeg.FolderPath = "");

            TrayLeftClickCommand = new DelegateCommand(() => hotKeyManager.FakeHotKey(settings.Tray.LeftClickAction));
            #endregion
        }

        public static IEnumerable<ObjectLocalizer<Alignment>> XAlignments { get; } = new[]
        {
            new ObjectLocalizer<Alignment>(Alignment.Start, nameof(LanguageManager.Left)),
            new ObjectLocalizer<Alignment>(Alignment.Center, nameof(LanguageManager.Center)),
            new ObjectLocalizer<Alignment>(Alignment.End, nameof(LanguageManager.Right))
        };

        public static IEnumerable<ObjectLocalizer<Alignment>> YAlignments { get; } = new[]
        {
            new ObjectLocalizer<Alignment>(Alignment.Start, nameof(LanguageManager.Top)),
            new ObjectLocalizer<Alignment>(Alignment.Center, nameof(LanguageManager.Center)),
            new ObjectLocalizer<Alignment>(Alignment.End, nameof(LanguageManager.Bottom))
        };

        private void OpenOutputFolder()
        {
            Settings.EnsureOutPath();

            Process.Start(Settings.OutPath);
        }

        private void SelectOutputFolder()
        {
            var folder = _dialogService.PickFolder(Settings.OutPath, Loc.SelectOutFolder);

            if (folder != null)
                Settings.OutPath = folder;
        }

        public event Action Refreshed;
    }
}