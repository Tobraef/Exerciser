using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace App1.Model.DBModels
{
    [Table(TableName)]
    public class PlaylistDBModel
    {
        public const string TableName = "Playlist";

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }

    [Table(TableName)]
    public class SongDBModel
    {
        public const string TableName = "Song";

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FilePath { get; set; }

        public string FileName { get; set; }

        public override string ToString()
        {
            return $"SONG Id {Id} FilePath {FilePath} FileName {FileName}";
        }

        public override bool Equals(object obj)
        {
            var s = (SongDBModel)obj;
            return s.FileName.Equals(FileName);
        }

        public override int GetHashCode()
        {
            return FileName.GetHashCode();
        }
    }

    [Table(TableName)]
    public class SongListDBModel
    {
        public const string TableName = "SongList";
        public int PlaylistId { get; set; }

        public int SongId { get; set; }

        public int Order { get; set; }

        public override string ToString()
        {
            return $"SONGLIST: PlaylistId {PlaylistId} SongId {SongId} Order {Order}"; 
        }
    }

    [Table(TableName)]
    public class ListWorkoutDBModel
    {
        public const string TableName = "ListWorkout";

        public int PlaylistId { get; set; }

        public int WorkoutId { get; set; }

        public string WorkoutType { get; set; }

        public override string ToString()
        {
            return $"LISTWORKOUT: PlaylistId {PlaylistId} WorkoutId {WorkoutId} WorkoutType {WorkoutType}";
        }
    }
}
