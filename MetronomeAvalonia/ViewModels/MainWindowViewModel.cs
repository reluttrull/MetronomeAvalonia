using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SFML.Audio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Timers;

namespace MetronomeMVVM.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private Timer timer;
        private List<long> tapTimes = [];
        [ObservableProperty]
        public int _count = 0; // current count within meter
        [ObservableProperty]
        public string _tempBpm = string.Empty;
        [ObservableProperty]
        public int _bpm = 60; // current beats per minute
        [ObservableProperty]
        public Enums.NumCounts _numCounts = Enums.NumCounts.Four; // current meter
        [ObservableProperty]
        public ObservableCollection<BeatState> _beatStates =
        [
            new BeatState { IsActive = true },
            new BeatState { IsActive = true },
            new BeatState { IsActive = true },
            new BeatState { IsActive = true }
        ]; // true: accent, false: silent, null: normal
        [ObservableProperty]
        private int _largeInterval = 5;
        [ObservableProperty]
        private int _smallInterval = 1;
        [ObservableProperty]
        private bool _isAnimating = false;
        [ObservableProperty]
        private bool _isEditingBpm = false;
        [ObservableProperty]
        private bool _isFlashOn = true;
        [ObservableProperty]
        private bool _isSoundOn = true;

        private Sound? normalSound;
        private Sound? accentSound;

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
                var accentSoundPath = Path.Combine(assemblyDirectory, "Assets", "accenthit.mp3");
                accentSound = new Sound(new SoundBuffer(accentSoundPath));
            }
            catch
            {
                normalSound = null;
                accentSound = null;
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
            SetBpm(Bpm + interval);
        }

        public void DecreaseBpm(int interval)
        {
            SetBpm(Bpm - interval);
        }

        public void SetBpm(int bpm)
        {
            if (bpm < 35 || bpm > 240) return;
            Bpm = bpm;
            StartMetronome();
        }

        public void ChangeMeter(int diff)
        {
            if ((int)NumCounts + diff < 1 || (int)NumCounts + diff > 12) return;
            NumCounts += diff;
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    BeatStates.Add(new BeatState { IsActive = true });
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

        public void Tap()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (tapTimes.Count > 0)
            {
                var diff = now - tapTimes.Last();
                if (diff > 2000) tapTimes = [];
            }

            tapTimes.Add(now);
            Debug.WriteLine($"Tapped at {now}");
            if (tapTimes.Count >= 3)
            {
                var avg = tapTimes.Zip(tapTimes.Skip(1), (prev, curr) => curr - prev).Average();
                SetBpm(MillisToBpm((int)avg));
            }
        }

        public void ToggleFlash()
        {
            IsFlashOn = !IsFlashOn;
        }
        public void ToggleSound()
        {
            IsSoundOn = !IsSoundOn;
        }

        private void OnTimedEvent(Object? source, ElapsedEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                int currentCount = Count % (int)NumCounts;
                if (IsSoundOn) MakeSound(currentCount);
                if (IsFlashOn) MakeFlash();
                Count = currentCount + 1;
                System.Diagnostics.Debug.WriteLine($"Count: {Count} at {e.SignalTime:HH:mm:ss.fff}");
            });
        }

        private void MakeSound(int count)
        {
            switch (BeatStates[count].IsActive)
            {
                case true:
                    accentSound?.Play();
                    break;
                case null:
                    normalSound?.Play();
                    break;
                case false:
                    break;
            }
        }

        private void MakeFlash()
        {
            IsAnimating = false;
            IsAnimating = true;
        }

        private static int BpmToMillis(int bpm)
        {
            return (int)(1000 / ((decimal)bpm / 60));
        }

        private static int MillisToBpm(int millis)
        {
            return (int)((60 * 1000) / millis);
        }
    }
    public partial class BeatState : ObservableObject
    {
        [ObservableProperty]
        private bool? _isActive;
    }
}
