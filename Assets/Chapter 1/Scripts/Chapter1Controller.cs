using System.Collections.Generic;
using System.Text;
using Chapter1;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Logika Chapter 1 "Investigasi metode masuk peretas" (Gambar 3.7):
///   1. Periksa file virus / malware pada lampiran email      (+15)
///   2. Analisis tautan yang mencurigakan                      (+40)
///   3. Periksa perangkat lunak yang digunakan (scan + update) (+25)
///   4. Identifikasi vulnerability dan celah keamanan          (+20)
/// FINISH -> pemain menyimpulkan metode masuk peretas.
///   Belum ditemukan -> -10 poin, tampilkan petunjuk, ulangi investigasi.
///   Ditemukan       -> hitung poin (maks. 100) + ringkasan keputusan -> Chapter 2 (konektor B).
/// </summary>
public class Chapter1Controller : MonoBehaviour
{
    public const int EmailPoints = 15;
    public const int LinkPoints = 40;
    public const int SoftwareCheckPoints = 25;
    public const int VulnerabilityPoints = 20;
    public const int WrongConclusionPenalty = 10;

    enum Tab { Email, Link, Software }

    [Header("Tab & tombol gameplay")]
    [SerializeField] Button tabEmail;
    [SerializeField] Button tabLink;
    [SerializeField] Button tabSoftware;
    [SerializeField] GameObject panelEmail;
    [SerializeField] GameObject panelLink;
    [SerializeField] GameObject panelSoftware;
    [SerializeField] Button hintButton;
    [SerializeField] Button finishButton;
    [SerializeField] Color tabActiveColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] Color tabInactiveColor = new Color(0.75f, 0.75f, 0.75f, 1f);

    [Header("Email")]
    [Tooltip("Prefab email_inbox_1, dibuat satu per email")]
    [SerializeField] EmailInboxItem inboxItemPrefab;
    [Tooltip("Tempat item inbox dibuat (ui_email_inbox)")]
    [SerializeField] Transform inboxContainer;
    [Tooltip("Instance prefab ui_email_outbox di scene")]
    [SerializeField] EmailDetailView emailDetail;
    [SerializeField] Color inboxNormalColor = Color.white;
    [SerializeField] Color inboxSelectedColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField] Color inboxTrashedColor = new Color(0.6f, 0.6f, 0.6f);

    [Header("Link")]
    [SerializeField] TMP_InputField addressBar;
    [SerializeField] TMP_Text shownLink;
    [SerializeField] TMP_Text linkCounter;
    [SerializeField] GameObject hoverTooltip;
    [SerializeField] TMP_Text hoverTooltipText;
    [SerializeField] Button notSafeButton;
    [SerializeField] Button safeButton;
    [Tooltip("Instance prefab ui_link_preview: pratinjau saat pemain membuka bit.ly/kode+")]
    [SerializeField] LinkPreviewView linkPreview;

    [Header("Software")]
    [Tooltip("Prefab ui_sofware, dibuat satu per perangkat lunak dan disusun ke bawah")]
    [SerializeField] SoftwareEntryView softwareEntryPrefab;
    [Tooltip("Tempat entri perangkat lunak dibuat (ui_scenario_software)")]
    [SerializeField] RectTransform softwareContainer;
    [SerializeField] Button scanButton;
    [SerializeField] float softwareEntrySpacing = 4f;
    [SerializeField] Color markerUnknown = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] Color markerGreen = new Color(0.3f, 0.85f, 0.3f);
    [SerializeField] Color markerYellow = new Color(1f, 0.82f, 0.3f);
    [SerializeField] Color markerRed = new Color(1f, 0.4f, 0.4f);
    [SerializeField] Color entryNormalColor = Color.white;
    [SerializeField] Color entryFlaggedColor = new Color(1f, 0.85f, 0.85f);

    [Header("Data")]
    [SerializeField] Chapter1Database database;

    [Header("Popup & alur")]
    [SerializeField] UIPopup popup;
    [SerializeField] string nextSceneName = "Chapter 2";
    [Tooltip("-1 = acak (berbeda dari permainan sebelumnya)")]
    [SerializeField] int forceVariation = -1;

    // ------------------------------------------------------------------ state
    Chapter1Scenario scenario;
    Tab currentTab;

    readonly List<EmailInboxItem> inboxItems = new List<EmailInboxItem>();
    bool[] emailOpened, emailTrashed;
    int selectedEmail = -1;

    int linkIndex;
    bool[] linkAnsweredSafe;
    bool[] linkAnswered;

    readonly List<SoftwareEntryView> softwareEntries = new List<SoftwareEntryView>();
    bool scanned;
    bool[] softwareUpdated, softwareFlagged;

    int penalty;
    int wrongConclusions;

    // ------------------------------------------------------------------ setup
    void Start()
    {
        int index = forceVariation >= 0
            ? Mathf.Clamp(forceVariation, 0, database.scenarios.Length - 1)
            : GameSession.PickChapter1Variation(database.scenarios.Length);
        scenario = database.scenarios[index];

        tabEmail.onClick.AddListener(() => ShowTab(Tab.Email));
        tabLink.onClick.AddListener(() => ShowTab(Tab.Link));
        tabSoftware.onClick.AddListener(() => ShowTab(Tab.Software));
        hintButton.onClick.AddListener(ShowHint);
        finishButton.onClick.AddListener(OnFinish);

        emailDetail.trashButton.onClick.AddListener(ToggleTrashSelected);
        notSafeButton.onClick.AddListener(() => AnswerLink(false));
        safeButton.onClick.AddListener(() => AnswerLink(true));
        scanButton.onClick.AddListener(Scan);

        // Kolom alamat bisa diketik: bit.ly/kode+ lalu Enter untuk melihat pratinjau tautan pendek.
        addressBar.readOnly = false;
        addressBar.lineType = TMP_InputField.LineType.SingleLine;
        addressBar.onSubmit.AddListener(OnAddressSubmit);
        if (!shownLink.TryGetComponent(out HoverRelay hover)) hover = shownLink.gameObject.AddComponent<HoverRelay>();
        shownLink.raycastTarget = true;
        hover.Enter = OnLinkHoverEnter;
        hover.Exit = OnLinkHoverExit;

        BuildInbox();
        BuildSoftwareList();
        ResetInvestigation();

        popup.Show("Chapter 1 - Investigasi", scenario.incidentReport, "Mulai Investigasi", null);
    }

    void BuildInbox()
    {
        // Hapus item contoh yang mungkin masih ada di scene.
        foreach (Transform child in inboxContainer)
            if (child.GetComponent<EmailInboxItem>()) Destroy(child.gameObject);

        for (int i = 0; i < scenario.emails.Length; i++)
        {
            var item = Instantiate(inboxItemPrefab, inboxContainer);
            item.name = "email_inbox_" + (i + 1);
            int idx = i;
            item.button.onClick.AddListener(() => SelectEmail(idx));
            inboxItems.Add(item);
        }
    }

    void BuildSoftwareList()
    {
        foreach (Transform child in softwareContainer)
            if (child.GetComponent<SoftwareEntryView>()) Destroy(child.gameObject);

        Vector2 start = ((RectTransform)softwareEntryPrefab.transform).anchoredPosition;
        for (int i = 0; i < scenario.software.Length; i++)
        {
            var e = Instantiate(softwareEntryPrefab, softwareContainer);
            e.name = "software_entry_" + (i + 1);
            float step = e.Rect.rect.height + softwareEntrySpacing;
            e.Rect.anchoredPosition = start - new Vector2(0f, step * i);

            int idx = i;
            e.updateButton.onClick.AddListener(() => UpdateSoftware(idx));
            // Klik entri = tandai/batalkan tanda sebagai celah keamanan (aksi 4).
            e.flagButton.onClick.AddListener(() => ToggleFlag(idx));
            softwareEntries.Add(e);
        }
        scanButton.transform.SetAsLastSibling();
    }

    /// <summary>Mulai (ulang) investigasi dari awal Chapter 1 dengan variasi yang sama.</summary>
    void ResetInvestigation()
    {
        int n = scenario.emails.Length;
        emailOpened = new bool[n];
        emailTrashed = new bool[n];
        selectedEmail = -1;

        linkIndex = 0;
        linkAnswered = new bool[scenario.links.Length];
        linkAnsweredSafe = new bool[scenario.links.Length];

        scanned = false;
        softwareUpdated = new bool[scenario.software.Length];
        softwareFlagged = new bool[scenario.software.Length];

        RefreshInbox();
        RefreshEmailDetail();
        RefreshLink();
        RefreshSoftware();
        ShowTab(Tab.Email);
    }

    // ------------------------------------------------------------------ tabs
    void ShowTab(Tab tab)
    {
        currentTab = tab;
        panelEmail.SetActive(tab == Tab.Email);
        panelLink.SetActive(tab == Tab.Link);
        panelSoftware.SetActive(tab == Tab.Software);
        SetTabColor(tabEmail, tab == Tab.Email);
        SetTabColor(tabLink, tab == Tab.Link);
        SetTabColor(tabSoftware, tab == Tab.Software);
        OnLinkHoverExit();
    }

    void SetTabColor(Button b, bool active)
    {
        if (b.targetGraphic) b.targetGraphic.color = active ? tabActiveColor : tabInactiveColor;
    }

    // ------------------------------------------------------------------ aksi 1: email
    void SelectEmail(int i)
    {
        selectedEmail = i;
        emailOpened[i] = true;
        RefreshInbox();
        RefreshEmailDetail();
    }

    void ToggleTrashSelected()
    {
        if (selectedEmail < 0) return;
        emailTrashed[selectedEmail] = !emailTrashed[selectedEmail];
        RefreshInbox();
        RefreshEmailDetail();
    }

    void RefreshInbox()
    {
        for (int i = 0; i < inboxItems.Count; i++)
        {
            var mail = scenario.emails[i];
            var item = inboxItems[i];
            item.title.text = (emailOpened[i] ? "" : "<b>") + mail.senderName + (emailOpened[i] ? "" : "</b>");
            item.subtitle.text = (emailTrashed[i] ? "[Dibuang] " : "") + mail.subject;
            if (item.background)
                item.background.color = emailTrashed[i] ? inboxTrashedColor
                                      : i == selectedEmail ? inboxSelectedColor : inboxNormalColor;
        }
    }

    void RefreshEmailDetail()
    {
        var v = emailDetail;
        // Isi email baru tampil setelah pemain mengklik salah satu email.
        bool has = selectedEmail >= 0;
        v.SetContentVisible(has);
        if (!has) return;

        var mail = scenario.emails[selectedEmail];
        v.title.text = mail.subject;
        v.senderName.text = mail.senderName;
        v.senderAddress.text = "<" + mail.senderAddress + ">";
        v.message.text = mail.body;
        v.SetAttachment(mail.attachmentName, mail.attachmentSize);
        if (v.trashLabel) v.trashLabel.text = emailTrashed[selectedEmail] ? "RESTORE" : "TRASH";
    }

    // ------------------------------------------------------------------ aksi 2: tautan
    void AnswerLink(bool safe)
    {
        if (linkIndex >= scenario.links.Length) return;
        linkAnswered[linkIndex] = true;
        linkAnsweredSafe[linkIndex] = safe;
        linkIndex++;
        RefreshLink();
    }

    void RefreshLink()
    {
        OnLinkHoverExit();
        addressBar.text = "";
        if (linkPreview) linkPreview.Hide();
        int total = scenario.links.Length;
        bool done = linkIndex >= total;
        notSafeButton.interactable = !done;
        safeButton.interactable = !done;
        if (linkCounter)
            linkCounter.text = done ? "Semua tautan sudah dianalisis" : $"Tautan {linkIndex + 1} dari {total}";
        shownLink.text = done
            ? "Semua tautan sudah dianalisis."
            : "<color=#1a5fd6><u>" + scenario.links[linkIndex].displayText + "</u></color>";
    }

    // Cara 2: hover menampilkan alamat tujuan di tooltip dan kolom alamat.
    // Alamat tetap di kolom alamat setelah kursor pergi supaya pemain bisa menambahkan "+".
    void OnLinkHoverEnter()
    {
        if (linkIndex >= scenario.links.Length) return;
        string url = scenario.links[linkIndex].targetUrl;
        if (!addressBar.isFocused) addressBar.text = url;
        if (hoverTooltip)
        {
            hoverTooltip.SetActive(true);
            hoverTooltipText.text = url;
        }
    }

    void OnLinkHoverExit()
    {
        if (hoverTooltip) hoverTooltip.SetActive(false);
    }

    // Cara 3: membuka bit.ly/kode+ di kolom alamat menampilkan tujuan asli tautan pendek.
    void OnAddressSubmit(string input)
    {
        string typed = NormalizeUrl(input);
        if (string.IsNullOrEmpty(typed)) return;

        if (typed.StartsWith("bit.ly/"))
        {
            if (typed.EndsWith("+"))
            {
                string code = typed.TrimEnd('+');
                string shortUrl = input.Trim();
                foreach (var l in scenario.links)
                {
                    if (l.IsShortLink && NormalizeUrl(l.targetUrl) == code)
                    {
                        linkPreview.ShowLink(shortUrl, l);
                        return;
                    }
                }
                linkPreview.ShowNotFound(shortUrl);
                return;
            }
            popup.Show("Tunggu Dulu!",
                "Membuka tautan pendek secara langsung bisa membawamu ke situs berbahaya tanpa kamu sadari.\n\n" +
                "Tambahkan tanda <b>+</b> di akhir tautan bit.ly (contoh: <b>bit.ly/abc123+</b>) lalu tekan Enter " +
                "untuk melihat pratinjau alamat tujuannya dengan aman.",
                "Mengerti", null);
            return;
        }

        popup.Show("Tunggu Dulu!",
            "Jangan membuka tautan yang belum kamu yakini keamanannya.\n\n" +
            "Baca alamatnya dengan teliti: perhatikan nama domain utama, HTTPS, dan ejaan yang meniru situs resmi. " +
            "Kolom alamat hanya dipakai untuk melihat pratinjau tautan pendek bit.ly dengan menambahkan <b>+</b>.",
            "Mengerti", null);
    }

    /// <summary>Hilangkan skema dan www., kecilkan huruf pada nama domain (kode tautan tetap peka huruf).</summary>
    static string NormalizeUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        string s = url.Trim();
        int scheme = s.IndexOf("://", System.StringComparison.Ordinal);
        if (scheme >= 0) s = s.Substring(scheme + 3);
        if (s.StartsWith("www.", System.StringComparison.OrdinalIgnoreCase)) s = s.Substring(4);
        int slash = s.IndexOf('/');
        return slash < 0 ? s.ToLowerInvariant() : s.Substring(0, slash).ToLowerInvariant() + s.Substring(slash);
    }

    // ------------------------------------------------------------------ aksi 3 & 4: perangkat lunak
    void Scan()
    {
        scanned = true;
        RefreshSoftware();
    }

    void UpdateSoftware(int i)
    {
        softwareUpdated[i] = true;
        RefreshSoftware();
    }

    void ToggleFlag(int i)
    {
        softwareFlagged[i] = !softwareFlagged[i];
        RefreshSoftware();
    }

    void RefreshSoftware()
    {
        scanButton.interactable = !scanned;
        var scanLabel = scanButton.GetComponentInChildren<TMP_Text>();
        if (scanLabel) scanLabel.text = scanned ? "Dipindai" : "Scan";

        for (int i = 0; i < softwareEntries.Count; i++)
        {
            var d = scenario.software[i];
            var e = softwareEntries[i];
            e.nameText.text = (softwareFlagged[i] ? "<color=#c00000>[CELAH]</color> " : "") + d.name;

            bool updated = softwareUpdated[i];
            e.versionText.text = scanned
                ? (updated ? $"versi {d.latestVersion} (sudah diperbarui)" : $"versi {d.installedVersion}  |  terbaru {d.latestVersion}")
                : $"versi {d.installedVersion}";
            if (e.descriptionText) e.descriptionText.text = scanned ? d.description : "Belum dipindai";

            e.marker.color = !scanned ? markerUnknown
                : updated ? markerGreen
                : d.status == SoftwareStatus.Red ? markerRed
                : d.status == SoftwareStatus.Yellow ? markerYellow : markerGreen;

            // Tombol Update hanya muncul pada entri kuning dan merah (Gambar 3.15b).
            e.updateButton.gameObject.SetActive(scanned && d.status != SoftwareStatus.Green);
            e.updateButton.interactable = !updated;
            if (e.updateLabel) e.updateLabel.text = updated ? "Updated" : "Update";

            if (e.background) e.background.color = softwareFlagged[i] ? entryFlaggedColor : entryNormalColor;
        }
    }

    // ------------------------------------------------------------------ HINT
    void ShowHint()
    {
        string body;
        switch (currentTab)
        {
            case Tab.Email:
                body = "Periksa alamat pengirim: apakah domainnya resmi (ptmaju.co.id)?\n" +
                       "Waspadai lampiran berekstensi ganda (.pdf.exe), program (.exe, .js), arsip yang diminta dijalankan, " +
                       "atau dokumen bermakro (.docm) yang meminta \"Enable Content\".\n\n" +
                       "Gunakan tombol TRASH untuk membuang email berbahaya.";
                break;
            case Tab.Link:
                body = "Ada tiga cara memeriksa tautan:\n" +
                       "1. <b>Lihat langsung</b> jika tautan sudah menampilkan alamat lengkapnya.\n" +
                       "2. <b>Arahkan kursor</b> ke tautan untuk melihat alamat tujuan sebenarnya di kolom alamat.\n" +
                       "3. Jika alamatnya <b>bit.ly</b>, tambahkan <b>+</b> di akhir (contoh: bit.ly/abc123+) di kolom alamat " +
                       "lalu tekan Enter untuk melihat tujuan aslinya.\n\n" +
                       "Perhatikan nama domain utama (bagian sebelum .com/.co.id), HTTPS, dan domain aneh seperti .ru, .xyz, .top.";
                break;
            default:
                body = "Tekan Scan untuk memeriksa versi perangkat lunak.\n" +
                       "Kuning = ada pembaruan, Merah = memiliki celah keamanan kritis.\n\n" +
                       "Klik entri perangkat lunak untuk menandainya sebagai [CELAH] keamanan, lalu tekan Update pada " +
                       "semua perangkat lunak yang perlu diperbarui.";
                break;
        }
        body += "\n\n<i>Ingat laporan awal:</i>\n" + scenario.incidentReport;
        popup.Show("Petunjuk", body, "Tutup", null);
    }

    // ------------------------------------------------------------------ FINISH
    void OnFinish()
    {
        var missing = new List<string>();
        for (int i = 0; i < emailOpened.Length; i++)
            if (!emailOpened[i]) { missing.Add("- Buka dan periksa semua email."); break; }
        if (linkIndex < scenario.links.Length) missing.Add("- Analisis semua tautan di tab Link.");
        if (!scanned) missing.Add("- Pindai (Scan) perangkat lunak di tab Software.");

        if (missing.Count > 0)
        {
            popup.Show("Investigasi Belum Lengkap",
                "Selesaikan dulu langkah berikut sebelum menyimpulkan:\n\n" + string.Join("\n", missing),
                "Kembali", null);
            return;
        }

        var options = new List<KeyValuePair<string, System.Action>>();
        foreach (var o in database.methodOptions)
        {
            var method = o.method;
            options.Add(new KeyValuePair<string, System.Action>(o.label, () => Conclude(method)));
        }
        popup.Show("Kesimpulan Investigasi",
            "Berdasarkan temuanmu, bagaimana peretas masuk ke sistem perusahaan?",
            "Kembali Menyelidiki", null, options);
    }

    void Conclude(EntryMethod chosen)
    {
        if (chosen != scenario.correctMethod)
        {
            // Metode masuk belum ditemukan: -10 poin, tampilkan petunjuk, ulangi investigasi.
            wrongConclusions++;
            penalty += WrongConclusionPenalty;
            popup.Show("Metode Masuk Belum Ditemukan",
                $"Kesimpulanmu belum tepat. Poin dikurangi {WrongConclusionPenalty}.\n\n<b>Petunjuk:</b> {scenario.failHint}\n\n" +
                "Investigasi akan diulang dari awal Chapter 1.",
                "Ulangi Investigasi", ResetInvestigation);
            return;
        }

        var summary = new StringBuilder();
        int score = 0;

        // Aksi 1: email
        bool emailOk = true;
        var emailNotes = new StringBuilder();
        for (int i = 0; i < scenario.emails.Length; i++)
        {
            var mail = scenario.emails[i];
            if (emailTrashed[i] != mail.malicious)
            {
                emailOk = false;
                emailNotes.Append($"\n   - \"{mail.subject}\" seharusnya {(mail.malicious ? "dibuang" : "disimpan")}: {mail.reason}");
            }
        }
        int emailScore = emailOk ? EmailPoints : 0;
        score += emailScore;
        summary.Append($"1. Periksa lampiran email: {emailScore}/{EmailPoints}{emailNotes}\n");

        // Aksi 2: tautan
        bool linkOk = true;
        var linkNotes = new StringBuilder();
        for (int i = 0; i < scenario.links.Length; i++)
        {
            var l = scenario.links[i];
            if (!linkAnswered[i] || linkAnsweredSafe[i] != l.safe)
            {
                linkOk = false;
                linkNotes.Append($"\n   - \"{l.displayText}\" ({(l.IsShortLink ? l.targetUrl + " -> " : "")}{l.FinalUrl}) seharusnya {(l.safe ? "Safe" : "Not Safe")}: {l.reason}");
            }
        }
        int linkScore = linkOk ? LinkPoints : 0;
        score += linkScore;
        summary.Append($"2. Analisis tautan: {linkScore}/{LinkPoints}{linkNotes}\n");

        // Aksi 3: periksa perangkat lunak (scan + update yang kuning/merah)
        bool checkOk = scanned;
        var checkNotes = new StringBuilder();
        for (int i = 0; i < scenario.software.Length; i++)
        {
            var s = scenario.software[i];
            if (s.status != SoftwareStatus.Green && !softwareUpdated[i])
            {
                checkOk = false;
                checkNotes.Append($"\n   - {s.name} {s.installedVersion} belum diperbarui ke {s.latestVersion}.");
            }
        }
        int checkScore = checkOk ? SoftwareCheckPoints : 0;
        score += checkScore;
        summary.Append($"3. Periksa perangkat lunak: {checkScore}/{SoftwareCheckPoints}{checkNotes}\n");

        // Aksi 4: identifikasi vulnerability (tandai tepat semua yang merah)
        bool vulnOk = true;
        var vulnNotes = new StringBuilder();
        for (int i = 0; i < scenario.software.Length; i++)
        {
            var s = scenario.software[i];
            bool isVuln = s.status == SoftwareStatus.Red;
            if (softwareFlagged[i] != isVuln)
            {
                vulnOk = false;
                vulnNotes.Append(isVuln
                    ? $"\n   - {s.name} memiliki celah: {s.description}"
                    : $"\n   - {s.name} tidak memiliki celah keamanan kritis.");
            }
        }
        int vulnScore = vulnOk ? VulnerabilityPoints : 0;
        score += vulnScore;
        summary.Append($"4. Identifikasi celah keamanan: {vulnScore}/{VulnerabilityPoints}{vulnNotes}\n");

        if (penalty > 0)
            summary.Append($"\nPengurangan ({wrongConclusions}x kesimpulan salah): -{penalty}\n");

        int total = Mathf.Clamp(score - penalty, 0, 100);
        string body =
            $"<b>Metode masuk peretas:</b> {database.MethodLabel(scenario.correctMethod)}\n\n" +
            summary +
            $"\n<b>Total Chapter 1: {total} / 100</b>";

        GameSession.Chapter1Score = total;
        GameSession.Chapter1Summary = body;

        popup.Show("Chapter 1 Selesai", body, "Lanjut ke Chapter 2", GoToNextChapter);
    }

    void GoToNextChapter()
    {
        if (!string.IsNullOrEmpty(nextSceneName) && Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }
        Debug.LogWarning($"Scene \"{nextSceneName}\" belum ada di Build Profiles. Skor Chapter 1 = {GameSession.Chapter1Score}.");
        popup.Show("Chapter 2", $"Scene Chapter 2 belum tersedia.\nSkor Chapter 1 tersimpan: {GameSession.Chapter1Score}/100.", "Tutup", null);
    }
}
