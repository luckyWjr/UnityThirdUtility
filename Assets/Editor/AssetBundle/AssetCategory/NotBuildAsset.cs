using System.IO;
using UnityEngine;

namespace AssetBundle
{
	public class NotBuildAsset : NormalAsset<Object> {

        public NotBuildAsset(string assetPath, string assetFolderPath, string outputFolderName)
            : base(assetPath, assetFolderPath, outputFolderName) {
            m_assetPath = assetPath;
        }

        public void Build(string folder) {
            string name = Path.GetFileName(m_assetPath);
            string targetPath = string.Format("{0}/{1}/{2}", folder, outputFolderName, name.ToLower());
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
