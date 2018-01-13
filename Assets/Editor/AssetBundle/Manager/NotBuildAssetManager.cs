using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundle
{
	public class NotBuildAssetManager : BaseAssetManager{

        public NotBuildAssetManager(string srcFolder, string filter, string outputFolder)
            : base(srcFolder, filter, outputFolder) {

        }

        protected override BaseAsset[] GetAssetArray() {
            string[] assetPaths = base.GetAssets();
            BaseAsset[] items = new BaseAsset[assetPaths.Length];

            for (int i = 0; i < assetPaths.Length; i++) {
                items[i] = new NotBuildAsset(assetPaths[i], base.outputFolder, base.srcFolder);
            }

            return items;
        }

        public void Build(string productFolder) {
            for (int i = 0; i < base.items.Length; i++) {
                NotBuildAsset item = (NotBuildAsset)base.items[i];
                item.Build(productFolder);
            }
        }
    }
}
