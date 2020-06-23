using System;

using SQLite;

namespace App1.Model.DBModels
{
    [Table(TableName)]
    public class RegularWorkoutDBModel
    {
        public const string TableName = "RegularWorkout";

        [PrimaryKey]
        public int Id { get; set; }

        public string Title { get; set; }

        public int BreakBetweenExercises { get; set; }
    }

    [Table(TableName)]
    public class ExerciseSetDBModel
    {
        public const string TableName = "RegularExercise";
        
        public int? Id { get; set; }

        public int WorkoutId { get; set; }

        public int Order { get; set; }

        public string Exercise { get; set; }

        public int Weight { get; set; }

        public int Reps { get; set; }

        public int Sets { get; set; }

        public int Change { get; set; }

        public int BreakBetween { get; set; }
    }
}
