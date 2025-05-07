using UnityEngine;

public class Door : GameBehaviour
{
    //[SerializeField] GameObject doorMesh;
    //public bool hasTriggered;
    [SerializeField] Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator != null) { Debug.LogError("AnimatorNotFound"); }
    }

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
            animator.SetTrigger("Open");
            _DG.TryFillingEntrypointsUntilStuck();

        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetTrigger("Close");
        }
    }
}
