using System.Reactive.Linq;
using System.Windows.Input;
using Captura.Base;
using Captura.Core.Models;
using Captura.Core.Settings;
using Captura.Core.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Captura.ViewCore.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RecordingViewModel : NotifyPropertyChanged
    {
        private readonly RecordingModel _recordingModel;

        public ICommand RecordCommand { get; }
        public ICommand PauseCommand { get; }

        public RecordingViewModel(RecordingModel recordingModel,
            Settings settings,
            TimerModel timerModel,
            VideoSourcesViewModel videoSourcesViewModel)
        {
            _recordingModel = recordingModel;

            RecordCommand = new[]
                {
                    settings.Audio
                        .ObserveProperty(audioSettings => audioSettings.Enabled),
                    videoSourcesViewModel
                        .ObserveProperty(sourcesViewModel => sourcesViewModel.SelectedVideoSourceKind)
                        .Select(videoSourceProvider => !(videoSourceProvider is NoVideoSourceProvider))
                }
                .CombineLatest(bools => bools[0] || bools[1])
                .ToReactiveCommand()
                .WithSubscribe(recordingModel.OnRecordExecute);

            PauseCommand = new[]
                {
                    timerModel
                        .ObserveProperty(model => model.Waiting),
                    recordingModel
                        .ObserveProperty(model => model.RecorderState)
                        .Select(recorderState => recorderState != RecorderState.NotRecording)
                }
                .CombineLatest(bools => !bools[0] && bools[1])
                .ToReactiveCommand()
                .WithSubscribe(recordingModel.OnPauseExecute);

            recordingModel.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "":
                    case null:
                    case nameof(RecorderState):
                        RaisePropertyChanged(nameof(RecorderState));
                        break;
                }
            };
        }

        public RecorderState RecorderState => _recordingModel.RecorderState;
    }
}