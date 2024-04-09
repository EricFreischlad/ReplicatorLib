using System.Collections.Generic;
using System.Linq;

namespace ReplicatorLib
{
    /// <summary>
    /// Manages enablement for a possible tile for a node in the wave function. A tile is a possible result for a node if it is allowed (enabled) by at least one neighboring tile in all directions.
    /// </summary>
    public sealed class TileEnablement
    {
        // Direction from which it is enabled, to number of tiles which enable it from that direction.
        private readonly Dictionary<MultiVector, int> _enablementCounts = new Dictionary<MultiVector, int>();

        public TileEnablement() { }
        public TileEnablement(TileEnablement other)
        {
            _enablementCounts = new Dictionary<MultiVector, int>(other._enablementCounts);
        }
        /// <summary>
        /// Set the number of tiles enabling this possibility from a given direction.
        /// </summary>
        public void SetEnablementCount(MultiVector fromDirection, int count)
        {
            _enablementCounts[fromDirection] = count;
        }
        /// <summary>
        /// Remove enablement for this possibility from a given direction. It is now less likely for the parent node to become that tile. If enablement for the possibility becomes 0 or less in any direction, the possibility no longer exists and the change should propagate.
        /// </summary>
        public void RemoveEnablementFromDirection(MultiVector fromDirection, int count, out bool isStillPossible)
        {
            _enablementCounts[fromDirection] -= count;

            // Tile is still possible if it is enabled by at least 1 tile from all directions.
            // Only need to check the updated direction, though.
            isStillPossible = _enablementCounts[fromDirection] > 0;
        }
    }
}