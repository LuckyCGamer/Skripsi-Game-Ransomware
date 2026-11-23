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

        // Tombol opsi dipakai ulang (bukan di-Destroy) supaya Inspector tidak menunjuk objek yang sudah hilang.
        int count = options?.Count ?? 0;
        while (spawned.Count < count)
            spawned.Add(Instantiate(optionTemplate, optionContainer));
        for (int i = 0; i < spawned.Count; i++)
        {
            var b = spawned[i];
            bool used = i < count;
            b.gameObject.SetActive(used);
            b.onClick.RemoveAllListeners();
            if (!used) continue;
            var opt = options[i];
            b.GetComponentInChildren<TMP_Text>().text = opt.Key;
            var cb = opt.Value;
            b.onClick.AddListener(() => { Hide(); cb?.Invoke(); });
        }
        optionContainer.gameObject.SetActive(count > 0);

        primaryButton.gameObject.SetActive(!string.IsNullOrEmpty(primary));
        primaryLabel.text = primary;
        primaryButton.onClick.RemoveAllListeners();
        primaryButton.onClick.AddListener(() => { Hide(); onPrimary?.Invoke(); });

        root.SetActive(true);
        root.transform.SetAsLastSibling();
    }

    public void Hide() => root.SetActive(false);
}
