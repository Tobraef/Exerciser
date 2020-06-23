using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using App1.Model;
using App1.Sound;
using App1.Tools;

namespace App1.Workouter
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CrossfitWorkouter : ContentPage
    {
        private readonly CrossfitViewModel workout_;
        private readonly SoundHandler sound_;

        private readonly Color colorExercising = Color.Pink;
        private readonly Color colorBreak = Color.LightGreen;
        private readonly Color colorIdle = Color.LightGray;

        private bool isOn = true;
        private int timer;

        private bool IsSoundOn => switchSound.IsToggled;

        public CrossfitWorkouter(CrossfitViewModel workout)
        {
            App.IsUserWorking = true;
            InitializeComponent();
            sound_ = new SoundHandler();
            workout_ = workout;
            InitBeginLayout();
        }

        private void InitBeginLayout()
        {
            var beginButton = new Button
            {
                Text = "Begin",
                BackgroundColor = Color.Green,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Button)),
            };
            var beginLabel = new Label
            {
                Text = "5",
                TextColor = Color.AliceBlue,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };
            frameExercise.Content = new StackLayout
            {
                Children =
                {
                    beginLabel,
                    beginButton
                }
            };
            frameExercise.BackgroundColor = colorIdle;
            beginButton.Clicked += (o, e) =>
            {
                TimeSpan second = TimeSpan.FromSeconds(1);
                Device.StartTimer(second, () =>
                {
                    var next = int.Parse(beginLabel.Text) - 1;
                    beginLabel.Text = next.ToString();
                    if (next == -1)
                    {
                        frameExercise.Content = panelExercise;
                        StartWorkout();
                        return false;
                    }
                    return true;
                });
            };
        }

        private void ExerciseTimer()
        {
            frameExercise.BackgroundColor = colorExercising;
            if (IsSoundOn)
            {
                sound_.PlayGo();
            }
            timer = workout_.Duration;
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                timer--;
                labelExerciseTime.Text = TimeParser.ReduceTime(labelExerciseTime.Text);
                if (timer == 0)
                {
                    BreakTimer();
                    return false;
                }
                return isOn;
            });
        }

        private void BreakTimer()
        {
            frameExercise.BackgroundColor = colorBreak;
            if (IsSoundOn)
            {
                sound_.PlayStop();
            }
            timer = workout_.BreakDuration;
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                timer--;
                labelBreakTime.Text = TimeParser.ReduceTime(labelBreakTime.Text);
                if (timer == 0)
                {
                    LoadNextExercise();
                    return false;
                }
                return isOn;
            });
        }

        private async Task Finish()
        {
            App.IsUserWorking = false;
            await Navigation.PopAsync();
        }

        private void LoadNextExercise()
        {
            if (workout_.Next())
            {
                SetupUI();
                ExerciseTimer();
            }
            else
            {
                DisplayAlert("Finished", "Nice work", "Ok").Wait();
                Finish().ConfigureAwait(false);
            }
        }

        private void StartWorkout()
        {
            workout_.Next();
            frameExercise.BackgroundColor = colorIdle;
            SetupUI();
            Device.StartTimer(TimeSpan.FromSeconds(3), () => {
                ExerciseTimer();
                buttonBreak.IsEnabled = true;
                buttonSkip.IsEnabled = true;
                return false; }); 
        }

        private void SetTimers()
        {
            labelExerciseTime.Text = TimeParser.ParseTime(workout_.Duration);
            labelBreakTime.Text = TimeParser.ParseTime(workout_.BreakDuration);
        }

        private void SetupUI()
        {
            labelSet.Text = "Set " + workout_.Set.ToString();
            SetTimers();
            labelExercise.Text = workout_.ExerciseName;
            labelNextExercise.Text = "Coming up: " + workout_.NextExerciseName;
        }

        private void ButtonResume_Clicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            button.Text = "Resume";
            isOn = true;
            if (string.Equals(labelExerciseTime.Text, "0"))
            {
                StartWorkout();
            }
            else
            {
                SetupUI();
                ExerciseTimer();
            }
            button.Clicked += ButtonBreak_Clicked;
            button.Clicked -= ButtonResume_Clicked;
        }

        private void ButtonBreak_Clicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            button.Text = "Resume";
            isOn = false;
            frameExercise.BackgroundColor = colorIdle;
            button.Clicked -= ButtonBreak_Clicked;
            button.Clicked += ButtonResume_Clicked;
        }

        private void ButtonSkip_Clicked(object sender, EventArgs e)
        {
            if (isOn)
            {
                isOn = false;
                var button = (Button)sender;
                button.IsEnabled = false;
                Device.StartTimer(TimeSpan.FromSeconds(3), () =>
                {
                    isOn = true;
                    button.IsEnabled = true;
                    return false;
                });
                StartWorkout();
            }
            else
            {
                workout_.Next();
                SetupUI();
            }
        }
    }
}