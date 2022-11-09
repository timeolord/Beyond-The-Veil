using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Jobs;
using UnityEngine;
using World;

public class GameManager : MonoBehaviour
{
    public WorldStreamer worldStreamer;

    private void Start()
    {
        worldStreamer.Init();
    }
    
    private void Update()
    {
        worldStreamer.ScheduleWork();
        
        JobHandle.ScheduleBatchedJobs();
    }

    private void LateUpdate()
    {
        worldStreamer.CompleteWork();
    }
}
