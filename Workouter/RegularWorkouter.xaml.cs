using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

using App1.Model;
using App1.Tools;

namespace App1.Workouter
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegularWorkouter : ContentPage
    {
        private readonly RegularViewModel workout_;
        private Label nextLabel_;
        private Label currentLabel_;
        private Label timeLabel_;
        private ProgressBar statusBar_;

        private bool isOn = true;

        private void InitializeMembers()
        {
            nextLabel_ = (Label)FindByName("labelNext");
            currentLabel_ = (Label)FindByName("labelCurrent");
            timeLabel_ = (Label)FindByName("labelTime");
            statusBar_ = (ProgressBar)FindByName("progressBar");
        }

        public RegularWorkouter(RegularViewModel workout)
        {
            App.IsUserWorking = true;
            InitializeComponent();
            InitializeMembers();
            workout_ = workout;
        }

        private void SetupUI()
        {
            currentLabel_.Text = "Current: " + workout_.FullExerciseName;
            nextLabel_.Text = "Coming up: " + workout_.NextExercise;
            timeLabel_.Text = TimeParser.ParseTime(workout_.TimeToNext);
            statusBar_.Progress = 1;
        }

        private bool TimeTick()
        {
            if (isOn)
            {
                timeLabel_.Text = TimeParser.ReduceTime(timeLabel_.Text);
                statusBar_.Progress -= (double)1 / workout_.TimeToNext;
                if (statusBar_.Progress <= 0)
                {
                    LoadNext();
                    return false;
                }
            }
            return isOn;
        }

        private void LoadNext()
        {
            if (workout_.Next())
            {
                SetupUI();
                Device.StartTimer(TimeSpan.FromSeconds(1), TimeTick);
                Vibration.Vibrate();
            }
            else
            {
                currentLabel_.Text = "FINISHED";
                FinishCommand.Execute(null);
            }
        }

        public Command BeginCommand =>
            new Command(() =>
            {
                workout_.Next();
                SetupUI();
                DeviceDisplay.KeepScreenOn = true;
                isOn = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), TimeTick);
            });

        

        public Command NextCommand =>
            new Command(() =>
            {
                if (isOn)
                {
                    isOn = false;
                    Device.StartTimer(TimeSpan.FromSeconds(2), () =>
                    {
                        isOn = true;
                        LoadNext();
                        return false;
                    });
                }
                else
                {
                    if (workout_.Next())
                    {
                        SetupUI();
                    }
                    else
                    {
                        FinishCommand.Execute(null);
                    }
                }
            });

        public Command FinishCommand =>
            new Command(async () =>
            {
                isOn = false;
                App.IsUserWorking = false;
                await Navigation.PopToRootAsync();
                DeviceDisplay.KeepScreenOn = false;
            });

        private void StopResume_Clicked(object sender, EventArgs e)
        {
            if (isOn)
            {
                ((Button)sender).Text = "Resume";
            }
            else
            {
                ((Button)sender).Text = "Stop";
                Device.StartTimer(TimeSpan.FromSeconds(1), TimeTick);
            }
            isOn = !isOn;
        }
    }
}