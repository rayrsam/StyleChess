using UnityEngine;

public class CameraController : MonoBehaviour
{
    
    [SerializeField] private float rotationSpeed;
    [SerializeField] private GameObject target;
    
    private void Update()
    {
        if (Input.GetKey(KeyCode.Q))
            transform.RotateAround(target.transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
        
        if (Input.GetKey(KeyCode.E))
            transform.RotateAround(target.transform.position, Vector3.up, -rotationSpeed * Time.deltaTime);
    }
}
