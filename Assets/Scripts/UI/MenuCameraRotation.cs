using UnityEngine;

public class MenuCameraRotation : MonoBehaviour
{
    public float rotationSpeed = 2f;

    void Start()
    {
        transform.rotation = Quaternion.Euler(10f, 0f, 0f);
    }

    void Update()
    {
        float speed = rotationSpeed + Mathf.Sin(Time.time * 0.5f) * 1f;
        transform.Rotate(0f, speed * Time.deltaTime, 0f);
    }
}