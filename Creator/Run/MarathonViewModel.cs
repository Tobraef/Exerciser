using System;
using System.Collections.Generic;
using System.Linq;

using App1.Model;
using App1.Model.DBModels;

using Xamarin.Forms;

namespace App1.Creator.Run
{
    public class MarathonViewModel : BindableBase
    {
        private readonly DB db_;
        private readonly int? dbInstance_;

        private string title_;
        public string Title { get { return title_; } set { SetProperty(ref title_, value, RefreshCommands); } }

        private TimeSpan duration_;
        public TimeSpan Duration { get => duration_; set => SetProperty(ref duration_, value, RefreshCommands); }

        private int distance_;
        public int Distance { get => distance_; set => SetProperty(ref distance_, value, RefreshCommands); }

        private double differenceNotif_;
        public double DifferenceNotify { get => differenceNotif_; set => SetProperty(ref differenceNotif_, value, RefreshCommands); }

        private bool notifOnDif_;
        public bool NotifOnDifference { get => notifOnDif_; set => SetProperty(ref notifOnDif_, value, RefreshCommands); }

        public Command Add { get; private set; }

        private void RefreshCommands()
        {
            Add.ChangeCanExecute();
        }

        private bool CanAdd()
        {
            if (notifOnDif_)
            {
                return !string.IsNullOrEmpty(title_) &&
                    duration_.TotalSeconds != 0 &&
                    distance_ != 0 &&
                    differenceNotif_ > 0;
            }
            else
            {
                return !string.IsNullOrEmpty(title_) &&
                    duration_.TotalSeconds + distance_ > 0;
            }
        }

        private void InitializeCommands()
        {
            Add = new Command(async () =>
            {
                if (!notifOnDif_)
                {
                    DifferenceNotify = 0;
                }
                if (dbInstance_.HasValue)
                {
                    await db_.Database.UpdateAsync(new MarathonDBModel
                    {
                        Id = dbInstance_,
                        Title = title_,
                        DifferenceNotify = differenceNotif_,
                        Distance = distance_,
                        Duration = (int)duration_.TotalSeconds
                    }, typeof(MarathonDBModel));
                }
                else
                {
                    await db_.Database.InsertAsync(new MarathonDBModel
                    {
                        Title = title_,
                        DifferenceNotify = differenceNotif_,
                        Distance = distance_,
                        Duration = (int)duration_.TotalSeconds
                    }, typeof(MarathonDBModel));
                }
            }, CanAdd);
        }

        public MarathonViewModel(DB db)
        {
            InitializeCommands();
            db_ = db;
            Distance = 5000;
            Duration = TimeSpan.FromMinutes(30);
            Title = "Marathon";
        }

        public MarathonViewModel(DB db, MarathonDBModel model)
        {
            InitializeCommands();
            db_ = db;
            Distance = model.Distance;
            Duration = TimeSpan.FromSeconds(model.Duration);
            Title = model.Title;
            NotifOnDifference = model.DifferenceNotify > 0;
            DifferenceNotify = model.DifferenceNotify;
            dbInstance_ = model.Id;
        }
    }
}
