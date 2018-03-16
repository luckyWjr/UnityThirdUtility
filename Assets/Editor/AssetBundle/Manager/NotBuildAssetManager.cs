using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundle
{
	public class NotBuildAssetManager : BaseAssetManager{

        public NotBuildAssetManager(string assetFolderPath, string filter, string outputFolderName)
            : base(assetFolderPath, filter, outputFolderName) {

        }

        protected override BaseAsset[] GetAssetArray() {
            string[] assetPaths = base.GetAssets();
            BaseAsset[] items = new BaseAsset[assetPaths.Length];

            for (int i = 0; i < assetPaths.Length; i++) {
                items[i] = new NotBuildAsset(assetPaths[i], base.assetFolderPath, base.outputFolderName);
            }

            return items;
        }

        public void Build(string folder) {
            for (int i = 0, j = base.items.Length; i < j; i++) {
                NotBuildAsset item = (NotBuildAsset)base.items[i];
                item.Build(folder);
            }
        }
    }
}
