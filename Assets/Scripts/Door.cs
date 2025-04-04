using UnityEngine;

public class Door : GameBehaviour
{
    [SerializeField] GameObject doorMesh;
    public GameObject dungeonGeneratorObj;
    public bool hasTriggered;

    private void Start()
    {
        dungeonGeneratorObj = GameObject.FindGameObjectWithTag("DungeonGenerator");
    }
    private void OnTriggerEnter(Collider other)
    {
        
        if(hasTriggered == false)
        {
            _DG.noOfRooms = 1;
            _DG.Generate();
            hasTriggered = true;
        }
        
        doorMesh.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        doorMesh.SetActive(true);
    }
}
