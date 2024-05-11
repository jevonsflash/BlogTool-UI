using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using BlogTool.Model;
using BlogTool.Helper;

namespace BlogTool.ViewModel
{
    public class SettingPageViewModel : ObservableObject
    {

        public SettingPageViewModel()
        {
            this.SubmitCommand = new RelayCommand(SubmitAction, CanSubmit);
            this.PropertyChanged += SettingPageViewModel_PropertyChanged;
            SettingInfo = LocalDataHelper.ReadObjectLocal<SettingInfo>();
            if (SettingInfo == null)
            {
                SettingInfo = new SettingInfo()
                {
                    HexoPath = string.Empty,
                    OutputPath = string.Empty,
                };
            }
            SettingInfo.PropertyChanged += SettingPageViewModel_PropertyChanged;
        }

        private bool _hasChanged;

        public bool HasChanged
        {
            get { return _hasChanged; }
            set
            {
                _hasChanged = value;
                OnPropertyChanged(nameof(HasChanged));
            }
        }


        private void SettingInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaiseSettingChanged();
        }

        public void RaiseSettingChanged()
        {
            HasChanged = true;
            SubmitCommand.NotifyCanExecuteChanged();
        }

        private void SettingPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingInfo) && SettingInfo != null)
            {
                SettingInfo.PropertyChanged += SettingInfo_PropertyChanged;

            }
            else if (e.PropertyName == nameof(SettingInfo.HexoPath) && !string.IsNullOrEmpty(SettingInfo.HexoPath))
            {
                if (string.IsNullOrWhiteSpace(SettingInfo.OutputPath))
                {
                    SettingInfo.OutputPath = SettingInfo.HexoPath.Trim() + "\\source\\_posts";
                }
            }
        }



        private void SubmitAction()
        {
            LocalDataHelper.SaveObjectLocal(SettingInfo);
            HasChanged = false;


        }

        private bool CanSubmit()
        {
            return this.SettingInfo != null && HasChanged;
        }

        private SettingInfo _settingInfo;

        public SettingInfo SettingInfo
        {
            get { return _settingInfo; }
            set
            {
                _settingInfo = value;
                OnPropertyChanged(nameof(SettingInfo));
            }
        }

        public RelayCommand SubmitCommand { get; set; }
    }
}
