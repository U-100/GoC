using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

[TestFixture]
public class TestPathfinding
{
    [Test]
    public void TestOne()
    {
        AStar aStar = new()
        {
            grid = new NativeArray<int>(4, Allocator.TempJob),
            size = int2(2, 2),
            from = int2(0, 0),
            to = int2(1, 1),
            path = new NativeList<int2>(1, Allocator.TempJob)
        };
        
        aStar.Run();

        aStar.grid.Dispose();
        
        foreach (int2 p in aStar.path)
            Debug.Log(p);
        
        aStar.path.Dispose();
    }
}
