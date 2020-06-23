using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Xamarin.Forms;

using App1.Model;
using App1.Model.DBModels;
using App1.Creator.Regular;
using App1.Creator.Crossfit;

using System.Runtime.CompilerServices;

namespace App1.Workouter.Managment
{
    public class WorkoutManagerViewModel : BindableObject
    {
        private readonly INavigation navigation_ = App.Current.MainPage.Navigation;
        private readonly DB database_ = new DB();

        private Task<bool> AskQuestion(string text, string title = "Are you sure?") =>
            App.Current.MainPage.DisplayAlert(title, text, "Ok", "Cancel");

        public ObservableCollection<IWorkout> Workouts { get; } = new ObservableCollection<IWorkout>();

        public async Task RefreshList()
        {
            Workouts.Clear();
            var workouts = await database_.GetDescriptions().ConfigureAwait(false);
            foreach (var w in workouts)
            {
                Workouts.Add(w);
            }
        }

        public Command Add { get; private set; }

        public Command<object> Edit { get; private set; }

        public Command<object> Delete { get; private set; }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Add.ChangeCanExecute();
            Edit.ChangeCanExecute();
            Delete.ChangeCanExecute();
        }

        public WorkoutManagerViewModel()
        {
            Add = new Command(async () =>
            {
                await navigation_.PushAsync(new WorkoutCreation());
                await RefreshList();
            });

            Edit = new Command<object>(async w =>
            {
                var workout = Workouts.First(e => e == w);
                await workout.Edit(database_);
                Workouts.Remove(workout);
                Workouts.Add(await database_.GetWorkout(workout.Id, workout.WorkoutType));
            });

            Delete = new Command<object>(async w =>
            {
                var workout = (IWorkout)w;
                if (await AskQuestion("Delete " + workout.Title + " is permament"))
                {
                    Workouts.Remove(workout);
                    await workout.RemoveFrom(database_);
                }
            });
        }
    }
}
