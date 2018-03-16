using UnityEngine;

namespace AssetBundle {
    public class NormalAssetManager<T> : BaseAssetManager where T : Object{

        /// <param name="assetFolderPath">需要打包的资源文件夹</param>
        /// <param name="filter">过滤器</param>
        /// <param name="outputFolderName">存放AssetBundle的文件夹</param>
        public NormalAssetManager(string assetFolderPath, string filter, string outputFolderName)
            : base(assetFolderPath, filter, outputFolderName) {

        }

        protected override BaseAsset[] GetAssetArray() {
            string[] assetPaths = base.GetAssets();
            BaseAsset[] items = new BaseAsset[assetPaths.Length];

            for (int i = 0, j = assetPaths.Length; i < j; i++) {
                items[i] = new NormalAsset<T>(assetPaths[i], base.assetFolderPath, base.outputFolderName);
            }
            return items;
        }
    }
}
