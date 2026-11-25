using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Chapter3
{
    public enum Outcome { None, Restored, Failed }
    public enum BackupHealth { Good, Missing, Encrypted }

    public class SystemState
    {
        public Chapter3System data;
        public GameSession.BackupRecord record; // keputusan Chapter 2; null = tidak dicadangkan
        public ServerState server;
        public bool isolated, busy, verified;
        public Outcome outcome;
        public string failureReason;

        public bool HasBackup => record != null && record.isBackedUp;
        public bool NeedsRestore => data.isInfected && outcome == Outcome.None;
        public BackupHealth Health =>
            !HasBackup ? BackupHealth.Missing
            : server != null && server.encrypted ? BackupHealth.Encrypted : BackupHealth.Good;
    }

    public class ServerState
    {
        public Chapter3BackupServer data;
        public bool isolated, busy, encrypted;
    }

    public class Activity
    {
        public string label;
        public float duration, elapsed;
        public Action onComplete;
        public float Progress => Mathf.Clamp01(elapsed / duration);
    }

    /// <summary>
    /// Aturan Chapter 3 (Gambar 3.9), tanpa UI:
    ///   Aktifkan rencana IR (+20), isolasi sistem terinfeksi (+20), tentukan sistem yang dipulihkan lebih dulu (+30),
    ///   pulihkan data dari backup (+30). Pemulihan gagal: -30.
    /// </summary>
    public class Chapter3Game
    {
        public const int ResponsePoints = 20;
        public const int IsolationPoints = 20;
        public const int PriorityPoints = 30;
        public const int RestorePoints = 30;
        public const int FailurePenalty = 30;

        readonly Chapter3Database database;
        readonly bool hasIncidentResponse;
        readonly int startingMoney;
        SystemState expectedFirst;
        bool isolationCorrect;

        public readonly List<SystemState> Systems = new List<SystemState>();
        public readonly List<ServerState> Servers = new List<ServerState>();
        public readonly List<Activity> Activities = new List<Activity>();
        public event Action<string> Logged;

        public float Minutes { get; private set; }
        public int Contractors { get; private set; }
        public bool ResponseActivated { get; private set; }
        public bool Investigated { get; private set; }
        public bool Investigating { get; private set; }
        public SystemState FirstRestored { get; private set; }

        public int StaffTotal => database.staffCount + Contractors;
        public int StaffFree => StaffTotal - Activities.Count;
        public bool ContractorAvailable => Contractors < database.maxContractors;
        public bool CanHire => ContractorAvailable && Money >= database.contractorCost;
        public bool Contained => Systems.All(s => !s.data.isInfected || s.isolated || s.outcome == Outcome.Restored);
        public bool Finished => Systems.All(s => !s.NeedsRestore);
        public bool Recovered => Systems.All(s => !s.data.isInfected || s.outcome == Outcome.Restored);
        public int ContractorCost => Contractors * database.contractorCost;
        public int Money => startingMoney - ContractorCost;

        public Chapter3Game(Chapter3Database database, IList<GameSession.BackupRecord> records, bool hasIncidentResponse, int money)
        {
            this.database = database;
            this.hasIncidentResponse = hasIncidentResponse;
            startingMoney = money;
            foreach (var d in database.backupServers)
                if (d) Servers.Add(new ServerState { data = d });
            foreach (var d in database.systems)
            {
                if (!d) continue;
                var record = d.backupSource ? records.FirstOrDefault(r => r.fileName == d.backupSource.displayName) : null;
                Systems.Add(new SystemState
                {
                    data = d,
                    record = record,
                    server = record == null ? null : Servers.FirstOrDefault(s => s.data.isOffsite == record.isOffsite),
                });
            }
        }

        // ------------------------------------------------------------------ waktu dan aktivitas
        public void Tick(float seconds)
        {
            Minutes += seconds * database.minutesPerSecond;

            foreach (var a in Activities.ToArray())
            {
                a.elapsed += seconds;
                if (a.elapsed < a.duration) continue;
                Activities.Remove(a);
                a.onComplete();
            }

            if (Contained || Minutes < database.localBackupEncryptMinutes) return;
            foreach (var s in Servers)
            {
                if (s.data.isOffsite || s.isolated || s.encrypted) continue;
                s.encrypted = true;
                Log($"{s.data.displayName} ikut terenkripsi karena serangan belum terkendali.");
            }
        }

        bool StartActivity(string label, float duration, Action onComplete)
        {
            if (StaffFree <= 0)
            {
                Log("Semua IT Staff sedang sibuk. Tunggu aktivitas selesai atau gunakan Hire Contractor.");
                return false;
            }
            Activities.Add(new Activity { label = label, duration = duration, onComplete = onComplete });
            Log(label + " dimulai.");
            return true;
        }

        void Log(string message) => Logged?.Invoke(message);

        public void Hire()
        {
            if (!CanHire) return;
            Contractors++;
            Log("Kontraktor bergabung membantu IT Staff.");
        }

        // ------------------------------------------------------------------ aksi 1: Incident Response
        /// <summary>Mengembalikan umpan balik untuk pemain.</summary>
        public string ChooseResponse(ResponseOption option)
        {
            ResponseActivated = option.activatesIncidentResponse && hasIncidentResponse;
            if (option.activatesIncidentResponse && !hasIncidentResponse)
                return "Rencana Incident Response tidak dapat diaktifkan karena belum disiapkan pada Chapter 2. Penanganan berjalan tanpa rencana.";
            return option.feedback;
        }

        // ------------------------------------------------------------------ aksi 2: isolasi
        public void Isolate(SystemState s)
        {
            if (s.isolated || s.busy) return;
            s.busy = StartActivity("Isolasi " + s.data.displayName, database.isolateSeconds, () =>
            {
                s.busy = false;
                s.isolated = true;
                Log(s.data.displayName + " sudah terisolasi." + (Contained ? " Penyebaran serangan terkendali." : ""));
            });
        }

        public void Reconnect(SystemState s)
        {
            if (!s.isolated || s.busy) return;
            s.busy = StartActivity("Menghubungkan " + s.data.displayName, database.reconnectSeconds, () =>
            {
                s.busy = false;
                s.isolated = false;
                Log(s.data.displayName + " terhubung kembali." + (Contained ? "" : " Serangan dapat menyebar lagi!"));
            });
        }

        public void Isolate(ServerState s)
        {
            if (s.isolated || s.busy) return;
            s.busy = StartActivity("Isolasi " + s.data.displayName, database.isolateSeconds, () =>
            {
                s.busy = false;
                s.isolated = true;
                Log(s.data.displayName + (s.encrypted ? " terisolasi, tetapi isinya sudah terenkripsi." : " terlindungi dari serangan."));
            });
        }

        // ------------------------------------------------------------------ aksi 3 dan 4: cadangan dan pemulihan
        public void Investigate()
        {
            if (Investigated || Investigating) return;
            Investigating = StartActivity("Investigasi cadangan", database.investigateSeconds, () =>
            {
                Investigating = false;
                Investigated = true;
                Log("Investigasi cadangan selesai. Periksa penanda warna setiap cadangan.");
            });
        }

        public void Verify(SystemState s)
        {
            if (s.busy) return;
            s.busy = StartActivity("Verifikasi cadangan " + s.data.displayName, database.verifySeconds, () =>
            {
                s.busy = false;
                s.verified = true;
                Log($"Cadangan {s.data.displayName}: {HealthText(s)}.");
            });
        }

        /// <summary>Pemulihan pertama menentukan nilai prioritas dan mengunci penilaian isolasi.</summary>
        public void Restore(SystemState s)
        {
            if (!s.NeedsRestore || s.busy) return;
            var usable = Systems.Where(x => x.NeedsRestore && x.Health == BackupHealth.Good).ToList();
            var candidates = usable.Count > 0 ? usable : Systems.Where(x => x.NeedsRestore).ToList();
            bool correct = Systems.All(x => x.isolated == x.data.isInfected);

            s.busy = StartActivity("Pemulihan " + s.data.displayName, database.restoreSeconds, () => CompleteRestore(s));
            if (!s.busy || FirstRestored != null) return;
            FirstRestored = s;
            expectedFirst = candidates.OrderBy(x => x.data.priority).First();
            isolationCorrect = correct;
        }

        void CompleteRestore(SystemState s)
        {
            s.busy = false;
            if (s.Health == BackupHealth.Good && Contained)
            {
                s.outcome = Outcome.Restored;
                Log($"{s.data.displayName} berhasil dipulihkan. Data yang hilang maksimal {s.record.dataLossHours} jam.");
                return;
            }
            s.outcome = Outcome.Failed;
            s.failureReason = FailureReason(s);
            Log($"Pemulihan {s.data.displayName} gagal: {s.failureReason}.");
        }

        string FailureReason(SystemState s)
        {
            switch (s.Health)
            {
                case BackupHealth.Missing: return "tidak dicadangkan pada Chapter 2";
                case BackupHealth.Encrypted: return "cadangan di lokasi yang sama ikut terenkripsi";
                default: return "serangan belum terkendali sehingga data terenkripsi lagi";
            }
        }

        public string HealthText(SystemState s)
        {
            switch (s.Health)
            {
                case BackupHealth.Missing: return "tidak ada cadangan";
                case BackupHealth.Encrypted: return "terenkripsi, tidak dapat dipakai";
                default: return $"utuh, data hilang maks. {s.record.dataLossHours} jam";
            }
        }

        // ------------------------------------------------------------------ hasil
        public long CalculateLoss()
        {
            long loss = Systems.Where(s => s.outcome == Outcome.Failed).Sum(s => (long)s.data.lossIfFailed);
            return loss + (long)(Minutes / 60f * database.lossPerHour);
        }

        public int CalculateScore(out string summary)
        {
            var s = new StringBuilder();
            int response = ResponseActivated ? ResponsePoints : 0;
            s.Append($"1. Aktifkan rencana IR: {response}/{ResponsePoints}\n");
            if (response == 0)
                s.Append(hasIncidentResponse
                    ? "   - Rencana IR seharusnya diaktifkan sebagai tindakan pertama.\n"
                    : "   - Rencana IR belum disiapkan pada Chapter 2.\n");

            int isolation = isolationCorrect ? IsolationPoints : 0;
            s.Append($"2. Isolasi sistem terinfeksi: {isolation}/{IsolationPoints}\n");
            if (!isolationCorrect)
                s.Append("   - Sebelum pemulihan dimulai, seluruh sistem terinfeksi (dan hanya itu) harus terisolasi: " +
                         string.Join(", ", Systems.Where(x => x.data.isInfected).Select(x => x.data.displayName)) + ".\n");

            int priority = FirstRestored != null && FirstRestored.data.priority == expectedFirst.data.priority ? PriorityPoints : 0;
            s.Append($"3. Sistem yang dipulihkan lebih dulu: {priority}/{PriorityPoints}\n");
            if (FirstRestored != null)
                s.Append($"   - Pilihanmu: {FirstRestored.data.displayName}." +
                         (priority > 0 ? "\n" : $" Seharusnya {expectedFirst.data.displayName}: {expectedFirst.data.reason}\n"));

            s.Append($"4. Pulihkan data dari cadangan: {RestorePoints}/{RestorePoints}\n");
            foreach (var x in Systems.Where(x => x.data.isInfected))
                s.Append($"   - {x.data.displayName}: " + (x.outcome == Outcome.Restored
                    ? $"berhasil, data hilang maks. {x.record.dataLossHours} jam.\n"
                    : $"gagal, {x.failureReason}.\n"));

            int penalty = Recovered ? 0 : FailurePenalty;
            if (penalty > 0) s.Append($"\nPengurangan (pemulihan gagal): -{penalty}\n");
            if (Contractors > 0) s.Append($"\nBiaya kontraktor: {Chapter3Controller.Rupiah(ContractorCost)}. Sisa uang: {Chapter3Controller.Rupiah(Money)}\n");

            int total = Mathf.Clamp(response + isolation + priority + RestorePoints - penalty, 0, 100);
            s.Append($"\n<b>Total Chapter 3: {total} / 100</b>");
            summary = s.ToString();
            return total;
        }
    }
}
