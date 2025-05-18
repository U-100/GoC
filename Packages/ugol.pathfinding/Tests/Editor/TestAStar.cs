using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

[TestFixture]
public class TestAStar
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
        try
        {
            aStar.Run();

            Assert.That(aStar.path.Length == 0);
        }
        finally
        {
            aStar.grid.Dispose();
            aStar.path.Dispose();
        }
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
        
        try
        {
            aStar.Run();
        
            Assert.That(aStar.path.Length == 0);
        }
        finally
        {
            aStar.grid.Dispose();
            aStar.path.Dispose();
        }
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
        
        try
        {
            aStar.Run();
        
            Debug.Log("Test2X2:");
            foreach (int2 p in aStar.path)
                Debug.Log(p);
        }
        finally
        {
            aStar.grid.Dispose();
            aStar.path.Dispose();
        }
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

        try
        {
            // add obstacles
            aStar.grid[0 + 1 * 3] = 1;
            aStar.grid[2 + 1 * 3] = 1;
        
            aStar.Run();

            Debug.Log("Test3X3:");
            foreach (int2 p in aStar.path)
                Debug.Log(p);
        }
        finally
        {
            aStar.grid.Dispose();
            aStar.path.Dispose();
        }
    }

    [Test]
    public void TestEnoughManhattanDistance()
    {
        AStar aStar = new()
        {
            grid = new NativeArray<int>(9, Allocator.TempJob),
            size = int2(3, 3),
            from = int2(0, 0),
            to = int2(2, 2),
            enoughManhattanDistance = 1,
            path = new NativeList<int2>(0, Allocator.TempJob)
        };

        try
        {
            // add obstacles
            aStar.grid[0 + 1 * 3] = 1;
            aStar.grid[2 + 1 * 3] = 1;

            aStar.Run();

            int2 pathEnd = aStar.path[^1];
            Assert.AreEqual(aStar.enoughManhattanDistance, abs(pathEnd.x - aStar.to.x) + abs(pathEnd.y - aStar.to.y));
        }
        finally
        {
            aStar.grid.Dispose();
            aStar.path.Dispose();
        }
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

        try
        {
            // add obstacles
            aStar.grid[0 + 1 * 3] = 1;
            aStar.grid[1 + 1 * 3] = 1;
            aStar.grid[2 + 1 * 3] = 1;
        
            aStar.Run();
        
            Assert.That(aStar.path.Length == 0);
        }
        finally
        {
            aStar.grid.Dispose();
            aStar.path.Dispose();
        }
    }

    [Test]
    public void TestLargeGrid()
    {
        int large = 64;

        AStar aStar = new()
        {
            grid = new NativeArray<int>(large * large, Allocator.TempJob),
            size = int2(large, large),
            from = int2(0, 0),
            to = int2(large - 1, large - 1),
            // from = int2(10, 10),
            // to = int2(20, 20),
            path = new NativeList<int2>(0, Allocator.TempJob)
        };

        try
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            aStar.Run();
            stopwatch.Stop();
            Debug.Log(stopwatch.ElapsedTicks);
        
            Assert.That(aStar.path.Length == large + large - 1);
        }
        finally
        {
            aStar.grid.Dispose();
            aStar.path.Dispose();
        }
    }
}
