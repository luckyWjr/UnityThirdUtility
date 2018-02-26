#define MainProject
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AssetBundle {
    /// <summary>
    /// 编AssetBundle的一些配置信息
    /// </summary>
    public static class BuildConfig {

        public static string clothesFolder = "Assets/_Temp";
        public static string clothesFBXFolder = "Assets/FBX/Character/Models";

        static string m_unityBaseFolder = "Assets/";
        public static string unityBaseFolder { get { return m_unityBaseFolder; } }

        public enum Project {
            Main,
            Music,
            Art,
        }

        static BuildConfig() {
            
        }

        public static List<BaseAssetManager> GetAssetManagerList() {
            IEnumerable<BaseAssetManager> config = null;

#if MainProject
            config = GetMainProjectConfig();
#elif MusicProject
            config = GetMusicProjConfig();
#elif ArtProject
            config = GetArtProjConfig();
#endif

            return config != null ? config.ToList() : null;
        }

#if MainProject
        static IEnumerable<BaseAssetManager> GetMainProjectConfig() {
            //string localizedAtlasFolder = "Assets/UI/Localize/Atlas";
            yield return new NormalAssetManager<UnityEngine.Object>(m_unityBaseFolder + "Res/Test", "t:Prefab", "Test");                                                 // common atlas
            yield return new NormalAssetManager<Texture>(m_unityBaseFolder+ "Res/Image", "t:Texture2D", "Image");                                                                                                                                  //yield return new UIPrefabs("Assets/UI/Prefab", "UI", localizedAtlasFolder);                                                         // ui prefab
                                                                                                                                                                                                                                                     //yield return new LuaCodes("Assets/Scripts/XLua/LuaScripts", "Lua");                                                                 // lua
            //yield return new NotBuildAssetManager("Assets/Config/Data", "f:*.dat", "Data");                                                             // data
        }
#endif

#if MusicProject
                static IEnumerable<AssetCategory> GetMusicProjConfig()
                {
                    yield return new AnimationClips("Assets/FBX/DanceAnimation", "Animation/Dance");                                                            // dance animation
                    yield return new NormalAssets<AudioClip>("Assets/Resources/Sound", "t:AudioClip", "Audio/Effect");                                          // sound
                    yield return new NormalAssets<AudioClip>("Assets/Resources/Music", @"r:^[1-9]\d{4,}\D+\.mp3$", "Audio/Music");                              // sound
                    yield return new NormalAssets<AudioClip>("Assets/Resources/BGM", "t:AudioClip", "Audio/Music");
                    yield return new NormalAssets<Dance.DanceAnim.DanceAnimData>("Assets/DanceAnimData", "f:*.asset", "AssetData/DanceAnim/{0}_asset");         // dance anim data, asset文件
                    yield return new MusicDataAssets("Assets/MusicDanceData", "f:*.asset", "AssetData/Music/{0}_asset");                                        // dance anim data, asset文件
                    yield return new NoPackAssets("Assets/Resources/AndroidEffect", "t:AudioClip", "Audio/AndroidEffect");                                      // audio for android
                    yield return new NormalAssets<GameObject>("Assets/TimelineData/Cinemachine", "t:Prefab", "timeline/cinemachine");               // timeline cav    
                    yield return new NormalAssets<TimelineAsset>("Assets/TimelineData/Timeline", "t:TimelineAsset", "timeline/timeline");               // timeline 
                }
#endif

#if ArtProject
                static IEnumerable<AssetCategory> GetArtProjConfig()
                {
                    yield return new NormalAssets<GameObject>("Assets/Resources/Build Asset/Model/Character", "t:Prefab", "Model/Character");       // character
                    yield return new NormalAssets<Material>("Assets/Resources/Build Asset/Model/Material", "t:Material", "Model/Material");         // material
                    yield return new NormalAssets<GameObject>("Assets/Resources/Build Asset/Model/Spirit", "t:Prefab", "Model/Spirit");             // spirit
                    yield return new NormalAssets<GameObject>("Assets/Resources/Build Asset/Effect/Scene", "t:Prefab", "Effect/Scene");             // scene effect
                    yield return new NormalAssets<GameObject>("Assets/Resources/Build Asset/Effect/UI", "t:Prefab", "Effect/UI");                   // ui effect
                    yield return new NormalAssets<GameObject>("Assets/Resources/Build Asset/Model/Other", "t:Prefab", "model/other");               // cat
                    yield return new NormalAssets<SceneAsset>("Assets/Resources/Build Asset/Scene", "t:Scene", "Scene");

                    yield return new AnimationClips("Assets/FBX/Character/Animations/Common/male", "Animation/Male");                               // male anim
                    yield return new AnimationClips("Assets/FBX/Character/Animations/Ride/male", "Animation/Male");
                    yield return new AnimationClips("Assets/FBX/Character/Animations/Card/male", "Animation/Male");
                    yield return new AnimationClips("Assets/FBX/Character/Animations/Common/female", "Animation/Female");                           // female anim
                    yield return new AnimationClips("Assets/FBX/Character/Animations/Ride/female", "Animation/Female");
                    yield return new AnimationClips("Assets/FBX/Character/Animations/Card/female", "Animation/Female");

                    yield return new ModelStages("Assets/Resources/Build Asset/Model/Stage", "t:Prefab", "Model/Stage");                            // stage
                    yield return new Clothes("Assets/FBX/Character/Models", ClothesFolder, "f:*.*", "Model/Cloth");        // cloth
                    yield return new ParticleSystemAtlases("atlases");                                                                              // cloth effect atlases
                    yield return new ParticleSystemObjs("Assets/Resources/Build Asset/Effect/Cloth", "t:Prefab", "Effect/Cloth");                   // cloth effect
                    yield return new ParticleSystemObjs("Assets/Resources/Build Asset/Model/Equip", "t:Prefab", "Model/Equip");                     // equip
                }
#endif

        /// <summary>
        /// 同步工具代码的工具
        /// </summary>
        /// <param name="toolRelativeFolder">如 "Editor/AssetBundle" </param>
        /// <param name="target">如 "C:/Project"</param>
        public static string SyncToolTo(string toolRelativeFolder, string target) {
            if (string.IsNullOrEmpty(toolRelativeFolder)) {
                throw new ArgumentException("string.IsNullOrEmpty(toolRelativeFolder)");
            }

            if (string.IsNullOrEmpty(target)) {
                throw new ArgumentException("string.IsNullOrEmpty(target)");
            }

            toolRelativeFolder = Uploader.FixPath(toolRelativeFolder).TrimStart('/').TrimEnd('/');
            target = Uploader.FixPath(target).TrimEnd('/');

            string assetName = "/Assets";
            if (target.EndsWith(assetName)) {
                target = target.Substring(0, target.Length - assetName.Length);
            }

            string toolFolder = string.Format("{0}/{1}", Application.dataPath, toolRelativeFolder);
            string targetFolder = string.Format("{0}{1}/{2}", target, assetName, toolRelativeFolder);

            // delete old
            if (Directory.Exists(targetFolder)) {
                Directory.Delete(targetFolder, true);
            }

            // 复制代码到其它项目
            string[] csFiles = Directory.GetFiles(toolFolder, "*.*", SearchOption.AllDirectories);
            foreach (var cs in csFiles) {
                string csPath = cs.Replace('\\', '/');
                string targetPath = csPath.Replace(toolFolder, targetFolder);
                string dirName = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);

                File.Copy(csPath, targetPath, true);
            }

            return targetFolder;
        }

        public static void SyncToolTo(string target, Project project) {
            string targetFolder = SyncToolTo("Editor/AssetBundle", target);

            // 打开对应的宏
            string configPath = string.Format("{0}/{1}.cs", targetFolder, typeof(BuildConfig).Name);
            string clothesCodePath = string.Format("{0}/OriginalAssets/Category/Clothes.cs", targetFolder);
            string clothCodePath = string.Format("{0}/OriginalAssets/Item/ClothAsset.cs", targetFolder);
            string uiPrefabsCodePath = string.Format("{0}/OriginalAssets/Category/UIPrefabs.cs", targetFolder);
            string uiPrefabCodePath = string.Format("{0}/OriginalAssets/Item/UIPrefab.cs", targetFolder);
            string clothGeneratorCodePath = string.Format("{0}/ClothesGenerator.cs", targetFolder);
            string psAtlasesCodePath = string.Format("{0}/OriginalAssets/Category/ParticleSystemAtlases.cs", targetFolder);
            string psAtlasCodePath = string.Format("{0}/OriginalAssets/Item/ParticleSystemAtlas.cs", targetFolder);
            string psObjsCodePath = string.Format("{0}/OriginalAssets/Category/ParticleSystemObjs.cs", targetFolder);
            string psObjCodePath = string.Format("{0}/OriginalAssets/Item/ParticleSystemObj.cs", targetFolder);
            string psCombinerCodePath = string.Format("{0}/ParticleSystemCombiner.cs", targetFolder);
            string musicDataCodePath = string.Format("{0}/OriginalAssets/Category/MusicDataAssets.cs", targetFolder);

            WriteMacros(targetFolder, configPath, project);
            WriteMacros(targetFolder, clothesCodePath, project);
            WriteMacros(targetFolder, clothCodePath, project);
            WriteMacros(targetFolder, uiPrefabsCodePath, project);
            WriteMacros(targetFolder, uiPrefabCodePath, project);
            WriteMacros(targetFolder, clothGeneratorCodePath, project);
            WriteMacros(targetFolder, psAtlasesCodePath, project);
            WriteMacros(targetFolder, psAtlasCodePath, project);
            WriteMacros(targetFolder, psObjsCodePath, project);
            WriteMacros(targetFolder, psObjCodePath, project);
            WriteMacros(targetFolder, psCombinerCodePath, project);
            WriteMacros(targetFolder, musicDataCodePath, project);

            EditorUtility.DisplayDialog("完成", string.Format("同步完成！目标项目：{0}，类型：{1}", target, project), "确定");
        }

        static void WriteMacros(string targetFolder, string configPath, Project project) {
            List<string> lines = File.ReadAllLines(configPath).ToList();
            List<int> oldMacrosIndexs = new List<int>();

            for (int i = lines.Count - 1; i >= 0; i--) {
                if (lines[i].StartsWith("#define ")) {
                    oldMacrosIndexs.Add(i);
                }
            }

            foreach (var index in oldMacrosIndexs) {
                lines.RemoveAt(index);
            }

            string macrosName;
            switch (project) {
                case Project.Main:
                    macrosName = "MainProject";
                    break;
                case Project.Music:
                    macrosName = "MusicProject";
                    break;
                case Project.Art:
                    macrosName = "ArtProject";
                    break;
                default:
                    throw new InvalidOperationException("Unknown project: " + project);
            }
            string macros = string.Format("#define {0}", macrosName);

            lines.Insert(0, macros);

            File.WriteAllLines(configPath, lines.ToArray());
        }

        public static string GetLoadingVersionName(Project project) {
            string name;
            switch (project) {
                case Project.Main:
                name = "mainversion";
                break;
                case Project.Music:
                name = "musicversion";
                break;
                case Project.Art:
                name = "artversion";
                break;
                default:
                throw new InvalidOperationException();
            }
            return name + ".dat";
        }

        public static Project currentProject {
            get {
                Project p;
#if MainProject
                        p = Project.Main;
#elif MusicProject
                        p = Project.Music;
#elif ArtProject
                        p = Project.Art;
#endif

                return p;
            }
        }

        public static string buildingRootFolder {
            get {
                string folder = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
                string path = string.Format("{0}/AssetBundles/{1}", folder, platformFolderName);
                return path.Replace("\\", "/");
            }
        }

        public static string buildingProductsFolder {
            get { return buildingRootFolder + "/Products"; }
        }

        public static string buildingVersionPath {
            get { return buildingRootFolder + "/version.txt"; }
        }

        public static string buildingListPath {
            get { return buildingRootFolder + "/list.csv"; }
        }

        public static string buildingLogPath {
            get { return buildingRootFolder + "/log.txt"; }
        }

        public static string loadingVersionPath {
            get {
                string name = GetLoadingVersionName(currentProject);
                return string.Format("{0}/{1}", buildingProductsFolder, name);
            }
        }

        public static string loadingListPath {
            get { return string.Format("{0}/{1}", buildingProductsFolder, GetLoadingListName(currentProject)); }
        }

#region temp

        public static string tempBuildingRootFolder {
            get {
                string folder = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
                string path = string.Format("{0}/AssetBundles/{1}_temp", folder, platformFolderName);
                return path.Replace("\\", "/");
            }
        }

        public static string tempBuildingProductsFolder {
            get { return tempBuildingRootFolder + "/Products"; }
        }

        public static string tempBuildingVersionPath {
            get { return tempBuildingRootFolder + "/version.txt"; }
        }

        public static string tempBuildingListPath {
            get { return tempBuildingRootFolder + "/list.csv"; }
        }

        public static string tempBuildingLogPath {
            get { return tempBuildingRootFolder + "/log.txt"; }
        }

        public static string tempLoadingVersionPath {
            get {
                string name = GetLoadingVersionName(currentProject);
                return string.Format("{0}/{1}", tempBuildingProductsFolder, name);
            }
        }

        public static string tempLoadingListPath {
            get { return string.Format("{0}/{1}", tempBuildingProductsFolder, GetLoadingListName(currentProject)); }
        }

#endregion

        public static string finalLoadingVersionName {
            get { return "version.dat"; }
        }

        public static string GetLoadingListName(Project project) {
            string name;
            switch (project) {
                case Project.Main:
                name = "mainlist";
                break;
                case Project.Music:
                name = "musiclist";
                break;
                case Project.Art:
                name = "artlist";
                break;
                default:
                throw new InvalidOperationException();
            }

            return name + ".dat";
        }

        public static string platformFolderName {
            get {
                string folder;
                switch (BuildSetting.instance.selectedBuildTarget) {
                    case BuildTarget.Android:
                    folder = "Android";
                    break;
                    case BuildTarget.iOS:
                    folder = "IOS";
                    break;
                    default:
                    folder = "PC";
                    break;
                }

                return folder;
            }
        }

        public static BuildAssetBundleOptions buildingOptions {
            get {
                BuildAssetBundleOptions options;

                switch (currentProject) {
                    case Project.Main:
                    options = BuildAssetBundleOptions.ChunkBasedCompression;
                    break;

                    case Project.Music:
                    case Project.Art:
                    options = BuildAssetBundleOptions.None;
                    break;

                    default:
                    throw new InvalidOperationException();
                }

                return options;
            }
        }
    }
}
