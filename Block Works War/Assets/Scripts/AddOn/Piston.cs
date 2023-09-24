using System.Linq;
using Blocks;
using Blocks.Sockets;
using UnityEngine;

public class Piston : InteractableBlock
{
    /*
    *  NOTE:
    *   We can add a threshold of how many blocks the piston
    *   can push
    */

    [SerializeField] private Transform _topPiece;
    [SerializeField] private Socket _topSocket;
    [SerializeField] private bool _extended;

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
            _topPiece.localPosition = Vector3.up * 0.04f;
        else
            _topPiece.localPosition = Vector3.up * 0.02f;

        UpdateConnectedBlocks();
        Debug.Log("Interacting with " + name);
    }

    private void UpdateConnectedBlocks()
    {
        Vector3 offset = _extended ? Vector3.up * 0.02f : -Vector3.up * 0.02f;
        UpdateConnectedBlockRecurse(_topSocket, offset);
    }

    private void UpdateConnectedBlockRecurse(Socket socket, Vector3 offset)
    {
        if (socket.ConnectedSocket == null)
            return;

        Block connected = socket.ConnectedSocket.Block;
        connected.transform.localPosition += offset;
        // foreach (Socket s in connected.Sockets.Where(c => c.name.ToLower().Contains("male")))
        //     UpdateConnectedBlockRecurse(s, offset);
    }
}
