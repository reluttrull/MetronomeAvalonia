using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SFML.Audio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
        public ObservableCollection<bool?> _beatStates = [true, true, true, true]; // accents - true: accent, false: normal, null: silent
        [ObservableProperty]
        private int _largeInterval = 5;
        [ObservableProperty]
        private int _smallInterval = 1;

        private Sound? normalSound;

        public MainWindowViewModel()
        {
            InitializeSound();
            StartMetronome();
        }

        private void InitializeSound()
        {
            try
            {
                var assemblyDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var soundPath = Path.Combine(assemblyDirectory, "Assets", "hit.mp3");
                normalSound = new Sound(new SoundBuffer(soundPath));
            }
            catch
            {
                normalSound = null;
            }
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

        public void IncreaseBpm(int interval)
        {
            Bpm += interval;
            StartMetronome();
        }

        public void DecreaseBpm(int interval)
        {
            Bpm -= interval;
            StartMetronome();
        }

        public void ChangeMeter(int diff)
        {
            if ((int)NumCounts + diff < 2 || (int)NumCounts + diff > 6) return;
            NumCounts += diff;
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    BeatStates.Add(true);
                }
            }
            else
            {
                for (int i = 0; i > diff; i--)
                {
                    BeatStates.RemoveAt(BeatStates.Count - 1);
                }
            }
        }

        private void OnTimedEvent(Object? source, ElapsedEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                normalSound?.Play();
                Count = ((Count) % (int)NumCounts) + 1;
                System.Diagnostics.Debug.WriteLine($"Count: {Count} at {e.SignalTime:HH:mm:ss.fff}");
            });
        }

        private static int BpmToMillis(int bpm)
        {
            return (int)(1000 / ((decimal)bpm / 60));
        }
    }
}
