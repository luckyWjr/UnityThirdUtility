using System.IO;
using UnityEngine;

namespace AssetBundle
{
	public class NotBuildAsset : NormalAsset<Object> {

        string m_assetPath;
        string m_outputFolder;

        public NotBuildAsset(string assetPath, string outputFolder, string srcFolder)
            : base(assetPath, outputFolder, srcFolder) {
            m_assetPath = assetPath;
            m_outputFolder = outputFolder;
        }

        public void Build(string productFolder) {
            string name = Path.GetFileName(m_assetPath);
            string targetPath = string.Format("{0}/{1}/{2}", productFolder, m_outputFolder.ToLower(), name.ToLower());
            Uploader.CreateDirectory(targetPath);
            File.Delete(targetPath);
            File.Copy(m_assetPath, targetPath);
        }

        public override string[] assetNames {
            get { return null; }
        }

        public override string ext {
            get { return Path.GetExtension(m_assetPath); }
        }
    }
}
