using UnityEngine;
using World;
using JetBrains.Annotations;

namespace Interface
{
    public class MouseManager : MonoBehaviour
    {
        public Camera mainCamera;
        
        private RaycastHit GetMouseRaycastHit()
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hit);
            return hit;
        }
        
        /*public (Tile, Vector3)? GetTileAtMouse(GameObject chunkObj)
        {
            var hit = GetMouseRaycastHit();
            if (hit.collider == null) return null;
            var position = hit.point;
            var chunk = chunkObj.GetComponent<Chunk>();
            var chunkPosition = chunk.transform.position;
            return (chunk.tiles[(int) (position.x - chunkPosition.x), (int) (position.z - chunkPosition.z)], position);
        }*/
    }
}