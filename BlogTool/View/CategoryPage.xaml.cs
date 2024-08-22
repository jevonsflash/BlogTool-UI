using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BlogTool.Core.Markdown;
using BlogTool.Helper;
using BlogTool.Model;
using BlogTool.ViewModel;

namespace BlogTool.View
{
    /// <summary>
    /// ProcedurePage.xaml 的交互逻辑
    /// </summary>
    public partial class CategoryPage : Page
    {
        public SettingInfo config { get; }

        public CategoryPage()
        {
            InitializeComponent();
            config = LocalDataHelper.ReadObjectLocal<SettingInfo>();

            string path = config.OutputPath;
            this.FileUrlTextBlock.Text = path;
        }


        private void ButtonRemove_OnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as FrameworkElement;
            var vm = this.DataContext as CategoryPageViewModel;
            vm.RemoveCommand.Execute(item.DataContext);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.Cleanup<CategoryPageViewModel>();

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

        private void ButtonOpen_OnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as FrameworkContentElement;
            var markdown = item.DataContext as HexoMarkdownFileInfo;

            string path = markdown.FilePath;
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select," + path);

            }
            catch (Exception ex)
            {
                MessageBox.Show("无法打开:" + ex);


            }
        }
    }
}
