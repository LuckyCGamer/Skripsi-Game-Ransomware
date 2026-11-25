using Chapter2;
using UnityEngine;

namespace Chapter3
{
    /// <summary>Satu sistem perusahaan (kartu pada tab File dan baris pada jendela Investigate Backup).</summary>
    [CreateAssetMenu(fileName = "Chapter3System", menuName = "Chapter 3/System")]
    public class Chapter3System : ScriptableObject
    {
        public string displayName;
        [TextArea(1, 3)] public string description;
        [Tooltip("Gejala yang terlihat saat serangan")]
        [TextArea(1, 3)] public string symptom;
        [Tooltip("Berkas pada Chapter 2 yang menjadi cadangan sistem ini")]
        public Chapter2BackupStrategy backupSource;

        [Header("Penilaian")]
        public bool isInfected;
        [Tooltip("1 = harus dipulihkan paling dulu")]
        [Min(1)] public int priority = 1;
        [Tooltip("Penjelasan prioritas untuk ringkasan keputusan")]
        [TextArea(1, 3)] public string reason;
        [Tooltip("Kerugian (rupiah) jika sistem ini gagal dipulihkan")]
        [Min(0)] public int lossIfFailed;
    }
}
