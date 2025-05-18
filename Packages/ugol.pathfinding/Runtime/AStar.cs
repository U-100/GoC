using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

[BurstCompile]
public struct AStar : IJob
{
    private struct Node
    {
        public int2 pos;
        public int g;
        public int h;

        public int2 parent;
    }

    private static readonly int2[] OFFSETS =
    {
        new(1, 0),
        new(-1, 0),
        new(0, 1),
        new(0, -1),
    };

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

        int2 fromTo = abs(from - to);
        int listInitialCapacity = fromTo.x + fromTo.y;
        NativeHashMap<int2, Node> openList = new(listInitialCapacity, Allocator.Temp);
        NativeHashMap<int2, Node> closedList = new(listInitialCapacity, Allocator.Temp);
        
        // add start position to open list
        //openList[from] = new Node { pos = from, g = 0, h = fromTo.x + fromTo.y, parent = from };
        openList[from] = new Node { pos = from, g = 0, h = H(ref fromTo), parent = from };

        do
        {
            // find the best node on the open list
            Node curNode = new Node();
            NativeArray<Node> nodes = openList.GetValueArray(Allocator.Temp);
            int foundF = int.MaxValue;
            for(int i = 0; i < nodes.Length; ++i)
            {
                Node node = nodes[i];
            
                int f = node.g + node.h;
                if (f < foundF)
                {
                    foundF = f;
                    curNode = node;
                }
            }
            nodes.Dispose();
            
            // remove from open list
            openList.Remove(curNode.pos);

            bool haveCandidate;
            do
            {
                haveCandidate = false;
                Node candidateNode = new();
                int candidateF = int.MaxValue;

                // add to closed list
                closedList[curNode.pos] = curNode;
            
                if (all(curNode.pos == to))
                    break; // destination reached
            
                for (int iOffset = 0; iOffset < OFFSETS.Length; ++iOffset)
                {
                    int2 neighbourPos = curNode.pos + OFFSETS[iOffset];
                    if (
                        all(neighbourPos >= 0 & neighbourPos < size)  // if in range
                        && grid[neighbourPos.x + neighbourPos.y * size.x] == 0 // is passable
                        && any(neighbourPos != curNode.parent) // is not current nodes parent
                    )
                    {
                        if (closedList.ContainsKey(neighbourPos))
                            continue;

                        int neighbourG = curNode.g + 1;
                        int2 absDelta = abs(neighbourPos - to);
                        int neighbourH = H(ref absDelta);

                        if (openList.TryGetValue(neighbourPos, out Node openNeighbourNode))
                        {
                            if (openNeighbourNode.g > neighbourG)
                            {
                                openNeighbourNode.g = neighbourG;
                                openList[neighbourPos] = openNeighbourNode;
                            }
                        }
                        else
                        {
                            if (neighbourG + neighbourH <= curNode.g + curNode.h && neighbourG + neighbourH < candidateF)
                            {
                                // we found a new candidate for immediate processing

                                if (haveCandidate) // if we already had a candidate
                                    openList.Add(candidateNode.pos, candidateNode); // store it on the open list

                                // assign a new candidate
                                candidateF = neighbourG + neighbourH;
                                candidateNode = new() { pos = neighbourPos, g = neighbourG, h = neighbourH, parent = curNode.pos };

                                haveCandidate = true;
                            }
                            else
                                openList.Add(neighbourPos, new() { pos = neighbourPos, g = neighbourG, h = neighbourH, parent = curNode.pos });
                        }
                    }
                }

                if (haveCandidate)
                    curNode = candidateNode;
                
            } while (haveCandidate);
            
            if (all(curNode.pos == to))
                break; // destination reached
            
        } while(openList.Count > 0);

        if (closedList.TryGetValue(to, out Node lastNode)) // if destination is on the closed list then we have found a path
        {
            int2 curPos = to;
            // there should be exactly toNode.g elements of the path
            path.Resize(lastNode.g + 1, NativeArrayOptions.UninitializedMemory);
            for (int i = lastNode.g; i >= 0; --i)
            {
                path[i] = curPos;
                curPos = lastNode.parent;
                lastNode = closedList[curPos];
            }
        }
        else
            path.Resize(0, NativeArrayOptions.UninitializedMemory);
        
        openList.Dispose();
        closedList.Dispose();
    }
    
    [BurstCompile]
    private static int H(ref int2 p) => p.x * p.x + p.y * p.y;
}
