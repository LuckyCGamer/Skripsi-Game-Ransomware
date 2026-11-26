using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chapter3
{
    /// <summary>Kartu sistem pada tab File (prefab ui_system_card).</summary>
    public class SystemCardView : MonoBehaviour
    {
        public TMP_Text nameText;
        public TMP_Text statusText;
        public TMP_Text descriptionText;
        public TMP_Text symptomText;
        public Button backupsButton;
        public Button isolateButton;
        public Button reconnectButton;

        [Header("Penanda status")]
        public Image background;
        public Image statusBadge;
        public Color cardColor = new Color(1f, 1f, 1f, 0.55f);
        [Tooltip("Warna kartu selama sistem terinfeksi")]
        public Color infectedCardColor = new Color(1f, 0.72f, 0.7f, 1f);
        public Color neutralColor = new Color(0.85f, 0.85f, 0.85f);
        public Color infectedColor = new Color(0.8f, 0.1f, 0.1f);
        public Color isolatedColor = new Color(1f, 0.78f, 0.3f);
        public Color safeColor = new Color(0.5f, 0.9f, 0.5f);
        public Color darkTextColor = new Color(0.196f, 0.196f, 0.196f);

        public void Show(SystemState s)
        {
            nameText.text = s.data.displayName;
            descriptionText.text = s.data.description;
            symptomText.text = s.isInfected ? s.data.infectedSymptom : s.data.normalSymptom;
        }

        public void Refresh(SystemState s, bool revealInfection)
        {
            bool infected = false;
            if (s.outcome == Outcome.Restored) SetStatus(s.isolated ? "PULIH - TERISOLASI" : "PULIH", safeColor, darkTextColor);
            else if (s.outcome == Outcome.Failed) SetStatus("GAGAL DIPULIHKAN", infectedColor, Color.white);
            else if (s.isolated) SetStatus("TERISOLASI", isolatedColor, darkTextColor);
            else if (!revealInfection) SetStatus("TERHUBUNG", neutralColor, darkTextColor);
            else if (s.isInfected)
            {
                infected = true;
                SetStatus("TERINFEKSI", infectedColor, Color.white);
            }
            else SetStatus("NORMAL", safeColor, darkTextColor);

            background.color = infected ? infectedCardColor : cardColor;
            isolateButton.interactable = !s.isolated && !s.busy;
            reconnectButton.interactable = s.isolated && !s.busy;
        }

        void SetStatus(string text, Color badge, Color textColor)
        {
            statusText.text = text;
            statusText.color = textColor;
            statusBadge.color = badge;
        }
    }
}
