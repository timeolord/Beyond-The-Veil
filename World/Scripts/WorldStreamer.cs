using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MeshUtil;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utils;

namespace World
{
    public class WorldStreamer : MonoBehaviour
    {
        public GameObject player;
        public GameObject chunkPrefab;
        public Settings settings;
        public GameObject worldContainer;

        private List<MeshFuture> MeshFutures { get; set; }
        private List<int> MeshFuturesIndex { get; set; }

        public void Init()
        {
            InitializeChunks();
            MeshFutures = new List<MeshFuture>();
            MeshFuturesIndex = new List<int>();
        }
        private void InitializeChunks()
        {
            Chunks.meshes = new Mesh[settings.mapSize * settings.mapSize];
            
            Chunks.completedChunks = new Dictionary<int, int>();
            Chunks.scheduledChunks = new Dictionary<int, int>();
            Chunks.positions = WorldGenerator.GetChunkPositions(settings.mapSize);
            Chunks.renderChunk = GetPlayerChunk(player, settings.chunkSize) + new Vector2Int(10, 10);
            (Chunks.gameObjects, Chunks.meshFilters) = InitializeChunkGameObjects(chunkPrefab, worldContainer, settings.renderDistance);
        }

        private static (GameObject[], MeshFilter[]) InitializeChunkGameObjects(GameObject chunkPrefab, GameObject worldContainer, int renderDistance)
        {
            var gameObjects = new GameObject[(renderDistance * 2 + 1) * (renderDistance * 2 + 1)];
            var meshFilters = new MeshFilter[(renderDistance * 2 + 1) * (renderDistance * 2 + 1)];
            var length = gameObjects.Length;
            
            for (var i = 0; i < length; i++)
            {
                gameObjects[i] = Instantiate(chunkPrefab, worldContainer.transform);
                meshFilters[i] = gameObjects[i].GetComponent<MeshFilter>();
            }
            
            return (gameObjects, meshFilters);
        }
        
        private static int[] GetChunksInRange(int range, int mapSize, Vector2Int playerPosition)
        {
            var xStart = playerPosition.x - range >= 0 ? playerPosition.x - range : 0;
            var xEnd = playerPosition.x + range < mapSize ? playerPosition.x + range : mapSize;
            var yStart = playerPosition.y - range >= 0 ? playerPosition.y - range : 0;
            var yEnd = playerPosition.y + range < mapSize ? playerPosition.y + range : mapSize;
            var xSideLength = xEnd - xStart;
            var ySideLength = yEnd - yStart;
            var activeChunks = new int[xSideLength * ySideLength];

            for (int y = yStart, i = 0; y < yEnd; y++)
            {
                for (var x = xStart; x < xEnd; x++, i++)
                {
                    activeChunks[i] = x + y * mapSize;
                }
            }

            return activeChunks;
        }

        private static Vector2Int GetPlayerChunk(GameObject player, int chunkSize)
        {
            var position = player.transform.position;
            var chunkPosition = new Vector2Int(
                Mathf.FloorToInt(position.x / chunkSize),
                Mathf.FloorToInt(position.z / chunkSize));
            return chunkPosition;
        }

        private static void RenderChunk(IReadOnlyList<int> chunkIndices, IReadOnlyList<Mesh> meshes,
            (GameObject[], MeshFilter[], int2[]) chunks, int chunkSize)
        {
            var length = chunkIndices.Count;
            for (var i = 0; i < length; i++)
            {
                var (gameObjects, meshFilters, positions) = chunks;
                gameObjects[i].transform.position = new Vector3 (
                    positions[chunkIndices[i]].x * chunkSize, 0, positions[chunkIndices[i]].y * chunkSize);
                meshFilters[i].mesh = meshes[chunkIndices[i]];
            }
        }

        private static void CompleteChunks(IList<MeshFuture> meshFutures, IList<int> chunksToSchedule,
            Mesh[] meshes, ref Dictionary<int, int> completedChunks)
        {
            var length = meshFutures.Count;
            for (var i = 0; i < length; i++)
            {
                var chunkIndex = chunksToSchedule[i];
                if (completedChunks.ContainsKey(chunkIndex)) continue;
                
                var mesh = meshFutures[i].Complete();
                meshes[chunkIndex] = mesh;
                completedChunks.Add(chunkIndex, 0);
            }
        }

        private static List<MeshFuture> ScheduleMeshes(IList<Future<CombineNoisesJob>> heightMaps,
            IList<Future<GenerateMaterialMapJob>> materialMap, IReadOnlyList<int> chunksToSchedule,
            ref Dictionary<int, int> scheduleChunks, int chunkSize)
        {
            var chunksLength = chunksToSchedule.Count;
            var meshFutures = new MeshFuture[chunksLength];
            for (var x = 0; x < chunksLength; x++)
            {
                meshFutures[x] =
                    WorldGenerator.GenerateChunkMeshFuture(heightMaps[x], materialMap[x], chunkSize);
                scheduleChunks.Add(chunksToSchedule[x], 0);
            }

            return meshFutures.ToList();
        }

        private static List<int> FindUnscheduledMeshes(int[] chunksToSchedule, Dictionary<int, int> scheduledChunks)
        {
            var length = chunksToSchedule.Length;
            var toSchedule = new List<int>(length);
            for (var i = 0; i < length; i++)
            {
                if (!scheduledChunks.ContainsKey(chunksToSchedule[i]))
                {
                    toSchedule.Add(chunksToSchedule[i]);
                }
            }

            return toSchedule;
        }

        public void CompleteWork()
        {
            var currentRenderChunk = GetPlayerChunk(player, settings.chunkSize);
            
            CompleteChunks(MeshFutures, MeshFuturesIndex, Chunks.meshes, ref Chunks.completedChunks);
            
            var chunksToRender = GetChunksInRange(settings.renderDistance, settings.mapSize, currentRenderChunk);
            
            RenderChunk(chunksToRender, Chunks.meshes, (Chunks.gameObjects, Chunks.meshFilters, Chunks.positions),
                settings.chunkSize);
        }

        public void ScheduleWork()
        {
            MeshFutures.Clear();
            MeshFuturesIndex.Clear();
            
            var currentRenderChunk = GetPlayerChunk(player, settings.chunkSize);
            
            if (currentRenderChunk == Chunks.renderChunk) return;
            
            var chunksToSchedule = GetChunksInRange(settings.scheduleDistance, settings.mapSize,
                currentRenderChunk);
            var unscheduledChunks = FindUnscheduledMeshes(chunksToSchedule,
                Chunks.scheduledChunks);
            
            if (unscheduledChunks.Count == 0) return;
            
            var heightMaps = WorldGenerator.ScheduleHeightMaps(unscheduledChunks, Chunks.positions, settings);
            var materialMaps = WorldGenerator.ScheduleMaterialMaps(heightMaps, settings.chunkSize);
            MeshFutures = ScheduleMeshes(heightMaps, materialMaps, unscheduledChunks, ref Chunks.scheduledChunks, 
                settings.chunkSize);
            MeshFuturesIndex = unscheduledChunks;
            
            Chunks.renderChunk = currentRenderChunk;
        }
    }
}

