using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float movementSpeed = 5.0f;
    [SerializeField] private float mouseSensitivity = 100.0f;
    private float verticalRotation = 0;
    [SerializeField] private float jumpPower = 5.0f;

    void Start()
    {
        this.rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        PerspectiveControl();
        MovementControl();
        JumpControl();
    }

    void PerspectiveControl()
    {
        float rotLeftRight = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(0, rotLeftRight, 0);

        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        verticalRotation = Mathf.Clamp(verticalRotation, -90, 90);  // 首の回転をリアルな範囲に制限
        Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

    }

    void MovementControl()
    {
        float forwardSpeed = Input.GetAxis("Vertical") * movementSpeed;
        float sideSpeed = Input.GetAxis("Horizontal") * movementSpeed;

        Vector3 speed = new Vector3(sideSpeed, 0, forwardSpeed);
        speed = transform.rotation * speed;
        transform.position += speed * Time.deltaTime;
    }

    void JumpControl()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }
}
