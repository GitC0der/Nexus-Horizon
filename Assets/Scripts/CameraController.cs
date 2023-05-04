using UnityEngine;

using UnityEngine;


using UnityEngine;

using UnityEngine;

using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float rotationSpeed = 100f;
    private float movementSpeed = 50f;

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


/*
public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float movementSpeed = 1000f;

    private Transform cameraTransform;

    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // Rotation
        float horizontalRotation = Input.GetAxis("HorizontalArrows") * Time.deltaTime * rotationSpeed;
        float verticalRotation = Input.GetAxis("VerticalArrows") * Time.deltaTime * rotationSpeed;

        cameraTransform.Rotate(0, horizontalRotation, 0, Space.World);
        cameraTransform.Rotate(-verticalRotation, 0, 0, Space.Self);

        // Movement
        float moveForward = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
        float moveRight = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        float moveUp = Input.GetKey(KeyCode.Space) ? 1 : Input.GetKey(KeyCode.LeftShift) ? -1 : 0;

        Vector3 moveDirection = new Vector3(moveRight, moveUp, moveForward).normalized;
        cameraTransform.position += cameraTransform.TransformDirection(moveDirection) * movementSpeed * Time.deltaTime;
    
        //cameraTransform.position += cameraTransform.TransformDirection(moveDirection) * (movementSpeed * Time.deltaTime);
    }
}
*/


/*
public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 100f;
    private Vector3 rotationAxis = new Vector3(0,1,0);
    private Transform cameraTransform;
    private float moveSpeed = 10.0f;
    private float rotationSpeedLR = 60f;
    private float rotationSpeedUD = 60f;

    void Start()
    {
        cameraTransform = gameObject.transform;
        rotationAxis = cameraTransform.position;
    }

    void Update()
    {
        
        // Camera horizontal and vertical movement
        float horizontal = Input.GetAxis("HorizontalWASD");
        float vertical = Input.GetAxis("VerticalWASD");
        float updown = Input.GetAxis("UpDown");

        cameraTransform.Translate(new Vector3(horizontal, updown, vertical) * (moveSpeed * Time.deltaTime));
        
        float horizontalRotation = Input.GetAxis("HorizontalArrows") * Time.deltaTime * rotationSpeed;
        float verticalRotation = Input.GetAxis("VerticalArrows") * Time.deltaTime * rotationSpeed;

        cameraTransform.RotateAround(rotationAxis, Vector3.up, horizontalRotation);
        cameraTransform.RotateAround(rotationAxis, cameraTransform.right, -verticalRotation);
    }
}
*/

/*
public class CameraController : MonoBehaviour
{
    private float moveSpeed = 10.0f;
    private float rotationSpeedLR = 60f;
    private float rotationSpeedUD = 60f;
    
    void Update()
    {
        // Camera horizontal and vertical movement
        float horizontal = Input.GetAxis("HorizontalWASD");
        float vertical = Input.GetAxis("VerticalWASD");
        float updown = Input.GetAxis("UpDown");

        transform.Translate(new Vector3(horizontal, updown, vertical) * (moveSpeed * Time.deltaTime));

        // Rotational movement
        if (Input.GetKey(KeyCode.LeftArrow)) {
            transform.Rotate(Vector3.up, -rotationSpeedLR * Time.deltaTime);
        } else if (Input.GetKey(KeyCode.RightArrow)) {
            transform.Rotate(Vector3.up, rotationSpeedLR * Time.deltaTime);
        }
        
        if (Input.GetKey(KeyCode.UpArrow)) {
            transform.Rotate(Vector3.right, -rotationSpeedUD * Time.deltaTime);
        } else if (Input.GetKey(KeyCode.DownArrow)) {
            transform.Rotate(Vector3.right, rotationSpeedUD * Time.deltaTime);
        }
        
    }
}
*/