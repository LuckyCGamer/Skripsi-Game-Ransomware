using UnityEngine;
using UnityEngine.UI;

namespace Chapter2
{
    /// <summary>
    /// Mengatur lebar sel GridLayoutGroup supaya kolom selalu memenuhi lebar area, berapa pun ukuran layarnya.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(GridLayoutGroup))]
    public class FillGridWidth : MonoBehaviour
    {
        [Tooltip("Lebar minimal satu sel; jumlah kolom menyesuaikan")]
        [Min(10)] public float minCellWidth = 100f;

        GridLayoutGroup grid;
        float lastWidth = -1f;

        void OnEnable()
        {
            grid = GetComponent<GridLayoutGroup>();
            lastWidth = -1f;
            Apply();
        }

        void OnRectTransformDimensionsChange() => Apply();

        void Apply()
        {
            if (!grid) return;
            float width = ((RectTransform)transform).rect.width - grid.padding.horizontal;
            if (width <= 0f || Mathf.Approximately(width, lastWidth)) return;
            lastWidth = width;
            int columns = Mathf.Max(1, Mathf.FloorToInt((width + grid.spacing.x) / (minCellWidth + grid.spacing.x)));
            float cell = Mathf.Floor((width - grid.spacing.x * (columns - 1)) / columns);
            grid.cellSize = new Vector2(cell, grid.cellSize.y);
        }
    }
}
