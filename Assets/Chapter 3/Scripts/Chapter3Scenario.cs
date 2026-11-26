using System;
using UnityEngine;

namespace Chapter3
{
    /// <summary>Penyebaran ransomware ke satu sistem pada menit tertentu setelah serangan dimulai.</summary>
    [Serializable]
    public class SpreadStep
    {
        public Chapter3System system;
        [Min(0)] public int minute;
    }

    /// <summary>
    /// Satu variasi serangan Chapter 3. Satu variasi dipilih acak setiap permainan baru,
    /// berbeda dari variasi yang dimainkan sebelumnya.
    /// </summary>
    [CreateAssetMenu(fileName = "Chapter3Scenario", menuName = "Chapter 3/Scenario")]
    public class Chapter3Scenario : ScriptableObject
    {
        [Tooltip("Kejadian serangan pada variasi ini, ditampilkan sebelum petunjuk umum")]
        [TextArea(3, 8)] public string attackDescription;
        [Tooltip("Sistem yang langsung terenkripsi saat serangan dimulai. Pilih sistem yang dicadangkan pada Chapter 2")]
        public Chapter3System[] infectedSystems = new Chapter3System[0];
        [Tooltip("Sistem yang terjangkau berikutnya jika serangan belum terkendali. Langkah sebelum serangan terdeteksi selalu terjadi")]
        public SpreadStep[] spread = new SpreadStep[0];
    }
}
