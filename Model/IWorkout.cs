using System;
using System.Collections.Generic;
using System.Text;

namespace App1.Model
{
    public enum WorkoutType
    {
        Marathon,
        Regular,
        Crossfit
    }


    public interface IWorkout
    {
        TimeSpan WorkoutDuration
        {
            get;
        }

        WorkoutType WorkoutType
        {
            get;
        }

        string Title
        {
            get;
        }

        string Description
        {
            get;
        }

        int Id
        {
            get;
        }

        System.Threading.Tasks.Task ExecuteWorkout(DB db);

        System.Threading.Tasks.Task RemoveFrom(DB db);

        System.Threading.Tasks.Task Edit(DB db);
    }
}
