﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BlogTool.Control
{
    /// <summary>
    /// MetaWeblogInputDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ClipboardInputDialog : UserControl
    {
        public ClipboardInputDialog()
        {
            InitializeComponent();
            Loaded += ClipboardInputDialog_Loaded;
        }

        private void ClipboardInputDialog_Loaded(object sender, RoutedEventArgs e)
        {
            var result = this.TextBoxContent.Focus();

        }
    }
}
