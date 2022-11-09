using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Utils;
using static Unity.Mathematics.math;
using static Unity.Mathematics.noise;

namespace World
{
    public static class NoiseGenerator
    {
        public static Future<T> ScheduleChunkNoise<T>(int chunkSize, int seed, int2 chunkPosition,
            NoiseSettings settings) where T : struct, IGenerateNoiseJob, IJobParallelFor
        {
            var length = (chunkSize + 1) * (chunkSize + 1);
            var job = new T()
            {
                ChunkSize = chunkSize,
                Seed = seed,
                ChunkPosition = chunkPosition,
                Lacunarity = settings.lacunarity,
                Frequency = settings.frequency,
                Amplitude = settings.amplitude,
                Persistence = settings.persistence,
                Octaves = settings.octaves,
                Noise = new NativeArray<float>(length, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory)
            };
            return new Future<T>()
            {
                Job = job,
                JobHandle = job.Schedule(length, length / Environment.ProcessorCount),
                Arrays = new IDisposable[] {job.Noise}
            };
        }

        public interface IGenerateNoiseJob
        {
            public int ChunkSize { get; init; }
            public float Seed { get; init; }
            public int2 ChunkPosition { get; init; }
            public float Persistence { get; init; }
            public float Lacunarity { get; init; }
            public float Octaves { get; init; }
            public float Frequency { get; init; }
            public float Amplitude { get; init; }
            public NativeArray<float> Noise { get; init; }
        }

        [BurstCompile]
        public readonly struct GenerateMountainNoiseJob : IGenerateNoiseJob, IJobParallelFor
        {
            [field: ReadOnly] public int ChunkSize { get; init; }
            [field: ReadOnly] public float Seed { get; init; }
            [field: ReadOnly] public int2 ChunkPosition { get; init; }
            [field: ReadOnly] public float Persistence { get; init; }
            [field: ReadOnly] public float Lacunarity { get; init; }
            [field: ReadOnly] public float Octaves { get; init; }
            [field: ReadOnly] public float Frequency { get; init; }
            [field: ReadOnly] public float Amplitude { get; init; }
            public NativeArray<float> Noise { get; init; }

            public void Execute(int index)
            {
                var noise = Noise;
                var x = index % (ChunkSize + 1);
                var y = index / (ChunkSize + 1);
                var frequency = Frequency;
                var amplitude = Amplitude;
                noise[index] = 0;

                for (var octave = 0; octave < Octaves; octave++)
                {
                    // ReSharper disable once PossibleLossOfFraction
                    var value = cnoise(float2(((Seed + (ChunkPosition.x)) + (x / (float) ChunkSize)) * frequency,
                        // ReSharper disable once PossibleLossOfFraction
                        ((Seed + (ChunkPosition.y)) + (y / (float) ChunkSize)) * frequency));
                    value = abs(value);
                    //Invert
                    value *= -1;
                    //Scale from 0 to 1;
                    value += 1;
                    noise[index] += value * amplitude * (Persistence / (octave + 1));
                    frequency *= Lacunarity;
                    amplitude /= Lacunarity;
                }
            }
        }
        [BurstCompile]
        public readonly struct GenerateTerrainNoiseJob : IGenerateNoiseJob, IJobParallelFor
        {
            [field: ReadOnly] public int ChunkSize { get; init; }
            [field: ReadOnly] public float Seed { get; init; }
            [field: ReadOnly] public int2 ChunkPosition { get; init; }
            [field: ReadOnly] public float Persistence { get; init; }
            [field: ReadOnly] public float Lacunarity { get; init; }
            [field: ReadOnly] public float Octaves { get; init; }
            [field: ReadOnly] public float Frequency { get; init; }
            [field: ReadOnly] public float Amplitude { get; init; }
            public NativeArray<float> Noise { get; init; }

            public void Execute(int index)
            {
                var noise = Noise;
                var x = index % (ChunkSize + 1);
                var y = index / (ChunkSize + 1);
                var frequency = Frequency;
                var amplitude = Amplitude;
                noise[index] = 0;

                for (var octave = 0; octave < Octaves; octave++)
                {
                    // ReSharper disable once PossibleLossOfFraction
                    var value = cnoise(float2(((Seed + (ChunkPosition.x)) + (x / (float) ChunkSize)) * frequency,
                        // ReSharper disable once PossibleLossOfFraction
                        ((Seed + (ChunkPosition.y)) + (y / (float) ChunkSize)) * frequency));
                    noise[index] += value * amplitude * (Persistence / (octave + 1));
                    frequency *= Lacunarity;
                    amplitude /= Lacunarity;
                }
            }
        }
        [BurstCompile]
        public readonly struct GenerateFlatnessNoiseJob : IGenerateNoiseJob, IJobParallelFor
        {
            [field: ReadOnly] public int ChunkSize { get; init; }
            [field: ReadOnly] public float Seed { get; init; }
            [field: ReadOnly] public int2 ChunkPosition { get; init; }
            [field: ReadOnly] public float Persistence { get; init; }
            [field: ReadOnly] public float Lacunarity { get; init; }
            [field: ReadOnly] public float Octaves { get; init; }
            [field: ReadOnly] public float Frequency { get; init; }
            [field: ReadOnly] public float Amplitude { get; init; }
            public NativeArray<float> Noise { get; init; }

            public void Execute(int index)
            {
                var noise = Noise;
                var x = index % (ChunkSize + 1);
                var y = index / (ChunkSize + 1);
                var frequency = Frequency;
                var amplitude = Amplitude;
                noise[index] = 0;

                for (var octave = 0; octave < Octaves; octave++)
                {
                    // ReSharper disable once PossibleLossOfFraction
                    var value = cnoise(float2(((Seed + (ChunkPosition.x)) + (x / (float) ChunkSize)) * frequency,
                        // ReSharper disable once PossibleLossOfFraction
                        ((Seed + (ChunkPosition.y)) + (y / (float) ChunkSize)) * frequency));
                    // Scale to 0 to 1
                    value += 1;
                    value *= 0.5f;
                    noise[index] += value * amplitude * (Persistence / (octave + 1));
                    frequency *= Lacunarity;
                    amplitude /= Lacunarity;
                }
            }
        }
        
        /*[BurstCompile]
        internal struct GenerateFlatnessNoiseJob : IGenerateNoiseJob
        {
            [field: ReadOnly] public int ChunkSize { get; init; }
            [field: ReadOnly] public float Seed { get; init; }
            [field: ReadOnly] public int2 ChunkPosition { get; init; }
            [field: ReadOnly] public float Size { get; init; }
            [field: ReadOnly] public float Frequency { get; init; }
            [field: ReadOnly] public float Amplitude { get; init; }
            [field: ReadOnly] public float Persistence { get; init; }
            [field: ReadOnly] public int Octave { get; init; }
            public NativeArray<float> Noise { get; set; }
            
            public void Execute()
            {
                for (var y = 0; y <= ChunkSize; y++)
                {
                    for (var x = 0; x <= ChunkSize; x++)
                    {
                        var value = cnoise(float2(((Seed + (ChunkPosition.x)) + (x / Size)) * Frequency,
                            ((Seed + (ChunkPosition.y)) + (y / Size)) * Frequency));
                        var noiseArray = Noise;
                        value += 1;
                        value *= 0.5f;
                        noiseArray[x + y * (ChunkSize + 1)] += (value * Amplitude) * (Persistence / (Octave + 1));
                        Noise = noiseArray;
                    }
                }
            }
        }*/
    }
}