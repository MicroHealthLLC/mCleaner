using System.Windows;
using System.Windows.Controls;
using mCleaner.ViewModel;
using Microsoft.Practices.ServiceLocation;

namespace mCleaner.UserControls.Window
{
    /// <summary>
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : UserControl
    {
        public PreferencesWindow()
        {
            InitializeComponent();
        }

        public ViewModel_Preferences VMPreferences
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_Preferences>();
            }
        }


        private void TxtPasswordbox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {

            VMPreferences.ProxyPassword = TxtPasswordbox.Password;
        }
    }
}
