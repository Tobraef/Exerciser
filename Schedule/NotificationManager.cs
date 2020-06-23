using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

using Plugin.LocalNotifications;
using Xamarin.Essentials;

using App1.Model;
using App1.Model.DBModels;

namespace App1.Schedule
{
    public class NotificationManager
    {
        private readonly DB database_;
        private readonly Timer timer_;
        private Notification nextToPop_;

        private async Task PushWorkout(Notification current)
        {
            if (App.IsAppFocused)
            {
                var reaction = await App.Current.MainPage.DisplayAlert("Time for scheduled workout: " + current.Workout.Title,
                    current.Workout.Description, "Let's go", "Dismiss");
                if (reaction)
                {
                    await current.Workout.ExecuteWorkout(database_);
                }
            }
            else
            {
                App.Current.Properties["Request"] = current.Workout;
                Vibration.Vibrate();
                CrossLocalNotifications.Current.Show("Scheduled workout: " + current.Workout.Title, current.Workout.Description);
            }
        }

        private async void Trigger(object sender, ElapsedEventArgs e)
        {
            var current = nextToPop_;
            if (!nextToPop_.Next.Equals(TimeSpan.Zero))
            {
                var s = await ScheduleDBModel.FromDB(nextToPop_.Id, database_);
                s.When += s.Interval;
                await s.AddOrUpdate(database_);
            }
            else
            {
                await database_.Database.DeleteAsync<ScheduleDBModel_>(current.Id);
            }
            if (App.IsUserWorking)
            {
                await Schedule(current.Workout, DateTime.Now + TimeSpan.FromMinutes(5));
                await ReloadSchedule();
                return;
            }
            await ReloadSchedule();
            await PushWorkout(current);
        }

        private async Task<Notification> CreateLocalNotification(ScheduleDBModel model)
        {
            var workout = await database_.GetWorkout(model.WorkoutId, model.WorkoutType);
            return new Notification
            {
                Id = model.Id,
                Next = model.Interval,
                TriggerTime = model.When,
                Workout = workout
            };
        }

        private async Task ClearAndUpdateOverdue()
        {
            var overdue = (await ScheduleDBModel.FromDB(database_))
                .Where(s => s.Interval == TimeSpan.Zero)
                .ToList();
            var toClear = overdue.Where(s => s.Interval == TimeSpan.Zero);
            var toUpdate = overdue.Except(toClear);
            foreach (var t in toClear)
            {
                await t.Delete(database_);
            }
            foreach (var s in toUpdate)
            {
                while (s.When < DateTime.Now)
                {
                    s.When += s.Interval;
                }
                await s.AddOrUpdate(database_);
            }
        }

        private async Task ReloadSchedule()
        {
            timer_.Stop();
            var items = await ScheduleDBModel.FromDB(database_).ConfigureAwait(false);
            var next = (from i in items
                        where i.IsActive && i.When > DateTime.Now
                        orderby i.When ascending
                        select i).FirstOrDefault();
            if (next != null)
            {
                ScheduleNotification(await CreateLocalNotification(next));
            }
            else
            {
                nextToPop_ = null;
            }
        }

        private Task<int> NextScheduleId => database_.Database.ExecuteScalarAsync<int>("SELECT COALESCE(MAX(Id) + 1,0) FROM " + ScheduleDBModel_.TableName);

        private Task AddScheduleToDb(Notification notif) =>
            new ScheduleDBModel
            {
                Id = notif.Id,
                Interval = notif.Next,
                IsActive = true,
                When = notif.TriggerTime,
                WorkoutId = notif.Workout.Id,
                WorkoutType = notif.Workout.WorkoutType
            }.AddOrUpdate(database_);

        private void ScheduleNotification(Notification notif)
        {
            timer_.Stop();
            timer_.Interval = (notif.TriggerTime - DateTime.Now).TotalMilliseconds;
            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
            {
                await App.Current.MainPage.DisplayAlert("title", (timer_.Interval / 1000).ToString(), "OK");
            });
            timer_.Start();
            nextToPop_ = notif;
        }

        public async Task ScheduleReocurring(IWorkout workout, DateTime when, TimeSpan interval)
        {
            Notification toInsert;
            if (when < nextToPop_?.TriggerTime)
            {
                toInsert = nextToPop_;
                nextToPop_.Workout = workout;
                nextToPop_.TriggerTime = when;
                nextToPop_.Id = await NextScheduleId;
                nextToPop_.Next = interval;
            }
            else
            {
                var id = await NextScheduleId;
                toInsert = new Notification
                {
                    Id = id,
                    Next = interval,
                    TriggerTime = when,
                    Workout = workout
                };
            }
            await AddScheduleToDb(toInsert);
            if (nextToPop_ == null)
            {
                await ReloadSchedule();
            }
        }

        public async Task RemoveSchedule(int id)
        {
            await database_.Database.DeleteAsync<ScheduleDBModel_>(id);
            if (nextToPop_?.Id == id)
            {
                await ReloadSchedule();
            }
        }

        public async Task ChangeScheduleState(int id, bool isActive)
        {
            var schedule = await ScheduleDBModel.FromDB(id, database_);
            schedule.IsActive = isActive;
            await schedule.AddOrUpdate(database_);
            await ReloadSchedule();
        }

        public async Task EditSchedule(int id, IWorkout workout, DateTime when, TimeSpan interval)
        {
            var schedule = await ScheduleDBModel.FromDB(id, database_);
            schedule.WorkoutId = workout.Id;
            schedule.WorkoutType = workout.WorkoutType;
            schedule.When = when;
            schedule.Interval = interval;
            await schedule.AddOrUpdate(database_);
            await ReloadSchedule();
        }

        /// <summary>
        /// Loads a schedule and refreshes states.
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            if (nextToPop_ != null) return;
            await ClearAndUpdateOverdue().ConfigureAwait(false);
            await ReloadSchedule().ConfigureAwait(false);
        }

        public Task Schedule(IWorkout workout, DateTime when) =>
            ScheduleReocurring(workout, when, TimeSpan.Zero);

        public NotificationManager()
        {
            database_ = new DB();
            timer_ = new Timer();
            timer_.Elapsed += Trigger;
        }
    }
}
