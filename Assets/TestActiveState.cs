using UnityEngine;

public class TestActiveState : MonoBehaviour
{
    void Awake()
    {
        // NEW: Make this test object persistent so it survives scene reloads
        DontDestroyOnLoad(gameObject);
        Debug.Log("TEST_ACTIVE_STATE: Awake called. GameObject activeInHierarchy: " + gameObject.activeInHierarchy + ", GameObject activeSelf: " + gameObject.activeSelf);
    }

    void OnEnable()
    {
        Debug.Log("TEST_ACTIVE_STATE: OnEnable called. GameObject activeInHierarchy: " + gameObject.activeInHierarchy + ", GameObject activeSelf: " + gameObject.activeSelf);
    }

    void Start()
    {
        Debug.Log("TEST_ACTIVE_STATE: Start called. GameObject activeInHierarchy: " + gameObject.activeInHierarchy + ", GameObject activeSelf: " + gameObject.activeSelf);
    }
}