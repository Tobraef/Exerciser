using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;

using App1.Model;
using App1.Model.DBModels;

namespace App1.Creator.Regular
{
    public class RegularCreatorViewModel : BindableBase
    {
        private readonly DB db_;
        private readonly int? dbId_;

        public List<Row> Rows { get; } = new List<Row>();
        private StackLayout rowsLayout_;
        public StackLayout RowsLayout { get => rowsLayout_; set { rowsLayout_ = value; Rows.ForEach(r => value.Children.Add(r)); } }

        private string name_;
        public string Name
        {
            get { return name_; }
            set { SetProperty(ref name_, value, RefreshCommands); }
        }

        private string breakBetweenExercises_;
        public string BreakBetween
        {
            get { return breakBetweenExercises_; }
            set { SetProperty(ref breakBetweenExercises_, value, RefreshCommands); }
        }

        public Command AddRow { get; private set; }

        public Command Add { get; private set; }

        public Command<object> RemoveRow { get; private set; }

        private Task<int> NextWorkoutId()
        {
            return db_.Database.ExecuteScalarAsync<int>("SELECT (COALESCE(MAX(Id), 0) + 1) FROM " + RegularWorkoutDBModel.TableName);
        }

        private RegularWorkoutDBModel CreateFromInput()
        {
            return new RegularWorkoutDBModel
            {
                Title = Name,
                BreakBetweenExercises = int.Parse(BreakBetween)
            };
        }

        private async Task EditWorkout()
        {
            var workout = CreateFromInput();
            workout.Id = dbId_.Value;
            await db_.Database.InsertOrReplaceAsync(workout, typeof(RegularWorkoutDBModel));
            for (int i = 0; i < Rows.Count; ++i)
            {
                await Rows[i].UpdateDB(workout.Id, i, db_);
            }
        }

        private async Task AddWorkout()
        {
            var wid = await NextWorkoutId();
            var workout = CreateFromInput();
            workout.Id = wid;
            await db_.Database.InsertAsync(workout, typeof(RegularWorkoutDBModel));
            for (int i = 0; i < Rows.Count; ++i)
            {
                await Rows[i].UpdateDB(workout.Id, i, db_);
            }
        }

        private void RefreshCommands()
        {
            Add.ChangeCanExecute();
        }

        private void InitializeCommands()
        {
            Add = new Command(async () =>
            {
                if (dbId_.HasValue)
                {
                    await EditWorkout();
                }
                else
                {
                    await AddWorkout();
                }
                await App.Current.MainPage.Navigation.PopAsync();
            }, () => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(BreakBetween) && Rows.Count > 0 && Rows.All(r => r.IsValid));
            AddRow = new Command(() =>
            {
                var row = new Row();
                row.ViewModel.PropertyChanged += (o, e) => RefreshCommands();
                Rows.Add(row);
                RowsLayout.Children.Add(row);
                RefreshCommands();
            });
            RemoveRow = new Command<object>(r =>
            {
                var rw = (Row)r;
                Rows.Remove((Row)r);
                RowsLayout.Children.Remove(rw);
                RefreshCommands();
            });
        }

        public RegularCreatorViewModel(DB db)
        {
            InitializeCommands();
            db_ = db;
            var row = new Row();
            row.ViewModel.PropertyChanged += (o, e) => RefreshCommands();
            Rows.Add(row);
        }

        public RegularCreatorViewModel(DB db, RegularWorkoutDBModel workout, List<ExerciseSetDBModel> exercises)
        {
            InitializeCommands();
            db_ = db;
            dbId_ = workout.Id;
            exercises.ForEach(e => Rows.Add(new Row(e)));
            foreach (var r in Rows)
            {
                r.ViewModel.PropertyChanged += (o, e) => RefreshCommands();
            }
            Name = workout.Title;
            BreakBetween = workout.BreakBetweenExercises.ToString();
        }
    }
}