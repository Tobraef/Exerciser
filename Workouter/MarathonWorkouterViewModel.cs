using System;
using System.Collections.Generic;
using System.Timers;
using System.Text;

using App1.Model;
using App1.Tools;
using App1.Model.DBModels;

using Xamarin.Forms;
using Xamarin.Essentials;

namespace App1.Workouter
{
    public class MarathonWorkouterViewModel : BindableBase
    {
        private readonly IWorkouter workouter;

        private double progress_ = 0.0;
        public double Progress { get => progress_; set => SetProperty(ref progress_, value); }

        private string description_;
        public string Description { get => description_; set => SetProperty(ref description_, value); }

        private string buttonText_ = "Start";
        public string WorkButtonText { get => buttonText_; set => SetProperty(ref buttonText_, value, RefreshCommands); }

        public string Title { get; }

        private string goal_;
        public string Goal { get => goal_; set => SetProperty(ref goal_, value); }

        public Command Start { get; private set; }

        public Command Finish { get; private set; }

        public SoundView SoundHandler { get; set; }

        private void NotifyUser()
        {
            Vibration.Vibrate();
        }

        private async void Final()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        private void HandleProgress(string description, double current)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Description = description;
                Progress = 1 - current;
            });
        }

        private void InitializeCommands()
        {
            Finish = new Command(Final, () => WorkButtonText.Equals("Stop") || WorkButtonText.Equals("Resume"));
            Start = new Command(() =>
            {
                switch (WorkButtonText)
                {
                    case "Start": workouter.Resume(); SoundHandler?.Start(); WorkButtonText = "Stop"; break;
                    case "Resume": workouter.Resume(); SoundHandler?.Start(); WorkButtonText = "Stop"; break;
                    case "Stop": workouter.Stop(); SoundHandler?.Stop(); WorkButtonText = "Resume"; break;
                }
            });
        }

        private void RefreshCommands()
        {
            Finish.ChangeCanExecute();
        }

        private void LockWhileGPSNotAvailable(IDistanceProvider provider)
        {
            try
            {
                provider.BeginMeasures().RunSynchronously();
            }
            catch (FeatureNotEnabledException)
            {
                App.Debug("This type of workout requires you to enable GPS, please do so");
                LockWhileGPSNotAvailable(provider);
            }
        }

        public MarathonWorkouterViewModel(MarathonDBModel model, IDistanceProvider provider)
        {
            Title = model.Title;
            if (model.Distance > 0 && model.Duration > 0)
            {
                Goal = model.Distance.ToString() + " meters";
                var work = new PromptingWorkouter(provider, model.Distance, model.Duration, model.DifferenceNotify);
                if (model.DifferenceNotify != 0)
                {
                    work.DifferenceNoticed += NotifyUser;
                }
                workouter = work;
            }
            else if (model.Distance > 0)
            {
                Goal = model.Distance.ToString() + " meters";
                workouter = new DistanceWorkouter(provider, model.Distance);
            }
            else
            {
                Goal = TimeSpan.FromSeconds(model.Duration).ToString();
                workouter = new TimeWorkouter(model.Duration);
            }
            workouter.Finished += Final;
            workouter.UpdateValues += HandleProgress;
            InitializeCommands();
            workouter.UpdateState();
        }

        #region Workouters
        private interface IWorkouter
        {
            event Action Finished;
            event Action<string, double> UpdateValues;
            void UpdateState();
            void Stop();
            void Resume();
        }
        private class TimeWorkouter : IWorkouter
        {
            private readonly int totalDuration_;
            private int ticksLeft_;
            private bool isOn_;

            public event Action Finished;
            public event Action<string, double> UpdateValues;

            public void UpdateState()
            {
                UpdateValues("Time left: " + TimeSpan.FromSeconds(ticksLeft_).ToString(), (double)ticksLeft_ / totalDuration_);
            }

            private bool Tick()
            {
                ticksLeft_--;
                UpdateState();
                if (ticksLeft_ == 0)
                {
                    Finished();
                    return false;
                }
                return isOn_;
            }

            public void Stop()
            {
                isOn_ = false;
            }

            public void Resume()
            {
                isOn_ = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), Tick);
            }

            public TimeWorkouter(int duration)
            {
                totalDuration_ = ticksLeft_ = duration;
            }
        }

        private class DistanceWorkouter : IWorkouter
        {
            private readonly IDistanceProvider distanceProvider_;
            private double currentDistance_;
            private readonly double totalDistance_;
            private bool isOn_ = true;
            private Timer distanceThread_;

            public event Action Finished;
            public event Action<string, double> UpdateValues;

            public void UpdateState()
            {
                UpdateValues("Remaining distance: " + currentDistance_.ToString("0.0") + " meters", currentDistance_ / totalDistance_);
            }

            public void Resume()
            {
                if (isOn_)
                {
                    distanceProvider_.BeginMeasures();
                    distanceThread_ = new Timer(5000);
                    distanceThread_.Elapsed += async (o, e) => currentDistance_ -= await distanceProvider_.Distance();
                    Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                    {
                        distanceThread_.Start();
                        return false;
                    });
                }
                else
                {
                    distanceThread_.Start();
                }
                isOn_ = true;
                Device.StartTimer(TimeSpan.FromSeconds(5), Tick);
            }

            public void Stop()
            {
                isOn_ = false;
                distanceThread_.Stop();
            }

            private bool Tick()
            {
                UpdateState();
                if (currentDistance_ <= 0)
                {
                    Finished();
                    return false;
                }
                return isOn_;
            }

            public DistanceWorkouter(IDistanceProvider provider, double distance)
            {
                isOn_ = true;
                distanceProvider_ = provider;
                totalDistance_ = currentDistance_ = distance;
            }
        }

        private class PromptingWorkouter : IWorkouter
        {
            private readonly IDistanceProvider provider_;
            private int tick_ = 7;
            private readonly double totalDistance_;
            private double currentDistance_;
            private readonly int totalTime_;
            private int currentTime_;
            private readonly double triggerValue_;
            private bool isOn_;
            private Timer distanceThread;

            public event Action Finished;
            public event Action<string, double> UpdateValues;
            public event Action DifferenceNoticed;

            private int nextNotif_ = 0;
            private void Confront()
            {
                if (nextNotif_ > 0)
                {
                    nextNotif_--;
                    return;
                }
                double shouldBe = (double)currentTime_ / totalTime_;
                double actuallyIs = currentDistance_ / totalDistance_;
                if (actuallyIs - shouldBe > triggerValue_)
                {
                    DifferenceNoticed?.Invoke();
                    nextNotif_ = 12;
                }
            }

            private string CreateMessage()
            {
                string message = "Time left: " + TimeSpan.FromSeconds(currentTime_).ToString();
                double shouldBe = (double)currentTime_ / totalTime_;
                double actuallyIs = currentDistance_ / totalDistance_;
                double result = actuallyIs - shouldBe;
                if (result > 0)
                {
                    message += "\nBehind the schedule: " + (result*100).ToString("0.0") + '%';
                }
                else
                {
                    message += "\nAhead of the schedule: " + (-result * 100).ToString("0.0") + '%';

                }
                message += "\nDistance left: " + currentDistance_.ToString("0.0");
                return message;
            }

            public void UpdateState()
            {
                UpdateValues(CreateMessage(), currentDistance_ / totalDistance_);
            }

            private bool Tick()
            {
                currentTime_--;
                if (--tick_ < 0)
                {
                    tick_ = 5;
                    Confront();
                    if (currentDistance_ < 0)
                    {
                        Finished();
                    }
                }
                UpdateState();
                return isOn_;
            }
            
            public void Resume()
            {
                if (isOn_)
                {
                    distanceThread = new Timer(5000);
                    distanceThread.Elapsed += async (o, e) =>
                    {
                        var dist = await provider_.Distance().ConfigureAwait(false);
                        currentDistance_ -= dist;
                    };
                    provider_.BeginMeasures();
                }
                isOn_ = true;
                distanceThread.Start();
                Device.StartTimer(TimeSpan.FromSeconds(1), Tick);
            }

            public void Stop()
            {
                isOn_ = false;
                distanceThread.Stop();
            }

            public PromptingWorkouter(IDistanceProvider provider, double distance, int time, double trigger)
            {
                isOn_ = true;
                triggerValue_ = trigger;
                provider_ = provider;
                currentDistance_ = totalDistance_ = distance;
                totalTime_ = currentTime_ = time;
            }
        }
        #endregion
    }
}
