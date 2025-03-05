using System.Collections.Generic;
using UnityEngine;
//Added "Systems.Collections.Generic;" for the List related command. This is not a Multiplayer game so Unity.Netcode; was not needed.
public class DungeonGenerator : MonoBehaviour
{

    /// <summary>
    /// Singleton : A component that has only one instance in a given world.
    /// </summary>
    public static DungeonGenerator Instance { get; private set; }

    /// <summary>
    /// This is the first room the player will start in
    /// </summary>
    [SerializeField] //SerializeField allows a value to be visible in the inspector but not editiable.
    private GameObject enterance;

    /// <summary>
    /// List of all the rooms that will be in the dungeon
    /// </summary>
    [SerializeField]
    private List<GameObject> rooms;

    /// <summary>
    /// This is a list for special rooms, i.e loot. However for the test, disable it.
    /// </summary>
    [SerializeField]
    private List<GameObject> specialRooms;

    /// <summary>
    /// Again delete for test, this is for alternate enterances for exits for a level. 
    /// (Like Fire Exits in Lethal)
    /// </summary>
    [SerializeField]
    private List<GameObject> alternateEnterances;

    /// <summary>
    /// Another list for hallways, (Just long rooms). Delete in test
    /// The reason its seperated from rooms is so we can controll how many hallways are inbetween rooms
    /// </summary>
    [SerializeField]
    private List<GameObject> hallways;

    [SerializeField] // Prefab for the doors that go inbetween doorways
    private GameObject door;

    /// <summary>
    /// This int will tell the algorithim how many rooms to genertate
    /// We will alter this to generate that many rooms continuously
    /// </summary>
    [SerializeField]
    private int noOfRooms = 10;

    /// <summary>
    /// Unity APIs use layerMasks to define which layers the API can interact with.
    /// [API stands for Application Programming Interface < The engines tools and abilities]
    /// </summary>
    [SerializeField]
    private LayerMask roomsLayermask;

    private List<DungeonPart> generatedRooms; //This list will contain all the rooms generated in the dungeon.

    private bool isGenerated = false; //This will tell the engine if something has already been made or not

    private void Awake() //Awake is like start, except it runs the scripts variables even if the script isnt active.
    {
        Instance = this;
    }

    private void Start()
    {
        generatedRooms = new List<DungeonPart>(); //Creates list
        //GenerateSingleRoom();
        Generate();
        //StartGeneration();
       
    }
    /// <summary>
    /// This is the function that generates the dungeon
    /// </summary>
    private void Generate()
    {
        //We take the count of alternate enterances from the noOFRooms to keep the same amount of rooms with the alternate enterances
        for (int i = 0; i < noOfRooms - alternateEnterances.Count; i++)
        { 
            //Bellow will run only at the start of Generation to create the starting room
            if (generatedRooms.Count < 1)
            {
                GameObject generatedRoom = Instantiate(enterance, transform.position, transform.rotation);

                if (generatedRoom.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
                {
                    generatedRooms.Add(dungeonPart); //Adds the Entrance room to the list
                }
            }
            else
            {
                bool shouldPlaceHallway = Random.Range(0f, 1f) > 0.5f; //This is basically a coin flip for the computer to decide if it should place a hallway or room.
                DungeonPart randomGeneratedRoom = null; //This indicates if a room is generated or not
                Transform room1Entrypoint = null; //This alligns entry points of rooms
                int totalRetries = 100;
                int retryIndex = 0;

                //This Loop checks all entrypoints and determins which are availaible or not
                //Adding totalRetries and retryIndex is a safety messure for the loop incase it cant find the generated room and stops it from looping.
                while (randomGeneratedRoom == null && retryIndex < totalRetries) 
                {
                    int randomLinkRoomIndex = Random.Range(0, generatedRooms.Count);
                    DungeonPart roomtoTest = generatedRooms[randomLinkRoomIndex];
                    if (roomtoTest.HasAvailableEntrypoint(out room1Entrypoint))
                    {
                        randomGeneratedRoom = roomtoTest;
                        break; //"Break" stops the while loop
                    }
                    retryIndex++;
                }

                GameObject doorToAlign = Instantiate(door, transform.position, transform.rotation);
                //If there is an available entry point then a door is created in its place

                if (shouldPlaceHallway)
                {
                    int randomIndex = Random.Range(0, hallways.Count);
                    GameObject generatedHallway = Instantiate(hallways[randomIndex], transform.position, transform.rotation);
                    //generatedHallway.transform.SetParent(null); (Multiplayer thing)
                    if (generatedHallway.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
                    {
                        if (dungeonPart.HasAvailableEntrypoint(out Transform room2Entrypoint))
                        {
                            generatedRooms.Add(dungeonPart); //This adds the hallway to the generated rooms list
                            doorToAlign.transform.position = room1Entrypoint.transform.position; // Aligns the room
                            doorToAlign.transform.rotation = room1Entrypoint.transform.rotation; // Aligns the room
                            AlignRooms(randomGeneratedRoom.transform, generatedHallway.transform, room1Entrypoint, room2Entrypoint);
                            if (HandleIntersection(dungeonPart))
                            {
                                dungeonPart.UnuseEntrypoint(room2Entrypoint);
                                randomGeneratedRoom.UnuseEntrypoint(room1Entrypoint);
                                RetryPlacement(generatedHallway, doorToAlign);
                                continue;
                                //This code refreshes the generator if there is overlapping rooms
                            }
                        }
                    }
                }
                else
                {
                    //Same logic of creating and placing hallways for rooms and special rooms

                    GameObject generatedRoom; //Gets the generated room

                    if (specialRooms.Count > 0) //Detects if there are special rooms and how often they should be generated
                    {                        
                        bool shouldPlaceSpecialRoom = Random.Range(0f, 1f) > 0.9f;

                        if (shouldPlaceSpecialRoom)
                        {
                            int randomIndex = Random.Range(0, specialRooms.Count);
                            generatedRoom = Instantiate(specialRooms[randomIndex], transform.position, transform.rotation);
                        }
                        else
                        {
                            int randomIndex = Random.Range(0, rooms.Count);
                            generatedRoom = Instantiate(rooms[randomIndex], transform.position, transform.rotation);
                        }
                    }
                    else //if not just spawns a normal room
                    {
                        int randomIndex = Random.Range(0, rooms.Count);
                        generatedRoom = Instantiate(rooms[randomIndex], transform.position, transform.rotation);
                    }

                    generatedRoom.transform.SetParent(null);

                    //Below justs creates the room and then aligns the rooms
                    if (generatedRoom.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
                    {
                        if (dungeonPart.HasAvailableEntrypoint(out Transform room2Entrypoint))
                        {
                            generatedRooms.Add(dungeonPart);
                            doorToAlign.transform.position = room1Entrypoint.transform.position;
                            doorToAlign.transform.rotation = room1Entrypoint.transform.rotation;
                            AlignRooms(randomGeneratedRoom.transform, generatedRoom.transform, room1Entrypoint, room2Entrypoint);

                            if (HandleIntersection(dungeonPart))
                            {
                                dungeonPart.UnuseEntrypoint(room2Entrypoint);
                                randomGeneratedRoom.UnuseEntrypoint(room1Entrypoint);
                                RetryPlacement(generatedRoom, doorToAlign);
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// If a room needs to be replaced it just reruns nearly identicle code. 
    /// Creates a room, checks if it will fit, then aligns the rooms
    /// </summary>
    /// <param name="itemToPlace"></param>
    /// <param name="doorToPlace"></param>
    private void RetryPlacement(GameObject itemToPlace, GameObject doorToPlace)
    {
        DungeonPart randomGeneratedRoom = null;
        Transform room1Entrypoint = null;
        int totalRetries = 100;
        int retryIndex = 0;

        while (randomGeneratedRoom == null && retryIndex < totalRetries) //Creates room and then checks it
        {
            int randomLinkRoomIndex = Random.Range(0, generatedRooms.Count - 1);
            DungeonPart roomToTest = generatedRooms[randomLinkRoomIndex];
            if (roomToTest.HasAvailableEntrypoint(out room1Entrypoint))
            {
                randomGeneratedRoom = roomToTest;
                break;
            }
            retryIndex++;
        }

        if (itemToPlace.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart)) //Aligns rooms together
        {
            if (dungeonPart.HasAvailableEntrypoint(out Transform room2Entrypoint))
            {
                doorToPlace.transform.position = room1Entrypoint.transform.position;
                doorToPlace.transform.rotation = room1Entrypoint.transform.rotation;
                AlignRooms(randomGeneratedRoom.transform, itemToPlace.transform, room1Entrypoint, room2Entrypoint);

                if (HandleIntersection(dungeonPart))
                {
                    dungeonPart.UnuseEntrypoint(room2Entrypoint);
                    randomGeneratedRoom.UnuseEntrypoint(room1Entrypoint);
                    RetryPlacement(itemToPlace, doorToPlace);
                }
            }
        }
    }

    private void FillEmptyEnterances()
    {
        generatedRooms.ForEach(room => room.FillEmptyDoors());
    }

    /// <summary>
    /// This Function Reads inputs from the room collider.triggers and determines if there are intersections or not.
    /// </summary>
    /// <param name="dungeonPart"></param>
    /// <returns></returns>
    private bool HandleIntersection(DungeonPart dungeonPart)
    {
        bool didIntersect = false;
        Collider[] hits = Physics.OverlapBox(dungeonPart.collider.bounds.center, dungeonPart.collider.bounds.size / 2, Quaternion.identity, roomsLayermask);

        foreach (Collider hit in hits)
        {
            if (hit == dungeonPart.collider) continue;

            if (hit != dungeonPart.collider)
            {
                didIntersect = true;
                break;
            }
        }

        return didIntersect;
    }

    /// <summary>
    /// This is the function that aligns rooms together
    /// </summary>
    /// <param name="room1"></param>
    /// <param name="room2"></param>
    /// <param name="room1Entry"></param>
    /// <param name="room2Entry"></param>
    private void AlignRooms(Transform room1, Transform room2, Transform room1Entry, Transform room2Entry)
    {
        // Debug.log($"room1 {room1}");
        // Debug.log($"room2 {room2}");
        // Debug.log($"room1Entry {room1Entry}");
        // Debug.log($"room2Entry {room2Entry}");
        // ^^^ IDK what this is, but it looked important, look into later ^^^
        float angle = Vector3.Angle(room1Entry.forward, room2Entry.forward);
        // Debug.Log($"Angle {angle}");

        //^^^ Finds the room 1 entry point angle, and room 2 entry point angle;

        room2.TransformPoint(room2Entry.position); //This makes it so either room1 rotates around room 2, or room 2 rotates around room 2 entry. (He doesnt specify)
        room2.eulerAngles = new Vector3(room2.eulerAngles.x, room2.eulerAngles.y + angle, room2.eulerAngles.z);
        
        Vector3 offset = room1Entry.position - room2Entry.position; //Calculates how far room 1 and room 2 are from eachother
        // Debug.Log($"offset {offset}");

        room2.position += offset; //This brings the 2 rooms together

        Physics.SyncTransforms();
        //When you are moving around objects with colliders, sometimes the transform of objects isnt synced properly. And this fixes it.
    }

    public List<DungeonPart> GetGeneratedRooms() => generatedRooms;

    public bool IsGenerated() => isGenerated;

    public void GenerateSingleRoom()
    {
        //We take the count of alternate enterances from the noOFRooms to keep the same amount of rooms with the alternate enterances
        for (int i = 0; i < 1; i++)
        {
            //Bellow will run only at the start of Generation to create the starting room
            if (generatedRooms.Count < 1)
            {
                GameObject generatedRoom = Instantiate(enterance, transform.position, transform.rotation);

                if (generatedRoom.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
                {
                    generatedRooms.Add(dungeonPart); //Adds the Entrance room to the list
                }
            }
            else
            {
                bool shouldPlaceHallway = Random.Range(0f, 1f) > 0.5f; //This is basically a coin flip for the computer to decide if it should place a hallway or room.
                DungeonPart randomGeneratedRoom = null; //This indicates if a room is generated or not
                Transform room1Entrypoint = null; //This alligns entry points of rooms
                int totalRetries = 100;
                int retryIndex = 0;

                //This Loop checks all entrypoints and determins which are availaible or not
                //Adding totalRetries and retryIndex is a safety messure for the loop incase it cant find the generated room and stops it from looping.
                while (randomGeneratedRoom == null && retryIndex < totalRetries)
                {
                    int randomLinkRoomIndex = Random.Range(0, generatedRooms.Count);
                    DungeonPart roomtoTest = generatedRooms[randomLinkRoomIndex];
                    if (roomtoTest.HasAvailableEntrypoint(out room1Entrypoint))
                    {
                        randomGeneratedRoom = roomtoTest;
                        break; //"Break" stops the while loop
                    }
                    retryIndex++;
                }

                GameObject doorToAlign = Instantiate(door, transform.position, transform.rotation);
                //If there is an available entry point then a door is created in its place

                if (shouldPlaceHallway)
                {
                    int randomIndex = Random.Range(0, hallways.Count);
                    GameObject generatedHallway = Instantiate(hallways[randomIndex], transform.position, transform.rotation);
                    //generatedHallway.transform.SetParent(null); (Multiplayer thing)
                    if (generatedHallway.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
                    {
                        if (dungeonPart.HasAvailableEntrypoint(out Transform room2Entrypoint))
                        {
                            generatedRooms.Add(dungeonPart); //This adds the hallway to the generated rooms list
                            doorToAlign.transform.position = room1Entrypoint.transform.position; // Aligns the room
                            doorToAlign.transform.rotation = room1Entrypoint.transform.rotation; // Aligns the room
                            AlignRooms(randomGeneratedRoom.transform, generatedHallway.transform, room1Entrypoint, room2Entrypoint);
                            if (HandleIntersection(dungeonPart))
                            {
                                dungeonPart.UnuseEntrypoint(room2Entrypoint);
                                randomGeneratedRoom.UnuseEntrypoint(room1Entrypoint);
                                RetryPlacement(generatedHallway, doorToAlign);
                                continue;
                                //This code refreshes the generator if there is overlapping rooms
                            }
                        }
                    }
                }
                else
                {
                    //Same logic of creating and placing hallways for rooms and special rooms

                    GameObject generatedRoom; //Gets the generated room

                    if (specialRooms.Count > 0) //Detects if there are special rooms and how often they should be generated
                    {
                        bool shouldPlaceSpecialRoom = Random.Range(0f, 1f) > 0.9f;

                        if (shouldPlaceSpecialRoom)
                        {
                            int randomIndex = Random.Range(0, specialRooms.Count);
                            generatedRoom = Instantiate(specialRooms[randomIndex], transform.position, transform.rotation);
                        }
                        else
                        {
                            int randomIndex = Random.Range(0, rooms.Count);
                            generatedRoom = Instantiate(rooms[randomIndex], transform.position, transform.rotation);
                        }
                    }
                    else //if not just spawns a normal room
                    {
                        int randomIndex = Random.Range(0, rooms.Count);
                        generatedRoom = Instantiate(rooms[randomIndex], transform.position, transform.rotation);
                    }

                    generatedRoom.transform.SetParent(null);

                    //Below justs creates the room and then aligns the rooms
                    if (generatedRoom.TryGetComponent<DungeonPart>(out DungeonPart dungeonPart))
                    {
                        if (dungeonPart.HasAvailableEntrypoint(out Transform room2Entrypoint))
                        {
                            generatedRooms.Add(dungeonPart);
                            doorToAlign.transform.position = room1Entrypoint.transform.position;
                            doorToAlign.transform.rotation = room1Entrypoint.transform.rotation;
                            AlignRooms(randomGeneratedRoom.transform, generatedRoom.transform, room1Entrypoint, room2Entrypoint);

                            if (HandleIntersection(dungeonPart))
                            {
                                dungeonPart.UnuseEntrypoint(room2Entrypoint);
                                randomGeneratedRoom.UnuseEntrypoint(room1Entrypoint);
                                RetryPlacement(generatedRoom, doorToAlign);
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }    
}

