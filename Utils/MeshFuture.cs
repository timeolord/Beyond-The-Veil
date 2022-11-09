using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mesh;

namespace MeshUtil
{
    public struct MeshFuture
    {
        public MeshDataArray meshDataArray;
        public JobHandle[] jobHandles;
        public NativeArray<Bounds> bounds;
        public IList<IDisposable> arrays;
        public int planeSize;

        public Mesh Complete()
        {
            for (var i = 0; i < jobHandles.Length; i++)
            {
                jobHandles[i].Complete();
            }
            var mesh = new Mesh()
            {
                bounds = bounds[0]
            };
            var meshData = meshDataArray[0];
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, planeSize * planeSize * 6)
            {
                firstVertex = 0,
                bounds = bounds[0],
                vertexCount = planeSize * planeSize * 4
            }, MeshUpdateFlags.DontRecalculateBounds);
            
            ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
            bounds.Dispose();
            
            for (var i = 0; i < arrays.Count; i++)
            {
                arrays[i].Dispose();
            }
            
            return mesh;
        }
    }
}