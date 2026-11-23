using UnityEngine;

namespace Chapter1
{
    /// <summary>
    /// Kumpulan variasi persoalan Chapter 1 dan pilihan kesimpulan metode masuk peretas.
    /// Satu variasi dipilih acak setiap permainan baru, berbeda dari variasi sebelumnya.
    /// </summary>
    [CreateAssetMenu(fileName = "Chapter1Database", menuName = "Chapter 1/Database")]
    public class Chapter1Database : ScriptableObject
    {
        public Chapter1Scenario[] scenarios;
        [Tooltip("Pilihan yang tampil saat pemain menyimpulkan metode masuk (FINISH)")]
        public MethodOption[] methodOptions;

        public string MethodLabel(EntryMethod method)
        {
            foreach (var o in methodOptions)
                if (o.method == method) return o.label;
            return method.ToString();
        }
    }
}
