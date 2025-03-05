using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    private bool isOccupied = false;

    public void SetOccupied(bool value = true) => isOccupied = value;

    public bool IsOccupied() => isOccupied;

    //Idk what this does yet...
}
