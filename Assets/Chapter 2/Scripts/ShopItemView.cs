using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chapter2
{
    /// <summary>Satu item di daftar Shop (prefab ui_shop_item).</summary>
    public class ShopItemView : MonoBehaviour
    {
        public Button button;
        public Image background;
        public ItemIcon icon;
        public TMP_Text nameText;
        public TMP_Text priceText;
        public CanvasGroup group;

        public Chapter2ShopItem Data { get; private set; }

        public void Show(Chapter2ShopItem data)
        {
            Data = data;
            icon.Show(data);
            nameText.text = data.displayName;
        }

        /// <summary>Item yang tidak dapat dibeli ditampilkan pudar, tetapi detailnya tetap bisa dilihat.</summary>
        public void Refresh(bool owned, bool canBuy)
        {
            group.alpha = canBuy ? 1f : 0.45f;
            priceText.text = owned ? "Dimiliki" : Chapter2Controller.Rupiah(Data.price);
        }
    }
}
