using System;

namespace AssetBundle {
    public class NormalAsset<T> : BaseAsset where T : UnityEngine.Object {
        string m_assetPath;//资源完整路径
        string m_srcFolder;//资源根目录
        string m_nameToSrcFolder;//ab 名称(路径+文件名 = m_assetPath - m_srcFolder)

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetPath">资源完整路径</param>
        /// <param name="outputFolder">输出目录</param>
        /// <param name="srcFolder">资源根目录</param>
        public NormalAsset(string assetPath, string outputFolder, string srcFolder)
            : base(outputFolder) {
            if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith("Assets")) {
                throw new ArgumentException("assetPath");
            }

            m_assetPath = assetPath.Replace("\\", "/");
            m_srcFolder = srcFolder.Replace("\\", "/").TrimEnd('/');
        }

        protected override string ComputeHash() {
            return ComputeHashWithDependencies(m_assetPath);
        }

        public override string[] assetNames {
            get { return new string[] { m_assetPath }; }
        }

        public override string name {
            get {
                if(string.IsNullOrEmpty(m_nameToSrcFolder)) {
                    m_nameToSrcFolder = GetNameToSrcFolder(m_assetPath, m_srcFolder);
                }
                return m_nameToSrcFolder;
            }
        }

        public string assetPath {
            get { return m_assetPath; }
        }

        protected string srcFolder {
            get { return m_srcFolder; }
        }

        protected virtual string GetNameToSrcFolder(string assetPath, string srcFolder) {
            srcFolder = srcFolder.Replace("\\", "/").TrimEnd('/');
            int startIndex = srcFolder.Length + 1;
            int lastIndex = assetPath.LastIndexOf(".");
            int length = lastIndex - startIndex;
            return assetPath.Substring(startIndex, length);
        }
    }
}
