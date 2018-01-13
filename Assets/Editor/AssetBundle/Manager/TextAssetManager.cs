using System.IO;
using System.Text;
using UnityEngine;

namespace AssetBundle {

	public class TextAssetManager : NormalAssetManager<Object> {

        public TextAssetManager(string srcFolder, string filter, string outputFolder)
            : base(srcFolder, filter, outputFolder) {

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