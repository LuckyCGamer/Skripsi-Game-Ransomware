using UnityEngine;

namespace Chapter2
{
    /// <summary>Daftar isi Chapter 2: kartu Backup Strategy, item Shop, serta uang dan storage awal.</summary>
    [CreateAssetMenu(fileName = "Chapter2Database", menuName = "Chapter 2/Database")]
    public class Chapter2Database : ScriptableObject
    {
        [Tooltip("Penyajian masalah di awal chapter")]
        [TextArea(4, 10)] public string introduction;
        [Tooltip("Uang awal dalam rupiah")]
        [Min(0)] public int startingMoney;
        [Min(0)] public int startingStorageGB;

        [Tooltip("Kartu pada layar Backup Strategy, sesuai urutan")]
        public Chapter2BackupStrategy[] backupStrategies = new Chapter2BackupStrategy[0];
        [Tooltip("Item pada layar Shop, sesuai urutan")]
        public Chapter2ShopItem[] shopItems = new Chapter2ShopItem[0];
    }
}
