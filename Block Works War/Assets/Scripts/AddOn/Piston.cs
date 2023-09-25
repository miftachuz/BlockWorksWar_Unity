using System.Collections.Generic;
using System.Linq;
using Blocks;
using Blocks.Sockets;
using UnityEngine;

public class Piston : InteractableBlock
{
    private static readonly Vector3 SOCKET_OFFSET = new Vector3(0.025f, 0.04f, 0.025f);
    private static readonly Vector3 SOCKET_OFFSET_EXT = new Vector3(0.025f, 0.06f, 0.025f);

    /*
    *  NOTE:
    *   We can add a threshold of how many blocks the piston
    *   can push
    */

    [SerializeField] private Block _block;
    [SerializeField] private Transform _topPiece;
    [SerializeField] private bool _extended;

    private Socket _topSocket;

    private void Start()
    {
        foreach (Socket socket in _block.Sockets)
        {
            if (socket.LocalOrientation == Quaternion.identity)
            {
                _topSocket = socket;
                return;
            }
        }
    }

    private void OnValidate()
    {
        if (_extended)
            _topPiece.localPosition = Vector3.up * 0.04f;
        else
            _topPiece.localPosition = Vector3.up * 0.02f;
    }

    public override void Interact()
    {
        _extended = !_extended;
        if (_extended)
        {
            _topPiece.localPosition = Vector3.up * 0.04f;
            _topSocket.LocalPosition = SOCKET_OFFSET_EXT;
        }
        else
        {
            _topPiece.localPosition = Vector3.up * 0.02f;
            _topSocket.LocalPosition = SOCKET_OFFSET;
        }

        UpdateConnectedBlocks();
    }

    private void UpdateConnectedBlocks()
    {
        Vector3 offset = _extended ? Vector3.up * 0.02f : -Vector3.up * 0.02f;
        _updateSetCache.Clear();
        UpdateConnectedBlockRecurse(_topSocket, offset);
    }

    private HashSet<int> _updateSetCache = new HashSet<int>();
    private void UpdateConnectedBlockRecurse(Socket socket, Vector3 offset)
    {
        if (!socket.IsConnected)
            return;
        
        Block block = socket.ConnectedSocket.Block;
        int hash = block.GetHashCode();
        if (_updateSetCache.Contains(hash))
            return;
        
        block.transform.localPosition += offset;
        _updateSetCache.Add(hash);

        // Updated multiple times
        foreach (Socket connected in GetConnectedSockets(block))
            UpdateConnectedBlockRecurse(connected, offset);
    }

    private IEnumerable<Socket> GetConnectedSockets(Block block)
    {
        foreach (Socket s in block.Sockets.Where(s => s.LocalOrientation == Quaternion.identity && s.IsConnected))
        {
            yield return s;
        }
    }
}
