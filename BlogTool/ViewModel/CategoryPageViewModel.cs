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
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using BlogTool.Markdown.Implements;

namespace BlogTool.ViewModel
{
    public class CategoryPageViewModel : ObservableObject
    {
        private static string _fileName = null;
        private static string _markdownFilesMd = "Markdown文件|*.md|所有文件|*.*";
        private static readonly string basePath = CommonHelper.AppBasePath;
        private ObservableCollection<object> _categoryTypeInfos;

        public CategoryPageViewModel()
        {
            this.ProcessResultList = new ObservableCollection<ProcessResultDto>();
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
            var config = LocalDataHelper.ReadObjectLocal<SettingInfo>();

            try
            {
                foreach (var file in Directory.GetFiles(config.OutputPath))
                {
                    File.Delete(file);
                    Console.WriteLine("文件已删除: " + file);
                }

                foreach (var dir in Directory.GetDirectories(config.OutputPath))
                {
                    Directory.Delete(dir, true);
                    Console.WriteLine("子目录已删除: " + dir);
                }

                Console.WriteLine("目录中的所有内容已被删除。");
                this.Entities.Clear();
                OnPropertyChanged(nameof(HasValue));
                MessageBox.Show("清空成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine("删除目录内容时发生错误: " + ex.Message);
            }

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

        private async void ImportFromMetaWeblogAction()
        {
            var dialog = new CustomDialog()
            {
                Content = new UserControl()
                {
                    Content = new MetaWeblogInputDialog() { Name = "MainDialog" }

                },

                Title = "从MetaWeblog导入"
            };

            await DialogManager.ShowMetroDialogAsync((MetroWindow)App.Current.MainWindow, dialog);

            dialog.FindChild<MetaWeblogInputDialog>("MainDialog").CancelButton.Click += async (o, e) =>
            {
                await DialogManager.HideMetroDialogAsync((MetroWindow)App.Current.MainWindow, dialog);

            };
            dialog.FindChild<MetaWeblogInputDialog>("MainDialog").CommitButton.Click += (o, e) =>
            {

                var username = dialog.FindChild<MetaWeblogInputDialog>("MainDialog").TextBoxUsername.Text;
                var password = dialog.FindChild<MetaWeblogInputDialog>("MainDialog").TextBoxPassword.Password;
                var metaWeblogURL = dialog.FindChild<MetaWeblogInputDialog>("MainDialog").TextBoxMetaWeblogURL.Text;
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return;
                }
                var task = InvokeHelper.InvokeOnUi<ProcessResultDto[]>(null, () =>
                {

                    var getMarkdownOption = new GetMarkdownOption()
                    {
                        ReadMorePosition = -1,
                        MetaWeblogOption = new Core.Options.MetaWeblogOption
                        {
                            MetaWeblogURL = metaWeblogURL,
                            Password = password,
                            Username = username,
                        },
                        AigcOption=new Core.Options.AigcOption()
                        {
                            Provider="DashScope",
                            Target= "Description,Tag",
                            ApiKey= "sk-abfb5186d29e4a0cbd6c329517b61cce"
                        }
                    };
                    return ProcessMarkdowns(getMarkdownOption, new MetaWeblogMarkdownProvider());

                }, async (t) =>
                {
                    foreach (var item in t)
                    {
                        this.ProcessResultList.Add(item);
                    }
                    await DialogManager.HideMetroDialogAsync((MetroWindow)App.Current.MainWindow, dialog);

                });
            };



        }


        private void ImportFromLocalAction()
        {

            var task = InvokeHelper.InvokeOnUi<ProcessResultDto[]>(null, () =>
            {
                var getMarkdownOption = new GetMarkdownOption()
                {
                    ReadMorePosition = -1,
                    AigcOption=new Core.Options.AigcOption()
                    {
                        Provider="DashScope",
                        Target= "Description,Tag",
                        ApiKey= "sk-abfb5186d29e4a0cbd6c329517b61cce"
                    }
                };
                string path = Path.Combine(basePath, "Data");
                var openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = path;
                openFileDialog.Filter = _markdownFilesMd;
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
                foreach (var item in t)
                {
                    this.ProcessResultList.Add(item);
                }

            });

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

        private bool _isRepost;

        public bool IsRepost
        {
            get { return _isRepost; }
            set
            {
                _isRepost = value;
                OnPropertyChanged(nameof(IsRepost));
            }
        }

        private string _author;

        public string Author
        {
            get { return _author; }
            set
            {
                _author = value;
                OnPropertyChanged(nameof(Author));
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


        private ObservableCollection<ProcessResultDto> _processResultList;

        public ObservableCollection<ProcessResultDto> ProcessResultList
        {
            get { return _processResultList; }
            set
            {
                _processResultList = value;
                OnPropertyChanged(nameof(ProcessResultList));
            }
        }


        public bool HasValue => this.Entities.Count > 0;

        public List<MenuCommand> ImportOptions => new List<MenuCommand>() {
            new MenuCommand("从文件夹导入", ImportFromLocalAction, () => true),
            new MenuCommand("从MetaWeblog接口导入", ImportFromMetaWeblogAction, () => true),
        };

        public RelayCommand GetDataCommand { get; set; }

        public RelayCommand RefreshCommand { get; set; }
        public RelayCommand ClearCommand { get; set; }
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


        private string MarkdownHandler(List<ProcessResultDto> processResultList, SettingInfo config, HttpClient client, AssetsStoreHandler handler, GetMarkdownOption getMarkdownOption, AssetsStoreOption assetsStoreOption, string templatePath, string fileDirectory, IMarkdown md)
        {
            string fileFullPath;
            var fileName = md.Title + ".md";

            fileFullPath = Path.Combine(fileDirectory, fileName);

            string templateMd = File.ReadAllText(templatePath);
            templateMd = templateMd.Replace("{{ title }}", md.Title);
            templateMd = templateMd.Replace("{{ date }}", md.DateCreated.HasValue ? md.DateCreated.Value.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            string categoriesNode = "categories:\n";
            if (md.Categories != null)
            {
                if (IsRepost)
                {
                    md.Categories.Add("转载");
                    templateMd = templateMd.Replace("categories:", categoriesNode);
                }
                foreach (var category in md.Categories)
                {
                    categoriesNode += $"  - {category}\n";
                }

                templateMd = templateMd.Replace("categories:", categoriesNode);
            }
            string keywordsNode = "tags:\n";
            if (md.Keywords != null)
            {
                var keywords = md.Keywords.Split(",");

                foreach (var keyword in keywords)
                {
                    keywordsNode += $"  - {keyword}\n";
                }

                templateMd = templateMd.Replace("tags:", keywordsNode);
            }

            templateMd = templateMd.Replace("{{ description }}", md.Description);


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
                    processResultList.Add(new ProcessResultDto(DateTime.Now, $"已上传图片跳过：{img} "));
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
                            processResultList.Add(new ProcessResultDto(DateTime.Now, $"无法解析图片名称：{img} "));

                            continue;
                        }
                    }
                    else
                    {

                        var imgPhyPath = HttpUtility.UrlDecode(Path.Combine(fileDirectory, img));
                        if (File.Exists(imgPhyPath) == false)
                        {
                            processResultList.Add(new ProcessResultDto(DateTime.Now, $"请检查Markdown图片路径是否正确，文件不存在：{imgPhyPath} "));
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
                    processResultList.Add(new ProcessResultDto(DateTime.Now, $"跳过图片[{img}]，异常原因：处理失败-{ex.Message}"));
                }

            }

            //替换
            fileContent = imgPathDic.Keys.Aggregate(
                fileContent, (current, key) => current.Replace(key, imgPathDic[key]));


            var content = string.Concat(templateMd, fileContent);

            int start = content.IndexOf("---");
            int end = content.LastIndexOf("---") + 3;

            var hexoPostMetadata = YamlHelper.ReadHexoPostMetadata(start, end, content) as IDictionary<object, object>;
            if (IsRepost)
            {
                hexoPostMetadata["author"]=Author;
            }

            content = YamlHelper.WriteHexoPostMetadata(start, end, content, hexoPostMetadata);
            DirFileHelper.WriteText(fileFullPath, content);
            processResultList.Add(new ProcessResultDto(DateTime.Now, $"Markdown文件处理完成，文件保存在：{fileFullPath}"));
            return fileFullPath;
        }


        private ProcessResultDto[] ProcessMarkdowns(GetMarkdownOption getMarkdownOption, IMarkdownProvider markdownCreatorProvider, params object[] objects)
        {
            var processResultList = new List<ProcessResultDto>();
            try
            {
                var config = LocalDataHelper.ReadObjectLocal<SettingInfo>();
                getMarkdownOption.RecentTakeCount = config.RecentTakeCount;
                var client = new HttpClient();

                var creator = new MarkdownCreator();
                var handler = new AssetsStoreHandler();

                var assetsStoreOption = new AssetsStoreOption()
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

                string templatePath = Path.Combine(config.HexoPath, "scaffolds", "post.md");

                string fileFullPath;


                var fileDirectory = Directory.Exists(config.OutputPath) == false
                    ? Directory.CreateDirectory(config.OutputPath).FullName
                    : new DirectoryInfo(config.OutputPath).FullName;



                if (File.Exists(templatePath))
                {
                    int index = 1;
                    foreach (var md in mds)
                    {
                        var progress = $"{index}|{mds.Count}";
                        WeakReferenceMessenger.Default.Send<string, string>(progress, MessengerToken.UPDATEPROGRESS);
                        assetsStoreOption.SubPath=md.Title;
                        handler.SetAssetsStoreProvider(assetsStoreOption, assetsStoreProvider);

                        fileFullPath = MarkdownHandler(processResultList, config, client, handler, getMarkdownOption, assetsStoreOption, templatePath, fileDirectory, md);
                        index++;
                    }

                }
                else
                {

                    processResultList.Add(new ProcessResultDto(DateTime.Now, $"找不到Hexo目录：{templatePath} "));

                }

            }
            catch (ArgumentException ex)
            {
                processResultList.Add(new ProcessResultDto(DateTime.Now, string.Format("{0}参数错误:{0}{1}", Environment.NewLine, ex)));
            }
            catch (Exception ex)
            {
                processResultList.Add(new ProcessResultDto(DateTime.Now, string.Format("{0}未知错误:{0}{1}", Environment.NewLine, ex)));
            }
            return processResultList.ToArray();
        }

    }

}
