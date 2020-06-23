using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using App1.Model;
using App1.Model.DBModels;
using App1.Sound;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1.Workouter
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoundView : ContentView
    {
        private SoundHandler handler_;
        private readonly DB db_ = new DB();
        private List<SongDBModel> songs_;
        private int currentIndex_ = -1;
        private List<PlaylistDBModel> playlists_;

        public void SeedPlaylists(List<PlaylistDBModel> playlists)
        {
            playlists_ = playlists;
            picker.ItemsSource = playlists_;
            if (playlists.Count == 1)
            {
                picker.SelectedItem = picker.ItemsSource[0];
            }
        }

        public SoundView()
        {
            InitializeComponent();
        }

        private void LoadNext()
        {
            if (++currentIndex_ >= songs_.Count)
            {
                currentIndex_ = 0;
            }
            else if (currentIndex_ < 0)
            {
                currentIndex_ = songs_.Count - 1;
            }
            var song = songs_[currentIndex_];
            songLabel.Text = song.FileName;
            handler_.Play(Path.Combine(song.FilePath, song.FileName));
        }

        public void Start()
        {
            if (handler_ == null)
            {
                handler_ = new SoundHandler();
                handler_.PlaybackFinished += LoadNext;
                LoadNext();
            }
            else
            {
                handler_.Resume();
            }
        }

        public void Stop()
        {
            handler_.Stop();
        }

        private void NextButton_Clicked(object sender, EventArgs e)
        {
            handler_.Stop();
            LoadNext();
        }

        private void Previousbutton_Clicked(object sender, EventArgs e)
        {
            handler_.Stop();
            currentIndex_ -= 2;
            LoadNext();
        }

        private async void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var list = (PlaylistDBModel)picker.SelectedItem;
            var sls = await db_.Database.Table<SongListDBModel>().Where(sl => sl.PlaylistId == list.Id).ToListAsync();
            var allsongs = await db_.Database.Table<SongDBModel>().ToListAsync();
            songs_ = allsongs.Where(s => sls.Any(sl => sl.SongId == s.Id)).ToList();
            picker.IsVisible = false;
            songLabel.IsVisible = nextButton.IsVisible = previousbutton.IsVisible = true;
            songLabel.Text = songs_.First().FileName;
        }
    }
}