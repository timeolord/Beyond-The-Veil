using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace Utils
{
    public readonly struct Future<T> : IDisposable
    {
        public JobHandle JobHandle { get; init; }
        public T Job { get; init; }
        public IList<IDisposable> Arrays { get; init; }

        public T Complete()
        {
            JobHandle.Complete();
            return Job;
        }
        
        public void Dispose()
        {
            var length = Arrays.Count;
            for (var i = 0; i < length; i++)
            {
                Arrays[i].Dispose();
            }
        }
    }
}