using UnityEngine;

public class VhsCameraJitter : MonoBehaviour
{
    public float jitterStrength = 0.005f;
    public float jitterSpeed = 15f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void LateUpdate()
    {
        float offsetY = Mathf.Sin(Time.time * jitterSpeed) * jitterStrength;
        transform.localPosition = startPos + new Vector3(0, offsetY, 0);
    }
}
