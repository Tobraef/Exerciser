using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1
{
    public partial class App : Application
    {
        public static Model.IWorkout Request { get; set; }
        public static bool IsAppFocused { get; private set; }

        public static bool IsUserWorking { get; set; }

        public static void ClearData()
        {
            Request = null;
        }

        public App()
        {
            InitializeComponent();
            DependencyService.Register<Schedule.NotificationManager>();
            MainPage = new NavigationPage(new MainPage());
        }

        private void SaveData()
        {
            Current.Properties[nameof(Request)] = Request;
        }

        private T LoadMember<T>(string name)
        {
            return Current.Properties.ContainsKey(name) ? (T)Current.Properties[name] : default;
        }

        private void LoadData()
        {
            Request = LoadMember<Model.IWorkout>(nameof(Request));
        }

        public static void Debug(string text)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Current.MainPage.DisplayAlert("Warning", text, "OK");
            });
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            IsAppFocused = true;
        }

        protected override void OnSleep()
        {
            IsAppFocused = false;
            SaveData();
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            IsAppFocused = true;
            LoadData();
            // Handle when your app resumes
        }
    }
}
