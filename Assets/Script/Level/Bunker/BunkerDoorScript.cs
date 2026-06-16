using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using VInspector; // Assuming you are using this for the [Button] attribute

public class SlidingDoors : MonoBehaviour
{
    public Transform NextLevel;
    public Transform AllEnemyParent;
    
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

    private bool isOpening = false;

    public GameObject[] FloorEnemies;
    
    void Start()
    {
        leftClosedPos = leftDoor.localPosition;
        rightClosedPos = rightDoor.localPosition;
        
        leftOpenPos = leftClosedPos + (Vector3.forward * openDistance);
        rightOpenPos = rightClosedPos + (Vector3.back * openDistance);

        List<GameObject> childList = new List<GameObject>();
        foreach (Transform child in AllEnemyParent)
        {
            childList.Add(child.gameObject);
        }
        FloorEnemies = childList.ToArray();
    }

    void Update()
    {
        // We no longer need to constantly Lerp the doors here!
        // We only use Update to check if the enemies are cleared.
        
        if (!isOpening)
        {
            bool HasEnemyLeft = false;
            foreach (GameObject obj in FloorEnemies)
            {
                if (obj.activeInHierarchy) HasEnemyLeft = true;
            }
            if(!HasEnemyLeft) NextFloor();
        }
    }

    [Button]
    public void NextFloor()
    {
        // Prevent this from triggering multiple times if NextFloor() is called repeatedly
        if (isOpening) return; 
        
        isOpening = true;
        NextLevel.gameObject.SetActive(true);

        // Duration-based movement using DOTween
        // SetEase(Ease.InOutSine) gives it a nice smooth start and stop.
        leftDoor.DOLocalMove(leftOpenPos, openDuration).SetEase(Ease.InOutSine);
        rightDoor.DOLocalMove(rightOpenPos, openDuration).SetEase(Ease.InOutSine);
        
        // You can easily sync your camera shake duration to the door duration now!
        GlobalReference.Instance.player.camController.transform.DOShakePosition(openDuration,0.5f,50);
    }
}