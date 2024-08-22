using System;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using BlogTool.Helper;
using BlogTool.Model;
using BlogTool.ViewModel;
using Microsoft.Win32;

namespace BlogTool.View
{
    /// <summary>
    /// ProcedurePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page
    {
        public SettingInfo config { get; }

        public HomePage()
        {
            InitializeComponent();
            config = LocalDataHelper.ReadObjectLocal<SettingInfo>();

            string path = config.OutputPath;
            this.FileUrlTextBlock.Text = path;
            Loaded+=HomePage_Loaded;
        }

        private async void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonRemove_OnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as FrameworkElement;
            var vm = this.DataContext as HomePageViewModel;
            vm.RemoveCommand.Execute(item.DataContext);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.Cleanup<HomePageViewModel>();

        }
        private void DataGrid_OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (string.IsNullOrEmpty(((PropertyDescriptor)e.PropertyDescriptor).DisplayName))
            {
                e.Cancel = true;
            }
            e.Column.Header = ((PropertyDescriptor)e.PropertyDescriptor).DisplayName;
        }


        private void Hyperlink_Click2(object sender, RoutedEventArgs e)
        {

            string path = config.OutputPath;
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", path);

            }
            catch (Exception ex)
            {
                MessageBox.Show("无法打开目录:" + ex);


            }

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await ThumbnailHelper.ExportAsync(new Common.ExportOption() { Width=100, Height=100 });

        }
    }
}
