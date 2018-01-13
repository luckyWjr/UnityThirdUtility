using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetBundle {
    public static class Builder {

        /// <summary>
        /// 打包AeestBundle和配置信息等到与Asset同级的AssetBundle目录下
        /// </summary>
        /// <returns></returns>
        public static IEnumerator Build() {
            yield return "开始打包...";

            // 1. 读取打包资源列表配置
            yield return "读取打包列表配置...";
            List<BaseAssetManager> managerList = BuildConfig.GetAssetManagerList();
            Debug.Log("managerList.count:"+ managerList.Count);
            if (managerList == null || managerList.Count < 1) {
                string str = "无任何要打包的资源，请检查配置是否正确。";
                yield return str;
                EditorUtility.DisplayDialog("错误", str, "确定");
                yield break;
            }

            StringBuilder sb = new StringBuilder("资源列表如下：\n");
            foreach (var c in managerList) {
                sb.AppendLine(c.ToString());
            }
            yield return sb.ToString();

            // 2. 读取上次打包结果
            yield return "读取上次打包结果...";
            string[] lastBuildConfig = null;
            if (File.Exists(BuildConfig.buildingListPath)) {
                lastBuildConfig = File.ReadAllLines(BuildConfig.buildingListPath);
                foreach (var c in managerList) {
                    foreach (var item in c.items) {
                        item.ReadConfig(lastBuildConfig);
                    }
                }
            } else {
                yield return "这是第一次打包。";
            }

            // 3. 计算资源的哈希值...
            yield return "计算资源的哈希值...";
            foreach (var c in managerList) {
                c.ComputeHash();
            }

            // 4. 与上次打包时资源哈希值比较...
            if (lastBuildConfig != null) {
                yield return "与上次打包时资源哈希值比较...";
            }
            bool needBuild = false;
            sb.Length = 0;
            sb.AppendLine("需要重新打包的资源如下：");
            foreach (var c in managerList) {
                if (c.isChange) {
                    needBuild = true;
                    sb.AppendLine(c.srcFolder);
                }
            }

            List<string> removedList;
            UpdateBuildingItemsFlag(managerList, lastBuildConfig, out removedList);

            // 5. 打包
            if (needBuild) {
                yield return sb.ToString();

                // 打包
                yield return "打包中..."+ BuildConfig.tempBuildingProductsFolder;

                // 先打到一个临时目录
                UnityBuild(BuildConfig.tempBuildingProductsFolder, managerList);

                // 检查是否所有的包都已正确生成
                bool succeed = IsAllAssetBuildSucceed(managerList, BuildConfig.buildingProductsFolder);
                if (!succeed) {
                    throw new InvalidOperationException("生成失败，未知原因");
                }

                yield return "打包完成！";

                // 打包完成事件
                foreach (var c in managerList) {
                    if (c.isChange) {
                        c.OnBuildFinished();
                    }
                }
            }

            sb.Length = 0;
            bool isVersionChanged = IsVersionChanged(managerList, removedList, ref sb);

            string buildResult = sb.ToString();
            yield return buildResult;

            // 6. 生成列表文件，版本文件，日志文件
            yield return "生成增量文件...";
            if (isVersionChanged) {
                SaveBuildingListFile(managerList, BuildConfig.tempBuildingListPath, ref sb);       // 供打包用的列表文件
                int version = UpdateVersion();
                WriteVersionFile(BuildConfig.tempBuildingVersionPath, version);                              // 供打包用的版本文件
            }

            // 供下载用的版本文件
            string strVersion = SaveLoadingVersionFile(BuildConfig.tempLoadingVersionPath);
            yield return "最新版本为： " + strVersion;

            // 日志文件
            if (isVersionChanged) {
                SaveLog(buildResult, strVersion, BuildConfig.tempBuildingLogPath);
            }

            // 供下载用的列表文件
            SaveLoadingListFile(managerList, BuildConfig.tempLoadingListPath);

            // 将打的包移动到对应目录
            CopyFiles(BuildConfig.tempBuildingRootFolder, BuildConfig.buildingRootFolder);

            // 删除移除的ab包
            DeleteRemovedAssetBundles(removedList);

            // dispose
            foreach (var c in managerList) {
                c.Dispose();
            }
        }

        static bool IsAllAssetBuildSucceed(List<BaseAssetManager> managerList, string folder) {
            bool succeed = true;
            bool exist;
            foreach (var manager in managerList) {
                foreach (var item in manager.items) {
                    if (item.flag != AssetFlag.NoChange) {
                        string path = string.Format("{0}/{1}{2}", BuildConfig.tempBuildingProductsFolder, item.outputRelativePath, item.ext);
                        exist = File.Exists(path);
                        if (!exist) {
                            succeed = false;
                            break;
                        }
                    }
                }
            }

            return succeed;
        }

        static void CopyFiles(string fromDir, string toDir) {
            string[] files = Directory.GetFiles(fromDir, "*.*", SearchOption.AllDirectories);
            int formDirLen = fromDir.Length;
            for (int i = 0, j = files.Length; i < j; i++) {
                string relativePath = files[i].Substring(formDirLen);
                string targetFile = string.Format("{0}{1}", toDir, relativePath);
                Uploader.CreateDirectory(targetFile);
                File.Copy(files[i], targetFile, true);
            }
        }

        static void UnityBuild(string folder, List<BaseAssetManager> managerList) {
            if (Directory.Exists(folder)) {
                Directory.Delete(folder, true);
            }
            Directory.CreateDirectory(folder);

            // 准备打包
            List<AssetBundleBuild> list = new List<AssetBundleBuild>();
            foreach (var manager in managerList) {
                if (manager.isChange) {
                    var noPackAssets = manager as NotBuildAssetManager;
                    if (noPackAssets != null) {
                        // 无需打包的资源
                        noPackAssets.Build(BuildConfig.tempBuildingProductsFolder);
                    } else {
                        manager.PrepareBuild();
                        list.AddRange(manager.assetBundleBuilds);
                    }
                }
            }
            AssetDatabase.Refresh();

            BuildPipeline.BuildAssetBundles(folder, list.ToArray(), BuildConfig.buildingOptions, BuildSetting.instance.selectedBuildTarget);
            // delete manifest files
            string[] manifestFiles = Directory.GetFiles(folder, "*.manifest", SearchOption.AllDirectories);
            foreach (var mf in manifestFiles) {
                File.Delete(mf);
            }
            string folderFile = string.Format("{0}/{1}", folder, Path.GetFileNameWithoutExtension(folder));
            File.Delete(folderFile);
        }

        static void UpdateBuildingItemsFlag(List<BaseAssetManager> managerList, string[] lastBuildConfig, out List<string> removedList) {
            foreach (var c in managerList) {
                foreach (var item in c.items) {
                    if (string.IsNullOrEmpty(item.lastHash)) {
                        item.flag = AssetFlag.NewAdded;   // 新增的
                    } else if (item.lastHash != item.currentHash) {
                        item.flag = AssetFlag.Modified;   // 修改的
                    } else {
                        item.flag = AssetFlag.NoChange;
                    }
                }
            }

            removedList = new List<string>();
            if (lastBuildConfig != null) {
                foreach (var line in lastBuildConfig) {
                    bool removed = true;
                    foreach (var c in managerList) {
                        foreach (var item in c.items) {
                            if (item.RightConfig(line)) {
                                removed = false;
                                break;
                            }
                        }
                    }

                    // 删除的
                    if (removed) {
                        string relativePath, hash;
                        BaseAsset.ParseConfigLine(line, out relativePath, out hash);
                        removedList.Add(relativePath);
                    }
                }
            }
        }

        static void DeleteRemovedAssetBundles(List<string> removedList) {
            for (int i = 0; i < removedList.Count; i++) {
                // 从磁盘中删除
                string fullPath = string.Format("{0}/{1}", BuildConfig.buildingProductsFolder.TrimEnd('/'), removedList[i]);
                if (Directory.Exists(Path.GetDirectoryName(fullPath)))
                    File.Delete(fullPath);
            }
        }

        static bool IsVersionChanged(List<BaseAssetManager> managerList, List<string> removedList, ref StringBuilder log) {
            log.Length = 0;

            List<string> addedList = new List<string>();
            List<string> modifiedList = new List<string>();

            foreach (var c in managerList) {
                foreach (var item in c.items) {
                    if (item.flag == AssetFlag.NewAdded)                // 新增的
                        addedList.Add(item.outputRelativePath);
                    else if (item.flag == AssetFlag.Modified)             // 修改的
                        modifiedList.Add(item.outputRelativePath);
                }
            }

            // log
            int totalCount = addedList.Count + modifiedList.Count + removedList.Count;
            if (totalCount < 1) {
                log.Append("无任何资源发生修改。");
                return false;
            }

            log.AppendLine(string.Format("共有{0}个包发生变化，详细如下：", totalCount));
            if (addedList.Count > 0) {
                log.AppendLine(string.Format("新增{0}个包：", addedList.Count));
                foreach (var item in addedList) {
                    log.AppendLine(item);
                }
            }

            if (modifiedList.Count > 0) {
                log.AppendLine(string.Format("有{0}个包发生修改：", modifiedList.Count));
                foreach (var item in modifiedList) {
                    log.AppendLine(item);
                }
            }

            if (removedList.Count > 0) {
                log.AppendLine(string.Format("移除了{0}个包：", removedList.Count));
                foreach (var item in removedList) {
                    log.AppendLine(item);
                }
            }

            return true;
        }

        static bool TryReadBuildingVersion(out int version) {
            string path = BuildConfig.tempBuildingVersionPath;
            if (File.Exists(path)) {
                string str = File.ReadAllText(path);
                if (int.TryParse(str, out version))
                    return true;
            }

            version = 0;
            return false;
        }

        static int UpdateVersion() {
            int version;
            TryReadBuildingVersion(out version);
            version++;
            return version;
        }

        static void WriteVersionFile(string path, int version) {
            File.WriteAllText(path, version.ToString());
        }

        static string SaveLoadingVersionFile(string path) {
            int version;
            TryReadBuildingVersion(out version);
            Uploader.CreateDirectory(path);
            File.WriteAllText(path, version.ToString());
            return version.ToString();
        }

        static void SaveLoadingListFile(List<BaseAssetManager> managerList, string path) {
            StringBuilder sb = new StringBuilder();

            // calc hash
            for (int i = 0; i < managerList.Count; i++) {
                for (int j = 0; j < managerList[i].items.Length; j++) {
                    var item = managerList[i].items[j];
                    string p;
                    if (item.flag == AssetFlag.NoChange)
                        p = string.Format("{0}/{1}{2}", BuildConfig.buildingProductsFolder, item.outputRelativePath, item.ext);
                    else
                        p = string.Format("{0}/{1}{2}", BuildConfig.tempBuildingProductsFolder, item.outputRelativePath, item.ext);

                    int startIndex = BuildConfig.buildingProductsFolder.Length + 1;
                    string name = p.Substring(startIndex);
                    byte[] bytes = File.ReadAllBytes(p);
                    string hash = BaseAsset.ComputeHash(bytes);

                    sb.AppendFormat("{0},{1},{2};", name, hash, bytes.Length);
                }
            }

            File.WriteAllText(path, sb.ToString());
        }

        static void SaveBuildingListFile(List<BaseAssetManager> managerList, string path, ref StringBuilder sb) {
            sb.Length = 0;
            foreach (var c in managerList) {
                foreach (var item in c.items) {
                    string config = item.GenerateConfigLine();
                    sb.AppendLine(config);
                }
            }
            Uploader.CreateDirectory(path);
            File.WriteAllText(path, sb.ToString());
        }

        static void SaveLog(string log, string version, string path) {
            if (string.IsNullOrEmpty(log))
                return;

            string oldLog = "";
            if (File.Exists(path))
                oldLog = File.ReadAllText(path);
            string currentLog = string.Format("打包时间：{0}\n版本：{1}\n{2}\n--------------------------------\n\n{3}", DateTime.Now, version, log, oldLog);
            File.WriteAllText(path, currentLog);
        }

    }
}
