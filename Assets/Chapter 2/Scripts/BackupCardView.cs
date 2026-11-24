using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chapter2
{
    /// <summary>Tampilan satu berkas/sistem (prefab ui_backup_card).</summary>
    public class BackupCardView : MonoBehaviour
    {
        public TMP_Text nameText;
        public TMP_Text descriptionText;
        public TMP_Text sizeText;
        public Transform frequencyContainer;
        public Transform storageContainer;
        [Tooltip("Prefab ui_option_button")]
        public Button optionPrefab;

        [Header("Keterangan pilihan")]
        public TMP_Text choiceText;
        public TMP_Text storageNeededText;
        public TMP_Text costText;
        public TMP_Text dataLossText;

        [Header("Warna tombol pilihan")]
        public Color normalColor = Color.white;
        public Color selectedColor = new Color(0.7f, 0.85f, 1f);

        readonly List<Button> frequencyButtons = new List<Button>();
        readonly List<Button> storageButtons = new List<Button>();
        BackupChoice choice;
        string defaultChoiceText;

        public void Show(BackupChoice choice, Action<FrequencyOption> onFrequency, Action<StorageOption> onStorage)
        {
            this.choice = choice;
            defaultChoiceText = choiceText.text;
            nameText.text = choice.file.displayName;
            descriptionText.text = choice.file.description;
            sizeText.text = choice.file.sizeGB + " GB";
            BuildOptions(frequencyContainer, frequencyButtons, choice.file.frequencyOptions, o => o.label, onFrequency);
            BuildOptions(storageContainer, storageButtons, choice.file.storageOptions, o => o.label, onStorage);
            Refresh();
        }

        void BuildOptions<T>(Transform container, List<Button> buttons, T[] options, Func<T, string> label, Action<T> onClick)
        {
            // Tombol contoh di prefab disembunyikan, lalu dibuat sesuai data.
            foreach (Transform child in container) child.gameObject.SetActive(false);
            foreach (T option in options)
            {
                var button = Instantiate(optionPrefab, container);
                button.GetComponentInChildren<TMP_Text>().text = label(option);
                button.onClick.AddListener(() => onClick(option));
                buttons.Add(button);
            }
        }

        public void Refresh()
        {
            for (int i = 0; i < frequencyButtons.Count; i++)
                Highlight(frequencyButtons[i], choice.file.frequencyOptions[i] == choice.frequency);
            for (int i = 0; i < storageButtons.Count; i++)
            {
                Highlight(storageButtons[i], choice.file.storageOptions[i] == choice.storage);
                storageButtons[i].interactable = choice.IsBackedUp; // lokasi hanya untuk berkas yang dicadangkan
            }

            string text = string.Join(" ", new[] { choice.frequency?.description, choice.storage?.description }
                .Where(t => !string.IsNullOrEmpty(t)));
            choiceText.text = text.Length > 0 ? text : defaultChoiceText;
            storageNeededText.text = choice.frequency == null ? "-" : choice.StorageNeeded + " GB";
            costText.text = choice.IsBackedUp && choice.storage == null || choice.frequency == null
                ? "-" : Chapter2Controller.Rupiah(choice.Cost);
            dataLossText.text = choice.frequency == null ? "-"
                : choice.IsBackedUp ? $"Maks. {choice.frequency.dataLossHours} jam" : "Semua data";
        }

        void Highlight(Button button, bool selected) =>
            button.targetGraphic.color = selected ? selectedColor : normalColor;
    }
}
