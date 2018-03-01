using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using SimpleJSON;

namespace newnamespace {

	public class Test : MonoBehaviour {

        [SerializeField] Transform m_canvasTrans;

        void Start() {

            //StartCoroutine(WebUtility.instance.GetData("data/qq.txt", (WWW _data) => {
            //    var jsonData = JSON.Parse(_data.text);
            //    if(null != jsonData) {
            //        for(int i = 0; i < jsonData.Count; i++) {
            //        }
            //    }
            //}));

            UIPrefabAssetItem asset = new UIPrefabAssetItem("", "TestPanel");
            asset.LoadAsync(() => {
                GameObject obj = Instantiate(asset.prefab);
                obj.transform.SetParent(m_canvasTrans, false);
                obj.transform.localPosition = Vector3.zero;
            });
        }

    }
}