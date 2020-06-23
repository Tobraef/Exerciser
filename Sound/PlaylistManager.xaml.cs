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
    public partial class PlaylistManager : ContentPage
    {
        public PlaylistManagerViewModel ViewModel { get; private set; }

        public PlaylistManager()
        {
            ViewModel = new PlaylistManagerViewModel(new DB(), Navigation);
            BindingContext = ViewModel;
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            if (!ViewModel.IsInitialized)
            {
                var db = new DB();
                var dblists = await db.Database.Table<PlaylistDBModel>().ToListAsync();
                var lists = dblists.Select(l => new PlaylistModel(l, db)).ToList();
                await Task.WhenAll(lists.Select(l => l.InitializeAsync()));
                var workouts = await db.GetDescriptions();
                ViewModel.Initialize(lists.ToList(), workouts);
                OnBindingContextChanged();
            }
        }
    }
}