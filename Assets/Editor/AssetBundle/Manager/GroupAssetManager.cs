using UnityEngine;
using System.IO;

namespace AssetBundle {
    public class GroupAssetManager<T> : BaseAssetManager where T : Object{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcFolder">需要打包的资源文件夹</param>
        /// <param name="filter">过滤器</param>
        /// <param name="outputFolder">存放AssetBundle的文件夹</param>
        public GroupAssetManager(string srcFolder, string filter, string outputFolder)
            : base(srcFolder, filter, outputFolder) {

        }

        protected override BaseAsset[] GetAssetArray() {
            string[] subFolders = Directory.GetDirectories(base.srcFolder);
            BaseAsset[] items = new BaseAsset[subFolders.Length];

            for(int i = 0; i < subFolders.Length; i++) {
                string subFolder = subFolders[i];
                string[] assets = GetAssets(subFolder, base.filter);
                items[i] = new GroupAsset<T>(base.srcFolder, subFolder, assets, base.outputFolder);
            }

            return items;
        }
    }
}
