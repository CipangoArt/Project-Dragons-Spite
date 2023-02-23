using UnityEngine;

public class MouseLock : MonoBehaviour
{
    
    
    // Start is called before the first frame update

    private void Awake()
    {
       
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
