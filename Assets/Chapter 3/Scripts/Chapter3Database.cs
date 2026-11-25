using System;
using UnityEngine;

namespace Chapter3
{
    /// <summary>Pilihan tindakan pertama saat serangan terjadi.</summary>
    [Serializable]
    public class ResponseOption
    {
        public string label;
        [Tooltip("Centang untuk pilihan yang mengaktifkan rencana Incident Response")]
        public bool activatesIncidentResponse;
        [TextArea(1, 4)] public string feedback;
    }

    /// <summary>Semua isi dan pengaturan Chapter 3.</summary>
    [CreateAssetMenu(fileName = "Chapter3Database", menuName = "Chapter 3/Database")]
    public class Chapter3Database : ScriptableObject
    {
        [Tooltip("Kejadian serangan yang ditampilkan di awal chapter")]
        [TextArea(4, 10)] public string introduction;
        public ResponseOption[] responseOptions = new ResponseOption[0];
        public Chapter3System[] systems = new Chapter3System[0];
        public Chapter3BackupServer[] backupServers = new Chapter3BackupServer[0];

        [Header("Waktu")]
        [Range(0, 23)] public int startHour = 8;
        [Tooltip("Menit di dalam game untuk setiap detik nyata")]
        [Min(0)] public float minutesPerSecond = 1f;
        [Tooltip("Menit sampai cadangan di lokasi yang sama ikut terenkripsi jika serangan belum terkendali")]
        [Min(0)] public float localBackupEncryptMinutes = 120f;

        [Header("IT Staff")]
        [Min(1)] public int staffCount = 5;
        [Min(0)] public int maxContractors = 2;
        [Tooltip("Biaya (rupiah) per kontraktor, dibayar dari sisa uang Chapter 2")]
        [Min(0)] public int contractorCost = 1000000;
        [Tooltip("Sisa uang untuk mode uji, saat scene dibuka tanpa Chapter 2")]
        [Min(0)] public int testMoney = 1500000;

        [Header("Lama aktivitas (detik)")]
        [Min(0.1f)] public float isolateSeconds = 4f;
        [Min(0.1f)] public float reconnectSeconds = 3f;
        [Min(0.1f)] public float investigateSeconds = 3f;
        [Min(0.1f)] public float verifySeconds = 4f;
        [Min(0.1f)] public float restoreSeconds = 8f;

        [Header("Kerugian")]
        [Tooltip("Kerugian (rupiah) untuk setiap jam gangguan")]
        [Min(0)] public int lossPerHour = 2000000;
    }
}
