using System;
using System.Linq;
using Blocks;
using Blocks.Builder;
using ElasticSea.Framework.Extensions;
using UnityEngine;
using UnityEngine.XR;
using Utils;

namespace Sandbox.Controller
{
    public class PlayerHand : MonoBehaviour
    {
        private bool triggerHeld;
        private Chunk chunkHeld;
        [SerializeField] private Renderer sphere;
        [SerializeField] private ChunkSpawner chunkSpawner;
        private Helper helper;

        private void Awake()
        {
            var leftHandedDevices = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            helper = gameObject.AddComponent<Helper>();
            helper.Device = leftHandedDevices;
            helper.RegisterButton(CommonUsages.triggerButton, isDown =>
            {
                switch (state)
                {
                    case State.Grab:
                        if (isDown)
                        {
                            GrabStart();
                        }
                        else
                        {
                            GrabEnd();
                        }
                        break;
                    case State.Disconnect:
                        if (isDown)
                        {
                            Disconnect();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
            helper.RegisterButton(CommonUsages.primaryButton, isDown =>
            {
                if (isDown)
                {
                    state = state.Next();
                    UpdateState();
                }
            });
        }

        private void Disconnect()
        {
            var blockCandidate = CheckForBlock();
            if (blockCandidate)
            {
                ChunkFactory.Disconnect(blockCandidate.Chunk, new[] {blockCandidate});
            }
        }

        private void Update()
        {
            if (chunkHeld)
            {
                var component = chunkHeld.GetComponent<Rigidbody>();
                component.isKinematic = true;
                component.transform.position = transform.position;
                component.transform.rotation = transform.rotation;
                // component.MovePosition(transform.position);
                // component.MoveRotation(transform.rotation);
            }
        
            var leftHandedDevices = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            var device = leftHandedDevices.GetDevice();
            if (device.GetFeatureValue(CommonUsages.secondaryButton) == true)
            {
                chunkSpawner.Spawn();
            }
            UpdateState();
        }

        public enum State
        {
            Grab, Disconnect
        }

        [SerializeField] private State state;
        private void UpdateState()
        {
            switch (state)
            {
                case State.Grab:
                    sphere.material.color = Color.green;
                    break;
                case State.Disconnect:
                    sphere.material.color = Color.red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void GrabStart()
        {
            var blockCandidate = CheckForChunk();

            if (blockCandidate)
            {
                chunkHeld = blockCandidate;
                chunkHeld.GetComponent<BuildPreviewManager>().StartPreview();
            }
            else
            {
                chunkHeld = null;
            }
        }
    
        private Chunk  CheckForChunk()
        {
            var blockCandidate = Physics.OverlapSphere(transform.position, 0.05f)
                .Where(c => c.GetComponent<Block>() )
                .Where(c => c.GetComponent<Block>().IsAnchored == false)
                .OrderBy(c => c.transform.position.Distance(transform.position))
                .FirstOrDefault();

            if (blockCandidate)
            {
                var chunk = blockCandidate.GetComponentInParent<Chunk>();
                if (chunk.GetComponent<Rigidbody>().isKinematic == false)
                {
                    return chunk;
                }
            }
      

            return null;
        }
        private Block  CheckForBlock()
        {
            var blockCandidate = Physics.OverlapSphere(transform.position, 0.05f)
                .Where(c => c.GetComponent<Block>() )
                .OrderBy(c => c.transform.position.Distance(transform.position))
                .FirstOrDefault();

            if (blockCandidate)
            {
                return blockCandidate.GetComponent<Block>();
            }

            return null;
        }

        private void GrabEnd()
        {
            if (chunkHeld)
            {
                chunkHeld.GetComponent<Rigidbody>().isKinematic = false;
                chunkHeld.GetComponent<BuildPreviewManager>().StopPreview();
            }

            chunkHeld = null;
        }
    }
}
