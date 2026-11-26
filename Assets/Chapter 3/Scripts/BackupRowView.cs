using System.Linq;
using Chapter2;
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
        public Color unknownColor = new Color(1f, 0.85f, 0.45f);
        public Color greenColor = new Color(0.5f, 0.95f, 0.5f);
        public Color redColor = new Color(1f, 0.5f, 0.5f);

        public BackupEntry Entry { get; private set; }

        /// <summary>Hijau: terverifikasi. Kuning: perlu diverifikasi. Merah: gagal, rusak, atau terenkripsi.</summary>
        public void Refresh(BackupEntry entry, SystemState s, Chapter3Game game)
        {
            Entry = entry;
            var state = game.StateOf(entry);
            nameText.text = game.TimeLabel(entry.minute);
            var location = game.UsableLocation(entry);
            frequencyText.text = "Lokasi: " + string.Join(" + ", entry.locations.Select(LocationLabel)) +
                                 (location == BackupLocation.Offline && entry.locations.Length > 1 ? " (tersisa offline)" : "");
            statusText.text =
                s.restoredFrom == entry ? "Dipakai untuk pemulihan"
                : state == BackupState.Failed ? "Pekerjaan backup gagal"
                : state == BackupState.Encrypted ? "Ikut terenkripsi"
                : state == BackupState.Corrupt ? "Rusak"
                : state == BackupState.Verified ? "Terverifikasi utuh"
                : "Perlu diverifikasi";
            marker.color = state == BackupState.Verified ? greenColor : state == BackupState.Unknown ? unknownColor : redColor;
            verifyButton.interactable = !s.busy && state == BackupState.Unknown;
            restoreButton.interactable = !s.busy && s.NeedsRestore && game.IsUsable(entry);
        }

        public static string LocationLabel(BackupLocation l) =>
            l == BackupLocation.Connected ? "terhubung" : l == BackupLocation.Separate ? "server terpisah" : "offline";
    }
}
