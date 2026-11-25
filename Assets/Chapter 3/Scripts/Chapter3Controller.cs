using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Chapter3
{
    /// <summary>
    /// Menghubungkan UI Chapter 3 (Gambar 3.18 dan 3.19) dengan aturan permainan di Chapter3Game (Gambar 3.9).
    /// </summary>
    public class Chapter3Controller : MonoBehaviour
    {
        [SerializeField] Chapter3Database database;

        [Header("Tab: File, Backup Protection, Activity")]
        [SerializeField] Button[] tabs;
        [SerializeField] GameObject[] panels;
        [SerializeField] Color tabActiveColor = new Color(0.57f, 1f, 0.45f);
        [SerializeField] Color tabInactiveColor = new Color(0.85f, 0.85f, 0.85f);

        [Header("Daftar")]
        [SerializeField] SystemCardView systemCardPrefab;
        [SerializeField] Transform systemContainer;
        [SerializeField] BackupServerCardView serverCardPrefab;
        [SerializeField] Transform serverContainer;
        [SerializeField] ActivityRowView activityRowPrefab;
        [SerializeField] Transform activityContainer;
        [SerializeField] GameObject activityEmptyText;

        [Header("Panel kanan")]
        [SerializeField] TMP_Text timeText;
        [SerializeField] Image containmentImage;
        [SerializeField] TMP_Text containmentText;
        [SerializeField] Color uncontainedColor = new Color(1f, 0.38f, 0.36f);
        [SerializeField] Color containedColor = new Color(0.5f, 0.9f, 0.5f);
        [SerializeField] Image staffBoxPrefab;
        [SerializeField] Transform staffContainer;
        [SerializeField] Color staffFreeColor = new Color(0.6f, 1f, 0.66f);
        [SerializeField] Color staffBusyColor = new Color(0.62f, 0.62f, 0.62f);
        [SerializeField] TMP_Text moneyText;
        [SerializeField] Button hireButton;

        [Header("Jendela Investigate Backup")]
        [SerializeField] GameObject backupWindow;
        [SerializeField] Button investigateButton;
        [SerializeField] Button closeWindowButton;
        [SerializeField] BackupRowView backupRowPrefab;
        [SerializeField] Transform backupRowContainer;

        [Header("Informasi, popup & alur")]
        [SerializeField] TMP_Text logText;
        [SerializeField] UIPopup popup;
        [SerializeField] string nextSceneName = "UI_final_result";

        const int MaxLogLines = 40;
        readonly List<string> log = new List<string>();
        readonly List<SystemCardView> systemCards = new List<SystemCardView>();
        readonly List<BackupServerCardView> serverCards = new List<BackupServerCardView>();
        readonly List<BackupRowView> backupRows = new List<BackupRowView>();
        readonly List<ActivityRowView> activityRows = new List<ActivityRowView>();
        readonly List<Image> staffBoxes = new List<Image>();
        Chapter3Game game;
        bool running;

        void Start()
        {
            // Scene dibuka langsung tanpa Chapter 2: pakai cadangan lengkap supaya bisa dicoba.
            bool standalone = GameSession.Chapter2Score < 0;
            var records = standalone
                ? database.systems.Where(s => s && s.backupSource).Select(s => new GameSession.BackupRecord
                    { fileName = s.backupSource.displayName, isBackedUp = true, dataLossHours = 4, isOffsite = true }).ToList()
                : GameSession.Chapter2Backups;
            game = new Chapter3Game(database, records, standalone || GameSession.Chapter2HasIncidentResponse,
                standalone ? database.testMoney : GameSession.Chapter2MoneyLeft);
            game.Logged += AddLog;

            for (int i = 0; i < tabs.Length; i++)
            {
                int index = i;
                tabs[i].onClick.AddListener(() => ShowTab(index));
            }
            hireButton.onClick.AddListener(Hire);
            investigateButton.onClick.AddListener(() => { game.Investigate(); Refresh(); });
            closeWindowButton.onClick.AddListener(() => backupWindow.SetActive(false));

            BuildLists();
            backupWindow.SetActive(false);
            ShowTab(0);
            if (standalone) AddLog("Mode uji: data Chapter 2 tidak ada, semua sistem dianggap dicadangkan setiap 4 jam secara offsite.");
            Refresh();
            popup.Show("Chapter 3 - Serangan Ransomware", database.introduction, "Tangani Serangan", AskResponse);
        }

        void Update()
        {
            // Waktu berhenti selama popup terbuka.
            if (!running || popup.IsOpen) return;
            game.Tick(Time.deltaTime);
            Refresh();
            if (game.Finished && game.Activities.Count == 0) ShowResult();
        }

        // ------------------------------------------------------------------ tampilan
        void BuildLists()
        {
            HideSamples(systemContainer);
            foreach (var s in game.Systems)
            {
                var card = Instantiate(systemCardPrefab, systemContainer);
                card.Show(s.data);
                card.backupsButton.onClick.AddListener(() => backupWindow.SetActive(true));
                card.isolateButton.onClick.AddListener(() => { game.Isolate(s); Refresh(); });
                card.reconnectButton.onClick.AddListener(() => { game.Reconnect(s); Refresh(); });
                systemCards.Add(card);
            }

            HideSamples(serverContainer);
            foreach (var s in game.Servers)
            {
                var card = Instantiate(serverCardPrefab, serverContainer);
                card.Show(s.data);
                card.isolateButton.onClick.AddListener(() => { game.Isolate(s); Refresh(); });
                serverCards.Add(card);
            }

            HideSamples(backupRowContainer);
            foreach (var s in game.Systems)
            {
                var row = Instantiate(backupRowPrefab, backupRowContainer);
                row.verifyButton.onClick.AddListener(() => { game.Verify(s); Refresh(); });
                row.restoreButton.onClick.AddListener(() => AskRestore(s));
                backupRows.Add(row);
            }

            HideSamples(activityContainer);
            HideSamples(staffContainer);
        }

        void ShowTab(int index)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                panels[i].SetActive(i == index);
                tabs[i].targetGraphic.color = i == index ? tabActiveColor : tabInactiveColor;
            }
        }

        void Refresh()
        {
            for (int i = 0; i < systemCards.Count; i++) systemCards[i].Refresh(game.Systems[i], game.ResponseActivated);
            for (int i = 0; i < serverCards.Count; i++) serverCards[i].Refresh(game.Servers[i]);
            for (int i = 0; i < backupRows.Count; i++) backupRows[i].Refresh(game.Systems[i], game);

            while (activityRows.Count < game.Activities.Count) activityRows.Add(Instantiate(activityRowPrefab, activityContainer));
            for (int i = 0; i < activityRows.Count; i++)
            {
                bool used = i < game.Activities.Count;
                activityRows[i].gameObject.SetActive(used);
                if (used) activityRows[i].Refresh(game.Activities[i]);
            }
            activityEmptyText.SetActive(game.Activities.Count == 0);

            while (staffBoxes.Count < game.StaffTotal) staffBoxes.Add(Instantiate(staffBoxPrefab, staffContainer));
            for (int i = 0; i < staffBoxes.Count; i++)
                staffBoxes[i].color = i < game.StaffFree ? staffFreeColor : staffBusyColor;
            hireButton.interactable = game.ContractorAvailable;
            moneyText.text = "Sisa uang\n<b>" + Rupiah(game.Money) + "</b>";

            int minutes = database.startHour * 60 + Mathf.FloorToInt(game.Minutes);
            timeText.text = $"{minutes / 60 % 24:00}:{minutes % 60:00}";
            containmentImage.color = game.Contained ? containedColor : uncontainedColor;
            containmentText.text = game.Contained ? "Contained" : "Not Contained";
            investigateButton.interactable = !game.Investigated && !game.Investigating;
        }

        void AddLog(string message)
        {
            int minutes = database.startHour * 60 + Mathf.FloorToInt(game.Minutes);
            log.Insert(0, $"[{minutes / 60 % 24:00}:{minutes % 60:00}] {message}"); // terbaru di atas
            if (log.Count > MaxLogLines) log.RemoveAt(MaxLogLines);
            logText.text = string.Join("\n", log);
        }

        // Objek contoh di scene hanya untuk melihat tata letak di Editor.
        static void HideSamples(Transform container)
        {
            foreach (Transform child in container) child.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ aksi 1: Incident Response
        void AskResponse()
        {
            var options = new List<KeyValuePair<string, Action>>();
            foreach (var o in database.responseOptions)
            {
                var option = o;
                options.Add(new KeyValuePair<string, Action>(option.label, () => ChooseResponse(option)));
            }
            popup.Show("Tindakan Pertama", "Apa yang pertama kali kamu lakukan?", "", null, options);
        }

        void ChooseResponse(ResponseOption option)
        {
            string feedback = game.ChooseResponse(option);
            AddLog(game.ResponseActivated ? "Rencana Incident Response diaktifkan. Sistem terinfeksi teridentifikasi."
                                          : "Penanganan berjalan tanpa rencana Incident Response.");
            Refresh();
            popup.Show(game.ResponseActivated ? "Rencana IR Aktif" : "Tanpa Rencana IR", feedback, "Lanjut", () => running = true);
        }

        void Hire()
        {
            if (!game.CanHire)
            {
                popup.Show("Uang Tidak Cukup",
                    $"Biaya kontraktor adalah {Rupiah(database.contractorCost)}, sedangkan sisa uangmu {Rupiah(game.Money)}.",
                    "Kembali", null);
                return;
            }
            popup.Show("Hire Contractor",
                $"Kontraktor menambah satu tenaga IT dengan biaya {Rupiah(database.contractorCost)}. Sisa uangmu {Rupiah(game.Money)}.",
                "Batal", null,
                new[] { new KeyValuePair<string, Action>("Gunakan kontraktor", () => { game.Hire(); Refresh(); }) });
        }

        // ------------------------------------------------------------------ aksi 3 dan 4: pemulihan
        void AskRestore(SystemState s)
        {
            if (!s.data.isInfected)
            {
                popup.Show("Tidak Perlu Dipulihkan", s.data.displayName + " tidak terinfeksi, sehingga datanya tidak perlu dipulihkan.", "Kembali", null);
                return;
            }
            string body = $"Pulihkan {s.data.displayName} dari cadangan? Pemulihan hanya dapat dilakukan satu kali untuk setiap sistem.";
            if (game.FirstRestored == null)
                body += "\n\nIni adalah sistem yang kamu pulihkan <b>lebih dulu</b>. Isolasi sistem juga dinilai saat pemulihan pertama dimulai.";
            popup.Show("Restore Backup", body, "Batal", null,
                new[] { new KeyValuePair<string, Action>("Ya, pulihkan", () => { game.Restore(s); Refresh(); }) });
        }

        // ------------------------------------------------------------------ hasil
        void ShowResult()
        {
            running = false;
            backupWindow.SetActive(false);
            GameSession.Chapter3Score = game.CalculateScore(out GameSession.Chapter3Summary);

            if (game.Recovered)
            {
                popup.Show("Pemulihan Berhasil", "Semua sistem yang terinfeksi berhasil dipulihkan dari cadangan.", "Lihat Skor", ShowSummary);
                return;
            }
            var failed = game.Systems.Where(s => s.outcome == Outcome.Failed).Select(s => "- " + s.data.displayName);
            popup.Show("Pemulihan Gagal",
                "Sistem berikut gagal dipulihkan:\n" + string.Join("\n", failed) +
                $"\n\n<b>Kerugian perusahaan: {Rupiah(game.CalculateLoss())}</b>\n\nPoin dikurangi {Chapter3Game.FailurePenalty}.",
                "Lihat Skor", ShowSummary);
        }

        void ShowSummary() =>
            popup.Show("Chapter 3 Selesai", GameSession.Chapter3Summary, "Lanjut ke Hasil Akhir", GoToFinalResult);

        void GoToFinalResult()
        {
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
                return;
            }
            popup.Show("Hasil Akhir", $"Layar hasil akhir belum tersedia.\nSkor Chapter 3 tersimpan: {GameSession.Chapter3Score}/100.",
                "Kembali ke Ringkasan", ShowSummary);
        }

        public static string Rupiah(long amount) =>
            "Rp" + amount.ToString("N0", CultureInfo.InvariantCulture).Replace(',', '.');
    }
}
