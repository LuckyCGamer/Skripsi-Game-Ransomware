using UnityEngine;

namespace Chapter3
{
    /// <summary>
    /// Satu variasi serangan Chapter 3. Satu variasi dipilih acak setiap permainan baru,
    /// berbeda dari variasi yang dimainkan sebelumnya.
    /// </summary>
    [CreateAssetMenu(fileName = "Chapter3Scenario", menuName = "Chapter 3/Scenario")]
    public class Chapter3Scenario : ScriptableObject
    {
        [Tooltip("Kejadian serangan pada variasi ini, ditampilkan sebelum petunjuk umum")]
        [TextArea(3, 8)] public string attackDescription;
        [Tooltip("Sistem yang terinfeksi. Pilih sistem yang dicadangkan pada Chapter 2 supaya pemulihan bisa berhasil")]
        public Chapter3System[] infectedSystems = new Chapter3System[0];
    }
}
