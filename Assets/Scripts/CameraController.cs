using UnityEngine;

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

        transform.Translate(new Vector3(horizontal, updown, vertical) * moveSpeed * Time.deltaTime);

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