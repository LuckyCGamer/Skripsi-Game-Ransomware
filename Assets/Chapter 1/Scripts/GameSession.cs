using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Menyimpan skor antar-chapter selama satu permainan (maks. 100 per chapter, 300 total).
/// </summary>
public static class GameSession
{
    const string LastChapter1VariationKey = "chapter1_last_variation";

    public static int Chapter1Score = -1;
    public static string Chapter1Summary = "";
    public static int Chapter1Variation = -1;

    /// <summary>Keputusan pencadangan satu berkas/sistem, dasar pemulihan pada Chapter 3.</summary>
    public class BackupRecord
    {
        public string fileName;
        public bool isBackedUp;
        public int dataLossHours;
        public bool isOffsite;
    }

    public static int Chapter2Score = -1;
    public static string Chapter2Summary = "";
    public static bool Chapter2HasDisasterRecovery;
    public static bool Chapter2HasIncidentResponse;
    public static readonly List<BackupRecord> Chapter2Backups = new List<BackupRecord>();

    /// <summary>Pilih variasi acak yang berbeda dari variasi permainan sebelumnya.</summary>
    public static int PickChapter1Variation(int count)
    {
        int last = PlayerPrefs.GetInt(LastChapter1VariationKey, -1);
        int pick = Random.Range(0, count);
        if (count > 1 && pick == last)
            pick = (pick + 1 + Random.Range(0, count - 1)) % count;
        PlayerPrefs.SetInt(LastChapter1VariationKey, pick);
        PlayerPrefs.Save();
        Chapter1Variation = pick;
        return pick;
    }
}
