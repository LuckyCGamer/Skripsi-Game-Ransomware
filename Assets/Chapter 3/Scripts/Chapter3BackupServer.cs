using Chapter2;
using UnityEngine;

namespace Chapter3
{
    /// <summary>Satu lokasi penyimpanan cadangan (kartu pada tab Backup Protection).</summary>
    [CreateAssetMenu(fileName = "Chapter3BackupServer", menuName = "Chapter 3/Backup Server")]
    public class Chapter3BackupServer : ScriptableObject
    {
        public string displayName;
        [TextArea(1, 3)] public string description;
        [Tooltip("Lokasi yang diwakili, sesuai pilihan Storage Strategy pada Chapter 2")]
        public BackupLocation location;
        [Tooltip("Menit setelah serangan dimulai sampai lokasi ini ikut terenkripsi, jika belum diisolasi dan serangan belum terkendali. -1 = tidak pernah")]
        public int encryptMinute = -1;
        [Tooltip("Tambahan lama pemulihan (detik) jika memulihkan dari lokasi ini")]
        [Min(0)] public float restoreExtraSeconds;
    }
}
