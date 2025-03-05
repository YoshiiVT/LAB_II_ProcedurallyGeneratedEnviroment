using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Spawn : MonoBehaviour
{
    GameObject playerCharacter;
    private void Awake()
    {
        playerCharacter = GameObject.FindGameObjectWithTag("Player");
    }

    void Start()
    {
        playerCharacter.transform.position = transform.position;
        playerCharacter.GetComponent<FirstPersonController>().enabled = false;
        StartCoroutine(WaitForTransform());
    }

    IEnumerator WaitForTransform()
    {
        yield return new WaitForSeconds(0.1f);
        playerCharacter.GetComponent<FirstPersonController>().enabled = true;
    }

}
