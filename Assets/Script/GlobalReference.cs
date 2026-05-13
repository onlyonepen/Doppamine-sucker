using UnityEngine;

public class GlobalReference : MonoBehaviour
{
    public PlayerStateManager player;
    public LayerMask playerLayer;
    public LayerMask TerrainLayer;

    public static GlobalReference Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
}
