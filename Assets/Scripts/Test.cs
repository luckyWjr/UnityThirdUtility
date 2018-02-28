using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using SimpleJSON;

namespace newnamespace {

	public class Test : MonoBehaviour {

        [SerializeField] Transform m_canvasTrans;

		void Start () {

            //StartCoroutine(WebUtility.instance.GetData("data/qq.txt", (WWW _data) => {
            //    var jsonData = JSON.Parse(_data.text);
            //    if(null != jsonData) {
            //        for(int i = 0; i < jsonData.Count; i++) {
            //        }
            //    }
            //}));

            //ImageAssetItem imageAsset = new ImageAssetItem(ImageAssetItem.iconFolder, "xun1");
            //imageAsset.Load(() => {
            //    GameObject.Find("RawImage").GetComponent<RawImage>().texture = imageAsset.texture;
            //});

            //SpriteAssetItem spriteAsset3 = new SpriteAssetItem(SpriteAssetItem.iconFolder, "xun3");
            //spriteAsset3.Load(() => {
            //    UIPrefabAssetItem imageAsset = new UIPrefabAssetItem("", "TestPanel");
            //    imageAsset.Load(() => {
            //        GameObject obj = Instantiate(imageAsset.prefab);
            //        obj.transform.SetParent(m_canvasTrans, false);
            //        obj.transform.localPosition = Vector3.zero;
            //    });
            //});
        }


    }
}