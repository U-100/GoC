using System;
using Unity.Collections;
using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
    public interface IReadOnlyGrid<T>
    {
        public int2 min { get; }
        public int2 max { get; }
        int2 size { get; }
        T this[int2 p] { get; }
    }
    
    public interface IGrid<T> : IReadOnlyGrid<T> where T : struct
    {
        new T this[int2 p] { get; set; }
        
        NativeArray<T> nativeArray { get; }
    }

    public class Grid<T> : IGrid<T>, IDisposable where T : struct
    {
        private NativeArray<T> _grid;
        
        public Grid(int2 initMin, int2 initMax)
        {
            if (any(initMin > initMax))
                throw new ArgumentException("initMin must be smaller or equal to initMax", nameof(initMin));
            
            min = initMin;
            max = initMax;
            
            _grid = new(size.x * size.y, Allocator.Persistent);
        }

        public void Dispose() => _grid.Dispose();

        public int2 min { get; }
        public int2 max { get; }
        public int2 size => max - min + 1;
        
        T IReadOnlyGrid<T>.this[int2 p] => this[p];
        
        public NativeArray<T> nativeArray => _grid;

        public T this[int2 p]
        {
            get
            {
                if (!IsInRange(p)) throw new ArgumentOutOfRangeException(nameof(p));
                return _grid[IndexOf(p)];
            }
            set
            {
                if (!IsInRange(p)) throw new ArgumentOutOfRangeException(nameof(p));
                _grid[IndexOf(p)] = value;
            }
        }

        private bool IsInRange(int2 p) => all(min <= p & p <= max);
        private int IndexOf(int2 p) => p.x - min.x + (p.y - min.y) * (max.y - min.y + 1);
    }
}
