using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public float animationDuration;
    private Animator myAnimator;

    // These 2 transforms can be moved in the scene view and placed in a position where you want the player to
    // stand when opening the door
    [SerializeField] private Transform frontOfDoorTransform;
    [SerializeField] private Transform backOfDoorTransform;

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        AnimationClip[] clips = myAnimator.runtimeAnimatorController.animationClips;
        foreach(AnimationClip c in clips)
        {
            if (c.name == "Open")
            {
                animationDuration = c.length;
            }
        }
    }

    public void OpenDoor()
    {
        myAnimator.ResetTrigger("CloseDoor");
        myAnimator.SetTrigger("OpenDoor");
        StartCoroutine(closeDoorCoroutine());
    }

    private IEnumerator closeDoorCoroutine()
    {
        yield return new WaitForSeconds(2f);
        myAnimator.ResetTrigger("OpenDoor");
        myAnimator.SetTrigger("CloseDoor");
    }


    // Finds which side of the door the player entered and returns the position he should stand in to wait
    public Vector3 GetWaitPosition(Vector3 position)
    {
        // Gets the direction from the door where the agent/player currently is
        Vector3 targetDirection = transform.position - position;

        // Gets the angle from the door to the player/agent based on the above direction
        float directionAngle = Vector3.Angle(transform.forward, targetDirection);

        if (Mathf.Abs(directionAngle) > 90f && Mathf.Abs(directionAngle) < 270f)
        {
            return frontOfDoorTransform.position;
        }
        else
        {
            return backOfDoorTransform.position;
        }
    }
}
