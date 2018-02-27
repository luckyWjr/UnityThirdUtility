using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace AssetBundle {
	public abstract class BaseAssetManager {

        string m_outputFolder;
        BaseAsset[] m_items;

        protected abstract BaseAsset[] GetAssetArray();

        #region static

        /// <summary>
        /// 获取指定目录的资源
        /// </summary>
        /// <param name="filter">过滤器，若以t:开头，表示用unity的方式过滤；若以f:开头，表示用windows的SearchPattern方式过滤；若以r:开头，表示用正则表达式的方式过滤。</param>
        public static string[] GetAssets(string folder, string filter) {
            if (string.IsNullOrEmpty(folder)) {
                throw new ArgumentException("folder");
            }
            if (string.IsNullOrEmpty(filter)) {
                throw new ArgumentException("filter");
            }

            if (filter.StartsWith("t:")) {
                string[] guids = AssetDatabase.FindAssets(filter, new string[] { folder });
                string[] paths = new string[guids.Length];
                for (int i = 0; i < guids.Length; i++) {
                    paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
                }
                return paths;
            } else if (filter.StartsWith("f:")) {
                string folderFullPath = BaseAsset.GetFullPath(folder);
                string searchPattern = filter.Substring(2);
                string[] files = Directory.GetFiles(folderFullPath, searchPattern, SearchOption.AllDirectories);
                string[] paths = new string[files.Length];
                for (int i = 0; i < files.Length; i++) {
                    paths[i] = BaseAsset.GetAssetPath(files[i]);
                }
                return paths;
            } else if (filter.StartsWith("r:")) {
                string folderFullPath = BaseAsset.GetFullPath(folder);
                string pattern = filter.Substring(2);
                string[] files = Directory.GetFiles(folderFullPath, "*.*", SearchOption.AllDirectories);
                List<string> list = new List<string>();
                for (int i = 0; i < files.Length; i++) {
                    string name = Path.GetFileName(files[i]);
                    if (Regex.IsMatch(name, pattern)) {
                        string p = BaseAsset.GetAssetPath(files[i]);
                        list.Add(p);
                    }
                }
                return list.ToArray();
            } else {
                throw new InvalidOperationException("Unexpected filter: " + filter);
            }
        }

        #endregion

        /// <summary>
        /// 资源类别，如lua是一个类别，ui prefab是一个类别
        /// </summary>
        /// <param name="srcFolder">资源所在项目目录</param>
        /// <param name="filter">过滤器，其中如f:*.*是自定义的过滤器，做为SearchPattern搜索匹配文件</param>
        /// <param name="outputFolder">此资源输出的目录</param>
        public BaseAssetManager(string srcFolder, string filter, string outputFolder) {
            if (string.IsNullOrEmpty(srcFolder) || !srcFolder.StartsWith(BuildConfig.unityBaseFolder)) {
                throw new ArgumentException("srcFolder");
            }
            if (string.IsNullOrEmpty(filter)) {
                throw new ArgumentException("filter");
            }
            if (string.IsNullOrEmpty(outputFolder)) {
                throw new ArgumentException("outputFolder");
            }

            this.srcFolder = srcFolder;
            this.filter = filter;
            this.outputFolder = outputFolder;
        }

        public virtual void ComputeHash() {
            foreach (var item in items) {
                item.ComputeHashIfNeeded();
            }
        }

        protected string[] GetAssets() {
            return GetAssets(srcFolder, filter);
        }

        public override string ToString() {
            return string.Format("{0}\t{1}\t{2}", srcFolder, filter, outputFolder);
        }

        public virtual void Dispose() {
            foreach (var item in items) {
                item.Dispose();
            }
        }

        public virtual void PrepareBuild() {

        }

        public virtual void OnBuildFinished() {

        }

        #region 属性
        public string srcFolder {
            private set;
            get;
        }

        public string filter {
            private set;
            get;
        }

        public string outputFolder {
            get { return m_outputFolder; }
            set { m_outputFolder = value.TrimStart('/').TrimEnd('/').ToLower(); }
        }

        public BaseAsset[] items {
            get {
                if (m_items == null) {
                    m_items = GetAssetArray();
                }
                return m_items;
            }
        }

        public bool isChange {
            get {
                if (items != null) {
                    foreach (var item in items) {
                        if (item.lastHash != item.currentHash) {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public IEnumerable<AssetBundleBuild> assetBundleBuilds {
            get {
                foreach (var item in items) {
                    if (item.isNeedBuild) {
                        yield return new AssetBundleBuild() {
                            assetBundleName = item.assetBundleName,
                            assetNames = item.assetNames,
                        };
                    }
                }
            }
        }
        #endregion
    }
}
