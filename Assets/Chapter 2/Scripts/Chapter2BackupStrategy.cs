using System;
using UnityEngine;

namespace Chapter2
{
    /// <summary>Satu tombol Backup Frequency.</summary>
    [Serializable]
    public class FrequencyOption
    {
        public string label;
        [Tooltip("Hapus centang untuk pilihan 'Tidak Dicadangkan'")]
        public bool isBackup = true;
        [Tooltip("Kebutuhan storage = ukuran data x nilai ini")]
        [Min(0)] public float storageMultiplier = 1f;
        [Tooltip("Data yang dapat hilang, dalam jam")]
        [Min(0)] public int dataLossHours;
        [Min(0)] public int points;
        [TextArea(1, 3)] public string description;
    }

    /// <summary>Satu tombol Storage Strategy (lokasi server cadangan).</summary>
    [Serializable]
    public class StorageOption
    {
        public string label;
        [Tooltip("Centang jika lokasinya berbeda (offsite)")]
        public bool isOffsite;
        [Tooltip("Biaya dalam rupiah untuk berkas ini")]
        [Min(0)] public int cost;
        [Min(0)] public int points;
        [TextArea(1, 3)] public string description;
    }

    /// <summary>Satu berkas/sistem pada layar Backup Strategy (satu kartu) beserta pilihannya.</summary>
    [CreateAssetMenu(fileName = "Chapter2BackupStrategy", menuName = "Chapter 2/Backup Strategy")]
    public class Chapter2BackupStrategy : ScriptableObject
    {
        public string displayName;
        [TextArea(2, 4)] public string description;
        [Min(0)] public int sizeGB;

        [Header("Penilaian")]
        [Tooltip("Centang jika berkas/sistem ini penting dan seharusnya dicadangkan")]
        public bool isImportant;
        [Tooltip("Penjelasan untuk ringkasan keputusan")]
        [TextArea(2, 4)] public string reason;

        [Header("Tombol pilihan")]
        public FrequencyOption[] frequencyOptions =
        {
            new FrequencyOption { label = "Tidak Dicadangkan", isBackup = false, storageMultiplier = 0f,
                description = "Berkas ini tidak dapat dipulihkan dari cadangan." },
            new FrequencyOption { label = "Setiap 4 Jam", storageMultiplier = 2f, dataLossHours = 4, points = 25,
                description = "Data hilang lebih sedikit, storage lebih besar." },
            new FrequencyOption { label = "Setiap 24 Jam", storageMultiplier = 1f, dataLossHours = 24, points = 10,
                description = "Storage lebih kecil, data hilang lebih banyak." },
        };
        public StorageOption[] storageOptions =
        {
            new StorageOption { label = "Lokasi Sama", cost = 500000, points = 5,
                description = "Lebih murah, tetapi cadangan rawan ikut terenkripsi." },
            new StorageOption { label = "Offsite", isOffsite = true, cost = 1500000, points = 20,
                description = "Lebih mahal, tetapi cadangan lebih aman." },
        };
    }
}
