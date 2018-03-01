using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Utility {
    /// <summary>
    /// 读取AB的工具类
    /// </summary>
	public class AssetBundleUtility {

        //public static readonly string suffix = ".assetbundle";
        public static readonly string streamingAssetsFileName = "Products";

        static AssetBundleManifest m_mainfest;
        static AssetBundleManifest mainfest {
            get {
                if(m_mainfest == null) {
                    AssetBundle ab = AssetBundle.LoadFromFile(streamingAssetsPath + "/" + streamingAssetsFileName);
                    if(ab != null) {
                        m_mainfest = (AssetBundleManifest)ab.LoadAsset("AssetBundleManifest");
                    } else {
                        Debug.LogError("Get Mainfest Error");
                        return null;
                    }
                }
                return m_mainfest;
            }
        }

        static Dictionary<string, AssetBundleItem> cacheAssets = new Dictionary<string, AssetBundleItem>();

        public static AssetBundleItem Load(string path, string fileName, bool isHasDependence = true) {
            if(mainfest != null) {
                path = path.ToLower();
                fileName = fileName.ToLower();

                if(isHasDependence) {
                    //读取依赖
                    string[] dps = mainfest.GetAllDependencies(path);
                    int len = dps.Length;
                    for(int i = 0; i < len; i++) {
                        AssetBundleItem dItem;
                        if(cacheAssets.ContainsKey(dps[i])) {
                            dItem = cacheAssets[dps[i]];
                        } else {
                            dItem = new AssetBundleItem(dps[i], fileName, false);
                            dItem.asset = AssetBundle.LoadFromFile(dItem.pathName);
                            cacheAssets[dps[i]] = dItem;
                        }
                        dItem.refCount++;
                    }
                }
                
                AssetBundleItem ab;
                if(cacheAssets.ContainsKey(path)) {
                    ab = cacheAssets[path];
                } else {
                    ab = new AssetBundleItem(path, fileName, isHasDependence);
                    ab.asset = AssetBundle.LoadFromFile(ab.pathName);
                    cacheAssets[path] = ab;
                }
                ab.refCount++;
                return ab;
            }
            return null;
        }

        public static IEnumerator LoadAsync(string path, string fileName, System.Action<AssetBundleItem> callback, bool isHasDependence = true) {
            if(mainfest != null) {
                path = path.ToLower();
                fileName = fileName.ToLower();

                if(isHasDependence) {
                    //读取依赖
                    string[] dps = mainfest.GetAllDependencies(path);
                    int len = dps.Length;
                    for(int i = 0; i < len; i++) {
                        AssetBundleItem dItem;
                        if(cacheAssets.ContainsKey(dps[i])) {
                            dItem = cacheAssets[dps[i]];
                        } else {
                            dItem = new AssetBundleItem(dps[i], fileName, false);
                            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(dItem.pathName);
                            yield return request;
                            dItem.asset = request.assetBundle;
                            cacheAssets[dps[i]] = dItem;
                        }
                        dItem.refCount++;
                    }
                }

                AssetBundleItem ab;
                if(cacheAssets.ContainsKey(path)) {
                    ab = cacheAssets[path];
                } else {
                    ab = new AssetBundleItem(path, fileName, isHasDependence);
                    AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(ab.pathName);
                    yield return request;
                    ab.asset = request.assetBundle;
                    cacheAssets[path] = ab;
                }
                ab.refCount++;
                if(callback != null) {
                    callback(ab);
                }
            }
        }

        public static void Delete(string path) {
            path = path.ToLower();
            if(cacheAssets.ContainsKey(path)) {
                AssetBundleItem ab = cacheAssets[path];
                ab.refCount--;
                if(ab.refCount <= 0) {
                    ab.asset.Unload(true);
                    cacheAssets.Remove(path);
                }

                if(ab.isHasDependence) {
                    //删除依赖
                    string[] dps = mainfest.GetAllDependencies(path);
                    for(int i = 0, len = dps.Length; i < len; i++) {
                        Delete(dps[i]);
                    }
                }
            }
        }

        private static StringBuilder getPathResult = new StringBuilder();
        private static string tmpPath = string.Empty;
        /// <summary>
        /// 资源同步加载路径（无 file:///）
        /// </summary>
        public static string GetAssetPath(string path) {
            // 先尝试从 persist 目录加载
            if(true) {
                getPathResult.Length = 0;
                getPathResult.Append(sandboxPath);
                getPathResult.Append("/");
                getPathResult.Append(path);
                tmpPath = getPathResult.ToString();
                if(File.Exists(tmpPath)) {
                    getPathResult.Length = 0;
                    return tmpPath;
                }
            }
            getPathResult.Length = 0;
            getPathResult.Append(streamingAssetsPath);
            getPathResult.Append("/");
            getPathResult.Append(path);
            tmpPath = getPathResult.ToString();
            return tmpPath;
        }

        /// <summary>
        /// 沙盒路径
        /// 可读可写，一般存放网上下载的资源
        /// </summary>
        public static string sandboxPath {
            get { return Application.persistentDataPath; }
        }

        /// <summary>
        /// StreamingAssets 路径
        /// </summary>
        public static string streamingAssetsPath {
            get {
#if UNITY_ANDROID
                return Application.dataPath + "!assets";   // 安卓平台
#else
                return Application.streamingAssetsPath;  // 其他平台
#endif
            }
            //get { return Application.streamingAssetsPath; }
        }
    }

    /// <summary>
    /// 存储单个AB资源信息
    /// </summary>
    public class AssetBundleItem {
        public string pathName;
        public string fileName;
        public AssetBundle asset;
        public int refCount;
        public bool isHasDependence;

        public AssetBundleItem(string path, string file, bool isHasDependence) {
            pathName = AssetBundleUtility.GetAssetPath(path);
            fileName = file;
            asset = null;
            refCount = 0;
            this.isHasDependence = isHasDependence;
        }

        public Object LoadAsset(System.Type type) {
            return LoadAsset(fileName, type);
        }

        public Object LoadAsset(string name, System.Type type) {
            if(asset != null) {
                return asset.LoadAsset(name, type);
            }
            return null;
        }
    }
}