using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using Utility;

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
            List<string> lastBuildConfigList = null;
            if (File.Exists(BuildConfig.buildingListPath)) {
                string[] lastBuildConfig = File.ReadAllLines(BuildConfig.buildingListPath);
                if(lastBuildConfig != null && lastBuildConfig.Length > 0) {
                    lastBuildConfigList = new List<string>(lastBuildConfig);
                    int findIndex;
                    foreach(var manager in managerList) {
                        foreach(var item in manager.items) {
                            findIndex = item.ReadConfig(lastBuildConfigList);
                            //找到对应文件下标则删除该列数据
                            if(findIndex >= 0) {
                                lastBuildConfigList.RemoveAt(findIndex);
                            }
                        }
                    }
                }
            } else {
                yield return "这是第一次打包。";
            }

            // 3. 计算资源的Md5...
            yield return "计算资源的Md5...";
            foreach (var manager in managerList) {
                manager.ComputeMd5();
            }

            
            bool needBuild = false;
            sb.Length = 0;
            sb.AppendLine("需要重新打包的资源如下：");
            foreach (var manager in managerList) {
                if (manager.isChange) {
                    needBuild = true;
                    sb.AppendLine(manager.assetFolderPath);
                }
            }

            //lastBuildConfigList还未删除的数据说明已经没有这些资源需要打包，即要删掉对应的AB包
            List<string> removedList = GetFileNeedRemoveArray(lastBuildConfigList);

            // 4. 打包
            if (needBuild) {

                UpdateBuildingItemsFlag(managerList);

                yield return sb.ToString();

                // 打包
                yield return "打包中..."+ BuildConfig.tempbuildingAssetBundlesFolder;

                // 先打到一个临时目录
                UnityBuild(BuildConfig.tempbuildingAssetBundlesFolder, managerList);

                // 检查是否所有的包都已正确生成
                bool succeed = IsAllAssetBuildSucceed(managerList, BuildConfig.buildingAssetBundlesFolder);
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
                SaveBuildingListFile(managerList, BuildConfig.tempBuildingListPath, ref sb);        // 供打包用的列表文件
                int version = UpdateVersion();
                WriteVersionFile(BuildConfig.tempBuildingVersionPath, version);                     // 供打包用的版本文件
            }

            // 供下载用的版本文件（存本地版本号，用于和服务器上最新版本号对比）
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
            foreach (var manager in managerList) {
                manager.Dispose();
            }
        }

        static bool IsAllAssetBuildSucceed(List<BaseAssetManager> managerList, string folder) {
            bool succeed = true;
            bool exist;
            string path;
            foreach (var manager in managerList) {
                foreach (var item in manager.items) {
                    if (item.flag != AssetFlag.NoChange) {
                        path = string.Format("{0}/{1}", BuildConfig.tempbuildingAssetBundlesFolder, item.assetBundleName);
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
            string relativePath, targetFile;
            for (int i = 0, j = files.Length; i < j; i++) {
                relativePath = files[i].Substring(formDirLen);
                targetFile = string.Format("{0}{1}", toDir, relativePath);
                Uploader.CreateDirectory(targetFile);
                File.Copy(files[i], targetFile, true);
            }
        }

        static void UnityBuild(string folder, List<BaseAssetManager> managerList) {
            if(Directory.Exists(folder)) {
                Directory.Delete(folder, true);
            }
            Directory.CreateDirectory(folder);

            // 准备打包
            List<AssetBundleBuild> list = new List<AssetBundleBuild>();
            foreach(var manager in managerList) {
                if(manager.isChange) {
                    var noPackAssets = manager as NotBuildAssetManager;
                    if(noPackAssets != null) {
                        // 无需打包的资源
                        noPackAssets.Build(BuildConfig.tempbuildingAssetBundlesFolder);
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
            foreach(var mf in manifestFiles) {
                File.Delete(mf);
            }
            //string folderFile = string.Format("{0}/{1}", folder, Path.GetFileNameWithoutExtension(folder));
            //File.Delete(folderFile);
        }

        /// <summary>
        /// 更新每个Asset资源的状态
        /// </summary>
        /// <param name="managerList">需要打包的Asset</param>
        static void UpdateBuildingItemsFlag(List<BaseAssetManager> managerList) {
            foreach (var manager in managerList) {
                foreach (var item in manager.items) {
                    if (string.IsNullOrEmpty(item.lastMd5)) {
                        item.flag = AssetFlag.NewAdded;   // 新增的
                    } else if (item.lastMd5 != item.currentMd5) {
                        item.flag = AssetFlag.Modified;   // 修改的
                    } else {
                        item.flag = AssetFlag.NoChange;
                    }
                }
            }
        }

        /// <summary>
        /// 获取需要删除的AB列表
        /// </summary>
        /// <param name="lastBuildConfig">剩下的表数据</param>
        /// <returns>要删除的AB文件路径数组</returns>
        static List<string> GetFileNeedRemoveArray(List<string> lastBuildConfig) {
            List<string> removedList = new List<string>();
            string relativePath, hash;
            if(lastBuildConfig != null) {
                foreach(var line in lastBuildConfig) {
                    BaseAsset.ParseConfigLine(line, out relativePath, out hash);
                    removedList.Add(relativePath);
                }
            }
            return removedList;
        }

        static void DeleteRemovedAssetBundles(List<string> removedList) {
            for (int i = 0; i < removedList.Count; i++) {
                // 从磁盘中删除
                string fullPath = string.Format("{0}/{1}", BuildConfig.buildingAssetBundlesFolder.TrimEnd('/'), removedList[i]);
                if (Directory.Exists(Path.GetDirectoryName(fullPath)))
                    File.Delete(fullPath);
            }
        }

        static bool IsVersionChanged(List<BaseAssetManager> managerList, List<string> removedList, ref StringBuilder log) {
            log.Length = 0;

            List<string> addedList = new List<string>();
            List<string> modifiedList = new List<string>();

            foreach (var manager in managerList) {
                foreach (var item in manager.items) {
                    if(item.flag == AssetFlag.NewAdded) {
                        addedList.Add(item.assetBundleName);
                    }            
                    else if (item.flag == AssetFlag.Modified) {
                        modifiedList.Add(item.assetBundleName);
                    }  
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

            string itemPath, name, md5;
            int startIndex;
            byte[] bytes;

            for (int i = 0, j = 0; i < managerList.Count; i++) {
                for (j = 0; j < managerList[i].items.Length; j++) {
                    var item = managerList[i].items[j];
                    
                    if (item.flag == AssetFlag.NoChange) {
                        itemPath = string.Format("{0}/{1}", BuildConfig.buildingAssetBundlesFolder, item.assetBundleName);
                    } else {
                        itemPath = string.Format("{0}/{1}", BuildConfig.tempbuildingAssetBundlesFolder, item.assetBundleName);
                    }

                    startIndex = BuildConfig.buildingAssetBundlesFolder.Length + 1;
                    name = itemPath.Substring(startIndex);
                    bytes = null;
                    bytes = File.ReadAllBytes(itemPath);
                    md5 = TypeConvertUtility.ByteToMd5(bytes);

                    sb.AppendFormat("{0},{1},{2};", name, md5, bytes.Length);
                }
            }

            File.WriteAllText(path, sb.ToString());
        }

        static void SaveBuildingListFile(List<BaseAssetManager> managerList, string path, ref StringBuilder sb) {
            sb.Length = 0;
            foreach (var manager in managerList) {
                foreach (var item in manager.items) {
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
