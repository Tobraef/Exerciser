using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using App1.Model;
using App1.Model.DBModels;

namespace App1.Creator.Regular
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Creator : ContentPage
    {
        public RegularCreatorViewModel ViewModel { get; private set; }

        public Creator()
        {
            ViewModel = new RegularCreatorViewModel(new DB());
            BindingContext = ViewModel;
            InitializeComponent();
            ViewModel.RowsLayout = stackList;
        }

        public Creator(RegularWorkoutDBModel workout, List<ExerciseSetDBModel> exers)
        {
            ViewModel = new RegularCreatorViewModel(new DB(), workout, exers);
            BindingContext = ViewModel;
            InitializeComponent();
            ViewModel.RowsLayout = stackList;
        }
    }
}