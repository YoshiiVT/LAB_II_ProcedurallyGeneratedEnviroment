using UnityEngine;

public class OLDDoor : MonoBehaviour
{
    [SerializeField] GameObject doorMesh;
    public GameObject dungeonGeneratorObj;
    public RoomGenerator dg;
    public bool hasTriggered;

    private void Start()
    {
        dungeonGeneratorObj = GameObject.FindGameObjectWithTag("DungeonGenerator");
        dg = dungeonGeneratorObj.GetComponent<RoomGenerator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        /*
        if(hasTriggered == false)
        {
            dg.PlaceNextRoom();
            hasTriggered = true;
        }
        */
        doorMesh.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        doorMesh.SetActive(true);
    }

}
