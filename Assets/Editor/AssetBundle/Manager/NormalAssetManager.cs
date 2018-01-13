using UnityEngine;

namespace AssetBundle {
    public class NormalAssetManager<T> : BaseAssetManager where T : Object{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcFolder">需要打包的资源文件夹</param>
        /// <param name="filter">过滤器</param>
        /// <param name="outputFolder">存放AssetBundle的文件夹</param>
        public NormalAssetManager(string srcFolder, string filter, string outputFolder)
            : base(srcFolder, filter, outputFolder) {

        }

        protected override BaseAsset[] GetAssetArray() {
            string[] assetPaths = base.GetAssets();
            BaseAsset[] items = new BaseAsset[assetPaths.Length];

            for (int i = 0; i < assetPaths.Length; i++) {
                items[i] = new NormalAsset<T>(assetPaths[i], base.outputFolder, base.srcFolder);
            }
            return items;
        }
    }
}
