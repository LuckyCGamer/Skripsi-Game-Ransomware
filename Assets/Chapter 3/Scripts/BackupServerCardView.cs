using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chapter3
{
    /// <summary>Kartu server cadangan pada tab Backup Protection (prefab ui_backup_server_card).</summary>
    public class BackupServerCardView : MonoBehaviour
    {
        public TMP_Text nameText;
        public TMP_Text statusText;
        public TMP_Text descriptionText;
        public Button isolateButton;

        public void Show(Chapter3BackupServer data)
        {
            nameText.text = data.displayName;
            descriptionText.text = data.description;
        }

        public void Refresh(ServerState s)
        {
            statusText.text = s.encrypted ? "TERENKRIPSI" : s.isolated ? "TERISOLASI" : "TERHUBUNG";
            isolateButton.interactable = !s.isolated && !s.busy;
        }
    }
}
