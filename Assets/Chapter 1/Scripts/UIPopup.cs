using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Jendela pop-up serbaguna: penyajian masalah, petunjuk, kesimpulan, dan ringkasan skor.
/// </summary>
public class UIPopup : MonoBehaviour
{
    [SerializeField] GameObject root;
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text bodyText;
    [SerializeField] Transform optionContainer;
    [SerializeField] Button optionTemplate;
    [SerializeField] Button primaryButton;
    [SerializeField] TMP_Text primaryLabel;

    readonly List<Button> spawned = new List<Button>();

    public bool IsOpen => root.activeSelf;

    void Awake()
    {
        optionTemplate.gameObject.SetActive(false);
        root.SetActive(false);
    }

    public void Show(string title, string body, string primary, Action onPrimary,
                     IList<KeyValuePair<string, Action>> options = null)
    {
        titleText.text = title;
        bodyText.text = body;

        foreach (var b in spawned) Destroy(b.gameObject);
        spawned.Clear();
        if (options != null)
        {
            foreach (var opt in options)
            {
                var b = Instantiate(optionTemplate, optionContainer);
                b.gameObject.SetActive(true);
                b.GetComponentInChildren<TMP_Text>().text = opt.Key;
                var cb = opt.Value;
                b.onClick.AddListener(() => { Hide(); cb?.Invoke(); });
                spawned.Add(b);
            }
        }
        optionContainer.gameObject.SetActive(spawned.Count > 0);

        primaryButton.gameObject.SetActive(!string.IsNullOrEmpty(primary));
        primaryLabel.text = primary;
        primaryButton.onClick.RemoveAllListeners();
        primaryButton.onClick.AddListener(() => { Hide(); onPrimary?.Invoke(); });

        root.SetActive(true);
        root.transform.SetAsLastSibling();
    }

    public void Hide() => root.SetActive(false);
}
