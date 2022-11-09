using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MeshUtil;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Utils;
using World;
using Random = UnityEngine.Random;
using static Unity.Mathematics.math;
using static World.TileName;
using static World.NoiseGenerator;

namespace World
{
    public static class WorldGenerator
    {
    
        public static IList<Future<CombineNoisesJob>> ScheduleHeightMaps(List<int> chunks, int2[] chunkPositions, Settings settings)
        {
            var length = chunks.Count;
            var heightMaps = new Future<CombineNoisesJob>[length];

            for (var i = 0; i < length; i++)
            {
                var chunkPosition = chunkPositions[chunks[i]];
                
                var terrainFuture = ScheduleChunkNoise<GenerateTerrainNoiseJob>(settings.chunkSize, settings.seed,
                    chunkPosition, settings.terrainSettings);
                var mountainFuture = ScheduleChunkNoise<GenerateMountainNoiseJob>(settings.chunkSize, settings.seed,
                    chunkPosition, settings.mountainSettings);
                var flatnessFuture = ScheduleChunkNoise<GenerateFlatnessNoiseJob>(settings.chunkSize, settings.seed,
                    chunkPosition, settings.flatnessSettings);
                
                var combineJob = new CombineNoisesJob()
                {
                    heightMap = terrainFuture.Job.Noise,
                    mountainNoise = mountainFuture.Job.Noise,
                    flatnessNoise = flatnessFuture.Job.Noise,
                };
                var combineFuture = new Future<CombineNoisesJob>()
                {
                    Job = combineJob,
                    JobHandle = combineJob.Schedule(terrainFuture.Job.Noise.Length,
                        terrainFuture.Job.Noise.Length / Environment.ProcessorCount, 
                        JobHandle.CombineDependencies(terrainFuture.JobHandle, mountainFuture.JobHandle,
                            flatnessFuture.JobHandle)),
                    Arrays =
                        terrainFuture.Arrays.Concat(mountainFuture.Arrays).Concat(flatnessFuture.Arrays).ToList()
                };

                heightMaps[i] = combineFuture;
            }
            
            return heightMaps;
        }
        public static IList<Future<GenerateMaterialMapJob>> ScheduleMaterialMaps(IList<Future<CombineNoisesJob>> heightMaps, int chunkSize)
        {
            var length = heightMaps.Count;
            var futures = new Future<GenerateMaterialMapJob>[length];
            for (var i = 0; i < length; i++)
            {
                var job = new GenerateMaterialMapJob()
                {
                    chunkSize = chunkSize,
                    heightMap = heightMaps[i].Job.heightMap,
                    materialMap = new NativeArray<int>(chunkSize * chunkSize, Allocator.TempJob,
                        NativeArrayOptions.UninitializedMemory),
                };
                futures[i] = new Future<GenerateMaterialMapJob>()
                {
                    Job = job,
                    JobHandle = job.Schedule(heightMaps[i].JobHandle),
                    Arrays = new List<IDisposable>(){job.materialMap}
                };
            }
            return futures;
        }
        
        
        public static MeshFuture GenerateChunkMeshFuture(Future<CombineNoisesJob> heightmap, Future<GenerateMaterialMapJob> materialMap, int chunkSize)
        {
            return MeshGenerator.GenerateFacetedPlaneFuture(heightmap, materialMap, chunkSize);
        }
        
        public static int2[] GetChunkPositions(int mapSize)
        {
            var chunkPositions = new int2[mapSize * mapSize]; 
            for (var y = 0; y < mapSize; y++)
            {
                for (var x = 0; x < mapSize; x++)
                {
                    chunkPositions[x + y * mapSize] = new int2(x, y);
                }
            }

            return chunkPositions;
        }
        
    }
    
    [BurstCompile]
    public struct CombineNoisesJob : IJobParallelFor
    {
        public NativeArray<float> heightMap;
        [ReadOnly] public NativeArray<float> mountainNoise;
        [ReadOnly] public NativeArray<float> flatnessNoise;
    
        public void Execute(int index)
        {
            heightMap[index] += mountainNoise[index] * flatnessNoise[index];
        }
    }
    [BurstCompile]
    public struct GenerateMaterialMapJob : IJob
    {
        [ReadOnly] public int chunkSize;
        [ReadOnly] public NativeArray<float> heightMap;
        public NativeArray<int> materialMap;

        public void Execute()
        {
            for (var y = 0; y < chunkSize; y++)
            {
                for (var x = 0; x < chunkSize; x++)
                {
                    var height = heightMap[x + y * (chunkSize + 1)];
                    var angle = GetTileAngle(x, y);
                    
                    materialMap[x + y * chunkSize] = (int) GetTileMaterialID(height, angle);
                }
            }
        }
        private static TileName GetTileMaterialID(float height, float slope)
        {
            if (slope > 50f)
            {
                return Stone;
            }
            return height switch
            {
                _ => Grass
            };
        }
    
        private float GetTileAngle(int x, int y)
        {
            var pos0 = new float3(0, heightMap[x + y * (chunkSize + 1)], 0);
            var pos1 = new float3(1, heightMap[x + 1 + y * (chunkSize + 1)], 0);
            var pos2 = new float3(0, heightMap[x + (y + 1) * (chunkSize + 1)], 1);
            var normalVector = -normalize(cross(pos1 - pos0, pos2 - pos0));
            var angle = Vector3.Angle(normalVector, Vector3.up);
            return angle;
        }
    }
}