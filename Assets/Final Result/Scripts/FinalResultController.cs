using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Hasil akhir (Gambar 3.10): skor total maks. 300, ringkasan keputusan ketiga chapter, dan pilihan bermain lagi.
/// </summary>
public class FinalResultController : MonoBehaviour
{
    [SerializeField] TMP_Text totalText;
    [SerializeField] ChapterResultView chapter1;
    [SerializeField] ChapterResultView chapter2;
    [SerializeField] ChapterResultView chapter3;
    [SerializeField] Button playAgainButton;
    [SerializeField] Button quitButton;
    [SerializeField] string mainMenuSceneName = "StartScreen";

    void Start()
    {
        chapter1.Show("Chapter 1 - Investigasi", GameSession.Chapter1Score, GameSession.Chapter1Summary);
        chapter2.Show("Chapter 2 - Persiapan Pertahanan", GameSession.Chapter2Score, GameSession.Chapter2Summary);
        chapter3.Show("Chapter 3 - Serangan Ransomware", GameSession.Chapter3Score, GameSession.Chapter3Summary);
        totalText.text = $"{GameSession.TotalScore} / 300";

        playAgainButton.onClick.AddListener(PlayAgain);
        quitButton.onClick.AddListener(Quit);
    }

    void PlayAgain()
    {
        GameSession.Reset();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
