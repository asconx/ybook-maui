namespace yBook
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App(AppShell shell)
        {
            InitializeComponent();
            MainPage = shell;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(MainPage!);
        }
    }
}
