using UnityEngine;

namespace Blocks
{
    [CreateAssetMenu(fileName = "Material", menuName = "Blocks/Material", order = 1)]
    public class BlockMaterial : ScriptableObject
    {
        [SerializeField] private float density = 1000;

        public float Density => density;
    }
}