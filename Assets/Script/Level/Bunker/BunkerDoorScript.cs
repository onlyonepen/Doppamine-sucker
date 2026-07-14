using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using VInspector; // Assuming you are using this for the [Button] attribute

public class SlidingDoors : MonoBehaviour
{
    public AudioSource doorSound;
    [Header("Door References")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Door Settings")]
    public float openDistance = 2.5f;
    public float openDuration = 1.5f; // Changed from slideSpeed to duration

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;
    
    void Start()
    {
        leftClosedPos = leftDoor.localPosition;
        rightClosedPos = rightDoor.localPosition;
        
        leftOpenPos = leftClosedPos + (Vector3.forward * openDistance);
        rightOpenPos = rightClosedPos + (Vector3.back * openDistance);
    }

    [Button]
    public void PlaySoundd()
    {
        doorSound.Play();
    }

    [Button]
    public void OpenDoor(bool skipAnim = false)
    {
        if (skipAnim)
        {
            leftDoor.localPosition = leftOpenPos;
            rightDoor.localPosition = rightOpenPos;
        }
        else
        {
            leftDoor.DOLocalMove(leftOpenPos, openDuration).SetEase(Ease.InOutSine);
            rightDoor.DOLocalMove(rightOpenPos, openDuration).SetEase(Ease.InOutSine);
            GlobalReference.Instance.player.CameraController.transform.DOShakePosition(openDuration,0.5f,50);
            doorSound.Play();
        }
    }
}