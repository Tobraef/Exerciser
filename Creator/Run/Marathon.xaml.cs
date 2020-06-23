using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using App1.Model;
using App1.Model.DBModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1.Creator.Run
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Marathon : ContentPage
    {
        public MarathonViewModel ViewModel { get; }

        public Marathon()
        {
            ViewModel = new MarathonViewModel(new DB());
            BindingContext = ViewModel;
            InitializeComponent();
        }

        public Marathon(MarathonDBModel model)
        {
            ViewModel = new MarathonViewModel(new DB(), model);
            BindingContext = ViewModel;
            InitializeComponent();
        }
    }
}