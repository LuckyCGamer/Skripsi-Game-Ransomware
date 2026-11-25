using UnityEngine;

namespace Chapter3
{
    /// <summary>Satu server cadangan (kartu pada tab Backup Protection).</summary>
    [CreateAssetMenu(fileName = "Chapter3BackupServer", menuName = "Chapter 3/Backup Server")]
    public class Chapter3BackupServer : ScriptableObject
    {
        public string displayName;
        [TextArea(1, 3)] public string description;
        [Tooltip("Centang untuk server offsite, yang tidak dapat ikut terenkripsi")]
        public bool isOffsite;
    }
}
