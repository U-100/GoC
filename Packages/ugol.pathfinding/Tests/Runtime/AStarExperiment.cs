using System.Collections;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public class AStarExperiment : MonoBehaviour
{
    private struct Node
    {
        public int2 pos;
        public int g;
        public int h;

        public int2 parent;
    }

    public int2 size;
    public NativeArray<int> grid;
    
    public int2 from;
    public int2 to;
    
    public NativeList<int2> path;

    private NativeHashMap<int2, Node> openList;
    private NativeHashMap<int2, Node> closedList;
    
    
    
    void OnDrawGizmos()
    {
        if (closedList.IsCreated)
        {
            for (int z = 0; z < size.y; z++)
            for (int x = 0; x < size.x; x++)
            {
                if (closedList.ContainsKey(int2(x, z)))
                    Gizmos.DrawWireCube(new Vector3(x + .5f, 0, z + .5f), new Vector3(1, 0, 1));
            }
        }

        if (path.IsCreated)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < path.Length; i++)
            {
                Gizmos.DrawWireCube(new Vector3(path[i].x + .5f, 0, path[i].y + .5f), new Vector3(1, 0, 1));
            }
        }
    }
    
    IEnumerator Start()
    {
        grid = new(size.x * size.y, Allocator.Persistent);
        path = new(0, Allocator.Persistent);
        yield return new WaitForSeconds(2f);
        
        
        if (any(from < 0 | from >= size | to < 0 | to >= size))
        {
            // from and to must be in range, otherwise the path doesn't exist
            path.Resize(0, NativeArrayOptions.UninitializedMemory);
            yield break;
        }

        int2 fromTo = abs(from - to);
        int listInitialCapacity = fromTo.x + fromTo.y;
        // NativeHashMap<int2, Node> openList = new(listInitialCapacity, Allocator.Temp);
        // NativeHashMap<int2, Node> closedList = new(listInitialCapacity, Allocator.Temp);
        openList = new(listInitialCapacity, Allocator.Persistent);
        closedList = new(listInitialCapacity, Allocator.Persistent);
        
        // add start position to open list
        //openList[from] = new Node { pos = from, g = 0, h = fromTo.x + fromTo.y, parent = from };
        openList[from] = new Node { pos = from, g = 0, h = fromTo.x * fromTo.x + fromTo.y * fromTo.y, parent = from };

        NativeArray<int2> offsets = new (4, Allocator.Persistent);//new (4, Allocator.Temp);
        offsets[0] = int2(1, 0);
        offsets[1] = int2(-1, 0);
        offsets[2] = int2(0, 1);
        offsets[3] = int2(0, -1);
        
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
            int2 curNodePos = curNode.pos;
            
            // remove from open list
            openList.Remove(curNodePos);
            
            // add to closed list
            closedList[curNodePos] = curNode;

            yield return new WaitForSeconds(.5f);
            
            if (all(curNodePos == to))
                break; // destination reached
            
            for (int iOffset = 0; iOffset < offsets.Length; ++iOffset)
            {
                int2 neighbourPos = curNodePos + offsets[iOffset];
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
                    //int neighbourH = absDelta.x + absDelta.y;
                    int neighbourH = absDelta.x * absDelta.x + absDelta.y * absDelta.y;

                    if (openList.TryGetValue(neighbourPos, out Node openNeighbourNode))
                    {
                        if (openNeighbourNode.g > neighbourG)
                        {
                            openNeighbourNode.g = neighbourG;
                            openList[neighbourPos] = openNeighbourNode;
                        }
                    }
                    else
                        openList.Add(neighbourPos, new() { pos = neighbourPos, g = neighbourG, h = neighbourH, parent = curNodePos });
                }
            }
        } while(openList.Count > 0);

        if (closedList.TryGetValue(to, out Node toNode)) // if destination is on the closed list then we have found a path
        {
            int2 curPos = to;
            // there should be exactly toNode.g elements of the path
            path.Resize(toNode.g + 1, NativeArrayOptions.UninitializedMemory);
            for (int i = toNode.g; i >= 0; --i)
            {
                path[i] = curPos;
                curPos = toNode.parent;
                toNode = closedList[curPos];
            }
        }
        else
            path.Resize(0, NativeArrayOptions.UninitializedMemory);
        
        //openList.Dispose();
        //closedList.Dispose();
        offsets.Dispose();
        
        
        grid.Dispose();
    }
}
