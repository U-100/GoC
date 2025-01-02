using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
    public static class GridExtensions
    {
        public static int2 FindEmptyCellAround(this IReadOnlyGrid<int> grid, int2 p)
        {
            int2x2 clockWise90 = int2x2(0, -1,  1, 0);
            int2 dir = int2(1, 0);

            int distToTurn = 1;
            int dist = distToTurn;
            while (any(p < grid.min | grid.max < p) || grid[p] != 0)
            {
                p += dir;
                if (--dist == 0)
                {
                    dir = mul(clockWise90, dir);
                    if (dir.y == 0)
                        distToTurn++;

                    dist = distToTurn;
                }
            }

            return p;
        }
    }
}
