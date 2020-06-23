using System;

using SQLite;

namespace App1.Model.DBModels
{
    [Table(TableName)]
    public class CrossfitWorkoutDBModel
    {
        public const string TableName = "CrossfitWorkout";

        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }

        [Unique]
        public string Name { get; set; }

        public int ExerciseTime { get; set; }

        public int Sets { get; set; }

        public int BreakBetweenExercises { get; set; }

        public int BreakBetweenSets { get; set; }

        /// <summary>
        /// Separated by '_' exercises
        /// </summary>
        public string QuedExercises { get; set; }
    }
}
