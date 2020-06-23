using System;
using System.Collections.Generic;
using System.Linq;

using App1.Model.DBModels;
using App1.Workouter;

using Xamarin.Forms;

namespace App1.Model
{
    public class RegularWorkout : IWorkout
    {
        public TimeSpan WorkoutDuration
        {
            get;
        }

        public WorkoutType WorkoutType => WorkoutType.Regular;

        public string Title
        {
            get;
        }

        public string Description
        {
            get;
        }

        public int Id
        {
            get;
        }

        private INavigation Navigation => App.Current.MainPage.Navigation;

        public async System.Threading.Tasks.Task ExecuteWorkout(DB db)
        {
            var workout = await db.Database.Table<RegularWorkoutDBModel>().FirstAsync(w => w.Id == Id);
            var exercises = await db.Database.Table<ExerciseSetDBModel>().Where(e => e.WorkoutId == Id).ToListAsync();
            var workouter = new RegularWorkouter(new RegularViewModel(workout, exercises));
            await Navigation.PushAsync(workouter);
        }

        public async System.Threading.Tasks.Task RemoveFrom(DB db)
        {
            await db.Database.Table<ExerciseSetDBModel>().DeleteAsync(e => e.WorkoutId == Id);
            await db.Database.Table<RegularWorkoutDBModel>().DeleteAsync(e => e.Id == Id);
        }

        public async System.Threading.Tasks.Task Edit(DB db)
        {
            var workout = await db.Database.Table<RegularWorkoutDBModel>().FirstAsync(w => w.Id == Id);
            var exercises = await db.Database.Table<ExerciseSetDBModel>().Where(e => e.WorkoutId == Id).ToListAsync();
            await App.Current.MainPage.Navigation.PushAsync(new Creator.Regular.Creator(workout, exercises));
        }

        public RegularWorkout(RegularWorkoutDBModel model, List<ExerciseSetDBModel> exercises)
        {
            WorkoutDuration = TimeSpan.FromSeconds(exercises.Sum(e => e.BreakBetween * (e.Sets-1)) + model.BreakBetweenExercises * (exercises.Count-1));
            Title = model.Title;
            Description = string.Join(", ", exercises.OrderBy(e => e.Order).Select(e => e.Exercise));
            Id = model.Id;
        }
    }
}
