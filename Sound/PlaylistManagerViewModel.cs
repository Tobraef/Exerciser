using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

using Xamarin.Forms;

using App1.Model;
using App1.Model.DBModels;

namespace App1.Sound
{
    public class PlaylistManagerViewModel : BindableBase
    {
        private readonly DB db_;
        private List<IWorkout> cachedWorkouts_;
        private readonly INavigation navigation_;

        public ObservableCollection<PlaylistModel> Playlists { get; private set; } = new ObservableCollection<PlaylistModel>();

        public Command<object> Edit { get; private set; }
        
        public Command<object> Delete { get; private set; }

        public Command<object> AttachToWorkout { get; private set; }

        public Command<object> DetachWorkout { get; private set; }

        public bool IsInitialized { get; private set; }

        private Task ClearUnusedSongs() =>
            db_.Database.ExecuteAsync($"DELETE FROM {SongDBModel.TableName} WHERE NOT EXISTS " +
                $"(SELECT null FROM {SongListDBModel.TableName} WHERE Id = SongId);");

        private void InitializeCommands()
        {
            Edit = new Command<object>(async e =>
            {
                var list = (PlaylistModel)e;
                Playlists.Remove(list);
                var data = await list.EditableData();
                await navigation_.PushAsync(new PlaylistCreator(data.Item1, data.Item2));
                await list.UpdateToChanges();
            });

            Delete = new Command<object>(async e =>
            {
                var list = (PlaylistModel)e;
                Playlists.Remove(list);
                await db_.Database.DeleteAsync<PlaylistDBModel>(list.Playlist.Id);
                await db_.Database.Table<SongListDBModel>().DeleteAsync(s => s.PlaylistId == list.Playlist.Id);
                await ClearUnusedSongs();
            });

            AttachToWorkout = new Command<object>(async e =>
            {
                var list = (PlaylistModel)e;
                var workouts = cachedWorkouts_.Where(c => !list.Workouts.Any(w => w.Id == c.Id)).Select(w => w.Title).ToArray();
                if (workouts.Length == 0) return;
                var result = await App.Current.MainPage.DisplayActionSheet("Choose workout to attach playlist to", "", null, workouts);
                var workout = cachedWorkouts_.Find(w => w.Title.Equals(result));
                await WorkoutAttach(workout, list);
            });

            DetachWorkout = new Command<object>(async e =>
            {
                var list = (PlaylistModel)e;
                if (list.Workouts.Count == 0) return;
                var workouts = list.Workouts.Select(w => w.Title).ToArray();
                var result = await App.Current.MainPage.DisplayActionSheet("Choose workout to detach playlist from", "", null, workouts);
                var workout = cachedWorkouts_.Find(w => w.Title.Equals(result));
                await WorkoutRemove(workout, list);
            });
        }

        private async Task WorkoutRemove(IWorkout workout, PlaylistModel list)
        {
            list.Workouts.Remove(list.Workouts.First(w => w.Id == workout.Id && w.WorkoutType == workout.WorkoutType));
            var type = workout.WorkoutType.ToString();
            await db_.Database.Table<ListWorkoutDBModel>().DeleteAsync(s =>
                s.PlaylistId == list.Playlist.Id && s.WorkoutId == workout.Id && s.WorkoutType.Equals(type));
        }

        private async Task WorkoutAttach(IWorkout workout, PlaylistModel list)
        {
            list.Workouts.Add(workout);
            await db_.Database.InsertAsync(new ListWorkoutDBModel
            {
                PlaylistId = list.Playlist.Id,
                WorkoutId = workout.Id,
                WorkoutType = workout.WorkoutType.ToString()
            });
        }

        public void Initialize(List<PlaylistModel> models, List<IWorkout> workouts)
        {
            models.ForEach(m => Playlists.Add(m));
            cachedWorkouts_ = workouts;
            IsInitialized = true;
        }

        public PlaylistManagerViewModel(DB db, INavigation nav)
        {
            navigation_ = nav;
            db_ = db;
            InitializeCommands();
        }
    }
}
