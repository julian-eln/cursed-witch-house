using UnityEngine;

public class Controller : MonoBehaviour
{
    public float speed = 6f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 2f;
    public float groundCheckDistance = 1.1f;

    public float bobSpeed = 9f;
    public float bobAmount = 0.025f;
    public float retroFramerate = 60f;

    public float swaySpeed = 0.5f;

    public Rigidbody rigidBody;
    public Transform cameraTransform;

    public bool isGrounded;

    private float xRotation = 0f;
    private float defaultCameraY = 0f;
    private float timer = 0f;
    private float swayTimer = 0f;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.constraints = RigidbodyConstraints.FreezeRotation;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform != null)
            defaultCameraY = cameraTransform.localPosition.y;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 90;
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance
        );

        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        // Movement
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            moveDirection += transform.forward;

        if (Input.GetKey(KeyCode.S))
            moveDirection -= transform.forward;

        if (Input.GetKey(KeyCode.A))
            moveDirection -= transform.right;

        if (Input.GetKey(KeyCode.D))
            moveDirection += transform.right;

        moveDirection = moveDirection.normalized;

        rigidBody.MovePosition(
            rigidBody.position +
            moveDirection * speed * Time.deltaTime
        );

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rigidBody.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );
        }

        // Camera bob and subtle idle sway
        swayTimer += Time.deltaTime;

        if (moveDirection.magnitude > 0.1f)
        {
            timer += Time.deltaTime * bobSpeed;

            float steppedTimer =
                Mathf.Floor(timer * retroFramerate) / retroFramerate;

            float bobX =
                Mathf.Sin(steppedTimer * 0.5f) *
                bobAmount *
                1.5f;

            float bobY =
                defaultCameraY +
                Mathf.Sin(steppedTimer) *
                bobAmount;

            float tiltZ =
                Mathf.Sin(steppedTimer * 0.5f) *
                0.2f;

            cameraTransform.localPosition =
                new Vector3(
                    bobX,
                    bobY,
                    cameraTransform.localPosition.z
                );

            cameraTransform.localRotation =
                Quaternion.Euler(
                    xRotation,
                    0f,
                    tiltZ
                );
        }
        else
        {
            timer = 0f;

            float idleDriftX =
                Mathf.Sin(swayTimer * swaySpeed) * 0.002f;

            float idleDriftY =
                Mathf.Sin(swayTimer * swaySpeed * 0.6f) * 0.001f;

            float idleTiltZ =
                Mathf.Sin(swayTimer * swaySpeed * 0.4f) * 0.1f;

            cameraTransform.localPosition =
                new Vector3(
                    idleDriftX,
                    Mathf.Lerp(
                        cameraTransform.localPosition.y,
                        defaultCameraY + idleDriftY,
                        Time.deltaTime * bobSpeed
                    ),
                    cameraTransform.localPosition.z
                );

            cameraTransform.localRotation =
                Quaternion.Euler(
                    xRotation,
                    0f,
                    idleTiltZ
                );
        }
    }
}
