using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Satu entri perangkat lunak di tab Software (prefab ui_sofware).</summary>
public class SoftwareEntryView : MonoBehaviour
{
    [Tooltip("Klik entri untuk menandai/membatalkan tanda [CELAH]")]
    public Button flagButton;
    public Image background;
    public Image marker;
    public TMP_Text nameText;
    public TMP_Text versionText;
    public TMP_Text descriptionText;
    public Button updateButton;
    public TMP_Text updateLabel;

    public RectTransform Rect => (RectTransform)transform;
}
