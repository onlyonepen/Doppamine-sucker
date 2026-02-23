using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public Rigidbody rb;
    public PlayerBaseMovement PBM;
    public Camera Cam;
    public PlayerRUD RUD = new PlayerRUD();

    [Header("Grapple")]
    public Transform Guntip;
    public float GrappleMaxDistance;
    public float GrappleTravelTime;
    public float JointSpring = 4.5f;
    public float JointDamper = 7f;
    public float JointMassScale = 4.5f;
    public float AirControlFwdForce = 600;
    public float AirControlHorizontalForce = 400;
    public float SwingDashPower = 20;
    public float SwingDashMaxPower = 18;
    public float SwingDashMinPower = 5;
    public LayerMask Swingable;
    public LayerMask Pullable;
    public LineRenderer GrappleLr;

    #region states

    public PlayerState CurrentState;

    public PlayerState BaseState = new PlayerBaseState();
    public PlayerState ThrowGrappleState = new ThrowGrappleState();
    public PlayerState SwingState = new SwingState();

    #endregion

    private void Start()
    {
        CurrentState = BaseState;
        CurrentState.OnStateEnter(this);
    }

    void Update()
    {
        CurrentState.OnStateUpdate();
    }

    void FixedUpdate()
    {
        CurrentState.OnStatePhysicsUpdate();
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
    public virtual void OnStatePhysicsUpdate() { }
    public virtual void OnStateExit() { }
}

public class PlayerRUD
{
    [HideInInspector] public Vector3 GrapplePoint;
    [HideInInspector] public GameObject GrappledObject;
}
