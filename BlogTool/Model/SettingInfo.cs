using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BlogTool.Model
{
    public class SettingInfo : ObservableObject
    {

        public static List<string> AssetsStoreTypes => new() { "嵌入Base64", "本地目录", "Hexo资源文件夹", "Hexo标签插件" }
       ;

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