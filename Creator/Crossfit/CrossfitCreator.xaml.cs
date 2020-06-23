using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1.Creator.Crossfit
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CrossfitCreator : ContentPage
    {
        public CrossfitCreatorViewModel ViewModel { get; private set; }

        public CrossfitCreator()
        {
            ViewModel = new CrossfitCreatorViewModel(new Model.DB());
            BindingContext = ViewModel;
            InitializeComponent();
        }

        public CrossfitCreator(Model.DBModels.CrossfitWorkoutDBModel model)
        {
            ViewModel = new CrossfitCreatorViewModel(new Model.DB(), model);
            BindingContext = ViewModel;
            InitializeComponent();
        }
    }
}