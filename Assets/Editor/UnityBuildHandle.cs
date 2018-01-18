using UnityEngine;
using UnityEditor;
//using UnityEditor.Callbacks;
using UnityEditor.Build;

namespace Utility {

	public class UnityBuildHandle : MonoBehaviour, IPreprocessBuild, IPostprocessBuild {

        //[PostProcessBuild]
        //public static void qwe(BuildTarget BuildTarget, string path)
        //{
        //    Debug.Log("项目build完成执行");
        //}


        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildTarget target, string path) {
            Debug.Log("项目build前运行");
        }

        public void OnPostprocessBuild(BuildTarget target, string path) {
            Debug.Log("项目build完成执行");
        }
    }
}