using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chapter3
{
    /// <summary>Satu cadangan pada jendela Investigate Backup (prefab ui_backup_row).</summary>
    public class BackupRowView : MonoBehaviour
    {
        public Image marker;
        public TMP_Text nameText;
        public TMP_Text frequencyText;
        public TMP_Text statusText;
        public Button verifyButton;
        public Button restoreButton;

        [Header("Warna penanda")]
        public Color unknownColor = new Color(0.65f, 0.65f, 0.65f);
        public Color greenColor = new Color(0.5f, 0.95f, 0.5f);
        public Color yellowColor = new Color(1f, 0.85f, 0.45f);
        public Color redColor = new Color(1f, 0.5f, 0.5f);

        public void Refresh(SystemState s, Chapter3Game game)
        {
            bool known = game.Investigated || s.verified;
            nameText.text = s.data.displayName;
            frequencyText.text = !known ? "Belum diinvestigasi"
                : !s.HasBackup ? "Tidak dicadangkan"
                : $"Setiap {s.record.dataLossHours} jam, {(s.record.isOffsite ? "offsite" : "lokasi sama")}";
            statusText.text =
                s.outcome == Outcome.Restored ? "Sudah dipulihkan"
                : s.outcome == Outcome.Failed ? "Pemulihan gagal"
                : s.verified ? "Terverifikasi: " + game.HealthText(s)
                : s.isInfected || !game.ResponseActivated ? "Belum diverifikasi" : "Tidak perlu dipulihkan";

            // Hijau: offsite. Kuning: lokasi sama (rawan). Merah: tidak ada atau terenkripsi.
            marker.color = !known ? unknownColor
                : s.Health != BackupHealth.Good ? redColor
                : s.record.isOffsite ? greenColor : yellowColor;
            verifyButton.interactable = !s.busy;
            restoreButton.interactable = !s.busy && s.outcome == Outcome.None;
        }
    }
}
