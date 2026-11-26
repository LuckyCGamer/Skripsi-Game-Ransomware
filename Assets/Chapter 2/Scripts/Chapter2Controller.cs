using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Chapter2
{
    /// <summary>
    /// Menghubungkan UI Chapter 2 (Gambar 3.16 dan 3.17) dengan aturan permainan di Chapter2Game (Gambar 3.8).
    /// </summary>
    public class Chapter2Controller : MonoBehaviour
    {
        [SerializeField] Chapter2Database database;

        [Header("Header")]
        [SerializeField] TMP_Text moneyText;
        [SerializeField] TMP_Text storageText;

        [Header("Navigasi")]
        [SerializeField] Button backupTab;
        [SerializeField] Button shopTab;
        [SerializeField] Button finishButton;
        [SerializeField] GameObject backupPanel;
        [SerializeField] GameObject shopPanel;
        [SerializeField] Color tabActiveColor = Color.white;
        [SerializeField] Color tabInactiveColor = new Color(0.75f, 0.75f, 0.75f);

        [Header("Backup Strategy")]
        [SerializeField] BackupCardView cardPrefab;
        [SerializeField] Transform cardContainer;

        [Header("Shop")]
        [SerializeField] ShopItemView shopItemPrefab;
        [SerializeField] Transform shopContainer;
        [SerializeField] ShopDetailView shopDetail;
        [SerializeField] Color itemNormalColor = Color.white;
        [SerializeField] Color itemSelectedColor = new Color(0.7f, 0.85f, 1f);

        [Header("Popup & alur")]
        [SerializeField] UIPopup popup;
        [SerializeField] string nextSceneName = "UI_chapter3_scenario";

        readonly List<BackupCardView> cards = new List<BackupCardView>();
        readonly List<ShopItemView> shopItems = new List<ShopItemView>();
        Chapter2Game game;
        ShopItemView selectedItem;

        void Start()
        {
            game = new Chapter2Game(database);

            backupTab.onClick.AddListener(() => ShowTab(false));
            shopTab.onClick.AddListener(() => ShowTab(true));
            finishButton.onClick.AddListener(OnFinish);
            shopDetail.buyButton.onClick.AddListener(BuySelected);

            HideSamples(cardContainer);
            foreach (var choice in game.Choices)
            {
                var card = Instantiate(cardPrefab, cardContainer);
                card.Show(choice, o => ChooseFrequency(choice, o), o => ChooseStorage(choice, o));
                cards.Add(card);
            }

            HideSamples(shopContainer);
            foreach (var data in database.shopItems)
            {
                if (!data) continue;
                var item = Instantiate(shopItemPrefab, shopContainer);
                item.Show(data);
                item.button.onClick.AddListener(() => SelectItem(item));
                shopItems.Add(item);
            }
            shopDetail.gameObject.SetActive(shopItems.Count > 0);
            if (shopItems.Count > 0) selectedItem = shopItems[0];

            ShowTab(false);
            Refresh();
            popup.Show("Chapter 2 - Persiapan Pertahanan", database.introduction, "Mulai", null);
        }

        // ------------------------------------------------------------------ tampilan
        void ShowTab(bool shop)
        {
            backupPanel.SetActive(!shop);
            shopPanel.SetActive(shop);
            backupTab.targetGraphic.color = shop ? tabInactiveColor : tabActiveColor;
            shopTab.targetGraphic.color = shop ? tabActiveColor : tabInactiveColor;
        }

        void Refresh()
        {
            moneyText.text = "Sisa uang: " + Rupiah(game.Money);
            storageText.text = $"Sisa storage: {game.StorageFree} GB / {game.StorageTotal} GB";
            foreach (var card in cards) card.Refresh();
            foreach (var item in shopItems)
            {
                item.Refresh(game.IsOwned(item.Data), game.CanBuy(item.Data));
                item.background.color = item == selectedItem ? itemSelectedColor : itemNormalColor;
            }
            if (selectedItem)
                shopDetail.Show(selectedItem.Data, game.IsOwned(selectedItem.Data), game.CanBuy(selectedItem.Data));
        }

        // Objek contoh di scene hanya untuk melihat tata letak di Editor.
        static void HideSamples(Transform container)
        {
            foreach (Transform child in container) child.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ Backup Strategy
        void ChooseFrequency(BackupChoice choice, FrequencyOption option)
        {
            var result = game.SetFrequency(choice, option, out int missingGB);
            Refresh();
            ShowChoiceResult(choice, result, missingGB, choice.storage);
        }

        void ShowChoiceResult(BackupChoice choice, ChoiceResult result, int missingGB, StorageOption storage)
        {
            if (result == ChoiceResult.NeedStorage)
            {
                popup.Show("Storage Tidak Cukup",
                    $"{choice.file.displayName} membutuhkan tambahan {missingGB} GB. Uangmu masih cukup untuk menambah storage.\n\n" +
                    "Beli storage di Shop, lalu pilih kembali.",
                    "Batal", null,
                    new[] { new KeyValuePair<string, Action>("Buka Shop", () => ShowTab(true)) });
            }
            else if (result == ChoiceResult.NoBudget)
            {
                popup.Show("Anggaran Tidak Cukup",
                    $"{choice.file.displayName} membutuhkan tambahan {missingGB} GB, tetapi sisa uang tidak cukup untuk menambah storage. " +
                    $"Poin dikurangi {Chapter2Game.NoBudgetPenalty}.\n\nPilih kembali berkas atau sistem yang akan dicadangkan.",
                    "Kembali", null);
            }
            else if (result == ChoiceResult.NoMoney && storage != null)
            {
                popup.Show("Uang Tidak Cukup",
                    $"Biaya {storage.label} untuk {choice.file.displayName} adalah {Rupiah(storage.cost)}, sedangkan sisa uangmu {Rupiah(game.Money)}.",
                    "Kembali", null);
            }
        }

        void ChooseStorage(BackupChoice choice, StorageOption option)
        {
            var result = game.SetStorage(choice, option, out int missingGB);
            Refresh();
            ShowChoiceResult(choice, result, missingGB, option);
        }

        // ------------------------------------------------------------------ Shop
        void SelectItem(ShopItemView item)
        {
            selectedItem = item;
            Refresh();
        }

        void BuySelected()
        {
            if (!selectedItem) return;
            game.Buy(selectedItem.Data);
            Refresh();
        }

        // ------------------------------------------------------------------ Finish
        void OnFinish()
        {
            var missing = game.MissingDecisions();
            if (missing.Count > 0)
            {
                popup.Show("Persiapan Belum Lengkap",
                    "Tentukan dulu keputusan berikut:\n\n" + string.Join("\n", missing), "Kembali", null);
                return;
            }

            string body = "Pemilihan berkas dan lokasi cadangan akan diselesaikan, dan keputusanmu tidak dapat diubah lagi.";
            var plans = game.MissingPlans();
            if (plans.Count > 0)
                body += $"\n\n<b>Belum disiapkan:</b> {string.Join(" dan ", plans)}. Rencana ini dapat dibeli di Shop.";
            popup.Show("Selesaikan Persiapan?", body, "Kembali", null,
                new[] { new KeyValuePair<string, Action>("Ya, selesaikan", Complete) });
        }

        void Complete()
        {
            int score = game.CalculateScore(out string summary);

            // Dasar pemulihan data pada Chapter 3.
            GameSession.Chapter2Score = score;
            GameSession.Chapter2Summary = summary;
            GameSession.Chapter2HasDisasterRecovery = game.HasDisasterRecovery;
            GameSession.Chapter2HasIncidentResponse = game.HasIncidentResponse;
            GameSession.Chapter2HasTraining = game.HasTraining;
            GameSession.Chapter2DetectMinutes = game.DetectMinutes;
            GameSession.Chapter2RestoreMultiplier = game.RestoreMultiplier;
            GameSession.Chapter2MoneyLeft = game.Money;
            GameSession.Chapter2Backups.Clear();
            foreach (var c in game.Choices)
                GameSession.Chapter2Backups.Add(new GameSession.BackupRecord
                {
                    fileName = c.file.displayName,
                    isBackedUp = c.IsBackedUp,
                    intervalHours = c.IsBackedUp ? c.frequency.intervalHours : 0,
                    locations = c.IsBackedUp ? c.storage.locations : new BackupLocation[0],
                    storageLabel = c.IsBackedUp ? c.storage.label : "",
                });

            ShowSummary();
        }

        void ShowSummary() =>
            popup.Show("Chapter 2 Selesai", GameSession.Chapter2Summary, "Lanjut ke Chapter 3", GoToNextChapter);

        void GoToNextChapter()
        {
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
                return;
            }
            // Kembali ke ringkasan supaya pemain tidak mengubah keputusan yang sudah dinilai.
            popup.Show("Chapter 3", $"Scene Chapter 3 belum tersedia.\nSkor Chapter 2 tersimpan: {GameSession.Chapter2Score}/100.",
                "Kembali ke Ringkasan", ShowSummary);
        }

        public static string Rupiah(int amount) =>
            "Rp" + amount.ToString("N0", CultureInfo.InvariantCulture).Replace(',', '.');
    }
}
