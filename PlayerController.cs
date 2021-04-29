using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    private NavMeshAgent myNavmeshAgent;
    private Animator myAnimator;

    private bool hasControl = true;

    private float doorOpenDelay;

    private bool hasDoorBeenTriggered = false;
    private float doorCoolDownTimer = 0;

    // Caches a reference to the door that this object is currently in a sequence with
    private DoorController doorInteractingWith;

    // Stores the final destination that the agent is trying to reach
    private Vector3 targetPosition;

    private void Awake()
    {
        myNavmeshAgent = GetComponent<NavMeshAgent>();
        myAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && hasControl)
        {
            ClickToMove();
        }

        if (Vector3.Distance(myNavmeshAgent.destination, transform.position) <= myNavmeshAgent.stoppingDistance)
        {
            myAnimator.SetBool("isRunning", false);
        }

        if (hasDoorBeenTriggered)
        {
            doorCoolDownTimer += Time.deltaTime;
            if (doorCoolDownTimer >= 1.4f)
            {
                hasDoorBeenTriggered = false;
                doorCoolDownTimer = 0;
            }
        }
    }

    private void ClickToMove()
    {        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool hasHit = Physics.Raycast(ray, out hit);
        if (hasHit)
        {
            SetDestination(hit.point);
        }
    }

    private void SetDestination(Vector3 target)
    {
        myAnimator.SetBool("isRunning", true);
        myNavmeshAgent.SetDestination(target);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Door" && !hasDoorBeenTriggered)
        {
            // Stores a cached reference to the door that we are interacting with
            doorInteractingWith = other.GetComponent<DoorController>();

            // Sets a bool so that we know that the agent is already interacting with a door
            hasDoorBeenTriggered = true;

            // Takes user input control away
            hasControl = false;

            // Saves the final destination position that the agent is trying to reach
            targetPosition = myNavmeshAgent.destination;

            // Gets the position the player should move to while waiting for the door to open
            Vector3 waitPosition = doorInteractingWith.GetWaitPosition(transform.position);
                        
            // Gets the length of the door open animation so we know how long to wait
            doorOpenDelay = doorInteractingWith.animationDuration;

            // Tell the agent to move to the position where he should wait for the door open animation
            SetDestination(waitPosition);

            // Starts the coroutine that will carry out the sequence of waiting and opening the door
            StartCoroutine(DelayCoroutine());
        }
    }

    private IEnumerator DelayCoroutine()
    {

        // Wait until the player reaches the wait position before opening the door
        while (Vector3.Distance(myNavmeshAgent.destination, transform.position) > myNavmeshAgent.stoppingDistance)
        {
            // DO NOTHING
            yield return null;
        }

        // Play the door open animation
        doorInteractingWith.OpenDoor();

        // Wait for the door open animation to complete
        yield return new WaitForSeconds(doorOpenDelay);

        // Tell the agent to move to the final destination
        SetDestination(targetPosition);

        doorOpenDelay = 0;
        hasControl = true;
        myNavmeshAgent.isStopped = false;
        myAnimator.SetBool("isRunning", true);
    }

}
