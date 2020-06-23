using System;
using System.Collections.Generic;
using System.Text;

using App1.Model;

namespace App1.Schedule
{
    public class Notification
    {
        public int Id { get; set; }
        public IWorkout Workout { get; set; }
        public DateTime TriggerTime { get; set; }
        public TimeSpan Next { get; set; }
    }

    public class NotificationSchedulesManager : Notification
    {
        public bool IsOn { get; set; }

        public string Description => Workout.Title;

        public string TimeDescription => TriggerTime.ToString();
    }
}
