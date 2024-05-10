using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using BlogTool.Common;
using BlogTool.Control;
using BlogTool.Core.Helper;
using BlogTool.Core;
using BlogTool.Model;
using BlogTool.View;
using BlogTool.Helper;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using BlogTool.Core.Markdown;

namespace BlogTool.ViewModel
{
    public class CategoryPageViewModel : ObservableObject
    {
        private ObservableCollection<object> _categoryTypeInfos;
        
        public CategoryPageViewModel()
        {
            this.SubmitCommand = new RelayCommand(() => { }, () => HasValue);
            this.ClearCommand = new RelayCommand(ClearAction);
            this.RemoveCommand = new RelayCommand<IMarkdown>(RemoveAction);
            this.PropertyChanged += CategoryPageViewModel_PropertyChanged;
            InitData();
        }


        private void ClearAction()
        {
            this.Entities.Clear();
            OnPropertyChanged(nameof(HasValue));
            MessageBox.Show("清空成功");
        }

        private void InitData()
        {
            IList<IMarkdown> data = null;

            var task = InvokeHelper.InvokeOnUi<IList<IMarkdown>>(null, () =>
        {
            var result = new List<IMarkdown>();


            return result;


        }, (t) =>
             {
                 data = t;
                 try
                 {
                     this.Entities = new ObservableCollection<object>(data);
                     this.Entities.CollectionChanged += CategoryInfos_CollectionChanged;


                 }
                 catch (Exception e)
                 {
                     Console.WriteLine(e);
                     throw;
                 }
             });


        }

        private void CategoryPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Entities))
            {
                SubmitCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(HasValue));
            }

        }

        private void CategoryInfos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasValue));

            SubmitCommand.NotifyCanExecuteChanged();
        }

        private void RemoveAction(IMarkdown obj)
        {
            if (obj == null)
            {
                return;

            }
            RemoveCategory(obj);
        }



        internal void RemoveCategory(IMarkdown CategoryInfo)
        {
            if (Entities.Any(c => (c as IMarkdown).Title == CategoryInfo.Title))
            {
                var current = Entities.FirstOrDefault(c => (c as IMarkdown).Title == CategoryInfo.Title);
                Entities.RemoveAt(Entities.IndexOf(current));

            }
            else
            {
                MessageBox.Show("条目不存在");
            }
        }


        private void ExportToExcelAction()
        {

            var odInfos = Entities.ToList();
            if (odInfos.Count > 0)
            {
                var task = InvokeHelper.InvokeOnUi<IEnumerable<object>>(null, () =>
                {
                    
                    return this.Entities;
                }, async (t) =>
                {
                    MessageBox.Show("已完成导出");

                });
            }
        }

      
       




        public ObservableCollection<object> Entities
        {
            get
            {
                if (_categoryTypeInfos == null)
                {
                    _categoryTypeInfos = new ObservableCollection<object>();
                }
                return _categoryTypeInfos;
            }
            set
            {
                _categoryTypeInfos = value;
                OnPropertyChanged(nameof(Entities));
            }
        }


        public List<MenuCommand> ExportOptions => new List<MenuCommand>() {
            new MenuCommand("导出到Excel", ExportToExcelAction, () => true),
        };



        public bool HasValue => this.Entities.Count>0;



        public RelayCommand GetDataCommand { get; set; }

        public RelayCommand SubmitCommand { get; set; }
        public RelayCommand ClearCommand { get; set; }
        public RelayCommand<IMarkdown> RemoveCommand { get; set; }

    }

}
