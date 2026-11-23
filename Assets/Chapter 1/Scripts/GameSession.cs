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
