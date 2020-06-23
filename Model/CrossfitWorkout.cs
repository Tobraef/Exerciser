using System;
using System.Collections.Generic;
using System.Linq;

using App1.Creator.Crossfit;
using App1.Model.DBModels;
using App1.Workouter;

namespace App1.Model
{
    public class CrossfitWorkout : IWorkout
    {
        public TimeSpan WorkoutDuration
        {
            get;
        }

        public WorkoutType WorkoutType => WorkoutType.Crossfit;

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

        public async System.Threading.Tasks.Task ExecuteWorkout(DB db)
        {
            var workout = await db.Database.Table<CrossfitWorkoutDBModel>().FirstAsync(w => w.Id == Id);
            await App.Current.MainPage.Navigation.PushAsync(new CrossfitWorkouter(new CrossfitViewModel(workout)));
        }

        public async System.Threading.Tasks.Task RemoveFrom(DB db)
        {
            await db.Database.Table<CrossfitWorkoutDBModel>().DeleteAsync(e => e.Id == Id);
        }

        public async System.Threading.Tasks.Task Edit(DB db)
        {
            var workout = await db.Database.Table<CrossfitWorkoutDBModel>().FirstAsync(w => w.Id == Id);
            await App.Current.MainPage.Navigation.PushAsync(new CrossfitCreator(workout));
        }

        public CrossfitWorkout(CrossfitWorkoutDBModel model)
        {
            WorkoutDuration = TimeSpan.FromSeconds(((model.QuedExercises.Count(c => c == '_') - 1) * model.BreakBetweenExercises) +
                (model.ExerciseTime * model.QuedExercises.Count(c => c == '_')) * model.Sets +
                model.BreakBetweenSets * (model.Sets - 1));
            Title = model.Name;
            Description = "Exercise duration: " + model.ExerciseTime + "\nExercise break: " + model.BreakBetweenExercises +
                "\nSets: " + model.Sets + ", with break between sets: " + model.BreakBetweenSets + "\nExercises: " +
                string.Join(", ", model.QuedExercises.Split('_'));
            Id = model.Id.Value;
        }
    }
}
