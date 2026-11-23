using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Isi email yang dipilih beserta lampiran dan tombol TRASH (prefab ui_email_outbox).</summary>
public class EmailDetailView : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text senderName;
    public TMP_Text senderAddress;
    public TMP_Text message;
    public TMP_Text attachmentText;
    public Button trashButton;
    public TMP_Text trashLabel;

    /// <summary>Isi kotak lampiran; kotak disembunyikan jika email tidak punya lampiran.</summary>
    public void SetAttachment(string fileName, string size)
    {
        bool has = !string.IsNullOrEmpty(fileName);
        // Kotak lampiran = induk dari attachmentText (email_attachment).
        attachmentText.transform.parent.gameObject.SetActive(has);
        attachmentText.text = has ? "Lampiran:\n<b>" + fileName + "</b>\n" + size : "";
    }

    /// <summary>Tampilkan/sembunyikan seluruh isi (judul, pengirim, pesan, lampiran, TRASH); latar panel tetap terlihat.</summary>
    public void SetContentVisible(bool visible)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(visible);
    }
}
