using UnityEngine;

public class Rotate : MonoBehaviour
{

    void FixedUpdate()
    {
        transform.Rotate(Vector3.up, 90 * Time.deltaTime);
    }
}
