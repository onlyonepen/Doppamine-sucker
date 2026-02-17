using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public PlayerBaseMovement PBM;
    public Camera Cam;
    public PlayerRUD RUD;

    [Header("Grapple")]
    public float GrappleMaxDistance;
    public float GrappleTravelTime;
    public LayerMask Grappable;

    #region states

    public PlayerState CurrentState;

    public PlayerState BaseState = new PlayerBaseState();
    public PlayerState ThrowGrapple = new ThrowGrappleState();

    #endregion

    void Update()
    {
        CurrentState.OnStateUpdate();
    }

    public void ChangeState(PlayerState state)
    {
        CurrentState.OnStateExit();
        CurrentState = state;
        CurrentState.OnStateEnter(this);
    }
}

public abstract class PlayerState
{
    public PlayerStateManager manager;
    public GameObject player;
    public virtual void OnStateEnter(PlayerStateManager gamestateManager)
    {
        manager = gamestateManager;
        player = manager.gameObject;
    }
    public virtual void OnStateUpdate() { }
    public virtual void OnStateExit() { }
}

public class PlayerRUD
{
    [HideInInspector] public Vector3 GrapplePoint;
    [HideInInspector] public GameObject GrappledObject;
}
