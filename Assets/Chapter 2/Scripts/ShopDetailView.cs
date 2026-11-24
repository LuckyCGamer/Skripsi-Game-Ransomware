using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chapter2
{
    /// <summary>Detail item yang dipilih dan tombol Buy (prefab ui_shop_detail).</summary>
    public class ShopDetailView : MonoBehaviour
    {
        public ItemIcon icon;
        public TMP_Text nameText;
        public TMP_Text priceText;
        public TMP_Text descriptionText;
        public Button buyButton;
        public TMP_Text buyLabel;

        public void Show(Chapter2ShopItem data, bool owned, bool canBuy)
        {
            icon.Show(data);
            nameText.text = data.displayName;
            priceText.text = Chapter2Controller.Rupiah(data.price);
            descriptionText.text = data.description;
            buyButton.interactable = canBuy;
            buyLabel.text = owned ? "Dimiliki" : canBuy ? "Buy" : "Uang Kurang";
        }
    }
}
