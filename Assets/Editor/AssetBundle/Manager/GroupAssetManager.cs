using UnityEngine;
using System.IO;

namespace AssetBundle {
    public class GroupAssetManager<T> : BaseAssetManager where T : Object{

        /// <param name="assetFolderPath">需要打包的资源文件夹</param>
        /// <param name="filter">过滤器</param>
        /// <param name="outputFolderName">存放AssetBundle的文件夹</param>
        public GroupAssetManager(string assetFolderPath, string filter, string outputFolderName)
            : base(assetFolderPath, filter, outputFolderName) {

        }

        protected override BaseAsset[] GetAssetArray() {
            //获取所有文件夹目录
            string[] assetGroups = Directory.GetDirectories(base.assetFolderPath);
            BaseAsset[] items = new BaseAsset[assetGroups.Length];

            for(int i = 0; i < assetGroups.Length; i++) {
                string folderName = assetGroups[i];
                string[] assets = GetAssets(folderName, base.filter);
                items[i] = new GroupAsset<T>(base.assetFolderPath, folderName, assets, base.outputFolderName);
            }

            return items;
        }
    }
}
