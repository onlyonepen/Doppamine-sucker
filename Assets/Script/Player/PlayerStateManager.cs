using DG.Tweening;
using UnityEngine;
using VInspector;

public class PlayerStateManager : MonoBehaviour
{
    [Header("Energy")]
    public float MaxEnergy = 100f;
    public float InitialThrowUsage = 20;
    public float GrappleLeapUsage = 40;
    public float GrappleDashUsage = 10;
    
    public float EnergyRegeneration = 5f;
    public float GroundedEnergyRegeneration = 50f;

    public float currentEnergy;
    
    [Header("BasicReference")]
    public Rigidbody rb;
    public PlayerBaseMovement PBM;
    public Camera Cam;
    public PlayerRUD RUD = new PlayerRUD();
    public LayerMask TerrainLayer;
    public Transform feetTrans;
    [Header("Wall run")]
    public float WallRunAccel = 50f;
    public float WallRunMaxSpeed = 12f; 
    public float WallClimbSpeed = 3f;
    public float WallJumpForce = 10;
    public float WallCheckDistance = 1f;
    public float GroundCheckDistance = 2f;
    public Transform SideRotateJoint;
    [Header("Sliding")]
    public float SlideSpeedMult = 0.2f;
    public float SlideSpeedTreshold = 2f;
    public float SlideFriction = 0.8f;
    [Header("Mantle")]
    public float PlayerHeightOffset = 1.6f;
    public float MantleFrontCastDist = 1.2f;
    [Header("Grapple")]
    public Transform grappleGun;
    public Transform Guntip;
    public float predictionSphereCastRadius = 3;
    public Transform predictionPoint;
    public float GrappleMaxDistance;
    public float GrappleTravelTime;
    public LayerMask Swingable;
    public LayerMask Pullable;
    public LayerMask HeavyPull;
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
    public float PullIntoSpeed = 1f;
    public float OvershootYAxis = 3f;

    #region states

    public PlayerState CurrentState;

    public PlayerState BaseState = new PlayerBaseState();
    public PlayerState ThrowGrappleState = new ThrowGrappleState();
    public PlayerState pullRopeBackState = new PullBackRopeState();
    public PlayerState SwingState = new SwingState();
    public PlayerState ReelState = new GrapplePullState();
    public PlayerState GrappleLeapState = new GrappleLeapState();
    public PlayerState WallRunState = new WallRunningState();
    public PlayerState MantleState = new MantleState();
    public PlayerState SlideState = new SlideState();

    #endregion

    private void Start()
    {
        CurrentState = BaseState;
        CurrentState.OnStateEnter(this);

        currentEnergy = MaxEnergy;
    }

    void Update()
    {
        CurrentState.OnStateUpdate();
        EnergyRegen();
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
    private void OnTriggerEnter(Collider other)
    {
        CurrentState.OnStateTriggerEnter(other);
    }
    public RaycastHit GrapplePrediction()
    {
        LayerMask AssisiPriority = HeavyPull | Pullable;
        
        Vector3 startSpherecasPos = Cam.transform.position + (Cam.transform.forward * GrappleMaxDistance);

        RaycastHit sphereCastHitOutward;
        bool sphereHitOutward = Physics.SphereCast(Cam.transform.position, predictionSphereCastRadius, Cam.transform.forward,
                            out sphereCastHitOutward, GrappleMaxDistance, Swingable | AssisiPriority);
        
        RaycastHit sphereCastHitInward;
        bool sphereHitInward = Physics.SphereCast(startSpherecasPos, predictionSphereCastRadius, -Cam.transform.forward,
            out sphereCastHitInward, GrappleMaxDistance - 5, Swingable | AssisiPriority);

        RaycastHit raycastHit;
        bool rayHit = Physics.Raycast(Cam.transform.position, Cam.transform.forward,
                            out raycastHit, GrappleMaxDistance, Swingable | AssisiPriority);

        RaycastHit finalHit = new RaycastHit();
        bool hasValidHit = false;

        // 2. Logic: If SphereCast hit a "Pullable" object, it takes absolute priority.
        bool sphereHitOutwardHitEnemy = sphereHitOutward &&
                                        ((1 << sphereCastHitOutward.collider.gameObject.layer) & AssisiPriority) != 0;
        bool sphereHitInwardHitEnemy = sphereHitInward &&
                                       ((1 << sphereCastHitInward.collider.gameObject.layer) & AssisiPriority) != 0;
        if (sphereHitInwardHitEnemy || sphereHitOutwardHitEnemy)
        {
            finalHit = sphereCastHitOutward;
            hasValidHit = true;
        }
        // 3. Otherwise, fall back to the precise Raycast if it hit anything.
        else if (rayHit)
        {
            finalHit = raycastHit;
            hasValidHit = true;
        }
        // 4. Last resort: use SphereCast for Swingables if Raycast missed.
        else if (sphereHitInward)
        {
            finalHit = sphereCastHitInward;
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
    }
    public void GuntipPointToGrapple()
    {
        grappleGun.LookAt(RUD.GrapplePoint);
    }
    public void GuntipDefault()
    {
        grappleGun.DOLocalRotate(Vector3.zero, .5f);
    }
    public void WaitToChangeState(PlayerState state, float WaitDur , float EnterTime)
    {
        if(Time.time - EnterTime > WaitDur)
        {
            ChangeState(state);
        }
    }
    public Vector3 GroundNormal()
    {
        if(!PBM.isGrounded) return Vector3.zero;
        else
        {
            RaycastHit hit;
            Physics.Raycast(transform.position, Vector3.down, out hit, 5f, TerrainLayer);
            return hit.normal;
        }
    }

    public bool UseEnergy(float Usage)
    {
        if (currentEnergy - Usage >= 0)
        {
            currentEnergy -= Usage;
            return true;
        }
        else return false;
    }

    private void EnergyRegen()
    {
        float Regen;
        if (PBM.isGrounded || CurrentState == WallRunState) Regen = GroundedEnergyRegeneration;
        else if (CurrentState == SwingState) Regen = 0;
        else Regen = EnergyRegeneration;

        if (currentEnergy + Regen * Time.deltaTime <= MaxEnergy)
        {
            currentEnergy += Regen * Time.deltaTime;
        }
        else currentEnergy = MaxEnergy;
    }
}

public abstract class PlayerState
{
    public PlayerStateManager manager;
    public GameObject player;

    public float stateEnterTime;
    public virtual void OnStateEnter(PlayerStateManager gamestateManager)
    {
        manager = gamestateManager;
        player = manager.gameObject;
        stateEnterTime = Time.time;
    }
    public virtual void OnStateUpdate() { }
    public virtual void OnStatePhysicsUpdate() { }
    public virtual void OnStateExit() { }
    public virtual void OnStateTriggerEnter(Collider collider) { }
}

public class PlayerRUD
{
    [HideInInspector] public Vector3 GrapplePoint;
    [HideInInspector] public GameObject GrappledObject;
    [HideInInspector] public Vector3 MantlePoint;
}
