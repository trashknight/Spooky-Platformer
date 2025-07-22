using UnityEngine;

public class Moveincircle : MonoBehaviour
{
    public float RotateSpeed = 5f;
    public float Radius = 0.1f;

    private float _angle;
    private Vector2 _initialOffset;

    void Start()
    {
        _initialOffset = transform.position;
    }

    void Update()
    {
        _angle += RotateSpeed * Time.deltaTime;

        // Always use the initial spawn offset + animation offset
        Vector2 offset = new Vector2(Mathf.Sin(_angle), Mathf.Cos(_angle)) * Radius;
        transform.position = _initialOffset + offset;
    }

    void OnEnable()
    {
        // Re-capture starting offset in case the object was disabled and re-enabled
        _initialOffset = transform.position;
        _angle = 0f;
    }
}