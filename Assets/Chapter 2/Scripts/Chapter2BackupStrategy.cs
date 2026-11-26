using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Chapter2
{
    /// <summary>Tempat salinan cadangan disimpan.</summary>
    public enum BackupLocation
    {
        Connected, // penyimpanan yang terhubung ke jaringan produksi
        Separate,  // server cadangan terpisah dengan kredensial sendiri
        Offline,   // media terputus dari jaringan (air-gapped)
    }

    /// <summary>Satu tombol Backup Frequency.</summary>
    [Serializable]
    public class FrequencyOption
    {
        public string label;
        [Tooltip("Hapus centang untuk pilihan 'Tidak Dicadangkan'")]
        public bool isBackup = true;
        [Tooltip("Kebutuhan storage = ukuran data x nilai ini x jumlah salinan")]
        [Min(0)] public float storageMultiplier = 1f;
        [Tooltip("Jarak antar-cadangan dalam jam; juga data maksimal yang bisa hilang")]
        [FormerlySerializedAs("dataLossHours")]
        [Min(0)] public int intervalHours;
        [Min(0)] public int points;
        [TextArea(1, 3)] public string description;
    }

    /// <summary>Satu tombol Storage Strategy.</summary>
    [Serializable]
    public class StorageOption
    {
        public string label;
        [Tooltip("Lokasi salinan cadangan. Multi-copy memakai lebih dari satu lokasi")]
        public BackupLocation[] locations = { BackupLocation.Connected };
        [Tooltip("Biaya dalam rupiah untuk berkas ini")]
        [Min(0)] public int cost;
        [Min(0)] public int points;
        [TextArea(1, 3)] public string description;

        public int Copies => Mathf.Max(1, locations.Length);
        /// <summary>Lokasi berbeda dari jaringan produksi (sesuai flowchart: lokasi berbeda/offsite).</summary>
        public bool IsOffsite => locations.Length > 0 && locations.All(l => l != BackupLocation.Connected);
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
            new FrequencyOption { label = "Setiap 4 Jam", storageMultiplier = 2f, intervalHours = 4, points = 25,
                description = "Data hilang lebih sedikit, storage lebih besar." },
            new FrequencyOption { label = "Setiap 24 Jam", storageMultiplier = 1f, intervalHours = 24, points = 10,
                description = "Storage lebih kecil, data hilang lebih banyak." },
        };
        public StorageOption[] storageOptions = DefaultStorageOptions();

        public static StorageOption[] DefaultStorageOptions() => new[]
        {
            new StorageOption { label = "Terhubung", locations = new[] { BackupLocation.Connected }, cost = 500000, points = 5,
                description = "Murah dan cepat, tetapi ransomware dapat menjangkaunya." },
            new StorageOption { label = "Server Terpisah", locations = new[] { BackupLocation.Separate }, cost = 1000000, points = 20,
                description = "Server dan kredensial sendiri. Ransomware menjangkaunya lebih lambat." },
            new StorageOption { label = "Offline", locations = new[] { BackupLocation.Offline }, cost = 900000, points = 20,
                description = "Terputus dari jaringan sehingga aman, tetapi pemulihan lebih lambat." },
            new StorageOption { label = "Multi-copy", locations = new[] { BackupLocation.Separate, BackupLocation.Offline }, cost = 1700000, points = 20,
                description = "Server terpisah ditambah salinan offline. Paling mahal dan paling tahan serangan." },
        };
    }
}
