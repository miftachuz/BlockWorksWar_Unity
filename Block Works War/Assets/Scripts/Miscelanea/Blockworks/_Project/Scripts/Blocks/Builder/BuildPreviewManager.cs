using UnityEngine;

namespace Blocks.Builder
{
    public class BuildPreviewManager : MonoBehaviour
    {
        private bool isPreviewEnabled;

        private BuildPreview buildPreview;

        public void StartPreview()
        {
            if (isPreviewEnabled == false)
            {
                buildPreview = BuildPreviewFactory.Build(GetComponent<Chunk>());
                buildPreview.BeginSnap();
                isPreviewEnabled = true;
            }
        }

        public void StopPreview()
        {
            if (isPreviewEnabled)
            {
                buildPreview.EndSnap();
                DestroyImmediate(buildPreview.gameObject);
                isPreviewEnabled = false;
            }
        }
    }
}