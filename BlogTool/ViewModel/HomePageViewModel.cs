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
using BlogTool.Core.Helper;
using BlogTool.Model.Dto;
using BlogTool.Core.AssetsStores;
using System.Net.Http;
using System.Web;
using BlogTool.Core.AssetsStores.Implements;
using CommunityToolkit.Mvvm.Messaging;
using BlogTool.Control;
using BlogTool.Core.Markdown.Implements;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Reflection.Metadata;

namespace BlogTool.ViewModel
{
    public class HomePageViewModel : ObservableObject
    {
        private static string _fileName = null;
        private static string _excelFilesXlsxXls = "Markdown文件|*.md|所有文件|*.*";
        private static readonly string basePath = CommonHelper.AppBasePath;
        private ObservableCollection<object> _categoryTypeInfos;
        AssetsStoreOption assetsStoreOption;
        IAssetsStoreProvider assetsStoreProvider;

        public HomePageViewModel()
        {

            this.RefreshCommand = new RelayCommand(() =>
            {
                InitData();

            });
            this.ImportFromClipboardCommand= new RelayCommand(ImportFromClipboardAction, () => true);
            this.ImportFromLocalCommand= new RelayCommand(ImportFromLocalAction, () => true);

            this.RemoveCommand = new RelayCommand<IMarkdown>(RemoveAction);
            this.PropertyChanged += HomePageViewModel_PropertyChanged;
            InitData();
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

        private void HomePageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
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



        private void ImportFromLocalAction()
        {

            var task = InvokeHelper.InvokeOnUi<IMarkdown>(null, () =>
            {
                var getMarkdownOption = new GetMarkdownOption()
                {
                    ReadMorePosition = -1,
                };
                string path = Path.Combine(basePath, "Data");
                var openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = path;
                openFileDialog.Filter = _excelFilesXlsxXls;
                openFileDialog.FileName = _fileName;
                openFileDialog.AddExtension = true;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Multiselect = true;
                var result = openFileDialog.ShowDialog();
                string[] filePaths;
                if (result == true)
                {
                    filePaths = openFileDialog.FileNames;

                }
                else
                {
                    filePaths = new string[0];
                }
                return ProcessMarkdowns(getMarkdownOption, new LocalFilesMarkdownProvider(), filePaths);

            }, (t) =>
            {
                //todo

            });

        }


        private async void ImportFromClipboardAction()
        {
            var dialog = new CustomDialog()
            {
                Content = new UserControl()
                {
                    Content = new ClipboardInputDialog() { Name = "MainDialog" }

                },

                Title = "从剪贴板导入"
            };

            await DialogManager.ShowMetroDialogAsync((MetroWindow)App.Current.MainWindow, dialog);

            dialog.FindChild<ClipboardInputDialog>("MainDialog").CancelButton.Click += async (o, e) =>
            {
                await DialogManager.HideMetroDialogAsync((MetroWindow)App.Current.MainWindow, dialog);

            };
            dialog.FindChild<ClipboardInputDialog>("MainDialog").TextBoxContent.TextChanged += (o, e) =>
            {
                var title = dialog.FindChild<ClipboardInputDialog>("MainDialog").TextBoxTitle.Text;
                var content = dialog.FindChild<ClipboardInputDialog>("MainDialog").TextBoxContent.Text;
                if (string.IsNullOrEmpty(title))
                {
                    title = content.Trim().Substring(0, Math.Min(content.Trim().Length, 12));

                }
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
                {
                    return;
                }
                var task = InvokeHelper.InvokeOnUi<IMarkdown>(null, () =>
                {

                    var getMarkdownOption = new GetMarkdownOption()
                    {
                        ReadMorePosition = -1,
                    };
                    return ProcessMarkdowns(getMarkdownOption, new TextMarkdownProvider(), new { Title = title, Content = content });

                }, async (t) =>
                {
                    if (t!=null)
                    {
                        this.MarkdownContent=t;
                    }
                    await DialogManager.HideMetroDialogAsync((MetroWindow)App.Current.MainWindow, dialog);

                });
            };



        }


        internal void RemoveCategory(IMarkdown CategoryInfo)
        {
            string markdownFilePath = (CategoryInfo as HexoMarkdownFileInfo).FilePath;
            string directoryName = Path.GetDirectoryName(markdownFilePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(markdownFilePath);
            string directoryToDelete = Path.Combine(directoryName, fileNameWithoutExtension);
            try
            {
                if (Directory.Exists(directoryToDelete))
                {
                    Directory.Delete(directoryToDelete, true);
                    Console.WriteLine("目录已成功删除: " + directoryToDelete);
                }
                if (File.Exists(markdownFilePath))
                {
                    File.Delete(markdownFilePath);
                    Console.WriteLine("文件已成功删除: " + markdownFilePath);
                }

                var current = Entities.FirstOrDefault(c => (c as IMarkdown).Title == CategoryInfo.Title);
                if (Entities.Any(c => (c as IMarkdown).Title == CategoryInfo.Title))
                {

                    Entities.RemoveAt(Entities.IndexOf(current));

                }
                else
                {
                    MessageBox.Show("条目不存在");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("删除文件时发生错误: " + ex.Message);
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

        private IMarkdown _markdownContent;

        public IMarkdown MarkdownContent
        {
            get { return _markdownContent; }
            set
            {
                _markdownContent = value;
                OnPropertyChanged();
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
        public RelayCommand ImportFromClipboardCommand { get; set; }
        public RelayCommand ImportFromLocalCommand { get; set; }
        public RelayCommand<IMarkdown> RemoveCommand { get; set; }


        private string InsertLine(string inputString, int lineNumber, string contentToInsert)
        {
            int indexToInsert = 0;
            for (int i = 0; i < lineNumber - 1; i++)
            {
                indexToInsert = inputString.IndexOf('\n', indexToInsert) + 1;
                if (indexToInsert == 0)
                {
                    return inputString;
                }
            }

            return inputString.Insert(indexToInsert, contentToInsert + "\n");
        }


        private string MarkdownHandler(SettingInfo config, HttpClient client, AssetsStoreHandler handler, GetMarkdownOption getMarkdownOption, AssetsStoreOption assetsStoreOption, string templatePath, string fileDirectory, IMarkdown md)
        {
            string fileFullPath;
            var fileName = md.Title + ".md";

            fileFullPath = Path.Combine(fileDirectory, fileName);

            string templateMd = File.ReadAllText(templatePath);
            templateMd = templateMd.Replace("{{ title }}", md.Title);
            templateMd = templateMd.Replace("{{ date }}", md.DateCreated.HasValue ? md.DateCreated.Value.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            string categoriesNode = "categories:\n";
            if (md.Keywords != null)
            {
                foreach (var category in md.Categories)
                {
                    categoriesNode += $"  - {category}\n";
                }

                templateMd = templateMd.Replace("categories:", categoriesNode);
            }
            string keywordsNode = "tags:\n";
            if (md.Keywords != null)
            {
                foreach (var keyword in md.Keywords.Split(","))
                {
                    keywordsNode += $"  - {keyword}\n";
                }

                templateMd = templateMd.Replace("tags:", keywordsNode);
            }



            var fileContent = md.Content;

            int lineNumberToInsert = getMarkdownOption.ReadMorePosition;

            if (lineNumberToInsert > 0)
            {
                fileContent = InsertLine(fileContent, lineNumberToInsert, "<!-- more -->");

            }
            fileContent = fileContent.Replace("@[toc]", "<!-- toc -->");

            var imgPathDic = new Dictionary<string, string>();
            foreach (var imgContent in RegexUtil.ExtractorImgFromMarkdown(fileContent))
            {
                var img = imgContent.Item2;
                var imgElement = imgContent.Item1;
                if (imgPathDic.ContainsKey(handler.IsReplaceAllElement ? imgElement : img))
                {
                    continue;
                }

                try
                {
                    string imgFileName;
                    Stream imgStream;
                    if (img.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        var sourceStream = client.GetStreamAsync(img).Result;

                        imgStream = new MemoryStream();
                        sourceStream.CopyTo(imgStream);

                        int lastIndex = img.LastIndexOf('/');
                        if (lastIndex != -1 && lastIndex < img.Length - 1)
                        {
                            imgFileName = img.Substring(lastIndex + 1);
                        }
                        else
                        {
                            throw new Exception($"无法解析图片名称：{img} ");
                            continue;
                        }
                    }
                    else
                    {

                        var imgPhyPath = HttpUtility.UrlDecode(Path.Combine(fileDirectory, img));
                        if (File.Exists(imgPhyPath) == false)
                        {
                            throw new Exception($"请检查Markdown图片路径是否正确，文件不存在：{imgPhyPath} ");
                            continue;

                        }

                        var imgFile = new FileInfo(imgPhyPath);
                        imgFileName = Path.GetFileName(imgFile.FullName);
                        imgStream = imgFile.OpenRead();
                    }


                    imgPathDic[handler.IsReplaceAllElement ? imgElement : img] = handler.HandleAsync(imgStream, imgFileName, md.Title, assetsStoreOption);
                    imgStream.Close();

                }
                catch (Exception ex) when (config.SkipFileWhenException)
                {
                }

            }

            //替换
            fileContent = imgPathDic.Keys.Aggregate(
                fileContent, (current, key) => current.Replace(key, imgPathDic[key]));


            var content = string.Concat(templateMd, fileContent);
            DirFileHelper.WriteText(fileFullPath, content);
            return fileFullPath;
        }


        private IMarkdown ProcessMarkdowns(GetMarkdownOption getMarkdownOption, IMarkdownProvider markdownCreatorProvider, params object[] objects)
        {
            try
            {
                var config = LocalDataHelper.ReadObjectLocal<SettingInfo>();
                getMarkdownOption.RecentTakeCount = config.RecentTakeCount;

                var creator = new MarkdownCreator();

                AssetsStoreOption assetsStoreOption = new AssetsStoreOption()
                {
                    OutputPath = config.OutputPath,
                    CompressionImage = false,
                    AddWatermark = false,
                };
                IAssetsStoreProvider assetsStoreProvider = null;

                if (config.AssetsStoreProvider == SettingInfo.EMBED)
                {
                    assetsStoreProvider=new EmbedAssetsStoreProvider();
                }
                else if (config.AssetsStoreProvider == SettingInfo.LOCAL)
                {
                    assetsStoreProvider=new LocalAssetsStoreProvider();
                }

                else if (config.AssetsStoreProvider == SettingInfo.HEXO_ASSET_FOLDER)
                {
                    assetsStoreProvider=new HexoAssetFolderAssetsStoreProvider();
                }

                else if (config.AssetsStoreProvider == SettingInfo.HEXO_TAG_PLUGIN)
                {
                    assetsStoreProvider=new HexoTagPluginAssetsStoreProvider();
                }


                creator.SetMarkdownProvider(getMarkdownOption, markdownCreatorProvider);
                var mds = creator.Create(objects);
                return mds.FirstOrDefault();
            }
            catch (ArgumentException ex)
            {

                MessageBox.Show(string.Format("{0}参数错误:{0}{1}", Environment.NewLine, ex));
            }
            return null;
        }
        private void Continue(GetMarkdownOption getMarkdownOption, IMarkdown md)
        {
            var handler = new AssetsStoreHandler();
            var client = new HttpClient();

            var config = LocalDataHelper.ReadObjectLocal<SettingInfo>();

            string templatePath = Path.Combine(config.HexoPath, "scaffolds", "post.md");

            string fileFullPath;


            var fileDirectory = Directory.Exists(config.OutputPath) == false
                ? Directory.CreateDirectory(config.OutputPath).FullName
                : new DirectoryInfo(config.OutputPath).FullName;



            if (File.Exists(templatePath))
            {

                assetsStoreOption.SubPath=md.Title;
                handler.SetAssetsStoreProvider(assetsStoreOption, assetsStoreProvider);

                fileFullPath = MarkdownHandler(config, client, handler, getMarkdownOption, assetsStoreOption, templatePath, fileDirectory, md);

            }

        }
    }
}
