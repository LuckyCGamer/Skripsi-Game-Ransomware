using TMPro;
using UnityEngine;

/// <summary>Skor dan ringkasan keputusan satu chapter (prefab ui_chapter_result).</summary>
public class ChapterResultView : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text scoreText;
    public TMP_Text summaryText;

    /// <param name="score">-1 jika chapter belum dimainkan.</param>
    public void Show(string title, int score, string summary)
    {
        titleText.text = title;
        scoreText.text = score < 0 ? "- / 100" : $"{score} / 100";
        summaryText.text = score < 0 ? "Chapter ini belum dimainkan." : summary;
    }
}
