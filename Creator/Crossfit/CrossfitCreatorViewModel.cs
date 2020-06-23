using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

using Xamarin.Forms;

using App1.Model;
using App1.Model.DBModels;

namespace App1.Creator.Crossfit
{
    public class Row
    {
        public string Number { get; }
        public string Name { get; set; }

        public Row(int number)
        {
            Number = number.ToString() + '.';
        }
        public Row(int number, string name)
        {
            Number = number.ToString() + '.';
            Name = name;
        }
    }

    public class CrossfitCreatorViewModel : BindableBase
    {
        private readonly DB db_;
        private readonly int? dbId_;

        public ObservableCollection<Row> Rows { get; } = new ObservableCollection<Row>();

        private string name_;
        public string Name { get { return name_; } set => SetProperty(ref name_, value, RefreshCommands); }

        private string sets_;
        public string Sets { get => sets_; set => SetProperty(ref sets_, value, RefreshCommands); }

        private string breakBetweenSets_;
        public string BreakBetweenSets { get => breakBetweenSets_; set => SetProperty(ref breakBetweenSets_, value, RefreshCommands); }

        private string exerciseDuration_;
        public string ExerciseDuration { get => exerciseDuration_; set => SetProperty(ref exerciseDuration_, value, RefreshCommands); }

        private string breakBetweenExercises_;
        public string BreakBetweenExercises { get => breakBetweenExercises_; set => SetProperty(ref breakBetweenExercises_, value, RefreshCommands); }

        public Command<object> RemoveRow { get; private set; }

        public Command Add { get; private set; }

        public Command AddRow { get; private set; }

        public void RefreshCommands()
        {
            Add.ChangeCanExecute();
        }

        private Task UpdateModel()
        {
            return db_.Database.InsertOrReplaceAsync(new CrossfitWorkoutDBModel
            {
                Id = dbId_,
                Name = Name,
                Sets = int.Parse(Sets),
                BreakBetweenExercises = int.Parse(BreakBetweenExercises),
                BreakBetweenSets = int.Parse(BreakBetweenSets),
                ExerciseTime = int.Parse(ExerciseDuration),
                QuedExercises = string.Join("_", Rows.Select(r => r.Name))
            });
        }

        public void InitializeCommands()
        {
            Add = new Command(async () =>
            {
                await UpdateModel();
                await App.Current.MainPage.Navigation.PopAsync();
            }, () => !(string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Sets) || string.IsNullOrEmpty(BreakBetweenExercises)
                    || string.IsNullOrEmpty(BreakBetweenSets) || string.IsNullOrEmpty(ExerciseDuration)) && Rows.Count > 0);
            Rows.CollectionChanged += (o,e) => RefreshCommands();
            AddRow = new Command(() =>
            {
                Rows.Add(new Row(Rows.Count + 1));
            });

            RemoveRow = new Command<object>(e =>
            {
                Rows.Remove((Row)e);
            });
        }

        public CrossfitCreatorViewModel(DB db) { db_ = db; InitializeCommands(); }

        public CrossfitCreatorViewModel(DB db, CrossfitWorkoutDBModel model) 
            :this(db)
        {
            dbId_ = model.Id;
            Name = model.Name;
            ExerciseDuration = model.ExerciseTime.ToString();
            BreakBetweenExercises = model.BreakBetweenExercises.ToString();
            BreakBetweenSets = model.BreakBetweenSets.ToString();
            Sets = model.Sets.ToString();
            var exercises = model.QuedExercises.Split('_');
            int i = 1;
            foreach (var e in exercises)
            {
                Rows.Add(new Row(i++, e));
            }
        }
    }
}
