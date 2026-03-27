using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [CreateAssetMenu(menuName = "IdlePancake/Prototypes/Level Table", fileName = "LevelTable")]
    public sealed class LevelTableConfig : ScriptableObject
    {
        [Tooltip("XP нужен для перехода на уровень [index+1]. Элемент 0 = XP для уровня 2.")]
        public int[] xpPerLevel = { 50, 120, 250, 500, 900, 1500, 2500, 4000 };

        public int MaxLevel => xpPerLevel.Length + 1;

        public int XpForNextLevel(int currentLevel)
        {
            int idx = currentLevel - 1;
            if (idx < 0 || idx >= xpPerLevel.Length) return int.MaxValue;
            return xpPerLevel[idx];
        }
    }
}
