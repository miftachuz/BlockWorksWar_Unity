using System;
using System.Linq;
using ElasticSea.Framework.Util;
using UnityEngine;

namespace Blocks.Sockets
{
    public class Socket : MonoBehaviour
    {
        [SerializeField] private Block block;
        [SerializeField] private float radius = 0.0125f;
        [SerializeField] private bool active = true;

        [SerializeField] private Socket connectedSocket;

        private SphereCollider socketCollider;


        public float Radius
        {
            get => radius;
            set
            {
                radius = value;
                if (socketCollider) socketCollider.radius = value;
            }
        }

        public bool Active
        {
            get => active;
            set
            {
                active = value;
                if (socketCollider) socketCollider.enabled = active && connectedSocket == false;
            }
        }

        public Socket ConnectedSocket => connectedSocket;

        public Block Block
        {
            get => block;
            set => block = value;
        }

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("SnapSocket");
            socketCollider = gameObject.AddComponent<SphereCollider>();
            socketCollider.center = Vector3.zero;
            socketCollider.isTrigger = false;
            Active = active;
            Radius = radius;
        }

        public Socket[] Trigger()
        {
            var position = transform.position;
            var radius = Radius * transform.lossyScale.x;
            var layerMask = LayerMask.GetMask("SnapSocket");
            var candidates = Physics.OverlapSphere(position, radius, layerMask);

            return candidates
                .Select(c => c.GetComponent<Socket>())
                .Where(s => s.Block != Block)
                .ToArray();
        }

        public void Connect(Socket socket)
        {
            if (connectedSocket == null)
            {
                AttachSocket(socket);
                socket.AttachSocket(this);
            }
        }

        private void AttachSocket(Socket other)
        {
            connectedSocket = other;
            socketCollider.enabled = false;
        }

        public void Disconnect()
        {
            if (connectedSocket != null)
            {
                connectedSocket.DetachSocket();
                DetachSocket();
            }
        }

        private void DetachSocket()
        {
            connectedSocket = null;
            socketCollider.enabled = active;
        }

        [SerializeField] private bool drawGizmos = true;

        private void OnDrawGizmos()
        {
            if (drawGizmos == false)
            {
                return;
            }

            if (active == false)
            {
                return;
            }

            if (connectedSocket != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = Color.gray.SetAlpha(.5f);
                Gizmos.DrawSphere(Vector3.zero, Radius / 2);
                return;
            }

            var color = Color.blue;
            if (block.Chunk.IsAnchored)
            {
                color = Color.magenta;
            }

            var candidates = Trigger();
            if (candidates.Any())
            {
                Gizmos.color = Color.yellow;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawSphere(Vector3.zero, Radius * .5f);

                foreach (var candidate in candidates)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.matrix = Matrix4x4.identity;
                    GizmoUtils.DrawLine(transform.position, candidate.transform.position, 5);
                }

                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = color.SetAlpha(.5f);
                Gizmos.DrawWireSphere(Vector3.zero, Radius);
            }
            else
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = color.SetAlpha(.5f);
                Gizmos.DrawSphere(Vector3.zero, Radius);
            }
        }
    }
}