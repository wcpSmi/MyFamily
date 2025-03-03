
namespace MyFamily
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            // Elérhetjük a Map példányát, ha megvan a referencia
            if (Current.MainPage is NavigationPage navigationPage &&
                navigationPage.CurrentPage is MyFamily.Map mapPage)
            {
                // Kikapcsoljuk az aktuális helyzet mutatását
                mapPage.StopMap();
            }
#if ANDROID
            LocationService.StopService(Android.App.Application.Context);
#endif
        }

    }
}
