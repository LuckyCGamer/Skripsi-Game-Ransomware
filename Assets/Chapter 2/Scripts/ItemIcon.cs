using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chapter2
{
    /// <summary>Kotak gambar item: menampilkan sprite, atau teks pengganti jika sprite belum ada.</summary>
    public class ItemIcon : MonoBehaviour
    {
        public Image image;
        public TMP_Text placeholder;

        Sprite frameSprite;
        Color frameColor;
        Image.Type frameType;
        bool cached;

        public void Show(Chapter2ShopItem data)
        {
            if (!cached)
            {
                frameSprite = image.sprite;
                frameColor = image.color;
                frameType = image.type;
                cached = true;
            }

            bool has = data.icon != null;
            image.sprite = has ? data.icon : frameSprite;
            image.color = has ? Color.white : frameColor;
            image.type = has ? Image.Type.Simple : frameType;
            image.preserveAspect = has;
            placeholder.gameObject.SetActive(!has);
            placeholder.text = data.iconText;
        }
    }
}
