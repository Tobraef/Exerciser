using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using App1.Model.DBModels;
using App1.Workouter;
using App1.Creator.Run;

namespace App1.Model
{
    public class MarathonWorkout : IWorkout
    {
        public TimeSpan WorkoutDuration { get; private set; }

        public WorkoutType WorkoutType => WorkoutType.Marathon;

        public string Title { get; private set; }

        public string Description { get; private set; }

        public int Id { get; private set; }

        private Task<MarathonDBModel> Instance(DB db) =>
            db.Database.FindAsync<MarathonDBModel>(Id);

        public async Task Edit(DB db)
        {
            await App.Current.MainPage.Navigation.PushAsync(new Marathon(await Instance(db)));
        }

        public async Task ExecuteWorkout(DB db)
        {
            var workout = await db.Database.FindAsync<MarathonDBModel>(Id);
            string type = WorkoutType.ToString();
            var pw = await db.Database.Table<ListWorkoutDBModel>().Where(p => p.WorkoutId == Id && p.WorkoutType.Equals(type)).ToListAsync();
            var playlists = await db.Database.Table<PlaylistDBModel>().ToListAsync();
            App.Debug(new string(playlists.SelectMany(p => p.ToString()).ToArray()));
            await App.Current.MainPage.Navigation.PushAsync(new MarathonWorkouter(workout, 
                playlists.Where(p => pw.Any(x => x.PlaylistId == p.Id)).ToList()));
        }

        public Task RemoveFrom(DB db)
        {
            return db.Database.DeleteAsync<MarathonDBModel>(Id);
        }

        private string NoDuration(int distance)
        {
            var km = distance / 1000;
            if (km != 0)
            {
                if (distance - km * 1000 != 0)
                {
                    return km + " km run";
                }
                else
                {
                    return km + "'" + (distance - km * 1000) + " km run";
                }
            }
            else
            {
                return distance + " m run";
            }
        }

        private string NoDistance(int duration)
        {
            return "Free run for " + TimeSpan.FromSeconds(duration).ToString();
        }

        private string BothPresent(MarathonDBModel model)
        {
            return NoDuration(model.Distance) + ", for " + TimeSpan.FromSeconds(model.Duration) + ", notif when difference is higher than "
                + model.DifferenceNotify.ToString("0.0");
        }

        private string BuildDescription (MarathonDBModel model)
        {
            if (model.Duration == 0)
            {
                return NoDuration(model.Distance);
            }
            else if (model.Distance == 0)
            {
                return NoDistance(model.Duration);
            }
            return BothPresent(model);
        }

        public MarathonWorkout(MarathonDBModel model)
        {
            if (model.Duration != 0)
            {
                WorkoutDuration = TimeSpan.FromSeconds(model.Duration);
            }
            else
            {
                WorkoutDuration = TimeSpan.FromMinutes((model.Distance / 1000) * 6);
            }
            Title = model.Title;
            Description = BuildDescription(model);
            Id = model.Id.Value;
        }
    }
}
