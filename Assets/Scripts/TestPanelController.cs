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

            //SpriteAssetItem spriteAsset3 = new SpriteAssetItem(SpriteAssetItem.iconFolder);
            //spriteAsset3.LoadAsync(() => {
            //    m_image1.sprite = spriteAsset3.GetSprite("xun1");
            //    m_image2.sprite = spriteAsset3.GetSprite("xun2");
            //    m_image3.sprite = spriteAsset3.GetSprite("xun3");
            //});

            ImageAssetItem imageAsset = new ImageAssetItem(ImageAssetItem.backgroundFolder, "siyuebg1");
            imageAsset.Load(false);
            GameObject.Find("RawImage").GetComponent<RawImage>().texture = imageAsset.texture;
        }
    }
}