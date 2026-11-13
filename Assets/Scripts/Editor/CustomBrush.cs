using UnityEngine;

namespace UnityEditor.Tilemaps
{
    [CustomGridBrush(true, false, false, "Custom Brush")]
    public class CustomBrush : GridBrush
    {
        /// <summary>
        /// Event invoked whenever tiles are painted, erased, or modified using this brush. <br/>
        /// The integer represents the layer index that was modified.
        /// -1 indicates a general modification not tied to a specific layer.
        /// </summary>
        public static event System.Action<GameObject> OnTilesUpdated;

        public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position)
        {
            OnTilesUpdated?.Invoke(brushTarget);
            base.Paint(grid, brushTarget, position);
        }

        public override void Erase(GridLayout grid, GameObject brushTarget, Vector3Int position)
        {
            OnTilesUpdated?.Invoke(brushTarget);
            base.Erase(grid, brushTarget, position);
        }

        public override void FloodFill(GridLayout grid, GameObject brushTarget, Vector3Int position)
        {
            OnTilesUpdated?.Invoke(brushTarget);
            base.FloodFill(grid, brushTarget, position);
        }

        public override void BoxFill(GridLayout grid, GameObject brushTarget, BoundsInt position)
        {
            OnTilesUpdated?.Invoke(brushTarget);
            base.BoxFill(grid, brushTarget, position);
        }

        public override void Rotate(RotationDirection direction, GridLayout.CellLayout layout)
        {
            OnTilesUpdated?.Invoke(null);
            base.Rotate(direction, layout);
        }

        public override void Flip(FlipAxis flip, GridLayout.CellLayout layout)
        {
            OnTilesUpdated?.Invoke(null);
            base.Flip(flip, layout);
        }

        public override void BoxErase(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            OnTilesUpdated?.Invoke(brushTarget);
            base.BoxErase(gridLayout, brushTarget, position);
        }

        public override void Move(GridLayout gridLayout, GameObject brushTarget, BoundsInt from, BoundsInt to)
        {
            OnTilesUpdated?.Invoke(brushTarget);
            base.Move(gridLayout, brushTarget, from, to);
        }


        [MenuItem("Assets/Create/2D/Tile Palette/Custom Brush")]
        public static void CreateBrush()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Custom Brush",
                "New Custom Brush",
                "asset",
                "Save Custom Brush",
                "Assets"
            );

            if (string.IsNullOrEmpty(path)) return;
            AssetDatabase.CreateAsset(CreateInstance<CustomBrush>(), path);
        }
    }

    [CustomEditor(typeof(CustomBrush))]
    public class CustomBrushEditor : GridBrushEditor { }
}
