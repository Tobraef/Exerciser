using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using SQLite;

namespace App1.Model.DBModels
{
    public class ScheduleDBModel
    {
        public const string TableName = "Schedule";
        public int Id { get; set; }
        public int WorkoutId { get; set; }
        public WorkoutType WorkoutType { get; set; }
        public TimeSpan Interval { get; set; }
        public DateTime When { get; set; }
        public bool IsActive { get; set; }

        private static DateTime StablePointInTime =>
            new DateTime(2020, 4, 20, 12, 0, 0);

        private static Task<int> ClearDueToNonExistantWorkouts(DB db, string tableName)
        {
            return db.Database.ExecuteAsync("DELETE FROM " + ScheduleDBModel_.TableName + " WHERE " +
                "NOT EXISTS (SELECT Id FROM " + tableName + "Workout WHERE Id = WorkoutId);");
        }

        private static ScheduleDBModel Create(ScheduleDBModel_ m)
        {
            ScheduleDBModel model = new ScheduleDBModel();
            model.Id = m.Id;
            model.WorkoutId = m.WorkoutId;
            model.WorkoutType = (WorkoutType)Enum.Parse(typeof(WorkoutType), m.WorkoutType);
            model.Interval = TimeSpan.FromMinutes(m.Interval);
            model.When = StablePointInTime + TimeSpan.FromMinutes(m.When);
            model.IsActive = m.IsActive;
            return model;
        }

        public static async Task<ScheduleDBModel> FromDB(int id, DB db)
        {
            return Create(await db.Database.Table<ScheduleDBModel_>().FirstAsync(s => s.Id == id));
        }

        public static async Task<List<ScheduleDBModel>> FromDB(DB db)
        {
            var items = await db.Database.Table<ScheduleDBModel_>().ToListAsync();
            List<ScheduleDBModel> toRet = new List<ScheduleDBModel>();
            foreach (var item in items)
            {
                if (await ClearDueToNonExistantWorkouts(db, item.WorkoutType) == 0)
                {
                    toRet.Add(Create(item));
                }
            }
            return toRet;
        }

        public Task AddOrUpdate(DB db)
        {
            return db.Database.InsertOrReplaceAsync(new ScheduleDBModel_
            {
                Id = Id,
                Interval = (int)Interval.TotalMinutes,
                IsActive = IsActive,
                When = (int)(When - StablePointInTime).TotalMinutes,
                WorkoutId = WorkoutId,
                WorkoutType = WorkoutType.ToString()
            }, typeof(ScheduleDBModel_));
        }

        public Task Delete(DB db)
        {
            return db.Database.DeleteAsync<ScheduleDBModel_>(Id);
        }
    }

    [Table(TableName)]
    public class ScheduleDBModel_
    {
        public const string TableName = "Schedules";
        [PrimaryKey]
        public int Id { get; set; }
        public int WorkoutId { get; set; }
        public string WorkoutType { get; set; }
        public int Interval { get; set; }
        [Indexed]
        public int When { get; set; }
        public bool IsActive { get; set; }
    }
}
