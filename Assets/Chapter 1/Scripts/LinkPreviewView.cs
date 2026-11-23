using Chapter1;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Halaman pratinjau tautan pendek yang muncul saat pemain membuka bit.ly/kode+ di kolom alamat
/// (prefab ui_link_preview).
/// </summary>
public class LinkPreviewView : MonoBehaviour
{
    public GameObject root;
    public TMP_Text shortLinkText;
    public TMP_Text destinationText;
    public TMP_Text detailText;
    public Button closeButton;

    void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    public void ShowLink(string shortUrl, LinkData link)
    {
        shortLinkText.text = shortUrl;
        destinationText.text =
            "Tautan pendek ini mengarah ke:\n<color=#1a5fd6><b>" + link.shortLinkDestination + "</b></color>";
        string title = string.IsNullOrEmpty(link.shortLinkPageTitle) ? "(tanpa judul)" : link.shortLinkPageTitle;
        detailText.text =
            $"Judul halaman: {title}\n" +
            $"Dibuat: {(string.IsNullOrEmpty(link.shortLinkCreated) ? "-" : link.shortLinkCreated)}    " +
            $"Total klik: {link.shortLinkClicks:N0}";
        root.SetActive(true);
    }

    public void ShowNotFound(string shortUrl)
    {
        shortLinkText.text = shortUrl;
        destinationText.text = "<b>Tautan tidak ditemukan.</b>";
        detailText.text = "Periksa kembali kode tautan pendek yang kamu ketik.";
        root.SetActive(true);
    }

    public void Hide() => root.SetActive(false);
}
