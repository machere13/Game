using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [CreateAssetMenu(menuName = "IdlePancake/Prototypes/WorldMap", fileName = "WorldMap")]
    public sealed class WorldMapConfig : ScriptableObject
    {
        [Tooltip("Локации в порядке линейной цепочки")]
        public LocationConfig[] locations;
    }
}
