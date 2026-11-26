using Chapter2;
using UnityEngine;
using UnityEngine.Serialization;

namespace Chapter3
{
    /// <summary>Satu sistem perusahaan (kartu pada tab File dan baris pada jendela Investigate Backup).</summary>
    [CreateAssetMenu(fileName = "Chapter3System", menuName = "Chapter 3/System")]
    public class Chapter3System : ScriptableObject
    {
        public string displayName;
        [TextArea(1, 3)] public string description;
        [Tooltip("Gejala yang terlihat jika sistem ini terinfeksi")]
        [FormerlySerializedAs("symptom")]
        [TextArea(1, 3)] public string infectedSymptom;
        [Tooltip("Gejala yang terlihat jika sistem ini tidak terinfeksi")]
        [TextArea(1, 3)] public string normalSymptom = "Berjalan normal, semua berkas masih dapat dibuka.";
        [Tooltip("Berkas pada Chapter 2 yang menjadi cadangan sistem ini")]
        public Chapter2BackupStrategy backupSource;

        [Header("Penilaian")]
        [Tooltip("1 = paling penting. Di antara sistem yang terinfeksi, yang paling penting harus dipulihkan lebih dulu")]
        [Min(1)] public int priority = 1;
        [Tooltip("Penjelasan prioritas untuk ringkasan keputusan")]
        [TextArea(1, 3)] public string reason;
        [Tooltip("Kerugian (rupiah) jika sistem ini gagal dipulihkan")]
        [Min(0)] public int lossIfFailed;
    }
}
