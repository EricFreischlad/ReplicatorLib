using System.Collections.Generic;
using System.Linq;

namespace ReplicatorLib
{
    public sealed class TileEnablement
    {
        // Direction from which it is enabled, to number of tiles which enable it from that direction.
        private readonly Dictionary<MultiVector, int> _enablementCounts = new Dictionary<MultiVector, int>();

        public TileEnablement() { }
        public TileEnablement(TileEnablement other)
        {
            _enablementCounts = new Dictionary<MultiVector, int>(other._enablementCounts);
        }
        public void SetEnablementCount(MultiVector fromDirection, int count)
        {
            _enablementCounts[fromDirection] = count;
        }
        public void RemoveEnablementFromDirection(MultiVector fromDirection, int count, out bool isStillPossible)
        {
            _enablementCounts[fromDirection] -= count;

            // Tile is still possible if it is enabled by at least 1 tile from all directions.
            // Only need to check the updated direction, though.
            isStillPossible = _enablementCounts[fromDirection] > 0;
        }
    }
}