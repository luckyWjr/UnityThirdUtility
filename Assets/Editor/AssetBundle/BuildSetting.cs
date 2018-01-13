using UnityEngine;
using Utility;
using UnityEditor;

namespace AssetBundle {
	public class BuildSetting : SingleClass<BuildSetting> {

        public BuildSetting() {
            Read();
        }

        BuildTarget m_selectedBuildTarget;
        string m_keyPrefix = "Project_" + BuildConfig.currentProject + "_BuildAssetBundle_";
        bool m_isBuild = true;
        bool m_isForceRebuildAll = false;
        bool m_isExportToMainProject = false;
        bool m_isUploadToFtp = false;
        bool m_isFtpPassive = false;
        bool m_isFtpUploadAll = false;
        string m_streamingFolder = "";
        string m_ftpUri = "ftp://120.92.34.206";
        string m_ftpUserName = "uniuftp";
        string m_ftpPassword = "ryx387b1";

        public void Read() {
            m_selectedBuildTarget = EditorUserBuildSettings.activeBuildTarget;

            string str = PlayerPrefs.GetString(m_keyPrefix + "Build", m_isBuild.ToString());
            bool.TryParse(str, out m_isBuild);

            str = PlayerPrefs.GetString(m_keyPrefix + "ForceRebuildAll", m_isForceRebuildAll.ToString());
            bool.TryParse(str, out m_isForceRebuildAll);

            str = PlayerPrefs.GetString(m_keyPrefix + "UploadToProj", m_isExportToMainProject.ToString());
            bool.TryParse(str, out m_isExportToMainProject);

            m_streamingFolder = PlayerPrefs.GetString(m_keyPrefix + "StreamingFolder", m_streamingFolder);

            str = PlayerPrefs.GetString(m_keyPrefix + "UploadToFtp", m_isUploadToFtp.ToString());
            bool.TryParse(str, out m_isUploadToFtp);

            m_ftpUri = PlayerPrefs.GetString(m_keyPrefix + "FtpUri", m_ftpUri);

            m_ftpUserName = PlayerPrefs.GetString(m_keyPrefix + "FtpUserName", m_ftpUserName);

            m_ftpPassword = PlayerPrefs.GetString(m_keyPrefix + "FtpPassword", m_ftpPassword);

            str = PlayerPrefs.GetString(m_keyPrefix + "FtpPassive", m_isFtpPassive.ToString());
            bool.TryParse(str, out m_isFtpPassive);
        }

        public void Save() {
            PlayerPrefs.SetString(m_keyPrefix + "Build", isBuild.ToString());
            PlayerPrefs.SetString(m_keyPrefix + "ForceRebuildAll", isForceRebuildAll.ToString());
            PlayerPrefs.SetString(m_keyPrefix + "UploadToProj", isExportToMainProject.ToString());
            PlayerPrefs.SetString(m_keyPrefix + "UploadToFtp", isUploadToFtp.ToString());
            PlayerPrefs.SetString(m_keyPrefix + "FtpPassive", isFtpPassive.ToString());
            PlayerPrefs.SetString(m_keyPrefix + "StreamingFolder", streamingFolder);
            PlayerPrefs.SetString(m_keyPrefix + "FtpUri", ftpUri);
            PlayerPrefs.SetString(m_keyPrefix + "FtpUserName", ftpUserName);
            PlayerPrefs.SetString(m_keyPrefix + "FtpPassword", ftpPassword);
        }

        public bool isBuild {
            get { return m_isBuild; }
            set { m_isBuild = value; }
        }

        public bool isForceRebuildAll {
            get { return m_isForceRebuildAll; }
            set { m_isForceRebuildAll = value; }
        }

        public bool isExportToMainProject {
            get { return m_isExportToMainProject; }
            set { m_isExportToMainProject = value; }
        }

        public string streamingFolder {
            get { return m_streamingFolder; }
            set { m_streamingFolder = value; }
        }

        public bool isUploadToFtp {
            get { return m_isUploadToFtp; }
            set { m_isUploadToFtp = value; }
        }

        public string ftpUri {
            get { return m_ftpUri; }
            set { m_ftpUri = value; }
        }

        public string ftpUserName {
            get { return m_ftpUserName; }
            set { m_ftpUserName = value; }
        }

        public string ftpPassword {
            get { return m_ftpPassword; }
            set { m_ftpPassword = value; }
        }

        public bool isFtpPassive {
            get { return m_isFtpPassive; }
            set { m_isFtpPassive = value; }
        }

        public bool isFtpUploadAll {
            get { return m_isFtpUploadAll; }
            set { m_isFtpUploadAll = value; }
        }

        public BuildTarget selectedBuildTarget {
            get { return m_selectedBuildTarget; }
            set { m_selectedBuildTarget = value; }
        }
    }
}
