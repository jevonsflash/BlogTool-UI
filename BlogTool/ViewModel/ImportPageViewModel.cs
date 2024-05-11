using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using BlogTool.Core.Helper;
using BlogTool.Model.Dto;
using CommunityToolkit.Mvvm.DependencyInjection;
using BlogTool.Helper;
using BlogTool.Common;
using BlogTool.Core.Markdown;
using System.Net.Http;
using BlogTool.Core.AssetsStores;
using BlogTool.Core.Markdown.Implements;
using BlogTool.Core.AssetsStores.Implements;
using System.Web;
using BlogTool.Model;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using System.Windows.Controls;
using BlogTool.Control;

namespace BlogTool.ViewModel
{
    public class ImportPageViewModel : ObservableObject
    {
        public event EventHandler OnFinished;

        private static string _fileName = null;
        private static string _excelFilesXlsxXls = "Markdown文件|*.md|所有文件|*.*";
        private static readonly string basePath = CommonHelper.AppBasePath;
        public ImportPageViewModel()
        {
            this.ValidDataCommand = new RelayCommand(GetDataAction, CanValidate);
            this.SubmitCommand = new RelayCommand(SubmitAction, CanSubmit);
            this.Entities = new ObservableCollection<object>();
            this.ProcessResultList = new ObservableCollection<ProcessResultDto>();
            this.ProcessResultList.CollectionChanged += ProcessResultList_CollectionChanged;
            this.PropertyChanged += ImportPageViewModel_PropertyChanged;
        }

        private void ImportPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.IsValidSuccess))
            {
                SubmitCommand.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(this.Entities))
            {
                SubmitCommand.NotifyCanExecuteChanged();
                ValidDataCommand.NotifyCanExecuteChanged();

            }
        }

        private void ProcessResultList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.IsValidSuccess = this.ProcessResultList.Count == 0;
        }

        private async void SubmitAction()
        {
            var task = InvokeHelper.InvokeOnUi<IEnumerable<object>>(null, () =>
            {

                foreach (var employee in Entities)
                {
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Ioc.Default.GetRequiredService<CategoryPageViewModel>().Entities.Add((IMarkdown)employee);
                    });

                }


                return Entities;



            }, async (t) =>
            {

                this.Entities.Clear();
                this.OnFinished?.Invoke(this, EventArgs.Empty);
                MessageBox.Show("已完成导入");

            });
        }

        private void GetDataAction()
        {
            this.ProcessResultList.Clear();
            foreach (var item in this.Entities)
            {

                var row = (item as IMarkdown).Title;
                var id = ProcessResultList.Count + 1;
                var level = 1;





            }
            var currentCount = ProcessResultList.Count();

        }



        private async void ImportFromMetaWeblogAction()
        {
            this.Entities.Clear();
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
                    };
                    return ProcessMarkdowns(getMarkdownOption, new MetaWeblogMarkdownProvider());

                }, async (t) =>
                {
                    foreach (var item in t)
                    {
                        this.ProcessResultList.Add(item);
                    }
                    this.IsValidSuccess = null;
                    await DialogManager.HideMetroDialogAsync((MetroWindow)App.Current.MainWindow, dialog);

                });
            };



        }


        private void ImportFromLocalAction()
        {

            this.Entities.Clear();

            var task = InvokeHelper.InvokeOnUi<ProcessResultDto[]>(null, () =>
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
                foreach (var item in t)
                {
                    this.ProcessResultList.Add(item);
                }
                this.IsValidSuccess = null;

            });

        }


        private async void ImportFromClipboardAction()
        {
            this.Entities.Clear();
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
                var task = InvokeHelper.InvokeOnUi<ProcessResultDto[]>(null, () =>
                {

                    var getMarkdownOption = new GetMarkdownOption()
                    {
                        ReadMorePosition = -1,
                    };
                    return ProcessMarkdowns(getMarkdownOption, new TextMarkdownProvider(), new { Title = title, Content = content });

                }, async (t) =>
                {
                    foreach (var item in t)
                    {
                        this.ProcessResultList.Add(item);
                    }
                    this.IsValidSuccess = null;
                    await DialogManager.HideMetroDialogAsync((MetroWindow)App.Current.MainWindow, dialog);

                });
            };



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
                    SubPath = "."
                };


                if (config.AssetsStoreProvider == SettingInfo.EMBED)
                {
                    handler.SetAssetsStoreProvider(assetsStoreOption, new EmbedAssetsStoreProvider());

                }
                else if (config.AssetsStoreProvider == SettingInfo.LOCAL)
                {

                    handler.SetAssetsStoreProvider(assetsStoreOption, new LocalAssetsStoreProvider());
                }

                else if (config.AssetsStoreProvider == SettingInfo.HEXO_ASSET_FOLDER)
                {

                    handler.SetAssetsStoreProvider(assetsStoreOption, new HexoAssetFolderAssetsStoreProvider());
                }

                else if (config.AssetsStoreProvider == SettingInfo.HEXO_TAG_PLUGIN)
                {

                    handler.SetAssetsStoreProvider(assetsStoreOption, new HexoTagPluginAssetsStoreProvider());
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
                    foreach (var md in mds)
                    {
                        fileFullPath = MarkdownHandler(processResultList, config, client, handler, getMarkdownOption, assetsStoreOption, templatePath, fileDirectory, md);
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
        private ObservableCollection<object> _entities;

        public ObservableCollection<object> Entities
        {
            get { return _entities; }
            set
            {
                _entities = value;
                OnPropertyChanged(nameof(Entities));
            }
        }


        private bool? _isValidSuccess;

        public bool? IsValidSuccess
        {
            get { return _isValidSuccess; }
            set
            {
                _isValidSuccess = value;

                OnPropertyChanged();
            }
        }

        private bool CanSubmit()
        {
            return IsValidSuccess.HasValue && IsValidSuccess.Value;
        }

        private bool CanValidate()
        {
            if (this.Entities.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<MenuCommand> ImportOptions => new List<MenuCommand>() {
            new MenuCommand("从剪贴板导入", ImportFromClipboardAction, () => true),
            new MenuCommand("从文件夹导入", ImportFromLocalAction, () => true),
            new MenuCommand("从MetaWeblog接口导入", ImportFromMetaWeblogAction, () => true),
        };


        public RelayCommand ValidDataCommand { get; set; }
        public RelayCommand SubmitCommand { get; set; }

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



            var fileContent = md.Description;

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
            DirFileHelper.WriteText(fileFullPath, content);
            processResultList.Add(new ProcessResultDto(DateTime.Now, $"Markdown文件处理完成，文件保存在：{fileFullPath}"));
            return fileFullPath;
        }


    }
}
