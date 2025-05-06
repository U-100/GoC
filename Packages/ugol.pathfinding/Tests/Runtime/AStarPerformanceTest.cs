using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public class AStarPerformanceTest : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        
        int large = 64;

        AStar2 aStar = new()
        {
            grid = new NativeArray<int>(large * large, Allocator.TempJob),
            size = int2(large, large),
            from = int2(0, 0),
            to = int2(large - 1, large - 1),
            path = new NativeList<int2>(0, Allocator.TempJob)
        };

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        aStar.Run();
        stopwatch.Stop();
        Debug.Log(stopwatch.ElapsedMilliseconds);
        
        if (aStar.path.Length == large + large - 1)
            Debug.Log("aStar.path.Length is correct");
        else
            Debug.Log("aStar.path.Length is incorrect");
        
        aStar.grid.Dispose();
        aStar.path.Dispose();
    }
}
