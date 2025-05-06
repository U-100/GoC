using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

[TestFixture]
public class TestAStar2
{
    [Test]
    public void TestLargeGrid()
    {
        int large = 64;

        AStar2 aStar2 = new()
        {
            grid = new NativeArray<int>(large * large, Allocator.TempJob),
            size = math.int2(large, large),
            from = int2(0, 0),
            to = int2(large - 1, large - 1),
            // from = int2(10, 10),
            // to = int2(20, 20),
            path = new NativeList<int2>(0, Allocator.TempJob)
        };

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        aStar2.Run();
        stopwatch.Stop();
        Debug.Log(stopwatch.ElapsedTicks);
        
        Assert.That(aStar2.path.Length == large + large - 1);
        
        aStar2.grid.Dispose();
        aStar2.path.Dispose();
    }
}
