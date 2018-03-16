using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace AssetBundle {
    public class BuildAssetBundle : EditorWindow {
        Vector2 scrollPoint;
        List<string> m_log = new List<string>();

        [MenuItem("BuildTool/AssetBundle/BuildAll %A")]
        static void Create() {
            GetWindow<BuildAssetBundle>("打包工具");
        }

        void OnGUI() {
            scrollPoint = GUILayout.BeginScrollView(scrollPoint);

            GUILayout.Label("设置：");
            BuildSetting.instance.selectedBuildTarget = (BuildTarget)EditorGUILayout.EnumPopup("平台：", BuildSetting.instance.selectedBuildTarget, GUILayout.Width(320));

            BuildSetting.instance.isBuild = GUILayout.Toggle(BuildSetting.instance.isBuild, "打包");
            if (BuildSetting.instance.isBuild) {
                BuildSetting.instance.isForceRebuildAll = GUILayout.Toggle(BuildSetting.instance.isForceRebuildAll, "强制全部重新打包（否则为增量打包）");
            }

            BuildSetting.instance.isExportToMainProject = GUILayout.Toggle(BuildSetting.instance.isExportToMainProject, "导出到主项目的 StreamingAssets");
            if (BuildSetting.instance.isExportToMainProject) {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("...", GUILayout.Width(30))) {
                    BuildSetting.instance.streamingFolder = EditorUtility.SaveFolderPanel("选择主项目的StreamingAssets目录", BuildSetting.instance.streamingFolder, BuildSetting.instance.streamingFolder);
                }
                BuildSetting.instance.streamingFolder = GUILayout.TextField(BuildSetting.instance.streamingFolder, GUILayout.Width(500));
                if (!BuildSetting.instance.streamingFolder.StartsWith(Application.dataPath)) {
                    GUILayout.Label("!此目录不在本项目");
                }
                GUILayout.EndHorizontal();
            }
            BuildSetting.instance.isUploadToFtp = GUILayout.Toggle(BuildSetting.instance.isUploadToFtp, "导出到ftp服务器");
            if (BuildSetting.instance.isUploadToFtp) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(50);
                GUILayout.Label("ftp 地址: ", GUILayout.Width(80));
                BuildSetting.instance.ftpUri = GUILayout.TextField(BuildSetting.instance.ftpUri, GUILayout.Width(300));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(50);
                GUILayout.Label("ftp 用户名:", GUILayout.Width(80));
                BuildSetting.instance.ftpUserName = GUILayout.TextField(BuildSetting.instance.ftpUserName, GUILayout.Width(100));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(50);
                GUILayout.Label("ftp 密码:", GUILayout.Width(80));
                BuildSetting.instance.ftpPassword = GUILayout.TextField(BuildSetting.instance.ftpPassword, GUILayout.Width(100));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(50);
                BuildSetting.instance.isFtpPassive = GUILayout.Toggle(BuildSetting.instance.isFtpPassive, " 被动模式");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(50);
                BuildSetting.instance.isFtpUploadAll = GUILayout.Toggle(BuildSetting.instance.isFtpUploadAll, " 上传所有的ab包，否则为增量上传");
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("执行", GUILayout.Width(200))) {
                Build();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("查看历史打包记录", GUILayout.Width(200))) {
                //m_mainThread = WatchLog();
            }

            if (m_log.Count > 0) {
                GUILayout.Space(10);
                if (GUILayout.Button("清除日志", GUILayout.Width(200))) {
                    m_log.Clear();
                }

                for (int i = 0; i < m_log.Count; i++) {
                    GUILayout.Label((i + 1) + ": " + m_log[i]);
                }
            }

            GUILayout.EndScrollView();
        }

        void Build() {
            IEnumerator etor = Execute(true);
            while (etor.MoveNext()) {
                // building...
                Debug.Log(etor.Current);
                AddLog((string)etor.Current);
            }
        }

        static IEnumerator Execute(bool showDialog) {
            if (showDialog) {
                //启用对话框，代码会停顿在此，等点击了才往下执行
                if (!EditorUtility.DisplayDialog("一键打包工具", "确定要执行吗？", "确定", "取消")) {
                    yield break;
                }
            }

            int count = 0;

            if (BuildSetting.instance.isExportToMainProject && (string.IsNullOrEmpty(BuildSetting.instance.streamingFolder)))// || !arg.StreamingFolder.EndsWith("StreamingAssets")))
            {
                if (showDialog) {
                    EditorUtility.DisplayDialog("错误", "导出目标项目StreamingAssets目录设置不正确...", "确定");
                }
                yield break;
            }

            // 计时
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            #region 流程

            // 打包
            if (BuildSetting.instance.isBuild) {
                count++;

                if (BuildSetting.instance.isForceRebuildAll && Directory.Exists(BuildConfig.buildingRootFolder)) {
                    Directory.Delete(BuildConfig.buildingRootFolder, true);
                }

                IEnumerator buildEtor = Builder.Build();
                while (buildEtor.MoveNext()) {
                    yield return buildEtor.Current;
                }
            }

            // 导出到 streaming asset
            if (BuildSetting.instance.isExportToMainProject) {
                count++;

                Uploader.UploadToStreamingAssetsFolder(BuildSetting.instance.streamingFolder);
                yield return "导出到主项目的 StreamingAssets 目录完毕。";

                AssetDatabase.Refresh();
                yield return "刷新项目资源...";
            }

            // 导出到 ftp 服务器
            if (BuildSetting.instance.isUploadToFtp) {
                count++;

                string ftpRelativePath = "AssetBundle/" + BuildConfig.platformFolderName;
                string srcFolder = BuildSetting.instance.isFtpUploadAll ? BuildConfig.buildingAssetBundlesFolder : BuildConfig.tempbuildingAssetBundlesFolder;
                Uploader.UploadToFtp(BuildSetting.instance.ftpUri, srcFolder, ftpRelativePath, BuildSetting.instance.ftpUserName, BuildSetting.instance.ftpPassword, BuildSetting.instance.isFtpPassive);
                yield return "已使用控制台程序上传ftp...";
            }

            #endregion

            BuildSetting.instance.Save();

            stopwatch.Stop();

            int totalSeconds = (int)(stopwatch.ElapsedMilliseconds / 1000f);
            int minutes = Mathf.FloorToInt(totalSeconds / 60f);
            int seconds = totalSeconds % 60;
            string dialog = count > 0 ? string.Format("执行结束！耗时： {0} 分 {1} 秒。", minutes, seconds) : "什么也没发生...";
            yield return dialog;

            // 弹出结果框
            if (showDialog) {
                EditorUtility.DisplayDialog("完成", dialog, "确定");
            }
        }

        void AddLog(string log) {
            if (!string.IsNullOrEmpty(log)) {
                int maxLen = 1000;
                log = log.Length > maxLen ? log.Substring(0, maxLen) + "..." : log;
                m_log.Add(log);
                base.Repaint();
            }
        }
    }
}
