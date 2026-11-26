using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Menyimpan skor antar-chapter selama satu permainan (maks. 100 per chapter, 300 total).
/// </summary>
public static class GameSession
{
    const string LastChapter1VariationKey = "chapter1_last_variation";
    const string LastChapter3VariationKey = "chapter3_last_variation";

    public static int Chapter1Score = -1;
    public static string Chapter1Summary = "";
    public static int Chapter1Variation = -1;

    /// <summary>Keputusan pencadangan satu berkas/sistem, dasar pemulihan pada Chapter 3.</summary>
    public class BackupRecord
    {
        public string fileName;
        public bool isBackedUp;
        /// <summary>Jarak antar-cadangan dalam jam.</summary>
        public int intervalHours;
        public Chapter2.BackupLocation[] locations = new Chapter2.BackupLocation[0];
        public string storageLabel = "";
    }

    public static int Chapter2Score = -1;
    public static string Chapter2Summary = "";
    public static bool Chapter2HasDisasterRecovery;
    public static bool Chapter2HasIncidentResponse;
    public static bool Chapter2HasTraining;
    /// <summary>Menit sampai serangan terdeteksi; -1 = tanpa monitoring.</summary>
    public static int Chapter2DetectMinutes = -1;
    public static float Chapter2RestoreMultiplier = 1f;
    /// <summary>Sisa uang Chapter 2, dipakai untuk menyewa kontraktor pada Chapter 3.</summary>
    public static int Chapter2MoneyLeft;
    public static readonly List<BackupRecord> Chapter2Backups = new List<BackupRecord>();

    public static int Chapter3Score = -1;
    public static int Chapter3Variation = -1;
    public static string Chapter3Summary = "";

    /// <summary>Skor total ketiga chapter (maks. 300). Chapter yang belum dimainkan dihitung 0.</summary>
    public static int TotalScore =>
        Mathf.Max(0, Chapter1Score) + Mathf.Max(0, Chapter2Score) + Mathf.Max(0, Chapter3Score);

    /// <summary>Hapus hasil permainan sebelum memulai permainan baru.</summary>
    public static void Reset()
    {
        Chapter1Score = Chapter2Score = Chapter3Score = -1;
        Chapter1Summary = Chapter2Summary = Chapter3Summary = "";
        Chapter1Variation = Chapter3Variation = -1;
        Chapter2HasDisasterRecovery = Chapter2HasIncidentResponse = Chapter2HasTraining = false;
        Chapter2DetectMinutes = -1;
        Chapter2RestoreMultiplier = 1f;
        Chapter2MoneyLeft = 0;
        Chapter2Backups.Clear();
    }

    /// <summary>Pilih variasi acak yang berbeda dari variasi permainan sebelumnya.</summary>
    public static int PickChapter1Variation(int count) => Chapter1Variation = PickVariation(LastChapter1VariationKey, count);

    /// <summary>Pilih variasi acak Chapter 3 yang berbeda dari variasi permainan sebelumnya.</summary>
    public static int PickChapter3Variation(int count) => Chapter3Variation = PickVariation(LastChapter3VariationKey, count);

    static int PickVariation(string key, int count)
    {
        int last = PlayerPrefs.GetInt(key, -1);
        int pick = Random.Range(0, count);
        if (count > 1 && pick == last)
            pick = (pick + 1 + Random.Range(0, count - 1)) % count;
        PlayerPrefs.SetInt(key, pick);
        PlayerPrefs.Save();
        return pick;
    }
}
