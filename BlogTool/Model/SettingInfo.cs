using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BlogTool.Model
{
    public class SettingInfo : ObservableObject
    {
        public const string EMBED = "嵌入Base64";
        public const string LOCAL = "本地目录";
        public const string HEXO_ASSET_FOLDER = "Hexo资源文件夹";
        public const string HEXO_TAG_PLUGIN = "Hexo标签插件";

        public static List<string> AssetsStoreTypes => new() { EMBED, LOCAL, HEXO_ASSET_FOLDER, HEXO_TAG_PLUGIN };

        public SettingInfo()
        {
            this.AssetsStoreProvider = AssetsStoreTypes.First();
        }

        private bool _skipFileWhenException;

        public bool SkipFileWhenException
        {
            get { return _skipFileWhenException; }
            set
            {
                _skipFileWhenException = value;
                OnPropertyChanged(nameof(SkipFileWhenException));

            }
        }

        private string _hexoPath;

        public string HexoPath
        {
            get { return _hexoPath; }
            set
            {
                _hexoPath = value;
                OnPropertyChanged(nameof(HexoPath));

            }
        }

        private string _outputPath;

        public string OutputPath
        {
            get { return _outputPath; }
            set
            {
                _outputPath = value;
                OnPropertyChanged(nameof(OutputPath));

            }
        }

        private int _readMorePosition;

        public int ReadMorePosition
        {
            get { return _readMorePosition; }
            set
            {
                _readMorePosition = value;
                OnPropertyChanged(nameof(ReadMorePosition));

            }
        }

        private int _recentTakeCount;

        public int RecentTakeCount
        {
            get { return _recentTakeCount; }
            set
            {
                _recentTakeCount = value;
                OnPropertyChanged(nameof(RecentTakeCount));

            }
        }

        private string _assetsStoreProvider;

        public string AssetsStoreProvider
        {
            get { return _assetsStoreProvider; }
            set
            {
                _assetsStoreProvider = value;
                OnPropertyChanged(nameof(AssetsStoreProvider));

            }
        }


    }
}