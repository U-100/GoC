using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public struct Node
{
    public int g;
    public int h;

    public int2 parent;
    public bool closed;
}

//[BurstCompile]
public struct AStar : IJob
{
    public int2 size;
    public NativeArray<int> grid;
    
    public int2 from;
    public int2 to;
    
    public NativeList<int2> path;

    public void Execute()
    {
        if (any(from < 0 | from >= size | to < 0 | to >= size))
        {
            // from and to must be in range, otherwise the path doesn't exist
            path.Resize(0, NativeArrayOptions.UninitializedMemory);
            return;
        }
        
        NativeArray<Node> nodes = new (size.x * size.y, Allocator.Temp);
        int openCount = 0;
        
        // add start position to open list
        nodes[from.x + from.y * size.x] = new Node { g = 0, h = Heuristic(from, to), parent = from };
        ++openCount;

        NativeArray<int2> offsets = new (4, Allocator.Temp);
        offsets[0] = int2(1, 0);
        offsets[1] = int2(-1, 0);
        offsets[2] = int2(0, 1);
        offsets[3] = int2(0, -1);
        
        do
        {
            (int2 cur, int curIndex, Node curNode) = FindBest(nodes);
            
            // remove from open list
            curNode.closed = true;
            nodes[curIndex] = curNode;
            --openCount;
            
            if (all(cur == to))
                break; // destination reached
            
            //if (curNode.g >= path.Length - 1)
              //  continue; // any path formed by a neighbour of cur node will be longer then the resulting path array, will not continue searching neighbours

            for (int iOffset = 0; iOffset < offsets.Length; ++iOffset)
            {
                int2 neighbour = cur + offsets[iOffset];
                int neighbourIndex = neighbour.x + neighbour.y * size.x;
                if (
                    all(neighbour >= 0 & neighbour < size)  // if in range
                    && grid[neighbourIndex] == 0 // is passable
                )
                {
                    Node neighbourNode = nodes[neighbourIndex];

                    if (!neighbourNode.closed) // if not on the closed list
                    {
                        int neighbourG = curNode.g + 1;
                        int neighbourH = Heuristic(neighbour, to);

                        if (neighbourNode.g == 0 && neighbourNode.h == 0) // if neighbour is not on the open list
                        {
                            // add neighbour to the open list
                            neighbourNode.parent = cur;
                            neighbourNode.g = neighbourG;
                            neighbourNode.h = neighbourH;
                            nodes[neighbourIndex] = neighbourNode;
                            ++openCount;
                        }
                        else
                        {
                            // check if we need to update existing neighbours open list value with a better g-value 
                            if (neighbourNode.g > neighbourG)
                            {
                                neighbourNode.g = neighbourG;
                                nodes[neighbourIndex] = neighbourNode;
                            }
                        }
                    }
                }
            }
        } while(openCount > 0);

        Node node = nodes[to.x + to.y * size.x];
        if (node.closed) // if destination is on the closed list then we have found a path
        {
            int2 cur = to;
            // there should be exactly toNode.g elements of the path
            path.Resize(node.g + 1, NativeArrayOptions.UninitializedMemory);
            for (int i = node.g; i >= 0; --i)
            {
                path[i] = cur;
                cur = node.parent;
                node = nodes[cur.x + cur.y * size.x];
            }
        }
        else
            path.Resize(0, NativeArrayOptions.UninitializedMemory);

        nodes.Dispose();
    }

    private int Heuristic(int2 p1, int2 p2)
    {
        int2 dx = abs(p1 - p2);
        return dx.x + dx.y;
    }

    private (int2, int, Node) FindBest(NativeArray<Node> nodes)
    {
        int i = 0;
        int foundF = int.MaxValue;
        int2 found = int2(-1, -1);
        Node foundNode = new Node();
        int foundIndex = -1;
        
        for(int2 p = int2(0, 0); p.y < size.y; ++p.y)
        for (p.x = 0; p.x < size.x; ++p.x, ++i)
        {
            Node node = nodes[i];
            if (node.closed)
                continue;
            
            int f = node.g + node.h;
            if (f > 0 && f < foundF)
            {
                foundF = f;
                found = p;
                foundNode = node;
                foundIndex = i;
            }
        }
        
        return (found, foundIndex, foundNode);
    }
}
