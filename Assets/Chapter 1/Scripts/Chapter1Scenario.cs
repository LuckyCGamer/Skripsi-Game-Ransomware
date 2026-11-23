using UnityEngine;

namespace Chapter1
{
    /// <summary>Satu variasi persoalan Chapter 1: email, tautan, dan perangkat lunak yang diselidiki.</summary>
    [CreateAssetMenu(fileName = "Chapter1Scenario", menuName = "Chapter 1/Scenario")]
    public class Chapter1Scenario : ScriptableObject
    {
        public string id;
        [Tooltip("Penyajian masalah di awal chapter (tahap PBL 1)")]
        [TextArea(4, 10)] public string incidentReport;
        public EntryMethod correctMethod;
        [Tooltip("Petunjuk saat metode masuk belum ditemukan")]
        [TextArea(3, 6)] public string failHint;
        public EmailData[] emails;
        public LinkData[] links;
        public SoftwareData[] software;
    }
}
