using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chapter2;
using UnityEngine;

namespace Chapter3
{
    public enum Outcome { None, Restored, Failed }
    public enum BackupState { Verified, Unknown, Failed, Corrupt, Encrypted }
    public enum RestoreCheck { Ok, NotContained, Unusable, NotNeeded, Busy }

    /// <summary>Satu cadangan pada riwayat sebuah sistem.</summary>
    public class BackupEntry
    {
        public int minute;        // menit sejak hari pertama riwayat pukul 00:00
        public bool failed;       // pekerjaan backup gagal, tidak ada salinan
        public bool corrupt;      // salinan rusak, baru ketahuan saat diverifikasi
        public bool? verified;    // null = belum diverifikasi
        public BackupLocation[] locations;
    }

    public class SystemState
    {
        public Chapter3System data;
        public GameSession.BackupRecord record; // keputusan Chapter 2; null = tidak dicadangkan
        public readonly List<BackupEntry> backups = new List<BackupEntry>(); // terlama di depan
        public bool isolated, busy, isInfected, investigated;
        public Outcome outcome;
        public string failureReason;
        public BackupEntry restoredFrom;
        public int failedRestores;

        public bool HasBackup => record != null && record.isBackedUp;
        public bool NeedsRestore => isInfected && outcome == Outcome.None;
    }

    public class ServerState
    {
        public Chapter3BackupServer data;
        public bool isolated, busy, encrypted;
        public bool IsOffline => data.location == BackupLocation.Offline;
    }

    public class Activity
    {
        public string label;
        public float duration, elapsed;
        public Action onComplete;
        public float Progress => Mathf.Clamp01(elapsed / duration);
    }

    /// <summary>Kesiapan dari Chapter 2 yang memengaruhi Chapter 3.</summary>
    public struct Readiness
    {
        public bool hasIncidentResponse, hasTraining;
        public int money, detectMinutes; // detectMinutes -1 = tanpa monitoring
        public float restoreMultiplier;
    }

    /// <summary>
    /// Aturan Chapter 3 (Gambar 3.9), tanpa UI:
    ///   Aktifkan rencana IR (+20), isolasi sistem terinfeksi sebelum serangan menyebar (+20),
    ///   tentukan sistem yang dipulihkan lebih dulu (+30), pulihkan data dari backup (+30). Pemulihan gagal: -30.
    /// Riwayat cadangan dibuat dari keputusan Chapter 2: frekuensi menentukan waktu cadangan,
    /// lokasi menentukan apakah cadangan ikut terenkripsi.
    /// </summary>
    public class Chapter3Game
    {
        public const int ResponsePoints = 20;
        public const int IsolationPoints = 20;
        public const int PriorityPoints = 30;
        public const int RestorePoints = 30;
        public const int FailurePenalty = 30;

        readonly Chapter3Database database;
        readonly Chapter3Scenario scenario;
        readonly Readiness readiness;
        readonly HashSet<SpreadStep> spreadDone = new HashSet<SpreadStep>();
        SystemState expectedFirst;
        bool spreadAfterDetection;

        public readonly List<SystemState> Systems = new List<SystemState>();
        public readonly List<ServerState> Servers = new List<ServerState>();
        public readonly List<Activity> Activities = new List<Activity>();
        public event Action<string> Logged;

        /// <summary>Menit setelah serangan terdeteksi (waktu bermain).</summary>
        public float Minutes { get; private set; }
        public int DetectMinute { get; }
        /// <summary>Menit sejak serangan dimulai.</summary>
        public float AttackMinutes => DetectMinute + Minutes;
        /// <summary>Waktu serangan, dalam menit sejak hari pertama riwayat pukul 00:00.</summary>
        public int AttackAbs { get; }
        public int Contractors { get; private set; }
        public bool ResponseActivated { get; private set; }
        public SystemState FirstRestored { get; private set; }

        float Speed => readiness.hasTraining ? database.trainingSpeed : 1f;
        public int StaffTotal => database.staffCount + (readiness.hasTraining ? database.trainingStaff : 0) + Contractors;
        public int StaffFree => StaffTotal - Activities.Count;
        public bool ContractorAvailable => Contractors < database.maxContractors;
        public bool CanHire => ContractorAvailable && Money >= database.contractorCost;
        public int ContractorCost => Contractors * database.contractorCost;
        public int Money => readiness.money - ContractorCost;
        public bool Contained => Systems.All(s => !s.isInfected || s.isolated || s.outcome == Outcome.Restored);
        public bool Finished => Systems.All(s => !s.NeedsRestore);
        public bool Recovered => Systems.All(s => !s.isInfected || s.outcome == Outcome.Restored);

        public Chapter3Game(Chapter3Database database, Chapter3Scenario scenario, IList<GameSession.BackupRecord> records,
                            Readiness readiness, int seed)
        {
            this.database = database;
            this.scenario = scenario;
            this.readiness = readiness;
            DetectMinute = readiness.detectMinutes >= 0 ? readiness.detectMinutes : database.detectMinutesWithoutMonitoring;
            int days = Mathf.CeilToInt(database.historyHours / 24f);
            AttackAbs = days * 1440 + database.startHour * 60;

            foreach (var d in database.backupServers)
                if (d) Servers.Add(new ServerState { data = d });

            var random = new System.Random(seed);
            foreach (var d in database.systems)
            {
                if (!d) continue;
                var s = new SystemState
                {
                    data = d,
                    record = d.backupSource ? records.FirstOrDefault(r => r.fileName == d.backupSource.displayName) : null,
                    isInfected = scenario.infectedSystems.Contains(d),
                };
                GenerateBackups(s, random);
                Systems.Add(s);
            }
        }

        void GenerateBackups(SystemState s, System.Random random)
        {
            if (!s.HasBackup || s.record.intervalHours <= 0) return;
            int step = s.record.intervalHours * 60;
            int start = AttackAbs - database.historyHours * 60;
            for (int t = (start + step - 1) / step * step; t < AttackAbs; t += step)
            {
                bool failed = random.NextDouble() < database.failRate;
                s.backups.Add(new BackupEntry
                {
                    minute = t,
                    failed = failed,
                    corrupt = !failed && random.NextDouble() < database.corruptRate,
                    locations = s.record.locations,
                });
            }
            // Pastikan selalu ada setidaknya satu cadangan yang benar-benar baik.
            if (s.backups.Count > 0 && s.backups.All(b => b.failed || b.corrupt))
            {
                var last = s.backups[s.backups.Count - 1];
                last.failed = last.corrupt = false;
            }
        }

        /// <summary>Jalankan kejadian sebelum serangan terdeteksi. Dipanggil sekali sebelum permainan dimulai.</summary>
        public void Begin()
        {
            foreach (var s in Systems.Where(x => x.isInfected))
                Log($"{s.data.displayName} terenkripsi saat serangan dimulai.", 0);
            CheckEvents(false);
            Log($"Pesan tebusan ditemukan. Serangan terdeteksi {DetectMinute} menit setelah dimulai.", DetectMinute);
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
            CheckEvents(true);
        }

        void CheckEvents(bool afterDetection)
        {
            float now = AttackMinutes;
            foreach (var step in scenario.spread.OrderBy(x => x.minute))
            {
                if (spreadDone.Contains(step) || step.system == null || now < step.minute) continue;
                spreadDone.Add(step);
                var s = Systems.FirstOrDefault(x => x.data == step.system);
                if (s == null || s.isInfected) continue;
                if (Contained && afterDetection) Log($"Penyebaran ke {s.data.displayName} tercegah karena serangan sudah terkendali.", step.minute);
                else if (s.isolated) Log($"{s.data.displayName} selamat karena sudah diisolasi sebelum terjangkau.", step.minute);
                else
                {
                    s.isInfected = true;
                    spreadAfterDetection |= afterDetection;
                    Log($"Ransomware menyebar: {s.data.displayName} terenkripsi.", step.minute);
                }
            }

            foreach (var server in Servers)
            {
                int at = server.data.encryptMinute;
                if (at < 0 || server.encrypted || server.isolated || now < at || (Contained && afterDetection)) continue;
                server.encrypted = true;
                Log($"{server.data.displayName} terjangkau ransomware. Semua cadangan di sana ikut terenkripsi.", at);
            }

            // Sistem terinfeksi tanpa cadangan yang masih dapat dipakai otomatis dinyatakan gagal dipulihkan.
            foreach (var s in Systems)
            {
                if (!s.NeedsRestore || s.busy || s.backups.Any(IsUsable)) continue;
                s.outcome = Outcome.Failed;
                s.failureReason = !s.HasBackup ? "tidak dicadangkan pada Chapter 2" : "tidak ada cadangan yang masih dapat dipakai";
                Log($"{s.data.displayName} tidak dapat dipulihkan: {s.failureReason}.");
            }
        }

        bool StartActivity(string label, float duration, Action onComplete)
        {
            if (StaffFree <= 0)
            {
                Log("Semua IT Staff sedang sibuk. Tunggu aktivitas selesai atau gunakan Hire Contractor.");
                return false;
            }
            Activities.Add(new Activity { label = label, duration = Mathf.Max(0.5f, duration * Speed), onComplete = onComplete });
            Log(label + " dimulai.");
            return true;
        }

        void Log(string message, float attackMinute = -1)
        {
            float at = attackMinute >= 0 ? attackMinute : AttackMinutes;
            Logged?.Invoke($"[{Clock(AttackAbs + Mathf.FloorToInt(at))}] {message}");
        }

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
            ResponseActivated = option.activatesIncidentResponse && readiness.hasIncidentResponse;
            if (option.activatesIncidentResponse && !readiness.hasIncidentResponse)
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
            if (s.isolated || s.busy || s.encrypted || s.IsOffline) return;
            s.busy = StartActivity("Isolasi " + s.data.displayName, database.isolateSeconds, () =>
            {
                s.busy = false;
                s.isolated = true;
                Log(s.data.displayName + (s.encrypted ? " terisolasi, tetapi isinya sudah terenkripsi." : " terlindungi dari serangan."));
            });
        }

        // ------------------------------------------------------------------ aksi 3 dan 4: cadangan dan pemulihan
        ServerState ServerFor(BackupLocation l) => Servers.FirstOrDefault(s => s.data.location == l);

        /// <summary>Lokasi salinan yang masih dapat dipakai; offline dipakai terakhir karena paling lambat.</summary>
        public BackupLocation? UsableLocation(BackupEntry b)
        {
            bool offline = false;
            foreach (var l in b.locations)
            {
                if (l == BackupLocation.Offline) { offline = true; continue; }
                var server = ServerFor(l);
                if (server == null || !server.encrypted) return l;
            }
            return offline ? BackupLocation.Offline : (BackupLocation?)null;
        }

        public BackupState StateOf(BackupEntry b) =>
            b.failed ? BackupState.Failed
            : UsableLocation(b) == null ? BackupState.Encrypted
            : b.verified == false ? BackupState.Corrupt
            : b.verified == true ? BackupState.Verified
            : BackupState.Unknown;

        public bool IsUsable(BackupEntry b) => StateOf(b) == BackupState.Verified || StateOf(b) == BackupState.Unknown;

        public void Investigate(SystemState s)
        {
            if (s.busy) return;
            s.busy = StartActivity("Investigasi cadangan " + s.data.displayName, database.investigateSeconds, () =>
            {
                s.busy = false;
                s.investigated = true;
                var latest = s.backups.Skip(Mathf.Max(0, s.backups.Count - database.investigateCount)).ToList();
                int failed = latest.Count(b => b.failed), corrupt = 0;
                foreach (var b in latest)
                    if (!b.failed && b.corrupt && b.verified == null) { b.verified = false; corrupt++; }
                Log($"{s.data.displayName}: dari {latest.Count} cadangan terbaru, {failed} pekerjaan gagal dan {corrupt} salinan rusak ditemukan.");
            });
        }

        public void Verify(SystemState s, BackupEntry b)
        {
            if (s.busy || StateOf(b) != BackupState.Unknown) return;
            s.busy = StartActivity($"Verifikasi cadangan {TimeLabel(b.minute)} {s.data.displayName}", database.verifySeconds, () =>
            {
                s.busy = false;
                b.verified = !b.corrupt;
                Log($"Cadangan {TimeLabel(b.minute)} {s.data.displayName} " + (b.corrupt ? "rusak dan tidak dapat dipakai." : "terverifikasi utuh."));
            });
        }

        public RestoreCheck CanRestore(SystemState s, BackupEntry b) =>
            !s.NeedsRestore ? RestoreCheck.NotNeeded
            : s.busy ? RestoreCheck.Busy
            : !Contained ? RestoreCheck.NotContained
            : !IsUsable(b) ? RestoreCheck.Unusable
            : RestoreCheck.Ok;

        public float RestoreSeconds(BackupEntry b)
        {
            float extra = UsableLocation(b) == BackupLocation.Offline ? ServerFor(BackupLocation.Offline)?.data.restoreExtraSeconds ?? 0f : 0f;
            return database.restoreSeconds * readiness.restoreMultiplier + extra;
        }

        /// <summary>Pemulihan pertama menentukan nilai prioritas.</summary>
        public void Restore(SystemState s, BackupEntry b)
        {
            if (CanRestore(s, b) != RestoreCheck.Ok) return;
            var candidates = Systems.Where(x => x.NeedsRestore && x.backups.Any(IsUsable)).ToList();
            if (candidates.Count == 0) candidates = Systems.Where(x => x.NeedsRestore).ToList();

            s.busy = StartActivity($"Pemulihan {s.data.displayName} dari {TimeLabel(b.minute)}", RestoreSeconds(b), () => CompleteRestore(s, b));
            if (!s.busy || FirstRestored != null) return;
            FirstRestored = s;
            expectedFirst = candidates.OrderBy(x => x.data.priority).First();
        }

        void CompleteRestore(SystemState s, BackupEntry b)
        {
            s.busy = false;
            if (b.corrupt || !IsUsable(b))
            {
                s.failedRestores++;
                if (b.corrupt) b.verified = false;
                Log($"Pemulihan {s.data.displayName} dari {TimeLabel(b.minute)} gagal: cadangan {(b.corrupt ? "rusak" : "tidak dapat dipakai")}. Pilih cadangan lain.");
                return;
            }
            s.outcome = Outcome.Restored;
            s.restoredFrom = b;
            Log($"{s.data.displayName} berhasil dipulihkan dari {TimeLabel(b.minute)}. Data hilang {DataGapText(b)}.");
        }

        // ------------------------------------------------------------------ waktu
        public static string Clock(int abs) => $"{abs / 60 % 24:00}:{abs % 60:00}";

        /// <summary>Contoh: "Hari ini 16:00", "Kemarin 20:00".</summary>
        public string TimeLabel(int minute)
        {
            int day = Mathf.FloorToInt(minute / 1440f) - AttackAbs / 1440;
            string d = day == 0 ? "Hari ini" : day == -1 ? "Kemarin" : $"{-day} hari lalu";
            return d + " " + Clock(minute);
        }

        public string DataGapText(BackupEntry b)
        {
            int gap = AttackAbs - b.minute;
            return gap < 60 ? $"{gap} menit" : $"{gap / 60} jam {gap % 60:00} menit";
        }

        // ------------------------------------------------------------------ hasil
        public long CalculateLoss()
        {
            long loss = Systems.Where(s => s.outcome == Outcome.Failed).Sum(s => (long)s.data.lossIfFailed);
            return loss + (long)(AttackMinutes / 60f * database.lossPerHour);
        }

        public int CalculateScore(out string summary)
        {
            var s = new StringBuilder();
            int response = ResponseActivated ? ResponsePoints : 0;
            s.Append($"1. Aktifkan rencana IR: {response}/{ResponsePoints}\n");
            if (response == 0)
                s.Append(readiness.hasIncidentResponse
                    ? "   - Rencana IR seharusnya diaktifkan sebagai tindakan pertama.\n"
                    : "   - Rencana IR belum disiapkan pada Chapter 2.\n");

            int isolation = spreadAfterDetection ? 0 : IsolationPoints;
            s.Append($"2. Isolasi sistem terinfeksi: {isolation}/{IsolationPoints}\n");
            if (spreadAfterDetection)
                s.Append("   - Ransomware sempat menyebar setelah terdeteksi. Isolasi sistem terinfeksi lebih cepat.\n");

            int priority = FirstRestored != null && FirstRestored.data.priority == expectedFirst.data.priority ? PriorityPoints : 0;
            s.Append($"3. Sistem yang dipulihkan lebih dulu: {priority}/{PriorityPoints}\n");
            if (FirstRestored != null)
                s.Append($"   - Pilihanmu: {FirstRestored.data.displayName}." +
                         (priority > 0 ? "\n" : $" Seharusnya {expectedFirst.data.displayName}: {expectedFirst.data.reason}\n"));

            s.Append($"4. Pulihkan data dari cadangan: {RestorePoints}/{RestorePoints}\n");
            foreach (var x in Systems.Where(x => x.isInfected))
                s.Append($"   - {x.data.displayName}: " + (x.outcome == Outcome.Restored
                    ? $"berhasil dari {TimeLabel(x.restoredFrom.minute)}, data hilang {DataGapText(x.restoredFrom)}.\n"
                    : $"gagal, {x.failureReason}.\n"));
            int failedRestores = Systems.Sum(x => x.failedRestores);
            if (failedRestores > 0)
                s.Append($"   - {failedRestores}x memulihkan dari cadangan rusak. Verifikasi cadangan dulu sebelum memulihkan.\n");

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
