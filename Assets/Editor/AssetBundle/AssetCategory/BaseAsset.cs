using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetBundle {
    public enum AssetFlag {
        NoChange,
        NewAdded,
        Modified,
    }

    /// <summary>
    /// 单个的ab包
    /// </summary>
    public abstract class BaseAsset {
        
        string m_outputFolderPath;
        string m_currentHash;

        protected abstract string ComputeHash();

        AssetFlag m_flag = AssetFlag.NoChange;
        
        #region 属性

        public string outputRelativePath {
            get {
                string path = m_outputFolderPath.Contains("{0}") ? string.Format(m_outputFolderPath, name) : string.Format("{0}/{1}", m_outputFolderPath, name);
                return path.ToLower();
            }
        }

        /// <summary>
        /// AB 的包名
        /// </summary>
        public string assetBundleName {
            get { return outputRelativePath + ext; }
        }

        public string lastHash { get; set; }

        public string currentHash {
            get {
                if (string.IsNullOrEmpty(m_currentHash)) {
                    m_currentHash = ComputeHash();
                }
                return m_currentHash;
            }
        }

        public string fullName {
            get { return name + ext; }
        }

        public bool isNeedBuild {
            get { return currentHash != lastHash; }
        }

        public virtual string ext {
            get { return ".u"; }
        }

        public AssetFlag flag {
            get {
                return m_flag;
            }
            set {
                m_flag = value;
            }
        }

        public abstract string name { get; }

        /// <summary>
        /// AB里面资源的名称（即路径）
        /// </summary>
        public abstract string[] assetNames { get; }
        #endregion

        #region static

        static MD5 m_md5;
        static MD5 md5 {
            get { return m_md5 ?? (m_md5 = MD5.Create()); }
        }

        public static void ParseConfigLine(string configLine, out string relativePath, out string hash) {
            string[] words = configLine.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            relativePath = words[0];
            hash = words[1];
        }

        /// <summary>
        /// asset path 转 full path
        /// </summary>
        public static string GetFullPath(string assetPath) {
            if (string.IsNullOrEmpty(assetPath)) {
                return "";
            }

            string p = Application.dataPath + assetPath.Substring(6);
            return p.Replace("\\", "/");
        }

        /// <summary>
        /// full path 转 asset path
        /// </summary>
        public static string GetAssetPath(string fullPath) {
            if (string.IsNullOrEmpty(fullPath)) {
                return "";
            }

            fullPath = fullPath.Replace("\\", "/");
            return fullPath.StartsWith("Assets/") ?  fullPath : "Assets" + fullPath.Substring(Application.dataPath.Length);
        }

        public static byte[] ReadAssetBytes(string assetPath) {
            string fullPath = GetFullPath(assetPath);
            if (!File.Exists(fullPath)) {
                return null;
            }

            List<byte> list = new List<byte>();

            var a = File.ReadAllBytes(fullPath);
            list.AddRange(a);

            string metaPath = fullPath + ".meta";
            var b = File.ReadAllBytes(metaPath);
            list.AddRange(b);

            return list.ToArray();
        }

        /// <summary>
        /// 计算哈希值字符串
        /// </summary>
        public static string ComputeHash(byte[] buffer) {
            if (buffer == null || buffer.Length < 1) {
                return "";
            }

            byte[] hash = md5.ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();

            foreach (var b in hash) {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 计算单个资源的哈希码
        /// </summary>
        public static string ComputeHash(string assetPath) {
            byte[] buffer = ReadAssetBytes(assetPath);
            return buffer != null ? ComputeHash(buffer) : null;
        }

        /// <summary>
        /// 计算单个文件联合依赖项的哈希码
        /// </summary>
        public static string ComputeHashWithDependencies(string assetPath) {
            byte[] buffer = ReadAssetBytes(assetPath);
            if (buffer == null) {
                return "";
            }

            List<byte> list = new List<byte>(buffer);

            // 依赖项
            string[] dependencies = AssetDatabase.GetDependencies(new string[] { assetPath });
            foreach (var d in dependencies) {
                byte[] bufferOfD = ReadAssetBytes(d);
                if (bufferOfD != null) {
                    list.AddRange(bufferOfD);
                }
            }

            return ComputeHash(list.ToArray());
        }

        /// <summary>
        /// 计算若干个文件合并成的哈希码
        /// </summary>
        public static string ComputeHashWithDependencies(string[] assetPaths) {
            List<byte> list = new List<byte>();
            foreach (var p in assetPaths) {
                byte[] buffer = ReadAssetBytes(p);
                if (buffer != null) {
                    list.AddRange(buffer);
                }
            }

            // 依赖项
            string[] dependencies = AssetDatabase.GetDependencies(assetPaths);
            foreach (var d in dependencies) {
                byte[] bufferOfD = ReadAssetBytes(d);
                if (bufferOfD != null) {
                    list.AddRange(bufferOfD);
                }
            }

            return ComputeHash(list.ToArray());
        }

        #endregion

        public BaseAsset(string outputFolder) {
            if (string.IsNullOrEmpty(outputFolder)) {
                throw new ArgumentException("outputFolder");
            }

            m_outputFolderPath = outputFolder.TrimStart('/').TrimEnd('/');
        }

        public void ComputeHashIfNeeded() {
            if (string.IsNullOrEmpty(m_currentHash)) {
                m_currentHash = ComputeHash();
            }
        }

        /// <summary>
        /// 生成一条配置
        /// </summary>
        public string GenerateConfigLine() {
            return string.Format("{0}{1},{2},", outputRelativePath, ext, currentHash);
        }

        /// <summary>
        /// 读取配置，从中找出属于自己的那条
        /// </summary>
        public void ReadConfig(string[] configLines) {
            foreach (var l in configLines) {
                if (RightConfig(l)) {
                    string r, m_lastHash;
                    ParseConfigLine(l, out r, out m_lastHash);
                    lastHash = m_lastHash;
                    break;
                }
            }
        }

        public bool RightConfig(string configLine) {
            return configLine.StartsWith(outputRelativePath + ext + ",");
        }

        public virtual void Dispose() {

        }
    }
}
