using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public Rigidbody rb;
    public PlayerBaseMovement PBM;
    public Camera Cam;
    public PlayerRUD RUD = new PlayerRUD();

    [Header("Grapple")]
    public Transform Guntip;
    public float predictionSphereCastRadius = 3;
    public Transform predictionPoint;
    public float GrappleMaxDistance;
    public float GrappleTravelTime;
    public LayerMask Swingable;
    public LayerMask Pullable;
    public LineRenderer GrappleLr;
    [Header("Swinging")]
    public float JointSpring = 4.5f;
    public float JointDamper = 7f;
    public float JointMassScale = 4.5f;
    public float AirControlFwdForce = 600;
    public float AirControlHorizontalForce = 400;
    public float SwingDashPower = 20;
    public float SwingDashMaxPower = 18;
    public float SwingDashMinPower = 5;
    [Header("Pull into")]
    public float OvershootYAxis = 3f;

    #region states

    public PlayerState CurrentState;

    public PlayerState BaseState = new PlayerBaseState();
    public PlayerState ThrowGrappleState = new ThrowGrappleState();
    public PlayerState SwingState = new SwingState();
    public PlayerState ReelState = new ReelState();
    public PlayerState HookIntoState = new HookIntoState();

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

    public RaycastHit GrapplePrediction()
    {
        RaycastHit sphereCastHit;
        bool sphereHit = Physics.SphereCast(Cam.transform.position, predictionSphereCastRadius, Cam.transform.forward,
                            out sphereCastHit, GrappleMaxDistance, Swingable | Pullable);
        RaycastHit raycastHit;
        bool rayHit = Physics.Raycast(Cam.transform.position, Cam.transform.forward,
                            out raycastHit, GrappleMaxDistance, Swingable | Pullable);

        RaycastHit finalHit = new RaycastHit();
        bool hasValidHit = false;

        // 2. Logic: If SphereCast hit a "Pullable" object, it takes absolute priority.
        if (sphereHit && ((1 << sphereCastHit.collider.gameObject.layer) & Pullable) != 0)
        {
            finalHit = sphereCastHit;
            hasValidHit = true;
        }
        // 3. Otherwise, fall back to the precise Raycast if it hit anything.
        else if (rayHit)
        {
            finalHit = raycastHit;
            hasValidHit = true;
        }
        // 4. Last resort: use SphereCast for Swingables if Raycast missed.
        else if (sphereHit)
        {
            finalHit = sphereCastHit;
            hasValidHit = true;
        }

        // Update Visuals
        if (hasValidHit)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = finalHit.point;
        }
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        return finalHit;
        //Vector3 finalPoint;
        //if (raycastHit.point != Vector3.zero) finalPoint = raycastHit.point;
        //else if (sphereCastHit.point != Vector3.zero) finalPoint = sphereCastHit.point;
        //else finalPoint = Vector3.zero;

        //if ((1 << sphereCastHit.collider.gameObject.layer & Pullable) != 0)
        //{
        //    finalPoint = sphereCastHit.point;
        //    predictionPoint.gameObject.SetActive(true);
        //    predictionPoint.position = finalPoint;
        //}
        //else if(finalPoint != Vector3.zero)
        //{
        //    predictionPoint.gameObject.SetActive(true);
        //    predictionPoint.position = finalPoint;
        //}
        //else predictionPoint.gameObject.SetActive(false);

        //return raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
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
