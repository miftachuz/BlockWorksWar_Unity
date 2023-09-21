using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 2.0f;

    private Vector2 _euler;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    private void HandleRotation()
    {
        _euler.x -= Input.GetAxis("Mouse Y");
        _euler.y += Input.GetAxis("Mouse X");

        _euler.x = Mathf.Clamp(_euler.x, -80, 80);

        transform.rotation = Quaternion.Euler(_euler);
    }

    private void HandleMovement()
    {
        Quaternion r = Quaternion.Euler(0, _euler.y, 0);
        Vector3 forward = r * Vector3.forward;
        Vector3 right = r * Vector3.right;
        
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (forward * z) + (right * x);

        if (Input.GetKey(KeyCode.Space))
            moveDir.y = 1;
        else if (Input.GetKey(KeyCode.LeftControl))
            moveDir.y = -1;

        transform.position += moveDir.normalized * _moveSpeed * Time.deltaTime;
    }
}
