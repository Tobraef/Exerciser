using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using App1.Model;
using App1.Model.DBModels;

namespace App1.Schedule
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SchedulesManager : ContentPage
    {
        private readonly DB database_ = new DB();
        private readonly NotificationManager manager_;
        private readonly ObservableCollection<NotificationSchedulesManager> notifs_;

        public Command Add => new Command(async () =>
        {
            await Navigation.PushAsync(new Scheduler(await database_.GetDescriptions()));
        });

        public Command<int> Remove => new Command<int>(async id =>
        {
            notifs_.Remove(notifs_.First(s => s.Id == id));
            await manager_.RemoveSchedule(id);
        });

        public Command<int> Edit => new Command<int>(async id =>
        {
            await Navigation.PushAsync(new Scheduler(await database_.GetDescriptions(), notifs_.First(s => s.Id == id)));
            await UpdateList();
        });

        private async Task UpdateList()
        {
            var items = await ScheduleDBModel.FromDB(database_).ConfigureAwait(false);
            notifs_.Clear();
            foreach (var s in items)
            {
                notifs_.Add(new NotificationSchedulesManager
                {
                    Id = s.Id,
                    IsOn = s.IsActive,
                    Next = s.Interval,
                    Workout = await database_.GetWorkout(s.WorkoutId, s.WorkoutType).ConfigureAwait(false),
                    TriggerTime = s.When
                });
            }
        }

        public SchedulesManager()
        {
            manager_ = DependencyService.Get<NotificationManager>();
            InitializeComponent();
            notifs_ = new ObservableCollection<NotificationSchedulesManager>();
            listViewSchedules.ItemsSource = notifs_;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await UpdateList();
        }

        public async void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            var member = ((NotificationSchedulesManager)((Switch)sender).BindingContext);
            // member is null on removal, weird but ok
            if (member != null)
            {
                await manager_.ChangeScheduleState(member.Id, member.IsOn);
            }
        }
    }
}