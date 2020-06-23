using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.Text;

using App1.Model.DBModels;

namespace App1.Model
{
    public class PlaylistModel : BindableBase
    {
        private readonly DB db_;

        public PlaylistDBModel Playlist { get; private set; }

        public string Title => Playlist.Title;

        public string[] Songs { get; private set; }

        public ObservableCollection<IWorkout> Workouts { get; private set; } = new ObservableCollection<IWorkout>();

        public override string ToString()
        {
            string toRet = $"{Title}: {string.Join(", ", Songs ?? new string[] { "No songs" })}.";
            if (Workouts.Count > 0)
            {
                toRet += $" Attached to {string.Join(", ", Workouts.Select(x => x?.Title ?? "nothing").ToList())}";
            }
            return toRet;
        }

        private string description_;
        public string Description { get => description_;  set => SetProperty(ref description_, value); }

        public async Task<Tuple<PlaylistDBModel, List<SongDBModel>>> EditableData()
        {
            return new Tuple<PlaylistDBModel, List<SongDBModel>>(Playlist, 
                (await db_.Database.Table<SongDBModel>().ToListAsync())
                .Where(s => Songs.Contains(s.FileName))
                .ToList());
        }

        public async Task UpdateToChanges()
        {
            Playlist = await db_.Database.FindAsync<PlaylistDBModel>(Playlist.Id);
            var result = await db_.Database.Table<SongListDBModel>()
                .Where(s => s.PlaylistId == Playlist.Id)
                .ToListAsync();
            Songs = (await db_.Database.Table<SongDBModel>().ToListAsync())
                .Where(s => result.Find(x => x.SongId == s.Id) != null)
                .Select(s => s.FileName)
                .ToArray();
        }

        public async Task InitializeAsync()
        {
            var result = await db_.Database.Table<SongListDBModel>()
                .Where(s => s.PlaylistId == Playlist.Id)
                .ToListAsync();
            Songs = (await db_.Database.Table<SongDBModel>().ToListAsync())
                .Where(s => result.Find(x => x.SongId == s.Id) != null)
                .Select(s => s.FileName)
                .ToArray();
            var workoutsResult = await db_.Database.Table<ListWorkoutDBModel>()
                .Where(x => x.PlaylistId == Playlist.Id)
                .ToListAsync();
            foreach (var r in workoutsResult)
            {
                Workouts.Add(await db_.GetWorkout(r.WorkoutId, r.WorkoutType));
            }
            Description = ToString();
        }

        /// <summary>
        /// Requires a call to initialize
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="db"></param>
        public PlaylistModel(PlaylistDBModel playlist, DB db)
        {
            Workouts.CollectionChanged += (o, e) => OnPropertyChanged(nameof(Description));
            db_ = db;
            Playlist = playlist;
        }
    }
}
