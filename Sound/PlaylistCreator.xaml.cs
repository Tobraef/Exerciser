using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using App1.Model;
using App1.Model.DBModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1.Sound
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlaylistCreator : ContentPage
    {
        public PlaylistCreatorViewModel ViewModel { get; private set; }

        public PlaylistCreator()
        {
            ViewModel = new PlaylistCreatorViewModel(new DB(), Navigation);
            BindingContext = ViewModel;
            InitializeComponent();
        }

        public PlaylistCreator(PlaylistDBModel playlist, List<SongDBModel> songs)
        {
            ViewModel = new PlaylistCreatorViewModel(new DB(), Navigation, songs, playlist);
            BindingContext = ViewModel;
            InitializeComponent();
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ViewModel.MoveSong.Execute(e.Item);
        }
    }
}