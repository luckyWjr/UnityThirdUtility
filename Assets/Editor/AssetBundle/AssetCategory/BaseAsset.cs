using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Utility;
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
        
        /// <summary>
        /// AB输出文件夹名称 如a/b
        /// </summary>
        string m_outputFolderName;

        string m_currentMd5;

        //存放临时变量
        int m_i, m_len;
        string m_tempString;

        protected abstract string ComputeMd5();

        AssetFlag m_flag = AssetFlag.NoChange;

        public BaseAsset(string outputFolderName) {
            if(string.IsNullOrEmpty(outputFolderName)) {
                throw new ArgumentException("outputFolder");
            }

            m_outputFolderName = outputFolderName.TrimStart('/').TrimEnd('/');
            m_outputFolderName = m_outputFolderName.ToLower();
        }

        #region 属性

        /// <summary>
        /// 资源文件名 不带扩展名
        /// </summary>
        public abstract string name { get; }

        /// <summary>
        /// 资源文件名，带扩展名
        /// </summary>
        public string fullName {
            get { return name + ext; }
        }

        /// <summary>
        /// AB 的包名 如：a/b/c.d
        /// </summary>
        public string assetBundleName {
            get {
                string path = m_outputFolderName.Contains("{0}") ? string.Format(m_outputFolderName, fullName) : string.Format("{0}/{1}", m_outputFolderName, fullName);
                return path.ToLower();
            }
        }

        public string outputFolderName {
            get {
                return m_outputFolderName;
            }
        }

        public string lastMd5;

        public string currentMd5 {
            get {
                if(string.IsNullOrEmpty(m_currentMd5)) {
                    m_currentMd5 = ComputeMd5();
                }
                return m_currentMd5;
            }
        }

        public bool isNeedBuild {
            get { return currentMd5 != lastMd5; }
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

        /// <summary>
        /// AB里面资源的名称（即路径）
        /// </summary>
        public abstract string[] assetNames { get; }
        #endregion

        public static void ParseConfigLine(string configLine, out string relativePath, out string hash) {
            string[] words = configLine.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            relativePath = words[0];
            hash = words[1];
        }

        /// <summary>
        /// 获取Asset和.meta文件的btye[]
        /// </summary>
        /// <param name="assetPath">Asset路径</param>
        /// <returns>Asset和.meta文件的btye[]</returns>
        public byte[] ReadAssetBytes(string assetPath) {
            string fullPath = PathUtility.GetFullPath(assetPath);
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
        /// 计算单个资源的md5码
        /// </summary>
        public string ComputeMd5(string assetPath) {
            byte[] buffer = ReadAssetBytes(assetPath);
            return buffer != null ? TypeConvertUtility.ByteToMd5(buffer) : null;
        }

        /// <summary>
        /// 计算若干个文件合并成的md5码
        /// </summary>
        public string ComputeMd5WithDependencies(string[] assetPaths) {
            List<byte> list = new List<byte>();
            foreach (var p in assetPaths) {
                byte[] buffer = ReadAssetBytes(p);
                if (buffer != null) {
                    list.AddRange(buffer);
                }
            }

            // 依赖项
            string[] dependencies = AssetDatabase.GetDependencies(assetPaths);
            byte[] bufferOfD;
            foreach(var d in dependencies) {
                bufferOfD = null;
                bufferOfD = ReadAssetBytes(d);
                if(bufferOfD != null) {
                    list.AddRange(bufferOfD);
                }
            }

            return TypeConvertUtility.ByteToMd5(list.ToArray());
        }

        /// <summary>
        /// 若Hash为空，则计算Hash
        /// </summary>
        public void ComputeMd5IfNeeded() {
            if (string.IsNullOrEmpty(m_currentMd5)) {
                m_currentMd5 = ComputeMd5();
            }
        }

        /// <summary>
        /// 生成一条配置
        /// </summary>
        public string GenerateConfigLine() {
            return string.Format("{0},{1},", assetBundleName, currentMd5);
        }

        /// <summary>
        /// 读取配置，从中找出属于自己的那条
        /// </summary>
        /// <param name="configLines">配置数组</param>
        /// <returns>对应下标，若没有返回-1</returns>
        public int ReadConfig(List<string> configLines) {
            for(m_i = 0, m_len = configLines.Count; m_i < m_len; m_i++) {
                if(RightConfig(configLines[m_i])) {
                    ParseConfigLine(configLines[m_i], out m_tempString, out lastMd5);
                    return m_i;
                }
            }
            return -1;
        }

        public bool RightConfig(string configLine) {
            return configLine.StartsWith(assetBundleName + ",");
        }

        public virtual void Dispose() {

        }
    }
}
