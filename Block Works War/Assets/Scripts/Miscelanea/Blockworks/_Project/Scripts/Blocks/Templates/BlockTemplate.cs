using UnityEngine;

namespace Blocks.Templates
{
    [CreateAssetMenu(fileName = "Template", menuName = "Blocks/Template", order = 1)]
    public class BlockTemplate : ScriptableObject
    {
        public string Name;
        public Vector3Int Size;
        public Vector2Int Offset;
        public BlockStrategy Strategy;
        public GameObject prefab;
        public Mesh meshPrefab;

        public enum BlockStrategy
        {
            Basic,
            Slope
        }
    }
}