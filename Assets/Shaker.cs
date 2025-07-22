using UnityEngine;
using Cinemachine;

public class Shaker : MonoBehaviour
{
    public CinemachineImpulseSource impulseSource;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (impulseSource != null)
            {
                impulseSource.GenerateImpulse();
                Debug.Log("Impulse triggered with R key.");
            }
            else
            {
                Debug.LogWarning("No CinemachineImpulseSource assigned to Shaker.cs");
            }
        }
    }
}