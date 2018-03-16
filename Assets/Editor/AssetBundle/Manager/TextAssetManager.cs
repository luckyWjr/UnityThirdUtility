using System.IO;
using System.Text;
using UnityEngine;

namespace AssetBundle {

	public class TextAssetManager : NormalAssetManager<Object> {

        public TextAssetManager(string assetFolderPath, string filter, string outputFolderName)
            : base(assetFolderPath, filter, outputFolderName) {

        }

        public override void PrepareBuild() {
            base.PrepareBuild();

            string[] assetPaths = base.GetAssets();
            foreach (string path in assetPaths) {
                string content = File.ReadAllText(path, Encoding.Default);
                File.WriteAllText(path, content, Encoding.UTF8);
            }
        }
    }
}