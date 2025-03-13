using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class GameBehaviour : MonoBehaviour
{   
    protected static DungeonGenerator _DG { get { return DungeonGenerator.instance; } }

    protected static FirstPersonController _PLAYER { get { return FirstPersonController.instance; } }

    public Transform GetClosestObject(Transform _origin, List<GameObject> _objects)
    {
        if (_objects == null || _objects.Count == 0)
            return null;

        float distance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject go in _objects)
        {
            float currentDistance = Vector3.Distance(_origin.transform.position, go.transform.position);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = go.transform;
            }
        }
        return closest;
    }
}
