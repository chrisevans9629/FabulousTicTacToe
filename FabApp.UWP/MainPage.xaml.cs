using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace FabApp.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadApplication(new FabApp.App());
        }
    }
}
