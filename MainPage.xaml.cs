using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

using App1.Model;
using App1.Model.DBModels;

namespace App1
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private readonly DB database_ = new DB();

        public MainPage()
        {
            Workouts = new ObservableCollection<IWorkout>();
            InitializeComponent();
            itemsList.ItemsSource = Workouts;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var page = new WorkoutCreation();
            await Navigation.PushAsync(page);
        }

        private async Task LaunchWorkout(string title)
        {
            var workout = Workouts.First(w => w.Title.Equals(title));
            await workout.ExecuteWorkout(database_);
        }

        public Command<string> CommandUseWorkout =>
            new Command<string>(async e => await LaunchWorkout(e));

        public ObservableCollection<IWorkout> Workouts
        { 
            get;
            set;
        }

        private async Task RefreshWorkouts()
        {
            var descs = await database_.GetDescriptions();
            Workouts.Clear();
            descs.ForEach(d => Workouts.Add(d));
        }

        private Task PostponeWorkout(IWorkout workout)
        {
            return DependencyService.Get<Schedule.NotificationManager>().Schedule(workout, DateTime.Now + TimeSpan.FromMinutes(5));
        }

        private async Task CheckIfNotified()
        {
            var workout = App.Request;
            if (workout != null)
            {
                App.ClearData();
                if (App.IsUserWorking)
                {
                    await PostponeWorkout(workout);
                }
                if (await DisplayAlert(workout.Title, "Time for the scheduled workout, proceed?", "Ok", "Postpone"))
                {
                    await workout.ExecuteWorkout(database_);
                }
                else
                {
                    await DependencyService.Get<Schedule.NotificationManager>().Schedule(workout, DateTime.Now + TimeSpan.FromMinutes(5));
                }
            }
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await DependencyService.Get<Schedule.NotificationManager>().Start();
            await CheckIfNotified();
            await RefreshWorkouts();
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
        }

        public Command NavigateManage =>
            new Command(async () =>
            {
                await Navigation.PushAsync(new Tabbs.Manage());
            });

        public Command NavigateCreate =>
            new Command(async () =>
            {
                await Navigation.PushAsync(new Tabbs.Create());
            });
    }
}
