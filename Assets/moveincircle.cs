using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class moveincircle : MonoBehaviour
{
    public float RotateSpeed = 5f;
    public float Radius = 0.1f;

    private Vector2 _centre;
    private float _angle;
    private Rigidbody2D rb;

    void Start()
    {
        _centre = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        _angle += RotateSpeed * Time.fixedDeltaTime;
        var offset = new Vector2(Mathf.Sin(_angle), Mathf.Cos(_angle)) * Radius;
        rb.MovePosition(_centre + offset);
    }
}