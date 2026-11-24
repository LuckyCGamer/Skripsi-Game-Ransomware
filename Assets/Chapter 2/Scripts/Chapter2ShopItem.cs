using UnityEngine;

namespace Chapter2
{
    public enum ShopItemEffect
    {
        AddStorage,       // menambah storage, boleh dibeli berkali-kali
        DisasterRecovery, // rencana DR, dibeli sekali
        IncidentResponse, // rencana IR, dibeli sekali
    }

    /// <summary>Satu item pada layar Shop.</summary>
    [CreateAssetMenu(fileName = "Chapter2ShopItem", menuName = "Chapter 2/Shop Item")]
    public class Chapter2ShopItem : ScriptableObject
    {
        public string displayName;
        [TextArea(3, 6)] public string description;
        [Tooltip("Harga dalam rupiah")]
        [Min(0)] public int price;
        [Tooltip("Gambar item. Jika kosong, Icon Text yang ditampilkan")]
        public Sprite icon;
        [Tooltip("Teks pengganti selama gambar belum ada")]
        public string iconText;

        [Header("Efek setelah dibeli")]
        public ShopItemEffect effect;
        [Tooltip("Hanya untuk Add Storage")]
        [Min(0)] public int storageGB;
    }
}
