using System;
using System.Collections.Generic;
using System.Linq;

using App1.Model.DBModels;

namespace App1.Workouter
{
    public class CrossfitViewModel
    {
        private readonly ExerciseSet exerciseSet_;
        private int sets_;
        private readonly int breakBetweenSets_;


        public string ExerciseName => exerciseSet_.ExerciseName;

        public int Duration => exerciseSet_.ExerciseDuration;

        public int Set => sets_;

        public string NextExerciseName => sets_ == 0 ? "FINISH" : exerciseSet_.NextExerciseName;

        public int BreakDuration
        {
            get;
            private set;
        }

        public bool Next()
        {
            if (sets_ == 0)
            {
                return false;
            }
            if (exerciseSet_.Next())
            {
                BreakDuration = exerciseSet_.ExerciseBreak;
            }
            else
            {
                --sets_;
                if (sets_ == 0)
                {
                    BreakDuration = 0;
                }
                else
                {
                    BreakDuration = breakBetweenSets_;
                }
            }
            return true;
        }

        public int Id { get; }

        public CrossfitViewModel(CrossfitWorkoutDBModel model)
        {
            exerciseSet_ = new ExerciseSet(model.BreakBetweenExercises, model.ExerciseTime, model.QuedExercises.Split('_'));
            breakBetweenSets_ = model.BreakBetweenSets;
            sets_ = model.Sets;
            Id = model.Id.Value;
        }

        private class ExerciseSet
        {
            private List<string> exercises_;
            private int index_;

            public string Names => string.Join(", ", exercises_);

            public string QuedNames => string.Join("_", exercises_);

            public string NextExerciseName => index_ == exercises_.Count - 1 ? exercises_[0] : exercises_[index_ + 1];

            public int ExerciseDuration
            {
                get;
            }

            public int ExerciseBreak
            {
                get;
            }

            public string ExerciseName => exercises_[index_];

            public bool Next()
            {
                if (++index_ == exercises_.Count)
                {
                    index_ = 0;
                }
                if (index_ == exercises_.Count - 1)
                {
                    return false;
                }
                return true;
            }

            public int TotalDuration => exercises_.Count * ExerciseDuration + (exercises_.Count - 1) * ExerciseBreak;

            public ExerciseSet(int breakDuration, int exerciseDuration, IEnumerable<string> exercises)
            {
                ExerciseBreak = breakDuration;
                ExerciseDuration = exerciseDuration;
                exercises_ = exercises.ToList();
                index_ = exercises_.Count - 1;
            }
        }
    }
}
