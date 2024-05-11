using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlogTool.Common;
using BlogTool.Model;
using BlogTool.Helper;
using BlogTool.Core.Markdown;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace BlogTool.ViewModel
{
    public class CategoryPageViewModel : ObservableObject
    {
        private ObservableCollection<object> _categoryTypeInfos;

        public CategoryPageViewModel()
        {
            this.RefreshCommand = new RelayCommand(() =>
            {
                InitData();

            });
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

        public void InitData()
        {
            IList<IMarkdown> data = null;

            var task = InvokeHelper.InvokeOnUi<IList<IMarkdown>>(null, () =>
        {
            var config = LocalDataHelper.ReadObjectLocal<SettingInfo>();

            var markdownFiles = Directory.GetFiles(config.OutputPath, "*.md", SearchOption.TopDirectoryOnly);

            var result = new List<IMarkdown>();

            foreach (var markdownFile in markdownFiles)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(markdownFile);

                result.Add(new HexoMarkdownFileInfo()
                {
                    FilePath = markdownFile,
                    Title = fileNameWithoutExtension,
                });
            }




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


        private HexoPostMetadata ReadHexoPostMetadata(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found: " + filePath);
                return null;
            }

            string content = File.ReadAllText(filePath);

            // 查找 YAML 头部信息的开始和结束位置  
            int start = content.IndexOf("---");
            int end = content.LastIndexOf("---") + 3; // 加上 "---" 的长度  

            if (start == -1 || end == -1 || start >= end)
            {
                Console.WriteLine("No YAML front matter found in the file.");
                return null;
            }

            // 提取 YAML 头部信息  
            string yamlContent = content.Substring(start, end - start);

            // 使用 YamlDotNet 解析 YAML 内容  
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention()) // 根据需要选择命名约定  
                .Build();

            try
            {
                return deserializer.Deserialize<HexoPostMetadata>(yamlContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deserializing YAML: " + ex.Message);
                return null;
            }
        }

        private void CategoryPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Entities))
            {
                OnPropertyChanged(nameof(HasValue));
            }

        }

        private void CategoryInfos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasValue));
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


    

        private object _entity;

        public object Entity
        {
            get { return _entity; }
            set
            {
                _entity = value;
                OnPropertyChanged(nameof(Entity));
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


   


        public bool HasValue => this.Entities.Count > 0;



        public RelayCommand GetDataCommand { get; set; }

        public RelayCommand RefreshCommand { get; set; }
        public RelayCommand ClearCommand { get; set; }
        public RelayCommand<IMarkdown> RemoveCommand { get; set; }

    }

}
