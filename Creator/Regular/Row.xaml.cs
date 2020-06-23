using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using App1.Model;
using App1.Model.DBModels;

namespace App1.Creator.Regular
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Row : ContentView
    {
        public ExerciseVM ViewModel { get; private set; }

        public bool IsValid => ViewModel.IsValid;

        public Row()
        {
            ViewModel = new ExerciseVM();
            InitializeComponent();
        }

        public Row(ExerciseSetDBModel exercise)
        {
            ViewModel = new ExerciseVM(exercise);
            InitializeComponent();
        }

        public Task UpdateDB(int workoutId, int order, DB db)
        {
            return ViewModel.UpdateDb(workoutId, order, db);
        }
    }

    public class ExerciseVM : BindableBase
    {
        private readonly int? dbId_;

        private string name_;
        public string Name { get { return name_; } set { SetProperty(ref name_, value); } }

        private string series_;
        public string Series { get { return series_; } set { SetProperty(ref series_, value); } }

        private string repeats_;
        public string Repeats { get { return repeats_; } set { SetProperty(ref repeats_, value); } }

        private string weight_;
        public string Weight { get { return weight_; } set { SetProperty(ref weight_, value); } }

        private string change_;
        public string Change { get { return change_; } set { SetProperty(ref change_, value); } }

        private string time_;
        public string Time { get { return time_; } set { SetProperty(ref time_, value); } }


        public bool IsValid
        {
            get
            {
                return !(string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Series) || string.IsNullOrEmpty(Repeats) ||
                    string.IsNullOrEmpty(Weight) || string.IsNullOrEmpty(Change) || string.IsNullOrEmpty(Time));
            }
        }

        private Task<int> NextExerciseId(DB db) =>
            db.Database.ExecuteScalarAsync<int>("SELECT (COALESCE(MAX(Id), 0) + 1) FROM " + ExerciseSetDBModel.TableName);

        public async Task UpdateDb(int workoutId, int order, DB db)
        {
            var dbModel = new ExerciseSetDBModel
            {
                WorkoutId = workoutId,
                Order = order,
                BreakBetween = int.Parse(Time),
                Change = int.Parse(Change),
                Sets = int.Parse(Series),
                Exercise = Name,
                Reps = int.Parse(Repeats),
                Weight = int.Parse(Weight)
            };
            if (dbId_.HasValue)
            {
                dbModel.Id = dbId_;
                await db.Database.Table<ExerciseSetDBModel>().DeleteAsync(e => e.Id == dbModel.Id);
                await db.Database.InsertAsync(dbModel);
            }
            else
            {
                dbModel.Id = await NextExerciseId(db);
                await db.Database.InsertAsync(dbModel);
            }
        }

        public ExerciseVM() { }

        public ExerciseVM(ExerciseSetDBModel exercise)
        {
            App.Debug(exercise.Id.ToString());
            dbId_ = exercise.Id;
            Series = exercise.Sets.ToString();
            Repeats = exercise.Reps.ToString();
            Time = exercise.BreakBetween.ToString();
            Weight = exercise.Weight.ToString();
            Change = exercise.Change.ToString();
            Name = exercise.Exercise;
        }
    }
}