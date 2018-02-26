using System;

namespace Utility {
    public abstract class NormalAssetItem {
        /// <summary>
        /// 同类资源根文件夹,如 Image/ ,Effect/ 等
        /// </summary>
        public string assetCategoryPath = string.Empty;
        /// <summary>
        /// 子文件夹 如 Image/ 下 Bg/ ,Icon/ 等
        /// </summary>
        public string folder = string.Empty;
        /// <summary>
        /// 文件名 如 Icon/ 下 suc ,fail 等
        /// </summary>
        public string name = string.Empty;

        protected UnityEngine.Object m_obj;

        protected Action m_callback;
        protected string m_fullPath;

        //public NormalAssetItem(string subFolder, string name) {
        //    folder = subFolder;
        //    this.name = name;
        //}

        public virtual void Load(Action callback = null) {
            AssetBundleItem ab = AssetBundleUtility.Load(m_fullPath, name);
            m_obj = ab.LoadAsset(name, typeof(UnityEngine.Object));
            if(callback != null) {
                m_callback = callback;
                m_callback();
            }
        }

        public void Destroy() {
            if(m_obj != null) {
                m_obj = null;
                AssetBundleUtility.Delete(m_fullPath);
            }
        }
    }
}