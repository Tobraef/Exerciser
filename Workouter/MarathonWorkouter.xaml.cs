using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using App1.Model;
using App1.Model.DBModels;
using App1.Tools;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1.Workouter
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MarathonWorkouter : ContentPage
    {
        public MarathonWorkouterViewModel ViewModel { get; }

        public MarathonWorkouter(MarathonDBModel model, List<PlaylistDBModel> playlists)
        {
            ViewModel = new MarathonWorkouterViewModel(model, new DistanceProvider());
            BindingContext = ViewModel;
            InitializeComponent();
            soundView.SeedPlaylists(playlists);
            ViewModel.SoundHandler = soundView;
        }
    }
}