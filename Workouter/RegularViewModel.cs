using System;
using System.Collections.Generic;
using System.Linq;

using App1.Model.DBModels;

namespace App1.Workouter
{
    public class RegularViewModel
    {
        private List<RegularExercise> exercises_;
        private int breakBetweenExercises_;
        private string title_;
        private int id_;
        private int index_;
        private int counter_;

        private const int exercisesFinishedBreak = 30;
        public string FullExerciseName => string.Concat(exercises_[index_].Name, " - ", exercises_[index_].Weight, "kg, times ",
            exercises_[index_].Repeats, ' ', counter_ + 1, '/', exercises_[index_].Series);
        public string NextExercise => index_ + 1 == exercises_.Count ?
            "FINISH" :
            exercises_[index_ + 1].Name;
        public int TimeToNext { get; private set; }

        public bool Next()
        {
            counter_++;
            if (counter_ == exercises_[index_].Series - 1)
            {
                if (index_ == exercises_.Count - 1)
                {
                    TimeToNext = exercisesFinishedBreak;
                }
                else
                {
                    TimeToNext = breakBetweenExercises_;
                }
            }
            else if (counter_ == exercises_[index_].Series)
            {
                counter_ = 0;
                index_++;
                if (index_ == exercises_.Count)
                {
                    return false;
                }
                TimeToNext = exercises_[index_].BreakBetween;
            }
            else
            {
                TimeToNext = exercises_[index_].BreakBetween;
            }
            return true;
        }

        public RegularViewModel(RegularWorkoutDBModel workout, List<ExerciseSetDBModel> exercises)
        {
            exercises_ = exercises.OrderBy(x => x.Order).Select(e => new RegularExercise(
                e.Exercise,
                breakBetween: e.BreakBetween,
                startReps: e.Reps,
                series: e.Sets,
                repsChange: e.Change,
                weight: e.Weight)).ToList();
            breakBetweenExercises_ = workout.BreakBetweenExercises;
            title_ = workout.Title;
            id_ = workout.Id;
            index_ = 0;
            counter_ = -1;
        }

        public class RegularExercise
        {
            private int repsChange_;

            public int Series
            {
                get;
            }

            public int Repeats
            {
                get;
                private set;
            }

            public string Name
            {
                get;
            }

            public int BreakBetween
            {
                get;
            }

            public int Weight
            {
                get;
            }

            public int RepsChange => repsChange_;

            /// <summary>
            /// Changes internal values of the exercise based on user input
            /// </summary>
            public void Change()
            {
                Repeats += repsChange_;
                if (Repeats < 1)
                {
                    Repeats = 1;
                }
            }

            public RegularExercise(string name, int breakBetween, int startReps, int series, int weight, int repsChange = 0)
            {
                repsChange_ = repsChange;
                Name = name;
                BreakBetween = breakBetween;
                Repeats = startReps;
                Series = series;
                Weight = weight;
            }
        }
    }
}
