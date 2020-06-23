using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using SQLite;

using App1.Model.DBModels;

namespace App1.Model
{
    static class CONST
    {
        public const string DatabaseFilename = "TodoSQLite.db3";

        public const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create;

        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }
    }

    public class DB
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(CONST.DatabasePath, CONST.Flags);
        });

        public virtual SQLiteAsyncConnection Database => lazyInitializer.Value;
        static bool initialized = false;


        public DB()
        {
            InitializeAsync().Wait();
        }

        private async Task Initialize(string tableName, Type type)
        {
            if (!Database.TableMappings.Any(m => m.MappedType.Name == tableName))
            {
                await Database.CreateTablesAsync(CreateFlags.None, type).ConfigureAwait(false);
            }
        }

        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == CrossfitWorkoutDBModel.TableName))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(CrossfitWorkoutDBModel)).ConfigureAwait(false);
                }

                if (!Database.TableMappings.Any(m => m.MappedType.Name == RegularWorkoutDBModel.TableName))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(RegularWorkoutDBModel)).ConfigureAwait(false);
                }

                if (!Database.TableMappings.Any(m => m.MappedType.Name == ExerciseSetDBModel.TableName))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(ExerciseSetDBModel)).ConfigureAwait(false);
                }

                if (!Database.TableMappings.Any(m => m.MappedType.Name == ScheduleDBModel_.TableName))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(ScheduleDBModel_)).ConfigureAwait(false);
                }
                if (!Database.TableMappings.Any(m => m.MappedType.Name == MarathonDBModel.TableName))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(MarathonDBModel)).ConfigureAwait(false);
                }
                await Initialize(SongDBModel.TableName, typeof(SongDBModel));
                await Initialize(PlaylistDBModel.TableName, typeof(PlaylistDBModel));
                await Initialize(SongListDBModel.TableName, typeof(SongListDBModel));
                await Initialize(ListWorkoutDBModel.TableName, typeof(ListWorkoutDBModel));
                initialized = true;
            }
        }

        private async Task<List<CrossfitWorkout>> GetCrossfitWorkouts()
        {
            var data = await Database.Table<CrossfitWorkoutDBModel>().ToListAsync().ConfigureAwait(false);
            return data.Select(w => new CrossfitWorkout(w)).ToList();
        }

        public async Task<CrossfitWorkout> GetCrossfitWorkout(int id)
        {
            var data = await Database.Table<CrossfitWorkoutDBModel>().FirstAsync(w => w.Id.Equals(id)).ConfigureAwait(false);
            return new CrossfitWorkout(data);
        }

        private async Task<List<RegularWorkout>> GetRegularWorkouts()
        {
            var data = await Database.Table<RegularWorkoutDBModel>().ToListAsync().ConfigureAwait(false);
            var exercises = await Database.Table<ExerciseSetDBModel>().ToListAsync().ConfigureAwait(false);
            return data.Select(w => new RegularWorkout(w, exercises.Where(e => e.WorkoutId == w.Id).ToList())).ToList();
        }

        public async Task<RegularWorkout> GetRegularWorkout(int id)
        {
            var data = await Database.Table<RegularWorkoutDBModel>().FirstAsync(w => w.Id.Equals(id)).ConfigureAwait(false);
            var exercises = await Database.Table<ExerciseSetDBModel>().Where(e => e.WorkoutId == data.Id).ToListAsync().ConfigureAwait(false);
            return new RegularWorkout(data, exercises);
        }

        private async Task<MarathonWorkout> GetMarathonWorkout(int id)
        {
            return new MarathonWorkout(await Database.FindAsync<MarathonDBModel>(id));
        }

        private async Task<IEnumerable<IWorkout>> GetMarathonWorkouts()
        {
            return (await Database.Table<MarathonDBModel>().ToListAsync().ConfigureAwait(false))
                .Select(e => new MarathonWorkout(e));
        }

        public Task<IWorkout> GetWorkout(int id, string type) =>
            GetWorkout(id, (WorkoutType)Enum.Parse(typeof(WorkoutType), type));

        public async Task<IWorkout> GetWorkout(int id, WorkoutType type)
        {
            switch (type)
            {
                case WorkoutType.Crossfit: return await GetCrossfitWorkout(id);
                case WorkoutType.Regular: return await GetRegularWorkout(id);
                case WorkoutType.Marathon: return await GetMarathonWorkout(id);
            }
            return null;
        }

        public async Task<List<IWorkout>> GetDescriptions()
        {
            IEnumerable<IWorkout> first = (await GetRegularWorkouts()).AsEnumerable();
            var second  = await GetCrossfitWorkouts();
            var third = await GetMarathonWorkouts();
            return first.Concat(second).Concat(third).ToList();
        }
    }
}
