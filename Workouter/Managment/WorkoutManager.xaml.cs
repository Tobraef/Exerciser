using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1.Workouter.Managment
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WorkoutManager : ContentPage
    {
        public WorkoutManagerViewModel ViewModel { get; private set; }

        public WorkoutManager()
        {
            ViewModel = new WorkoutManagerViewModel();
            InitializeComponent();
            BindingContext = ViewModel;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await ViewModel.RefreshList();
        }
    }
}