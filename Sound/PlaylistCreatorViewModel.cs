using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using App1.Model;
using App1.Model.DBModels;

using Xamarin.Forms;

namespace App1.Sound
{
    public class PlaylistCreatorViewModel : BindableBase
    {
        private readonly DB db_;
        private readonly INavigation navigation_;
        private readonly int? dbInstance_;

        public ObservableCollection<SongDBModel> SongsIn { get; set; } = new ObservableCollection<SongDBModel>();

        public ObservableCollection<SongDBModel> AvailableSongs { get; set; } = new ObservableCollection<SongDBModel>();

        private string title_;
        public string Title { get => title_; set => SetProperty(ref title_, value, RefreshCommands); }

        public Command<object> MoveSong { get; private set; }

        public Command Add { get; private set; }

        public void RefreshCommands()
        {
            Add.ChangeCanExecute();
        }

        private async Task<PlaylistDBModel> InsertPlaylist()
        {
            await db_.Database.InsertAsync(new PlaylistDBModel { Title = Title }, typeof(PlaylistDBModel));
            return await db_.Database.Table<PlaylistDBModel>().FirstAsync(r => r.Title.Equals(Title));
        }

        private async Task<List<SongDBModel>> InsertSongs()
        {
            var inDb = await db_.Database.Table<SongDBModel>()
                .ToListAsync();
            var toInsert = SongsIn.Where(s => inDb.FirstOrDefault(i => s.FileName.Equals(i.FileName)) == null);
            await db_.Database.InsertAllAsync(toInsert, typeof(SongDBModel));
            inDb = await db_.Database.Table<SongDBModel>()
                .ToListAsync();
            return inDb.Where(i => SongsIn.FirstOrDefault(s => s.FileName.Equals(i.FileName)) != null).ToList();
        }

        private Task ClearUnusedSongs()
        {
            return db_.Database.ExecuteAsync("DELETE FROM " + SongDBModel.TableName + " WHERE NOT EXISTS " +
                "(SELECT * FROM " + SongListDBModel.TableName + " WHERE SongId = Id);");
        }

        private async Task InsertPlaySongs(List<SongDBModel> songs, PlaylistDBModel list)
        {
            List<SongListDBModel> models = new List<SongListDBModel>();
            for (int i = 0; i < SongsIn.Count; ++i)
            {
                models.Add(new SongListDBModel
                {
                    Order = i,
                    PlaylistId = list.Id,
                    SongId = songs.FirstOrDefault(x => x.Equals(SongsIn[i])).Id
                });
            }
            await db_.Database.InsertAllAsync(models, typeof(SongListDBModel));
        }

        private async Task EditInDb()
        {
            var model = new PlaylistDBModel { Id = dbInstance_.Value, Title = title_ };
            await db_.Database.UpdateAsync(model);
            await db_.Database.Table<SongListDBModel>().DeleteAsync(o => o.PlaylistId == dbInstance_.Value);
            var songs = await InsertSongs();
            await InsertPlaySongs(songs, model);
            await ClearUnusedSongs();
        }

        public void InitializeCommands()
        {
            MoveSong = new Command<object>(s =>
            {
                var song = (SongDBModel)s;
                if (SongsIn.Contains(song))
                {
                    SongsIn.Remove(song);
                    AvailableSongs.Add(song);
                }
                else
                {
                    AvailableSongs.Remove(song);
                    SongsIn.Add(song);
                }
            });

            Add = new Command(async() =>
            {
                if (dbInstance_.HasValue)
                {
                    await EditInDb();
                }
                else
                {
                    var list = await InsertPlaylist();
                    var songs = await InsertSongs();
                    await InsertPlaySongs(songs, list);
                }
                await navigation_.PopAsync();
            }, () => SongsIn.Count > 0 && !string.IsNullOrEmpty(Title));
        }

        public PlaylistCreatorViewModel(DB db, INavigation navigation)
        {
            db_ = db;
            navigation_ = navigation;
            InitializeCommands();
            var folders = DependencyService.Get<IFileSupplier>().MusicFolders;
            foreach (var folder in folders)
            {
                var files = Directory.EnumerateFiles(folder, "*.mp3");
                foreach (var file in files)
                {
                    AvailableSongs.Add(new SongDBModel { FileName = Path.GetFileName(file), FilePath = folder });
                }
            }
            SongsIn.CollectionChanged += (o, e) => RefreshCommands();
        }

        public PlaylistCreatorViewModel(DB db, INavigation nav, List<SongDBModel> songs, PlaylistDBModel model)
            :this(db, nav)
        {
            dbInstance_ = model.Id;
            Title = model.Title;
            foreach (var song in songs)
            {
                SongsIn.Add(song);
                AvailableSongs.Remove(song);
            }
        }
    }
}
