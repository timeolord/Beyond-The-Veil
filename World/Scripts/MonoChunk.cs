using System;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace World
{
    public class MonoChunk : MonoBehaviour
    {
        /*private void OnDrawGizmosSelected()
        {
            var mesh = GetComponent<MeshFilter>().sharedMesh;
            /*Debug.Log(mesh?.GetInstanceID());#1#
            /#1#/Display the normals
            for (var i = 0; i < mesh.vertexCount; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position + mesh.vertices[i],transform.position + mesh.vertices[i] + mesh.normals[i] * 0.5f);
            }#1#
        }*/
        /*public Settings settings;
        public Tile[,] tiles;
        public float[,] heightMap;
        private NativeArray<TileMeshData> _meshJobDataArray;
        private Mesh.MeshDataArray[] _meshArrays;
        private JobHandle _meshJobHandle;

        
        public void CombineMeshes()
        {
            var materials = SortTilesByMaterials();
            var meshAndMaterials = CombineMeshByMaterial(materials);
            CombineSubMeshesByMaterial(meshAndMaterials);
        }

        private void CombineSubMeshesByMaterial((IReadOnlyList<Mesh>, List<Material>) lists)
        {
            var (meshes, materials) = lists;
            var combinedMesh = new Mesh();
            var combineInstance = new CombineInstance[meshes.Count];
            for (var i = 0; i < meshes.Count; i++)
            {
                combineInstance[i].mesh = meshes[i];
                combineInstance[i].transform = transform.localToWorldMatrix;
            }

            combinedMesh.CombineMeshes(combineInstance, false, false);
            GetComponent<MeshFilter>().sharedMesh = combinedMesh;
            GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
            GetComponent<MeshCollider>().sharedMesh = combinedMesh;
        }

        private (List<Mesh>,List<Material>) CombineMeshByMaterial(IReadOnlyDictionary<Material, List<Tile>> materials)
        {
            var meshes = new List<Mesh>();
            var materialsList = settings.tileMaterials.ToList();
            foreach (var material in settings.tileMaterials)
            {
                List<Tile> tileList;
                try
                {
                    tileList = materials[material];
                }
                catch (KeyNotFoundException)
                {
                    materialsList.Remove(material);
                    continue;
                }

                var combine = new CombineInstance[tileList.Count];
                var i = 0;
                foreach (var tile in tileList)
                {
                    combine[i].mesh = tile.mesh;
                    i++;
                }

                var mesh = new Mesh();
                mesh.CombineMeshes(combine, true, false);
                meshes.Add(mesh);
            }

            return (meshes, materialsList);
        }

        private Dictionary<Material, List<Tile>> SortTilesByMaterials()
        {
            var materials = new Dictionary<Material, List<Tile>>();
            foreach (var tile in tiles)
            {
                if (!materials.ContainsKey(tile.material))
                {
                    materials.Add(tile.material, new List<Tile>());
                }

                materials[tile.material].Add(tile);
            }

            return materials;
        }

        public void ScheduleGenerateTiles()
        {
            tiles = new Tile[settings.chunkSize, settings.chunkSize];
            _meshJobDataArray = new NativeArray<TileMeshData>(settings.chunkSize * settings.chunkSize, Allocator.TempJob);
            _meshArrays = new Mesh.MeshDataArray[settings.chunkSize * settings.chunkSize];
            
            for (var x = 0; x < settings.chunkSize; x++)
            {
                for (var z = 0; z < settings.chunkSize; z++)
                {
                    var (meshArray, meshJobData) = LoadJobData(x, z);
                    _meshJobDataArray[x + z * settings.chunkSize] = meshJobData;
                    _meshArrays[x + z * settings.chunkSize] = meshArray;
                }
            }
            
            var meshJob = new GenerateTileMeshJob
            {
                meshJobDataArray = _meshJobDataArray
            };
            
            _meshJobHandle = meshJob.Schedule(_meshJobDataArray.Length, 8);
        }

        public void CompleteGenerateTiles()
        {
            _meshJobHandle.Complete();

            for (var x = 0; x < settings.chunkSize; x++)
            {
                for (var z = 0; z < settings.chunkSize; z++)
                {
                    GenerateTile(x, z);
                }
            }

            _meshJobDataArray.Dispose();
        }

        private void GenerateTile(int x, int z)
        {
            var tileMesh = new Mesh()
            {
                bounds = _meshJobDataArray[x + z * settings.chunkSize].bounds,
                name = "TileMesh"
            };
            Mesh.ApplyAndDisposeWritableMeshData(_meshArrays[x + z * settings.chunkSize], tileMesh);
            
            var vertexHeight = new Vector4(heightMap[x, z], heightMap[x, z + 1], heightMap[x + 1, z],
                heightMap[x + 1, z + 1]);
            var maxHeight = Mathf.Max(vertexHeight.x, vertexHeight.y, vertexHeight.z, vertexHeight.w);
            
            var material = settings.GetMaterialFromHeight(maxHeight);
            var position = transform.position;
            tiles[x, z] = new Tile(tileMesh, material, new int2((int) position.x + x, (int) position.z + z));
        }

        private (Mesh.MeshDataArray, TileMeshData) LoadJobData(int x, int z)
        {
            
            var position = new Vector3(x, 0, z);
            var meshDataArray = Mesh.AllocateWritableMeshData(1);
            var vertexHeight = new Vector4(heightMap[x, z], heightMap[x, z + 1], heightMap[x + 1, z],
                heightMap[x + 1, z + 1]);
            var maxHeight = Mathf.Max(vertexHeight.x, vertexHeight.y, vertexHeight.z, vertexHeight.w);
            var minHeight = Mathf.Min(vertexHeight.x, vertexHeight.y, vertexHeight.z, vertexHeight.w);
            
            return (meshDataArray, new TileMeshData(meshDataArray[0], vertexHeight, position, maxHeight, minHeight));
        }

        private void OnMouseOver()
        {
            Events.onMouseEnterChunk.Invoke(gameObject);
        }

        private void OnMouseExit()
        {
            Events.onMouseExitChunk.Invoke(gameObject);
        }

        private struct TileMeshData
        {
            public Mesh.MeshData meshData;
            public Bounds bounds;
            public readonly Vector4 heightMap;
            public readonly Vector3 position;
            public readonly float max;
            public readonly float min;

            public TileMeshData(Mesh.MeshData meshData, Vector4 heightMap, Vector3 position, float max, float min)
            {
                this.meshData = meshData;
                this.heightMap = heightMap;
                this.position = position;
                this.max = max;
                this.min = min;
                bounds = new Bounds();
            }
        }

        [BurstCompile]
        private struct GenerateTileMeshJob : IJobParallelFor
        {
            public NativeArray<TileMeshData> meshJobDataArray;

            public void Execute(int index)
            {
                const int triangleIndexCount = 6;
                const int vertexCount = 4;
                
                var meshData = meshJobDataArray[index];
                
                //Set Vertex Attributes
                const int vertexAttributeCount = 3;
                var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
                    vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
                );
                vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
                vertexAttributes[1] = new VertexAttributeDescriptor(
                    VertexAttribute.Normal, dimension: 3, stream: 1
                );
                vertexAttributes[2] = new VertexAttributeDescriptor(
                    VertexAttribute.TexCoord0, dimension: 2, stream: 2
                );
                meshData.meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
                vertexAttributes.Dispose();
                
                //Set Positions
                var positions = meshData.meshData.GetVertexData<float3>();
                positions[0] = float3(meshData.position.x, meshData.heightMap.x, meshData.position.z);
                positions[1] = float3(meshData.position.x + 1, meshData.heightMap.z, meshData.position.z);
                positions[2] = float3(meshData.position.x, meshData.heightMap.y, meshData.position.z + 1f);
                positions[3] = float3(meshData.position.x + 1, meshData.heightMap.w, meshData.position.z + 1f);
                
                //Set Normals
                var normals = meshData.meshData.GetVertexData<float3>(1);
                normals[0] = normals[2] = normals[1] = normals[3] = -normalize(cross(positions[1] - positions[0], positions[2] - positions[0]));
                
                //Set UVs
                var texCoords = meshData.meshData.GetVertexData<float2>(2);
                texCoords[0] = float2(0f);
                texCoords[1] = float2(1f, 0f);
                texCoords[2] = float2(0f, 1f);
                texCoords[3] = float2(1f);
                
                //Set Triangle Indices
                meshData.meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt16);
                var triangleIndices = meshData.meshData.GetIndexData<ushort>();
                triangleIndices[0] = 0;
                triangleIndices[1] = 2;
                triangleIndices[2] = 1;
                triangleIndices[3] = 1;
                triangleIndices[4] = 2;
                triangleIndices[5] = 3;
                
                //Set Bounds
                var middle = (meshData.max + meshData.min) / 2;
                meshData.bounds = new Bounds(
                    new Vector3(meshData.position.x + 0.5f, middle, meshData.position.z + 0.5f),
                    new Vector3(1f,meshData.max - meshData.min, 1f));
                meshData.meshData.subMeshCount = 1;
                
                //Set Submesh
                meshData.meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount)
                {
                    bounds = meshData.bounds,
                    vertexCount = vertexCount
                }, MeshUpdateFlags.DontRecalculateBounds);
            }
        }*/
    }
}