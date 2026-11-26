using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Chapter2
{
    public enum ChoiceResult
    {
        Accepted,
        NeedStorage, // storage kurang, uang cukup untuk menambah storage
        NoBudget,    // storage kurang dan uang juga kurang (-5 poin)
        NoMoney,     // uang tidak cukup untuk biaya lokasi penyimpanan
    }

    /// <summary>Keputusan pemain untuk satu berkas/sistem. null = belum dipilih.</summary>
    public class BackupChoice
    {
        public Chapter2BackupStrategy file;
        public FrequencyOption frequency;
        public StorageOption storage;

        public bool IsBackedUp => frequency != null && frequency.isBackup;
        public int StorageNeeded => StorageFor(frequency, storage);
        public int Cost => IsBackedUp && storage != null ? storage.cost : 0;

        /// <summary>Ukuran data x pengali frekuensi x jumlah salinan.</summary>
        public int StorageFor(FrequencyOption f, StorageOption s) =>
            f != null && f.isBackup ? Mathf.CeilToInt(file.sizeGB * f.storageMultiplier * (s != null ? s.Copies : 1)) : 0;
    }

    /// <summary>
    /// Aturan Chapter 2 (Gambar 3.8), tanpa UI:
    ///   Pilih berkas/sistem penting (+30), frekuensi (4 jam +25 / 24 jam +10), pemilihan selesai (+15),
    ///   lokasi (berbeda/offsite +20 / terhubung +5), DR dan IR (+10). Storage dan uang tidak cukup: -5.
    /// Item kesiapan (pelatihan, monitoring, infrastruktur DR) tidak memberi poin, tetapi membantu pada Chapter 3.
    /// </summary>
    public class Chapter2Game
    {
        public const int SelectionPoints = 30;
        public const int SelectionDonePoints = 15;
        public const int PlanPoints = 10;
        public const int NoBudgetPenalty = 5;

        readonly Chapter2Database database;
        readonly HashSet<Chapter2ShopItem> owned = new HashSet<Chapter2ShopItem>();
        int spentOnItems;
        int noBudgetCount;

        public readonly List<BackupChoice> Choices = new List<BackupChoice>();
        public int StorageTotal { get; private set; }
        public bool HasDisasterRecovery { get; private set; }
        public bool HasIncidentResponse { get; private set; }
        public bool HasTraining { get; private set; }
        /// <summary>Menit sampai serangan terdeteksi; -1 = tanpa monitoring.</summary>
        public int DetectMinutes { get; private set; } = -1;
        public float RestoreMultiplier { get; private set; } = 1f;

        public int StorageUsed => Choices.Sum(c => c.StorageNeeded);
        public int StorageFree => StorageTotal - StorageUsed;
        public int Money => database.startingMoney - spentOnItems - Choices.Sum(c => c.Cost);

        public Chapter2Game(Chapter2Database database)
        {
            this.database = database;
            StorageTotal = database.startingStorageGB;
            foreach (var file in database.backupStrategies)
                if (file) Choices.Add(new BackupChoice { file = file });
        }

        // ------------------------------------------------------------------ Backup Strategy
        public ChoiceResult SetFrequency(BackupChoice choice, FrequencyOption option, out int missingGB)
        {
            var storage = option.isBackup ? choice.storage : null;
            return Apply(choice, option, storage, out missingGB);
        }

        public ChoiceResult SetStorage(BackupChoice choice, StorageOption option, out int missingGB)
        {
            missingGB = 0;
            if (!choice.IsBackedUp) return ChoiceResult.Accepted;
            return Apply(choice, choice.frequency, option, out missingGB);
        }

        ChoiceResult Apply(BackupChoice choice, FrequencyOption f, StorageOption s, out int missingGB)
        {
            int extraGB = choice.StorageFor(f, s) - choice.StorageNeeded;
            missingGB = extraGB - StorageFree;
            if (extraGB > 0 && missingGB > 0)
            {
                if (CanAffordStorage(missingGB)) return ChoiceResult.NeedStorage;
                noBudgetCount++;
                return ChoiceResult.NoBudget;
            }
            int newCost = f != null && f.isBackup && s != null ? s.cost : 0;
            if (newCost - choice.Cost > Money) return ChoiceResult.NoMoney;

            choice.frequency = f;
            choice.storage = s;
            return ChoiceResult.Accepted;
        }

        /// <summary>Apakah sisa uang cukup untuk membeli tambahan storage sebesar ini (boleh beberapa item).</summary>
        bool CanAffordStorage(int missingGB)
        {
            var items = database.shopItems.Where(i => i && i.effect == ShopItemEffect.AddStorage && i.storageGB > 0).ToArray();
            var cheapest = new long[missingGB + 1]; // biaya termurah untuk memperoleh minimal n GB
            for (int gb = 1; gb <= missingGB; gb++)
            {
                cheapest[gb] = long.MaxValue;
                foreach (var item in items)
                {
                    long rest = cheapest[Mathf.Max(0, gb - item.storageGB)];
                    if (rest != long.MaxValue) cheapest[gb] = System.Math.Min(cheapest[gb], rest + item.price);
                }
            }
            return cheapest[missingGB] <= Money;
        }

        // ------------------------------------------------------------------ Shop
        public bool IsOwned(Chapter2ShopItem item) => owned.Contains(item);
        public bool CanBuy(Chapter2ShopItem item) => !IsOwned(item) && item.price <= Money;

        public bool Buy(Chapter2ShopItem item)
        {
            if (!CanBuy(item)) return false;
            spentOnItems += item.price;
            if (item.effect != ShopItemEffect.AddStorage) owned.Add(item); // selain storage hanya dibeli sekali
            switch (item.effect)
            {
                case ShopItemEffect.AddStorage: StorageTotal += item.storageGB; break;
                case ShopItemEffect.DisasterRecovery: HasDisasterRecovery = true; break;
                case ShopItemEffect.IncidentResponse: HasIncidentResponse = true; break;
                case ShopItemEffect.Training: HasTraining = true; break;
                case ShopItemEffect.Monitoring:
                    DetectMinutes = DetectMinutes < 0 ? item.detectMinutes : Mathf.Min(DetectMinutes, item.detectMinutes);
                    break;
                case ShopItemEffect.RecoverySite: RestoreMultiplier = Mathf.Min(RestoreMultiplier, item.restoreMultiplier); break;
            }
            return true;
        }

        // ------------------------------------------------------------------ Finish
        /// <summary>Keputusan yang belum diambil; kosong berarti siap diselesaikan.</summary>
        public List<string> MissingDecisions()
        {
            var missing = new List<string>();
            foreach (var c in Choices)
            {
                if (c.frequency == null) missing.Add($"- {c.file.displayName}: pilih Backup Frequency.");
                else if (c.IsBackedUp && c.storage == null) missing.Add($"- {c.file.displayName}: pilih Storage Strategy.");
            }
            return missing;
        }

        public int CalculateScore(out string summary)
        {
            var s = new StringBuilder();
            var backedUp = Choices.Where(c => c.IsBackedUp).ToList();
            int score = 0;

            // Pilih berkas/sistem penting: penuh jika tepat, 0 jika tidak.
            var wrong = Choices.Where(c => c.IsBackedUp != c.file.isImportant).ToList();
            int selection = backedUp.Count > 0 && wrong.Count == 0 ? SelectionPoints : 0;
            score += selection;
            s.Append($"1. Pilih berkas/sistem penting: {selection}/{SelectionPoints}\n");
            foreach (var c in wrong)
                s.Append($"   - {c.file.displayName} seharusnya {(c.file.isImportant ? "dicadangkan" : "tidak dicadangkan")}: {c.file.reason}\n");

            // Frekuensi dan lokasi memakai pilihan terendah di antara berkas yang dicadangkan.
            int frequency = backedUp.Count > 0 ? backedUp.Min(c => c.frequency.points) : 0;
            int frequencyMax = Max(c => c.file.frequencyOptions.Select(o => o.points));
            score += frequency;
            s.Append($"2. Frekuensi pencadangan: {frequency}/{frequencyMax}\n");
            foreach (var c in backedUp) s.Append($"   - {c.file.displayName}: {c.frequency.label}. {c.frequency.description}\n");

            int done = backedUp.Count > 0 ? SelectionDonePoints : 0;
            score += done;
            s.Append($"3. Pemilihan berkas selesai: {done}/{SelectionDonePoints}\n");

            int location = backedUp.Count > 0 ? backedUp.Min(c => c.storage.points) : 0;
            int locationMax = Max(c => c.file.storageOptions.Select(o => o.points));
            score += location;
            s.Append($"4. Lokasi server cadangan: {location}/{locationMax}\n");
            foreach (var c in backedUp) s.Append($"   - {c.file.displayName}: {c.storage.label}. {c.storage.description}\n");

            int plan = HasDisasterRecovery && HasIncidentResponse ? PlanPoints : 0;
            score += plan;
            s.Append($"5. Rencana DR dan IR: {plan}/{PlanPoints}\n");
            if (plan == 0)
                s.Append($"   - Belum disiapkan: {string.Join(" dan ", MissingPlans())}.\n");

            var extras = new List<string>();
            if (HasTraining) extras.Add("pelatihan IR");
            if (DetectMinutes >= 0) extras.Add($"monitoring (deteksi ~{DetectMinutes} menit)");
            if (RestoreMultiplier < 1f) extras.Add("infrastruktur DR");
            if (extras.Count > 0) s.Append($"\nKesiapan tambahan untuk Chapter 3: {string.Join(", ", extras)}.\n");

            int penalty = noBudgetCount * NoBudgetPenalty;
            if (penalty > 0) s.Append($"\nPengurangan ({noBudgetCount}x anggaran tidak cukup): -{penalty}\n");

            int total = Mathf.Clamp(score - penalty, 0, 100);
            s.Append($"\n<b>Total Chapter 2: {total} / 100</b>");
            summary = s.ToString();
            return total;
        }

        public List<string> MissingPlans()
        {
            var plans = new List<string>();
            if (!HasDisasterRecovery) plans.Add("Disaster Recovery (DR)");
            if (!HasIncidentResponse) plans.Add("Incident Response (IR)");
            return plans;
        }

        int Max(System.Func<BackupChoice, IEnumerable<int>> points) =>
            Choices.SelectMany(points).DefaultIfEmpty(0).Max();
    }
}
