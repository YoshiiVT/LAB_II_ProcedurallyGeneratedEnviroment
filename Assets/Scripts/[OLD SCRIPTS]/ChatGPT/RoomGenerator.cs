using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject startingRoomPrefab;
    public GameObject doorPrefab;
    public List<GameObject> roomPrefabs; // A list of room prefabs to randomly choose from

    [Header("Room Generation Settings")]
    public int maxRooms = 10;  // Number of rooms to generate
    public float roomSpacing = 10f;  // Minimum spacing between rooms
    public int maxAttemptsPerRoom = 10; // Maximum attempts to place a room before giving up

    private List<Vector3> roomPositions = new List<Vector3>();
    private GameObject player;

    void Start()
    {
        // Initialize player reference if needed
        player = Camera.main.gameObject;

        // Start the room generation
        GenerateRooms();
    }

    void GenerateRooms()
    {
        // Start by placing the starting room at position (0, -100, 0)
        Vector3 startingPos = new Vector3(0, -100, 0);
        roomPositions.Add(startingPos);
        Instantiate(startingRoomPrefab, startingPos, Quaternion.identity);

        // Generate the remaining rooms
        GenerateRandomRoomArrangement();
    }

    void GenerateRandomRoomArrangement()
    {
        int roomsGenerated = 1;  // We already have the starting room
        int attempts = 0;

        // Try to generate rooms until we reach the max room count
        while (roomsGenerated < maxRooms && attempts < maxAttemptsPerRoom)
        {
            attempts++;

            // Randomly select a room prefab
            GameObject roomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];

            // Try to place the new room in a valid position adjacent to an existing room
            bool roomPlaced = false;

            // Iterate through each room already placed to try and place the new room nearby
            foreach (Vector3 roomPosition in roomPositions)
            {
                // Try to connect to one of the 4 possible entry points (top, bottom, left, right)
                for (int i = 0; i < 4; i++)
                {
                    // Calculate the potential new room position based on the entry point offset
                    Vector3 newRoomPosition = roomPosition + GetEntranceOffset(i);

                    // Ensure Y position is fixed at -100
                    newRoomPosition.y = -100;

                    // Check if the new position is occupied
                    if (!roomPositions.Contains(newRoomPosition) && !IsPositionOccupied(newRoomPosition, roomPrefab))
                    {
                        // If valid, instantiate the new room and align it
                        roomPositions.Add(newRoomPosition);
                        GameObject newRoom = Instantiate(roomPrefab, newRoomPosition, Quaternion.identity);

                        // Align the new room with the existing room's entry point
                        AlignRoomWithEntryPoint(newRoom, roomPosition, i);

                        // Create doors between rooms if needed
                        CreateDoorsBetweenRooms(newRoomPosition, i);

                        // Increment the room count
                        roomsGenerated++;
                        roomPlaced = true;
                        break;
                    }
                }

                if (roomPlaced)
                    break;
            }

            // If no room was placed after trying all options, increment attempts
            if (!roomPlaced)
            {
                attempts++;
            }
        }

        // Handle failure to generate the desired number of rooms
        if (roomsGenerated < maxRooms)
        {
            Debug.LogWarning("Could not generate the desired number of rooms after multiple attempts.");
        }
    }

    // Get the offset for a given entry point (top, bottom, left, right)
    Vector3 GetEntranceOffset(int entranceIndex)
    {
        switch (entranceIndex)
        {
            case 0: return new Vector3(0, 0, 5);  // Top
            case 1: return new Vector3(0, 0, -5); // Bottom
            case 2: return new Vector3(5, 0, 0);  // Right
            case 3: return new Vector3(-5, 0, 0); // Left
            default: return Vector3.zero;
        }
    }

    // Check if the position is already occupied by another room
    bool IsPositionOccupied(Vector3 position, GameObject roomPrefab)
    {
        // Get the room's collider bounds for collision checking
        Collider roomCollider = roomPrefab.GetComponent<Collider>();
        if (roomCollider == null) return false;

        // Check for overlap with existing rooms
        Bounds roomBounds = roomCollider.bounds;
        roomBounds.center = position;

        Collider[] colliders = Physics.OverlapBox(roomBounds.center, roomBounds.extents, Quaternion.identity);
        return colliders.Length > 0;
    }

    // Create doors between adjacent rooms
    void CreateDoorsBetweenRooms(Vector3 roomPosition, int entranceIndex)
    {
        // Place a door at the entrance position of the new room
        Vector3 doorPosition = roomPosition + GetEntranceOffset(entranceIndex);
        doorPosition.y = -100;  // Ensure door is at y = -100
        Instantiate(doorPrefab, doorPosition, Quaternion.identity);
    }

    // Align a new room with the entry point of an adjacent room
    void AlignRoomWithEntryPoint(GameObject newRoom, Vector3 existingRoomPosition, int entranceIndex)
    {
        // Find the "Entries" GameObject in the new room prefab
        Transform entriesTransform = newRoom.transform.Find("Entries");
        if (entriesTransform != null)
        {
            // Ensure that the entranceIndex is valid
            if (entranceIndex < entriesTransform.childCount)
            {
                // Get the entry point at the specified index
                Transform entryPoint = entriesTransform.GetChild(entranceIndex);

                // Calculate the correct position for the new room by aligning its entry point with the adjacent room's entry point
                Vector3 entryPointPosition = entryPoint.position;
                Vector3 targetPosition = existingRoomPosition + GetEntranceOffset(entranceIndex);
                targetPosition.y = -100; // Keep Y constant

                // Calculate the offset and apply it to align the new room
                Vector3 offset = targetPosition - entryPointPosition;
                newRoom.transform.position += offset;

                // Optionally, adjust the rotation of the new room if necessary
                AlignRotationWithEntryPoint(newRoom, entranceIndex);
            }
            else
            {
                Debug.LogWarning($"Room does not have an EntryPoint at index {entranceIndex}. Skipping alignment for this side.");
            }
        }
        else
        {
            Debug.LogWarning("Entries GameObject not found in the room prefab.");
        }
    }

    // Align the rotation of the new room based on the entry point
    void AlignRotationWithEntryPoint(GameObject newRoom, int entranceIndex)
    {
        Transform entriesTransform = newRoom.transform.Find("Entries");
        if (entriesTransform != null && entranceIndex < entriesTransform.childCount)
        {
            Transform entryPoint = entriesTransform.GetChild(entranceIndex);
            newRoom.transform.rotation = entryPoint.rotation;
        }
    }
}
