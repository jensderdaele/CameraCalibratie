using System.ComponentModel.Composition;
using Caliburn.Micro;
using BundleAdjuster.Properties;
using Gemini.Modules.Settings;

namespace BundleAdjuster.Shell.ViewModel
{
    
    [Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BundleAdjusterSettingsViewModel : PropertyChangedBase, ISettingsEditor
    {
        private bool _confirmExit;

        public BundleAdjusterSettingsViewModel()
        {
            ConfirmExit = Settings.Default.ConfirmExit;
        }

        public bool ConfirmExit {
            get => _confirmExit;
            set {
                if (value.Equals(_confirmExit)) return;
                _confirmExit = value;
                NotifyOfPropertyChange(() => ConfirmExit);
            }
        }

        public string SettingsPageName => Resources.SettingsPageGeneral;

        public string SettingsPagePath => Resources.SettingsPathEnvironment;

        public void ApplyChanges()
        {
            if (ConfirmExit == Settings.Default.ConfirmExit)
            {
                return;
            }

            Settings.Default.ConfirmExit = ConfirmExit;
            Settings.Default.Save();
        }
    }
    
}
