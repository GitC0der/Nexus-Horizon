using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float rotationSpeed = 40f;
    private float movementSpeed = 15;

    private Transform cameraTransform;

    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // Movement
        float moveForward = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
        float moveRight = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        float moveUp = Input.GetKey(KeyCode.Space) ? 1 : Input.GetKey(KeyCode.LeftShift) ? -1 : 0;

        Vector3 moveDirection = new Vector3(moveRight, moveUp, moveForward).normalized;
        cameraTransform.position += movementSpeed * Time.deltaTime * cameraTransform.TransformDirection(moveDirection);

        // Rotation
        float horizontalRotation = Input.GetAxis("HorizontalArrows") * Time.deltaTime * rotationSpeed;
        float verticalRotation = Input.GetAxis("VerticalArrows") * Time.deltaTime * rotationSpeed;

        cameraTransform.Rotate(0, horizontalRotation, 0, Space.World);
        cameraTransform.Rotate(-verticalRotation, 0, 0, Space.Self);
    }
}
