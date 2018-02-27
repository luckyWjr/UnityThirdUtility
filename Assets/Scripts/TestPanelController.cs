using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace newnamespace {

	public class TestPanelController : MonoBehaviour {

        [SerializeField] Image m_image1;
        [SerializeField] Image m_image2;
        [SerializeField] Image m_image3;
        [SerializeField] RawImage m_rawImage1;

        void Start () {
            Debug.Log("TestPanelController");
            //SpriteAssetItem spriteAsset1 = new SpriteAssetItem(SpriteAssetItem.iconFolder, "xun1");
            //spriteAsset1.Load(() => {
            //    m_image1.sprite = spriteAsset1.sprite;
            //});

            //SpriteAssetItem spriteAsset2 = new SpriteAssetItem(SpriteAssetItem.iconFolder, "xun2");
            //spriteAsset2.Load(() => {
            //    m_image2.sprite = spriteAsset2.sprite;
            //});

            //SpriteAssetItem spriteAsset3 = new SpriteAssetItem(SpriteAssetItem.iconFolder, "xun3");
            //spriteAsset3.Load(() => {
            //    m_image3.sprite = spriteAsset3.sprite;
            //});
        }

        void Update () {
			
		}
	}
}