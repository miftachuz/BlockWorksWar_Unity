using Blocks;
using Blocks.Builder;
using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    private GameObject _currentFocused;
    private float _grabDistance;
    private Chunk _current;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && _currentFocused != null)
        {
            InteractableBlock b = _currentFocused.GetComponentInParent<InteractableBlock>();
            if (b != null)
                b.Interact();
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleGrabStart();
        }
        else if (Input.GetMouseButton(0))
        {
            if (_current == null)
                return;

            _current.transform.position = transform.position + (transform.forward * _grabDistance);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            HandleGrabEnd();
        }
    }

    private void FixedUpdate()
    {
        CheckForFocusedObject();
    }

    private void CheckForFocusedObject()
    {
        Ray ray = _camera.ScreenPointToRay(new Vector2((Screen.width * 0.5f) - 1, (Screen.height * 0.5f) - 1));
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            _currentFocused = hit.collider.gameObject;
        }
    }

    private void HandleGrabStart()
    {
        Chunk c = CheckForChunk();
        if (c != null)
        {
            _current = c;
            _current.GetComponent<BuildPreviewManager>().StartPreview();
            _current.GetComponent<Rigidbody>().isKinematic = true;

            _grabDistance = Vector3.Distance(_current.transform.position, transform.position);
            return;
        }

        _current = null;
    }

    private void HandleGrabEnd()
    {
        if (_current != null)
        {
            _current.GetComponent<Rigidbody>().isKinematic = false;
            _current.GetComponent<BuildPreviewManager>().StopPreview();
        }

        _current = null;
    }

    private void Disconnect()
    {
        Block b = CheckForBlock();
        if (b != null)
        {
            ChunkFactory.Disconnect(b.Chunk, new[] {b});
        }
    }

    private Chunk CheckForChunk()
    {
        Block b = CheckForBlock();
        if (b != null && b.IsAnchored == false)
        {
            Chunk c = b.GetComponentInParent<Chunk>();
            if (c.GetComponent<Rigidbody>().isKinematic == false)
            {
                return c;
            }
        }

        return null;
    }

    private Block CheckForBlock()
    {
        if (_currentFocused != null)
        {
            return _currentFocused.GetComponent<Block>();
        }

        return null;
    }
}
