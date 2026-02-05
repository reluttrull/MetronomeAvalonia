using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Timers;

namespace MetronomeMVVM.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private Timer timer;
        [ObservableProperty]
        public int _count = 0; // current count within meter
        [ObservableProperty]
        public int _bpm = 60; // current beats per minute
        [ObservableProperty]
        public Enums.NumCounts _numCounts = Enums.NumCounts.Four; // current meter
        [ObservableProperty]
        private int _interval = 5; // current amount to change +/- bpm when user clicks buttons

        public MainWindowViewModel()
        {
            StartMetronome();
        }

        [MemberNotNull(nameof(timer))]
        public void StartMetronome()
        {
            //System.Diagnostics.Debug.WriteLine($"setting metronome to {Bpm} bpm");
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
            Bpm += Interval;
            StartMetronome();
        }

        public void DecreaseBpm()
        {
            Bpm -= Interval;
            StartMetronome();
        }

        private void OnTimedEvent(Object? source, ElapsedEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                Count = ((Count) % (int)NumCounts) + 1;
                //System.Diagnostics.Debug.WriteLine($"Count: {Count} at {e.SignalTime:HH:mm:ss.fff}");
            });
        }

        private static int BpmToMillis(int bpm)
        {
            return (int)(1000 / ((decimal)bpm / 60));
        }
    }
}
