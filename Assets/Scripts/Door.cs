using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] GameObject doorMesh;
    public GameObject dungeonGeneratorObj;
    public DungeonGenerator dg;
    public bool hasTriggered;

    private void Start()
    {
        dungeonGeneratorObj = GameObject.FindGameObjectWithTag("DungeonGenerator");
        dg = dungeonGeneratorObj.GetComponent<DungeonGenerator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(hasTriggered == false)
        {
            dg.GenerateSingleRoom();
            hasTriggered = true;
        }
        
        doorMesh.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        doorMesh.SetActive(true);
    }

}
