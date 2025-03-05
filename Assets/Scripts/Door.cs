using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] GameObject doorMesh;

    
    private void OnCollisionEnter(Collision collision)
    {
        doorMesh.SetActive(false);
        
    }

    private void OnCollisionExit(Collision collision)
    {
        doorMesh.SetActive(true);
    }
}
