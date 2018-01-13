using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace AssetBundle {
	public static class Uploader {

        public static string FixPath(this string old) {
            return !string.IsNullOrEmpty(old) ? old.Replace("\\", "/") : old;
        }

        public static void CreateDirectory(string filePath) {
            if (!string.IsNullOrEmpty(filePath)) {
                string dirName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dirName)) {
                    Directory.CreateDirectory(dirName);
                }
            }
        }

        public static void UploadToStreamingAssetsFolder(string tarFolder) {
            if (string.IsNullOrEmpty(tarFolder)) {
                throw new ArgumentException("tarFolder");
            }

            tarFolder = FixPath(tarFolder).TrimEnd('/');

            string srcFolder = BuildConfig.buildingProductsFolder;

            // 先删除目标文件夹里的包
            string[] srcSubFolders = Directory.GetDirectories(srcFolder, "*", SearchOption.AllDirectories);
            foreach (var subFolder in srcSubFolders) {
                if (subFolder.Contains(".svn")) {
                    continue;
                }

                string[] childFolders = Directory.GetDirectories(subFolder, "*", SearchOption.AllDirectories);
                if (childFolders.Length <= 0) {
                    string sf = subFolder.FixPath();
                    string relativePath = sf.Substring(srcFolder.Length);
                    string tarSubFolder = string.Format("{0}{1}", tarFolder, relativePath);
                    if (Directory.Exists(tarSubFolder)) {
                        Directory.Delete(tarSubFolder, true);
                    }
                }
            }

            // top 目录里的文件
            string[] topFiles = Directory.GetFiles(srcFolder, "*.*", SearchOption.AllDirectories);
            foreach (var topFile in topFiles) {
                if (topFile.Contains(".svn")) {
                    continue;
                }

                string tf = topFile.FixPath();
                string targetPath = tf.Replace(srcFolder, tarFolder);

                CreateDirectory(targetPath);
                File.Copy(tf, targetPath, true);
            }

            // 整合version文件
            CreateVersionFile(tarFolder);
        }

        static void CreateVersionFile(string tarFolder) {
            string versionPath = string.Format("{0}/{1}", tarFolder, BuildConfig.finalLoadingVersionName);
            int mainVersion = 0, musicVersion = 0, artVersion = 0;
            if (File.Exists(versionPath)) {
                string versionString = File.ReadAllLines(versionPath)[0];
                string[] versionLines = versionString.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                mainVersion = int.Parse(versionLines[0]);
                musicVersion = int.Parse(versionLines[1]);
                artVersion = int.Parse(versionLines[2]);
            }

            string mainVersionPath = string.Format("{0}/{1}", tarFolder, BuildConfig.GetLoadingVersionName(BuildConfig.Project.Main));
            string musicVersionPath = string.Format("{0}/{1}", tarFolder, BuildConfig.GetLoadingVersionName(BuildConfig.Project.Music));
            string artVersionPath = string.Format("{0}/{1}", tarFolder, BuildConfig.GetLoadingVersionName(BuildConfig.Project.Art));

            if (File.Exists(mainVersionPath)) {
                string mainVersionString = File.ReadAllText(mainVersionPath);
                mainVersion = int.Parse(mainVersionString);
                File.Delete(mainVersionPath);
            }

            if (File.Exists(musicVersionPath)) {
                string musicVersionString = File.ReadAllText(musicVersionPath);
                musicVersion = int.Parse(musicVersionString);
                File.Delete(musicVersionPath);
            }

            if (File.Exists(artVersionPath)) {
                string artVersionString = File.ReadAllText(artVersionPath);
                artVersion = int.Parse(artVersionString);
                File.Delete(artVersionPath);
            }

            string finalVersionString = string.Format("{0}.{1}.{2}\n{3},{4},{5}", mainVersion, musicVersion, artVersion,
                BuildConfig.GetLoadingListName(BuildConfig.Project.Main),
                BuildConfig.GetLoadingListName(BuildConfig.Project.Music),
                BuildConfig.GetLoadingListName(BuildConfig.Project.Art));
            CreateDirectory(versionPath);
            File.WriteAllText(versionPath, finalVersionString);
        }

        public static void UploadToFtp(string uri, string srcFolder, string tarFolder, string userName, string password, bool usePassive) {
#if UNITY_EDITOR_OSX
            MacUploadToFtp(uri, srcFolder, tarFolder, userName, password, usePassive);
#else
            WinUploadToFtp(uri, srcFolder, tarFolder, userName, password, usePassive);
#endif
        }

        static void WinUploadToFtp(string uri, string srcFolder, string tarFolder, string userName, string password, bool usePassive) {
            StringBuilder sb = new StringBuilder();
            sb.Append(uri);
            sb.Append(",");
            sb.Append(userName);
            sb.Append(",");
            sb.Append(password);
            sb.Append(",");
            sb.Append(usePassive.ToString());
            sb.Append(",");
            sb.Append(true.ToString());
            sb.Append(",");
            sb.Append(srcFolder);
            sb.Append(",");
            sb.Append(tarFolder);
            sb.Append(",");

            Process process = new Process();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            process.StartInfo.FileName = string.Format("{0}/Editor/AssetBundle/Tools/FtpUploader.exe", Application.dataPath);
            process.StartInfo.Arguments = sb.ToString();
            process.Start();
        }

        static void MacUploadToFtp(string uri, string srcFolder, string tarFolder, string userName, string password, bool usePassive) {
            // arguments
            StringBuilder sb = new StringBuilder();
            sb.Append(uri);
            sb.Append(",");
            sb.Append(tarFolder);
            sb.Append(",");
            sb.Append(userName);
            sb.Append(",");
            sb.Append(password);
            sb.Append(",");
            sb.Append(usePassive.ToString());
            sb.Append(",");
            sb.Append(srcFolder);
            sb.Append(",");

            // save arguments to file
            string rootPath = string.Format("{0}/Editor/AssetBundle/Tools/", Application.dataPath.TrimEnd('/'));
            string tempFilePath = rootPath + "MacFtpUploader_Arguments";
            File.WriteAllText(tempFilePath, sb.ToString());

            // invoke tool
            if (File.Exists(tempFilePath)) {
                string toolPath = rootPath + "MacFtpUploader";
                string shell = string.Format("{0}MacFtpUploaderInvoker.sh {1}", rootPath, toolPath);
                Process.Start("/bin/bash", shell);
            } else {
                throw new InvalidOperationException(string.Format("Save arguments to file {0} failed, reupload again please...", tempFilePath));
            }
        }
    }
}
