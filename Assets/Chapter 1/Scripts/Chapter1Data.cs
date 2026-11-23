using System;
using UnityEngine;

namespace Chapter1
{
    /// <summary>Metode masuk peretas yang bisa disimpulkan pemain (sesuai tab Email, Link, dan Software).</summary>
    public enum EntryMethod
    {
        MalwareAttachment,
        PhishingLink,
        SoftwareVulnerability,
    }

    /// <summary>Penanda warna hasil pemindaian perangkat lunak (Gambar 3.15b).</summary>
    public enum SoftwareStatus
    {
        Green,  // versi terbaru, aman
        Yellow, // ada pembaruan, perlu di-update
        Red,    // memiliki vulnerability / celah keamanan kritis
    }

    [Serializable]
    public class EmailData
    {
        public string senderName;
        public string senderAddress;
        public string subject;
        [TextArea(3, 8)] public string body;
        [Tooltip("Kosongkan jika email tanpa lampiran")]
        public string attachmentName;
        public string attachmentSize;
        [Tooltip("Centang jika email ini harus dibuang (TRASH)")]
        public bool malicious;
        [Tooltip("Penjelasan untuk ringkasan keputusan")]
        [TextArea(2, 4)] public string reason;
    }

    /// <summary>
    /// Tautan yang dianalisis pemain. Ada tiga cara mengenalinya:
    /// 1. Dilihat langsung: isi displayText dengan alamatnya sendiri.
    /// 2. Di-hover: targetUrl tampil di kolom alamat dan tooltip.
    /// 3. Tautan pendek bit.ly: targetUrl berupa https://bit.ly/kode, dan pemain mengetik
    ///    bit.ly/kode+ di kolom alamat untuk melihat pratinjau tujuan aslinya (shortLinkDestination).
    /// </summary>
    [Serializable]
    public class LinkData
    {
        [Tooltip("Teks tautan yang terlihat. Bisa berupa teks biasa atau alamat lengkap (cara 1: dilihat langsung)")]
        public string displayText;
        [Tooltip("Alamat yang tampil saat di-hover (cara 2). Isi https://bit.ly/kode untuk tautan pendek (cara 3)")]
        public string targetUrl;
        public bool safe;
        [TextArea(2, 4)] public string reason;

        [Header("Hanya untuk tautan bit.ly (pratinjau bit.ly/kode+)")]
        [Tooltip("Alamat tujuan asli yang disembunyikan tautan pendek")]
        public string shortLinkDestination;
        [Tooltip("Judul halaman tujuan yang tampil di pratinjau")]
        public string shortLinkPageTitle;
        public string shortLinkCreated;
        public int shortLinkClicks;

        public bool IsShortLink => !string.IsNullOrEmpty(targetUrl) &&
                                   targetUrl.IndexOf("bit.ly/", StringComparison.OrdinalIgnoreCase) >= 0;

        /// <summary>Alamat tujuan akhir, yang menentukan aman atau tidaknya tautan.</summary>
        public string FinalUrl => IsShortLink && !string.IsNullOrEmpty(shortLinkDestination) ? shortLinkDestination : targetUrl;
    }

    [Serializable]
    public class SoftwareData
    {
        public string name;
        public string installedVersion;
        public string latestVersion;
        [TextArea(2, 4)] public string description;
        public SoftwareStatus status;
    }

    [Serializable]
    public class MethodOption
    {
        public EntryMethod method;
        public string label;
    }
}
