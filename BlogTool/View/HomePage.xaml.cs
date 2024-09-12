using System;
using System.ComponentModel;
using System.IO;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        public HomePageViewModel vm => this.DataContext as HomePageViewModel;

        public HomePage()
        {
            InitializeComponent();
            config = LocalDataHelper.ReadObjectLocal<SettingInfo>();

            string path = config.OutputPath;
            this.FileUrlTextBlock.Text = path;
            Loaded+=HomePage_Loaded;
            Unloaded+=HomePage_Unloaded;
        }

        private void HomePage_Unloaded(object sender, RoutedEventArgs e)
        {
            //Web1.Dispose();
        }

        private async void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (vm.PreviewInnerHtml!=null)
            {
                RenderWebBrowser();

            }
            vm.PropertyChanged+=Vm_PropertyChanged;

        }

        private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName==nameof(vm.PreviewInnerHtml))
            {
                RenderWebBrowser();
            }
        }

        private void RenderWebBrowser()
        {
            var content = $@"<html lang=""en""><head>
  <meta charset=""utf-8"">
  <title>marked.js preview</title>
</head>
<body>{vm.PreviewInnerHtml}
</body></html>";
            Web1.NavigateToString(content);
        }

        private void ButtonRemove_OnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as FrameworkElement;
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


    }
}
