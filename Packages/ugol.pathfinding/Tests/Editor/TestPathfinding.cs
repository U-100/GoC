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
    public void TestStartOutsideRange()
    {
        AStar aStar = new()
        {
            grid = new NativeArray<int>(4, Allocator.TempJob),
            size = int2(2, 2),
            from = int2(0, 2),
            to = int2(1, 1),
            path = new NativeList<int2>(0, Allocator.TempJob)
        };
        
        aStar.Run();
        
        Assert.That(aStar.path.Length == 0);

        aStar.grid.Dispose();
        aStar.path.Dispose();
    }
    
    [Test]
    public void TestDestinationOutsideRange()
    {
        AStar aStar = new()
        {
            grid = new NativeArray<int>(4, Allocator.TempJob),
            size = int2(2, 2),
            from = int2(0, 0),
            to = int2(1, 2),
            path = new NativeList<int2>(0, Allocator.TempJob)
        };
        
        aStar.Run();
        
        Assert.That(aStar.path.Length == 0);

        aStar.grid.Dispose();
        aStar.path.Dispose();
    }
    
    [Test]
    public void Test2X2()
    {
        AStar aStar = new()
        {
            grid = new NativeArray<int>(4, Allocator.TempJob),
            size = int2(2, 2),
            from = int2(0, 0),
            to = int2(1, 1),
            path = new NativeList<int2>(0, Allocator.TempJob)
        };
        
        aStar.Run();
        
        foreach (int2 p in aStar.path)
            Debug.Log(p);
        
        aStar.grid.Dispose();
        aStar.path.Dispose();
    }

    [Test]
    public void Test3X3()
    {
        AStar aStar = new()
        {
            grid = new NativeArray<int>(9, Allocator.TempJob),
            size = int2(3, 3),
            from = int2(0, 0),
            to = int2(2, 2),
            path = new NativeList<int2>(0, Allocator.TempJob)
        };

        // add obstacles
        aStar.grid[0 + 1 * 3] = 1;
        aStar.grid[2 + 1 * 3] = 1;
        
        aStar.Run();
        
        foreach (int2 p in aStar.path)
            Debug.Log(p);
        
        aStar.grid.Dispose();
        aStar.path.Dispose();
    }

    [Test]
    public void TestNoPath()
    {
        AStar aStar = new()
        {
            grid = new NativeArray<int>(9, Allocator.TempJob),
            size = int2(3, 3),
            from = int2(0, 0),
            to = int2(2, 2),
            path = new NativeList<int2>(0, Allocator.TempJob)
        };

        // add obstacles
        aStar.grid[0 + 1 * 3] = 1;
        aStar.grid[1 + 1 * 3] = 1;
        aStar.grid[2 + 1 * 3] = 1;
        
        aStar.Run();
        
        Assert.That(aStar.path.Length == 0);
        
        aStar.grid.Dispose();
        aStar.path.Dispose();
    }
}
