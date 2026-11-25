using TMPro;
using UnityEngine;

namespace Chapter3
{
    /// <summary>Satu aktivitas beserta bilah progresnya pada tab Activity (prefab ui_activity_row).</summary>
    public class ActivityRowView : MonoBehaviour
    {
        public TMP_Text nameText;
        public TMP_Text remainingText;
        [Tooltip("Bagian bilah yang terisi; lebarnya mengikuti progres")]
        public RectTransform fill;

        public void Refresh(Activity a)
        {
            nameText.text = a.label;
            remainingText.text = $"Sisa {Mathf.CeilToInt(Mathf.Max(0f, a.duration - a.elapsed))} detik";
            fill.anchorMax = new Vector2(a.Progress, 1f);
        }
    }
}
