using UnityEngine;

public class IDInspector : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"{gameObject.name} (Instance ID: {gameObject.GetInstanceID()})", gameObject);
    }
}