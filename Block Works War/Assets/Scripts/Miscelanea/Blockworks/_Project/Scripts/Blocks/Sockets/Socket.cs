using Blocks.Builder;
using UnityEngine;

namespace Blocks.Sockets
{
    [System.Serializable]
    public class Socket
    {
        private const string SnapSocketLayer = "SnapSocket";
        private static float DefaultRadius = 0.0125f;
        private static Collider[] SocketBuffer = new Collider[4];

        [SerializeField] private Vector3 position;
        [SerializeField] private Vector3 orientation;
        [SerializeField] private bool isActive = true;

        // [SerializeField]
        private Block block;
        private Socket connectedSocket;
        private SphereCollider collider;

        /// <summary>
        /// Local position relative to the owner block
        /// </summary>
        public Vector3 LocalPosition 
        { 
            get => position; 
            set => position = value;
        }

        /// <summary>
        /// World space position
        /// </summary>
        public Vector3 Position { get => block.transform.TransformPoint(position); }

        /// <summary>
        /// Local orientation relative to the owner block
        /// </summary>
        public Quaternion LocalOrientation 
        { 
            get => Quaternion.Euler(orientation); 
            set => orientation = value.eulerAngles;
        }

        /// <summary>
        /// World space orientation
        /// </summary>
        public Quaternion Orientation { get => block.transform.rotation * LocalOrientation; }

        public float Radius { get => DefaultRadius; }
        public Block Block { get => block; }
        public Socket ConnectedSocket { get => connectedSocket; }
        public SphereCollider Collider { get => collider; }

        public bool IsInitialized { get => block != null; }

        public bool IsConnected { get => connectedSocket != null; }

        public bool IsActive 
        {
            get => isActive;
            set
            {
                isActive = value;

                if (collider != null)
                    collider.enabled = isActive && !IsConnected;
            }
        }

        public void Init(Block block)
        {
            this.block = block;

            if (Application.isPlaying)
                CreateCollider();
        }
        
        public void Connect(Socket other)
        {
            if (IsConnected || other.IsConnected)
                return;
            
            connectedSocket = other;
            other.Connect(this);

            collider.enabled = false;
        }

        public void Disconnect()
        {
            if (!IsConnected)
                return;
            
            connectedSocket.Disconnect();
            connectedSocket = null;

            collider.enabled = true;
        }

        private void CreateCollider()
        {
            GameObject g = new GameObject("SocketCollider");
            g.transform.SetParent(block.transform);
            g.transform.localPosition = LocalPosition;
            g.layer = LayerMask.NameToLayer(SnapSocketLayer);

            collider = g.AddComponent<SphereCollider>();
            collider.radius = DefaultRadius;
            collider.center = Vector3.zero;
            collider.isTrigger = true;

            g.AddComponent<SocketIdentifier>().Socket = this;

            IsActive = IsActive;
        }

        /// <summary>
        /// Get the closest socket from other block
        /// </summary>
        /// <returns></returns>
        public Socket GetSocketCandidate()
        {
            LayerMask layer = LayerMask.GetMask(SnapSocketLayer);
            int hits = Physics.OverlapSphereNonAlloc(Position, Radius, SocketBuffer, layer, QueryTriggerInteraction.Collide);

            if (hits > 0)
            {
                for (int i = 0; i < hits; i++)
                {
                    if (SocketBuffer[i].TryGetComponent(out SocketIdentifier id))
                    {
                        if (id.Socket.Block != block && id.Socket.LocalOrientation != LocalOrientation)
                            return id.Socket;
                    }
                }
            }

            return null;
        }

        private class SocketIdentifier : MonoBehaviour
        {
            [HideInInspector]
            public Socket Socket;
        }
    }
}