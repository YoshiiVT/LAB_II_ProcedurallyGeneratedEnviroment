using UnityEngine;

public enum EntrypointSize
{
    Small,
    Large
}

public class EntryPoint : MonoBehaviour
{
    public EntrypointSize entrypointSize;

    [SerializeField]
    private bool isOccupied = false;

    public void SetOccupied(bool value = true) => isOccupied = value;

    public bool IsOccupied() => isOccupied;

    //Idk what this does yet...
}
