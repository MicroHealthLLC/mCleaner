/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:mCleaner"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace mCleaner.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ViewModel_CleanerML>();
            SimpleIoc.Default.Register<ViewModel_Clam>();
            SimpleIoc.Default.Register<ViewModel_Preferences>();
            SimpleIoc.Default.Register<ViewModel_Shred>();
            SimpleIoc.Default.Register<ViewModel_DuplicateChecker>();
            SimpleIoc.Default.Register<ViewModel_ReleaseNotes>();
            SimpleIoc.Default.Register<ViewModel_PrivacyPolicy>();
            SimpleIoc.Default.Register<ViewModel_Uninstaller>();
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public ViewModel_CleanerML CleanerML
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_CleanerML>();
            }
        }

        public ViewModel_Clam Clam
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_Clam>();
            }
        }

        public ViewModel_Preferences Prefs
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_Preferences>();
            }
        }

        public ViewModel_Shred ShredWindow
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_Shred>();
            }
        }

        public ViewModel_DuplicateChecker DupChecker
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_DuplicateChecker>();
            }
        }

        public ViewModel_ReleaseNotes ReleaseNote
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_ReleaseNotes>();
            }
        }

        public ViewModel_PrivacyPolicy PrivacyPolicy
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_PrivacyPolicy>();
            }
        }

        public ViewModel_Uninstaller Uninstaller
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_Uninstaller>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}