using UnityEngine;
using World;

namespace Interface
{
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    public class TileHighlighter : MonoBehaviour
    {
        /*public Interface.Settings settings;
        public World.Settings worldSettings;
        public MouseManager mouseManager;
        
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Tile _previousTile;
        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            Events.onMouseEnterChunk += HighlightTile;
            Events.onMouseExitChunk += UnhighlightTile;
        }

        private void HighlightTile(GameObject chunkObj)f
        {
            var hit = mouseManager.GetTileAtMouse(chunkObj);
            if (hit == null) return;
            var (tile, position) = hit.Value;
            if (tile.Equals(_previousTile)) return;

            _meshRenderer.enabled = true;
            gameObject.transform.position = chunkObj.transform.position + Vector3.up * settings.highlightHeight;
            _meshFilter.sharedMesh = tile.mesh;
            _meshRenderer.material.color = tile.material.color + settings.highlightColor;
            _previousTile = tile;
        }
        
        private void UnhighlightTile(GameObject chunk)
        {
            _meshRenderer.enabled = false;
        }*/
    }

}
