using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogTool.Core.Helper;
using Newtonsoft.Json;

namespace BlogTool.Helper
{
    public class LocalDataHelper
    {
        //const string _dirPrefix = "LocalData";
        const string _dirPrefix = "Data";

        //const string local = "local_";
        const string _filePrefix = "_";
        private readonly static string basePath = CommonHelper.AppBasePath;

        public static string GetPath<T>(string defaultFileName = default)
        {
            var dirPath = Path.Combine(basePath, _dirPrefix);
            var fileName = !string.IsNullOrEmpty(defaultFileName) ? defaultFileName : string.Format("{1}{0}.json", typeof(T).Name, _filePrefix);
            var filePath = Path.Combine(dirPath, fileName);
            return filePath;
        }

        public static string GetBasePath()
        {
            var dirPath = Path.Combine(basePath, _dirPrefix);
            return dirPath;
        }

        public static string GetPath(Type type, string defaultFileName = default)
        {
            var dirPath = Path.Combine(basePath, _dirPrefix);
            var fileName = !string.IsNullOrEmpty(defaultFileName) ? defaultFileName : string.Format("{1}{0}.json", type.Name, _filePrefix);
            var filePath = Path.Combine(dirPath, fileName);
            return filePath;
        }


        public static string GetPath(string typeName, string defaultFileName = default)
        {
            var dirPath = Path.Combine(basePath, _dirPrefix);
            var fileName = !string.IsNullOrEmpty(defaultFileName) ? defaultFileName : string.Format("{1}{0}.json", typeName, _filePrefix);
            var filePath = Path.Combine(dirPath, fileName);
            return filePath;
        }

        public static bool IsExist<T>(string defaultFileName = default)
        {
            var filePath = GetPath<T>(defaultFileName);
            var result = DirFileHelper.IsExistFile(filePath);
            return result;
        }

        public static bool IsExist(Type type, string defaultFileName = default)
        {
            var filePath = GetPath(type, defaultFileName);
            var result = DirFileHelper.IsExistFile(filePath);
            return result;
        }

        public static bool IsExist(string typeName, string defaultFileName = default)
        {
            var filePath = GetPath(typeName, defaultFileName);
            var result = DirFileHelper.IsExistFile(filePath);
            return result;
        }

        public static IList<T> ReadCollectionLocal<T>(string defaultFileName = default) where T : class
        {
            var dirPath = Path.Combine(basePath, _dirPrefix);
            if (DirFileHelper.IsExistDirectory(dirPath))
            {
                var fileName = !string.IsNullOrEmpty(defaultFileName) ? defaultFileName : string.Format("{1}{0}.json", typeof(T).Name, _filePrefix);
                var filePath = Path.Combine(dirPath, fileName);
                if (DirFileHelper.IsExistFile(filePath))
                {
                    var serializedstr = DirFileHelper.ReadFile(filePath);
                    var deserializedobj = JsonConvert.DeserializeObject<IList<T>>(serializedstr);
                    return deserializedobj;
                }
            }
            return new List<T>();
        }


        public static void SaveCollectionLocal<T>(IList<T> source, string defaultFileName = default) where T : class
        {
            var dirPath = Path.Combine(basePath, _dirPrefix);
            DirFileHelper.CreateDir(dirPath);
            var serializedstr = JsonConvert.SerializeObject(source);
            var fileName = !string.IsNullOrEmpty(defaultFileName) ? defaultFileName : string.Format("{1}{0}.json", typeof(T).Name, _filePrefix);
            var filePath = Path.Combine(dirPath, fileName);
            DirFileHelper.CreateFile(filePath, serializedstr);
        }

        public static T ReadObjectLocal<T>(string defaultFileName = default) where T : class
        {

            var dirPath = Path.Combine(basePath, _dirPrefix);
            if (DirFileHelper.IsExistDirectory(dirPath))
            {
                var fileName = !string.IsNullOrEmpty(defaultFileName) ? defaultFileName : string.Format("{1}{0}.json", typeof(T).Name, _filePrefix);
                var filePath = Path.Combine(dirPath, fileName);
                if (DirFileHelper.IsExistFile(filePath))
                {
                    var serializedstr = DirFileHelper.ReadFile(filePath);
                    var deserializedobj = JsonConvert.DeserializeObject<T>(serializedstr);
                    return deserializedobj;
                }
            }
            return default;
        }

        public static void SaveObjectLocal<T>(T source, string defaultFileName = default) where T : class
        {
            var dirPath = Path.Combine(basePath, _dirPrefix);
            DirFileHelper.CreateDir(dirPath);
            var serializedstr = JsonConvert.SerializeObject(source);
            var fileName = !string.IsNullOrEmpty(defaultFileName) ? defaultFileName : string.Format("{1}{0}.json", typeof(T).Name, _filePrefix);
            var filePath = Path.Combine(dirPath, fileName);
            DirFileHelper.CreateFile(filePath, serializedstr);
        }


        public static void InitLocalPath()
        {
            var dirPath = Path.Combine(basePath, _dirPrefix);
            if (DirFileHelper.IsExistDirectory(dirPath))
            {
                DirFileHelper.ClearDirectory(dirPath);
            }
            else
            {
                DirFileHelper.CreateDir(dirPath);
            }


        }
    }
}
