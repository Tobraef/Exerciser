using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using App1.Model;
using App1.Model.DBModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1.Schedule
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Scheduler : ContentPage
    {
        private readonly NotificationManager manager_;
        private readonly int? notifyId_;

        public bool CanSchedule => (datePicker.Date + timePicker.Time) > DateTime.Now;

        private List<KeyValuePair<string, TimeSpan>> RepeatTimePicker =>
            new List<KeyValuePair<string, TimeSpan>>
            {
                new KeyValuePair<string, TimeSpan>( "Every 12 hours", TimeSpan.FromHours(12) ),
                new KeyValuePair<string, TimeSpan>(  "Everyday", TimeSpan.FromDays(1) ),
                new KeyValuePair<string, TimeSpan>(  "Every two days", TimeSpan.FromDays(2) ),
                new KeyValuePair<string, TimeSpan>(  "Every three days", TimeSpan.FromDays(3) ),
                new KeyValuePair<string, TimeSpan>(  "Every four days", TimeSpan.FromDays(4) ),
                new KeyValuePair<string, TimeSpan>(  "Every five days", TimeSpan.FromDays(5) ),
                new KeyValuePair<string, TimeSpan>(  "Every six days", TimeSpan.FromDays(6) ),
                new KeyValuePair<string, TimeSpan>(  "Every week", TimeSpan.FromDays(7) )
            };

        public Scheduler(List<IWorkout> workouts)
        {
            manager_ = DependencyService.Get<NotificationManager>();
            InitializeComponent();
            datePicker.Date = DateTime.Today;
            pickerRepeat.ItemsSource = RepeatTimePicker;
            pickerRepeat.SelectedIndex = 1;
            listView.ItemsSource = workouts;
        }

        public Scheduler(List<IWorkout> workouts, Notification notif)
            :this(workouts)
        {
            notifyId_ = notif.Id;
            pickerRepeat.SelectedItem = pickerRepeat.ItemsSource
                .Cast<KeyValuePair<string, TimeSpan>>().First(kvp => kvp.Value == notif.Next);
            listView.SelectedItem = notif.Workout;
            timePicker.Time = notif.TriggerTime.TimeOfDay;
            timePicker.PropertyChanged += (o, e) => scheduleButton.IsEnabled = CanSchedule;
            datePicker.PropertyChanged += (o, e) => scheduleButton.IsEnabled = CanSchedule;
        }

        private async Task AddNotification(DateTime when)
        {
            if (switchRepeat.IsToggled)
            {
                await manager_.ScheduleReocurring((IWorkout)listView.SelectedItem, when,
                    ((KeyValuePair<string, TimeSpan>)pickerRepeat.SelectedItem).Value);
            }
            else
            {
                await manager_.Schedule((IWorkout)listView.SelectedItem, when);
            }
        }

        private async Task EditNotification(DateTime when)
        {
            if (switchRepeat.IsToggled)
            {
                await manager_.EditSchedule(notifyId_.Value, (IWorkout)listView.SelectedItem, when,
                    ((KeyValuePair<string, TimeSpan>)pickerRepeat.SelectedItem).Value);
            }
            else
            {
                await manager_.EditSchedule(notifyId_.Value, (IWorkout)listView.SelectedItem, when,
                    TimeSpan.Zero);
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            if (listView.SelectedItem == null) return;
            DateTime when = datePicker.Date + timePicker.Time;
            if (notifyId_.HasValue)
            {
                await EditNotification(when);
            }
            else
            {
                await AddNotification(when);
            }
            await Navigation.PopAsync();
        }

        private void Switch_Repeat(object sender, ToggledEventArgs e)
        {
            if (switchRepeat.IsToggled)
            {
                ((StackLayout)switchRepeat.Parent).Children.Add(pickerRepeat);
            }
            else
            {
                ((StackLayout)switchRepeat.Parent).Children.Remove(pickerRepeat);
            }
        }
    }
}