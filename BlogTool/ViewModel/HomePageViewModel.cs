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
using YamlDotNet.Core.Tokens;
using BlogTool.Markdown.Implements;

namespace BlogTool.ViewModel
{
    public class HomePageViewModel : ObservableObject
    {
        private static string _fileName = null;
        private static string _markdownFilesMd = "Markdown文件|*.md|所有文件|*.*";
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
            InitData();
        }


        public void InitData()
        {
            //todo:

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
                openFileDialog.Multiselect = false;
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
                var content = dialog.FindChild<ClipboardInputDialog>("MainDialog").TextBoxContent.Text;

                var task = InvokeHelper.InvokeOnUi<IMarkdown>(null, () =>
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
                    return ProcessMarkdowns(getMarkdownOption, new TextMarkdownProvider(), new { Title = string.Empty, Content = content });

                }, async (t) =>
                {
                    if (t!=null)
                    {
                        this.MarkdownContent=t;
                        this.PreviewInnerHtml= await JavaScriptHelper.GetHtmlFromMarkdownAsync(new Common.GetHtmlFromMarkdownOption() { Markdown=this.MarkdownContent.Content });
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("删除文件时发生错误: " + ex.Message);
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

        private string _previewInnerHtml;

        public string PreviewInnerHtml
        {
            get { return _previewInnerHtml; }
            set
            {
                _previewInnerHtml = value;
                OnPropertyChanged(nameof(PreviewInnerHtml));
            }
        }
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

            fileContent=AssetsHandler(config, client, handler, assetsStoreOption, fileDirectory, md, fileContent);

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
            return fileFullPath;
        }

        private string AssetsHandler(SettingInfo config, HttpClient client, AssetsStoreHandler handler, AssetsStoreOption assetsStoreOption, string fileDirectory, IMarkdown md, string fileContent)
        {
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
            return fileContent;
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
