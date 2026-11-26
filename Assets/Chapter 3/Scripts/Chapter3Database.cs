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
        [Tooltip("Petunjuk umum, ditampilkan di awal chapter setelah kejadian serangan dari variasi")]
        [TextArea(4, 10)] public string introduction;
        [Tooltip("Variasi serangan; satu dipilih acak setiap permainan baru")]
        public Chapter3Scenario[] scenarios = new Chapter3Scenario[0];
        public ResponseOption[] responseOptions = new ResponseOption[0];
        public Chapter3System[] systems = new Chapter3System[0];
        public Chapter3BackupServer[] backupServers = new Chapter3BackupServer[0];

        [Header("Waktu")]
        [Tooltip("Jam serangan dimulai")]
        [Range(0, 23)] public int startHour = 19;
        [Tooltip("Menit di dalam game untuk setiap detik nyata")]
        [Min(0)] public float minutesPerSecond = 1f;
        [Tooltip("Menit sampai serangan terdeteksi jika tidak membeli monitoring pada Chapter 2")]
        [Min(0)] public int detectMinutesWithoutMonitoring = 35;

        [Header("Riwayat cadangan")]
        [Tooltip("Rentang riwayat cadangan sebelum serangan, dalam jam")]
        [Min(1)] public int historyHours = 48;
        [Tooltip("Peluang satu pekerjaan backup gagal")]
        [Range(0, 0.6f)] public float failRate = 0.07f;
        [Tooltip("Peluang cadangan yang berhasil ternyata rusak (baru ketahuan saat diverifikasi)")]
        [Range(0, 0.6f)] public float corruptRate = 0.05f;
        [Tooltip("Jumlah cadangan terbaru yang diperiksa oleh Investigate Backup")]
        [Min(1)] public int investigateCount = 10;

        [Header("IT Staff")]
        [Min(1)] public int staffCount = 5;
        [Tooltip("Pengali lama aktivitas jika membeli pelatihan IR pada Chapter 2 (0,8 = 20% lebih cepat)")]
        [Range(0.1f, 1f)] public float trainingSpeed = 0.8f;
        [Tooltip("Tambahan IT Staff dari pelatihan IR")]
        [Min(0)] public int trainingStaff = 1;
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
