using UnityEngine;

namespace Chapter2
{
    public enum ShopItemEffect
    {
        AddStorage,       // menambah storage, boleh dibeli berkali-kali
        DisasterRecovery, // rencana DR (poin flowchart), dibeli sekali
        IncidentResponse, // rencana IR (poin flowchart), dibeli sekali
        Training,         // pelatihan IR: +1 IT Staff dan tugas lebih cepat pada Chapter 3
        Monitoring,       // serangan terdeteksi lebih cepat pada Chapter 3
        RecoverySite,     // infrastruktur DR: pemulihan lebih cepat pada Chapter 3
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
        [Tooltip("Hanya untuk Monitoring: menit sampai serangan terdeteksi")]
        [Min(0)] public int detectMinutes = 18;
        [Tooltip("Hanya untuk Recovery Site: pengali lama pemulihan (0,7 = 30% lebih cepat)")]
        [Range(0.1f, 1f)] public float restoreMultiplier = 0.7f;
    }
}
