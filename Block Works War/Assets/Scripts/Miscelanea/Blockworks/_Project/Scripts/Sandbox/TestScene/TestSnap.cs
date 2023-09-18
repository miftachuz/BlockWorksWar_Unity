using System;
using Blocks;
using Blocks.Builder;
using UnityEngine;

namespace Sandbox.TestScene
{
    public class TestSnap : MonoBehaviour
    {
        private void Start()
        {
            foreach (var tag in FindObjectsOfType<TestTag>())
            {
                switch (tag.type)
                {
                    case TestSnapType.Preview:
                        tag.GetComponent<BuildPreviewManager>().StartPreview();
                        break;
                    case TestSnapType.Connect:
                        tag.GetComponent<BuildPreviewManager>().StartPreview();
                        tag.GetComponent<BuildPreviewManager>().StopPreview();
                        break;
                    case TestSnapType.Disconnect:
                        var block = tag.GetComponentInChildren<Block>();
                        tag.GetComponent<BuildPreviewManager>().StartPreview();
                        tag.GetComponent<BuildPreviewManager>().StopPreview();
                        ChunkFactory.Disconnect(block.Chunk, new[] {block});
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
