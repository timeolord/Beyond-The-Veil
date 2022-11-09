using Unity.Mathematics;
using UnityEngine;

namespace MeshUtil
{
    public static class MeshCombiner
    {
        public static Mesh CombineMeshes(Mesh[] meshes, int2[] positions)
        {
            var length = meshes.Length;
            var combines = new CombineInstance[length];
            for (var i = 0; i < length; i++)
            {
                combines[i].mesh = meshes[i];
                combines[i].transform = Matrix4x4.Translate(new Vector3(positions[i].x, 0, positions[i].y));
            }
            var mesh = new Mesh();
            mesh.CombineMeshes(combines);
            return mesh;
        }
    }
}