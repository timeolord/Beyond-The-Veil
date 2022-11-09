using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;
using Utils;
using World;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;
using static UnityEngine.Mesh;
using Random = UnityEngine.Random;

namespace MeshUtil
{
    public static class MeshGenerator
    {
        public static MeshFuture GenerateFacetedPlaneFuture(Future<CombineNoisesJob> heightMap, Future<GenerateMaterialMapJob> materialMap, int planeSize)
        {
            var meshDataArray = AllocateWritableMeshData(1);
            var meshData = meshDataArray[0];
            SetVertexAttributes(meshData, planeSize * planeSize * 4);
            var boundsArray = new NativeArray<Bounds>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            var verticesJob = ScheduleVertices(meshData, heightMap, materialMap, planeSize);
            var trianglesJob = ScheduleTriangles(meshData, planeSize);
            var bounds = ScheduleBounds(heightMap, boundsArray, planeSize);
            var normals = ScheduleNormals(meshData, verticesJob, planeSize);

            return new MeshFuture()
            {
                meshDataArray = meshDataArray,
                arrays = heightMap.Arrays.Concat(materialMap.Arrays).ToList(),
                bounds = boundsArray,
                jobHandles = new []{verticesJob, trianglesJob, bounds, normals},
                planeSize = planeSize
            };
        }
        private static void SetVertexAttributes(MeshData meshData, int vertexCount)
        {
            const int vertexAttributeCount = 3;
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
                vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );
            vertexAttributes[0] = new VertexAttributeDescriptor(
                dimension: 3, stream: 0
            );
            vertexAttributes[1] = new VertexAttributeDescriptor(
                VertexAttribute.Normal, dimension: 3, stream: 1
            );
            vertexAttributes[2] = new VertexAttributeDescriptor(
                VertexAttribute.TexCoord0, dimension: 3, stream: 2
            );
            meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
            vertexAttributes.Dispose();
        }
        private static JobHandle ScheduleBounds(Future<CombineNoisesJob> heightMap, NativeArray<Bounds> bounds, int planeSize)
        {
            var job = new CalculateBoundsJob()
            {
                heightMap = heightMap.Job.heightMap,
                bounds = bounds,
                planeSize = planeSize,
            };
            return job.Schedule(heightMap.JobHandle);
        }
        
        private static JobHandle ScheduleVertices(MeshData meshData, Future<CombineNoisesJob> heightMap, Future<GenerateMaterialMapJob> materialMap, int planeSize)
        {
            var job = new GenerateVerticesJob()
            {
                meshData = meshData,
                heightMap = heightMap.Job.heightMap,
                materialMap = materialMap.Job.materialMap,
                planeSize = planeSize
            };
            return job.Schedule(JobHandle.CombineDependencies(heightMap.JobHandle, materialMap.JobHandle));
        }
        private static JobHandle ScheduleTriangles(MeshData meshData, int planeSize)
        {
            var job = new GenerateTrianglesJob()
            {
                meshData = meshData,
                planeSize = planeSize
            };
            return job.Schedule();
        }
        
        private static JobHandle ScheduleNormals(MeshData meshData, JobHandle verticesJob, int planeSize)
        {
            var job = new CalculateNormalsJob()
            {
                meshData = meshData,
                planeSize = planeSize
            };
            return job.Schedule(verticesJob);
        }
    }
}
internal struct GenerateVerticesJob: IJob
{
    public MeshData meshData;
    [ReadOnly] public NativeArray<float> heightMap;
    [ReadOnly] public NativeArray<int> materialMap;
    [ReadOnly] public int planeSize;

    [BurstCompile]
    public void Execute ()
    {
        var positions =  meshData.GetVertexData<float3>(0);
        var uvs = meshData.GetVertexData<float3>(2);
        for (int z = 0, i = 0; z < planeSize; z++)
        {
            for (var x = 0; x < planeSize; x++, i += 4)
            {
                var pos0 = x + z * (planeSize + 1);
                var pos1 = x + 1 + z * (planeSize + 1);
                var pos2 = x + (z + 1) * (planeSize + 1);
                var pos3 = x + 1 + (z + 1) * (planeSize + 1);

                positions[i] = float3(x, heightMap[pos0], z);
                positions[i + 1] = float3(x + 1, heightMap[pos1], z);
                positions[i + 2] = float3(x, heightMap[pos2], z + 1);
                positions[i + 3] = float3(x + 1, heightMap[pos3], z + 1);
                
                uvs[i] = float3(0, 0, materialMap[x + z * planeSize]);
                uvs[i + 1] = float3(1, 0, materialMap[x + z * planeSize]);
                uvs[i + 2] = float3(0, 1, materialMap[x + z * planeSize]);
                uvs[i + 3] = float3(1, 1, materialMap[x + z * planeSize]);
            }
        }
    }
}
[BurstCompile]
internal struct GenerateTrianglesJob: IJob
{
    public MeshData meshData;
    [ReadOnly] public int planeSize;

    [BurstCompile]
    public void Execute ()
    {
        meshData.SetIndexBufferParams(planeSize * planeSize * 6, IndexFormat.UInt32);
        var triangles = meshData.GetIndexData<uint>();
        uint i = 0;
        for (int z = 0, triangleIndex = 0; z < planeSize; z++)
        {
            for (var x = 0; x < planeSize; x++, triangleIndex += 6, i += 4)
            {
                triangles[triangleIndex] = i;
                triangles[triangleIndex + 1] = i + 2;
                triangles[triangleIndex + 2] = i + 1;
                triangles[triangleIndex + 3] = i + 1;
                triangles[triangleIndex + 4] = i + 2;
                triangles[triangleIndex + 5] = i + 3;
            }
        }
    }
}

[BurstCompile]
internal struct CalculateNormalsJob: IJob
{
    public MeshData meshData;
    [ReadOnly] public int planeSize;

    [BurstCompile]
    public void Execute ()
    {
        var positions = meshData.GetVertexData<float3>(0);
        var normals = meshData.GetVertexData<float3>(1);
        for (int z = 0, i = 0; z < planeSize; z++)
        {
            for (var x = 0; x < planeSize; x++, i += 4)
            {
                var normal = -normalize(cross(positions[i + 1] - positions[i], positions[i + 2] - positions[i]));
                normals[i] = normals[i + 1] = normals[i + 2] = normals[i + 3] = normal;
            }
        }
    }
}
[BurstCompile]
internal struct CalculateBoundsJob: IJob
{
    public NativeArray<Bounds> bounds;
    [ReadOnly] public int planeSize;
    [ReadOnly] public NativeArray<float> heightMap;

    [BurstCompile]
    public void Execute ()
    {
        var (max, min) = MinMax();
        var mid = (max + min) / 2;
        var bound = bounds[0];
        
        bound.center = float3((float) planeSize / 2, mid, (float) planeSize / 2);
        bound.extents = float3(planeSize, max - min, planeSize);
        
        bounds[0] = bound;
    }

    private (int, int) MinMax()
    {
        var max = 0f;
        var min = Mathf.Infinity;
        for (var i = 0; i < heightMap.Length; i++)
        {
            if (heightMap[i] > max)
            {
                max = heightMap[i];
            }
            if (heightMap[i] < min)
            {
                min = heightMap[i];
            }
        }
        return ((int) max,(int) min);
    }
}