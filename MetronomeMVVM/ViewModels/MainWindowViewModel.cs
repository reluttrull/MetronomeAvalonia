using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Timers;

namespace MetronomeMVVM.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private Timer timer;
        [ObservableProperty]
        public int _count = 0;
        [ObservableProperty]
        public int _bpm = 60;
        [ObservableProperty]
        public int _numCounts = 4;
        private int interval = 5;
        public MainWindowViewModel()
        {
            StartMetronome();
        }

        public void StartMetronome()
        {
            System.Diagnostics.Debug.WriteLine($"setting metronome to {Bpm} bpm");
            if (timer is not null)
            {
                timer.Enabled = false;
                timer.Interval = BpmToMillis(Bpm);
            }
            else
            {
                timer = new Timer(BpmToMillis(Bpm));
                timer.Elapsed += OnTimedEvent;
                timer.AutoReset = true;
            }
            timer.Enabled = true;
        }

        public void IncreaseBpm()
        {
            Bpm += interval;
            StartMetronome();
        }

        public void DecreaseBpm()
        {
            Bpm -= interval;
            StartMetronome();
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                Count = ((Count) % NumCounts) + 1;
                System.Diagnostics.Debug.WriteLine($"Count: {Count} at {e.SignalTime:HH:mm:ss.fff}");
            });
        }

        private int BpmToMillis(int bpm)
        {
            return (int)(1000 / ((decimal)bpm / 60));
        }
    }
}
