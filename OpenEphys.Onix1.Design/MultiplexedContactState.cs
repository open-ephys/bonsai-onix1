using System.Collections.Generic;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Tracks contact-selection state for probes that have more contacts than recording channels,
    /// where hardware constraints determine which contacts can be simultaneously active.
    /// </summary>
    /// <remarks>
    /// Maintains the active contact-to-channel map, the potential channel each contact would
    /// occupy if enabled, the set of user-pinned channels that must be preserved across preset
    /// changes, and the contacts blocked by those pins. Depends only on generic ProbeInterface
    /// types and integer indices and can be reused by any probe-specific dialog that follows this
    /// multiplexed architecture.
    /// </remarks>
    internal sealed class MultiplexedContactState
    {
        /// <summary>
        /// Contacts currently wired to a device channel.
        /// </summary>
        public HashSet<int> EnabledContactIndices { get; private set; } = new();

        /// <summary>
        /// Map of contact index to its active device channel.
        /// </summary>
        public Dictionary<int, int> ElectrodeToChannel { get; private set; } = new();

        /// <summary>
        /// Map of contact index to the hardware channel it would occupy if enabled.
        /// </summary>
        public Dictionary<int, int> ElectrodePotentialChannel { get; private set; } = new();

        /// <summary>
        /// Device channels the user has pinned.
        /// </summary>
        public HashSet<int> PinnedChannels { get; } = new();

        /// <summary>
        /// Contacts that cannot be enabled because their channel is held by a pin.
        /// </summary>
        public HashSet<int> BlockedContactIndices { get; } = new();

        /// <summary>
        /// Rebuilds <see cref="EnabledContactIndices"/> and <see cref="ElectrodeToChannel"/>
        /// from the probe group's current channel map.
        /// </summary>
        public void RebuildActiveMap(IReadOnlyDictionary<int, int> channelMap)
        {
            EnabledContactIndices = channelMap == null
                ? new HashSet<int>()
                : new HashSet<int>(channelMap.Values);
            ElectrodeToChannel = new Dictionary<int, int>();
            if (channelMap != null)
                foreach (var kvp in channelMap)
                    ElectrodeToChannel[kvp.Value] = kvp.Key;
        }

        /// <summary>
        /// Rebuilds <see cref="ElectrodePotentialChannel"/> using the probe group's channel
        /// resolver.
        /// </summary>
        public void RebuildPotentialMap(int contactCount, IMultiplexedProbeGroup probeGroup)
        {
            ElectrodePotentialChannel = new Dictionary<int, int>(contactCount);
            for (int i = 0; i < contactCount; i++)
                ElectrodePotentialChannel[i] = probeGroup.GetChannel(i);
        }

        /// <summary>
        /// Recomputes <see cref="BlockedContactIndices"/>: a contact is blocked when its
        /// potential channel is pinned by a different, already-active contact.
        /// </summary>
        public void RecomputeBlocked(int contactCount)
        {
            BlockedContactIndices.Clear();
            if (PinnedChannels.Count == 0) return;
            for (int i = 0; i < contactCount; i++)
            {
                if (ElectrodeToChannel.TryGetValue(i, out int activeCh) && PinnedChannels.Contains(activeCh))
                    continue;
                if (ElectrodePotentialChannel.TryGetValue(i, out int potCh) && PinnedChannels.Contains(potCh))
                    BlockedContactIndices.Add(i);
            }
        }

        /// <summary>
        /// Clears all pins and the blocked set they induce.
        /// </summary>
        public void ClearPins()
        {
            PinnedChannels.Clear();
            BlockedContactIndices.Clear();
        }
    }
}
