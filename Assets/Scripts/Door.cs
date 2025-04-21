using UnityEngine;

public class Door : GameBehaviour
{
    [SerializeField] GameObject doorMesh;
    public bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
    
        /*if(hasTriggered == false)
        {
            //_DG.noOfRooms = 1;
            //_DG.Generate();
            hasTriggered = true;
        }
        */
        if (other.CompareTag("Player"))
        {
            doorMesh.SetActive(false);
            _DG.TryFillingEntrypointsUntilStuck();

        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorMesh.SetActive(true);
        }
    }
}
