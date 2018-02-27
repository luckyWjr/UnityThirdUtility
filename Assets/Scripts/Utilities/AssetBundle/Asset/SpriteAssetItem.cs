using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility {
    /// <summary>
    /// 存储单个图片资源信息
    /// </summary>
	public class SpriteAssetItem : NormalAssetItem {
        const string m_spriteAssetFolder = @"Sprites/";

        public const string iconFolder = @"Icon";

        public SpriteAssetItem(string folder, string name) {
            assetCategoryPath = m_spriteAssetFolder;
            this.folder = folder;
            this.name = name;
            m_fullPath = string.Format("{0}{1}.u", assetCategoryPath, folder);
        }

        public Sprite sprite {
            get {
                return (Sprite)m_obj;
            }
        }
    }
}