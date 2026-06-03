using UnityEngine;

public class BunkerDoorScript : MonoBehaviour
{
    public GameObject nextFloor;
    [SerializeField] private GameObject LeftDoor;
    [SerializeField] private GameObject RightDoor;

    public void NextLevel()
    {
        
    }

    private void OpenDoor()
    {
        
    }
    
    private void LoadNextFloor()
    {
        nextFloor.SetActive(true);
    }
}
