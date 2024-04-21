using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private ChunkManager chunkManager;
    [SerializeField] private float movementSpeed = 5.0f;
    [SerializeField] private float mouseSensitivity = 100.0f;
    private float verticalRotation = 0;
    [SerializeField] private float jumpPower = 5.0f;
    [SerializeField] private float flyingSpeed = 10.0f;
    [SerializeField] private float dashSpeedRate = 2.0f;
    private float doubleTapDelay = 0.3f;
    private bool isFlying = false;
    private bool isDashing = false;
    private IInputHandler inputHandler;
    void Start()
    {
        this.rb = GetComponent<Rigidbody>();
        this.chunkManager = GameObject.Find("ChunkManager").GetComponent<ChunkManager>();
        this.inputHandler = new KeyboardInputHandler(doubleTapDelay);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {   
        DashControl();
        PerspectiveControl();
        ToggleFlyingMode();
        ActionControl();

        if (isFlying)
        {
            FlyingControl();
        } else {
            MovementControl();
            JumpControl();
        }
    }

    void DashControl()
    {
        isDashing = this.inputHandler.GetDash();
    }

    float FixedMovementSpeed() {return movementSpeed * (isDashing ? dashSpeedRate : 1.0f);}
    float FixedFlyingSpeed() {return flyingSpeed * (isDashing ? dashSpeedRate : 1.0f);}
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
        float forwardSpeed = this.inputHandler.GetVertical() * FixedMovementSpeed();
        float sideSpeed = this.inputHandler.GetHorizontal() * FixedMovementSpeed();

        Vector3 speed = new Vector3(sideSpeed, 0, forwardSpeed);
        speed = transform.rotation * speed;
        transform.position += speed * Time.deltaTime;
    }

    void JumpControl()
    {
        if (this.inputHandler.GetJump())
        {
            // 地面に接している場合のみジャンプできる
            if (Physics.Raycast(transform.position, Vector3.down, 1.1f))
            {
                this.rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            }
        }
    }

    void FlyingControl()
    {
        float forwardSpeed = this.inputHandler.GetVertical() * FixedFlyingSpeed();
        float sideSpeed = this.inputHandler.GetHorizontal() * FixedFlyingSpeed();
        float upSpeed = this.inputHandler.GetFlyUp() ? FixedFlyingSpeed() : 0;
        float downSpeed = this.inputHandler.GetFlyDown() ? FixedFlyingSpeed() : 0;

        Vector3 speed = new Vector3(sideSpeed, upSpeed - downSpeed , forwardSpeed);
        speed = transform.rotation * speed;
        transform.position += speed * Time.deltaTime;
    }

    private void ActionControl()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var (hitBlock, hitNormal) = GetHitBlock();
            if (hitBlock == null) return;
            var newBlockPosition = hitBlock.transform.position + hitNormal;
            chunkManager.CreateNewBlock(newBlockPosition);
        }
    }

    private (GameObject, Vector3) GetHitBlock()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 30.0f))
        {
            GameObject hitBlock = hit.collider.gameObject;
            Vector3 hitNormal = hit.normal;
            return (hitBlock, hitNormal);
        }
        return (null, Vector3.zero);
    }

    private void ToggleFlyingMode()
    {
        if (this.inputHandler.GetFlyToggle())
        {
            isFlying = !isFlying;
            if (isFlying)
            {
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
            }
            else
            {
                rb.useGravity = true;
            }
        }
    }
}

public interface IInputHandler
{
    float GetHorizontal();
    float GetVertical();
    bool GetJump();
    bool GetDash();
    bool GetFlyToggle();
    bool GetFlyUp();
    bool GetFlyDown();
}

// TODO: これを MonoBehaviour していい感じにできる？ Dash周りの処理が微妙なので
public class KeyboardInputHandler : IInputHandler
{
    private float lastSpaceInputTime = -1f;
    private float doubleTapDelay;
    private bool isLeftCtrlDown = false;

    public KeyboardInputHandler(float doubleTapDelay)
    {
        this.doubleTapDelay = doubleTapDelay;
    }
    public float GetHorizontal()
    {
        return Input.GetAxis("Horizontal");
    }

    public float GetVertical()
    {
        return Input.GetAxis("Vertical");
    }

    public bool GetJump()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    public bool GetDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isLeftCtrlDown = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isLeftCtrlDown = false;
        }
        return isLeftCtrlDown;
    }

    public bool GetFlyToggle()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.time - lastSpaceInputTime < doubleTapDelay)
            {
                return true;
            }
            lastSpaceInputTime = Time.time;
        }
        return false;
    }

    public bool GetFlyUp()
    {
        return Input.GetKey(KeyCode.Space);
    }

    public bool GetFlyDown()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }
}