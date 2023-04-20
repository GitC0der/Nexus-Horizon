using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float moveSpeed = 10.0f;
    private float rotationSpeed = 360.0f;
    private float rotationSpeedLR = 60f;
    private float rotationSpeedUD = 60f;
    
    void Update()
    {
        // Camera horizontal and vertical movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float updown = Input.GetAxis("UpDown");

        transform.Translate(new Vector3(horizontal, updown, vertical) * moveSpeed * Time.deltaTime);

		// Rotational movement
        if (Input.GetKey(KeyCode.Q)) 
        {
            transform.Rotate(Vector3.up, -rotationSpeedLR * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.up, rotationSpeedLR * Time.deltaTime);
        }
        
        if (Input.GetKey(KeyCode.X))
        {
            transform.Rotate(Vector3.right, -rotationSpeedUD * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.Y))
        {
            transform.Rotate(Vector3.right, rotationSpeedUD * Time.deltaTime);
        }

    }
}