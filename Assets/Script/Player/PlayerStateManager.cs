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
    public PlayerState GrapplePullState = new GrapplePullState();
    public PlayerState GrapplePullinState = new GrapplePullintoState();
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
    public float minAimAssistRadius = 0.5f; 
    public float maxAimAssistRadius = 4.0f; 
    
    public RaycastHit GrapplePrediction()
    {
        LayerMask assistPriority = HeavyPull | Pullable;
        LayerMask allGrappleMasks = Swingable | assistPriority;
    
        // 1. Precise Raycast
        RaycastHit directHit;
        bool foundDirect = Physics.Raycast(
            Cam.transform.position, 
            Cam.transform.forward, 
            out directHit, 
            GrappleMaxDistance, 
            allGrappleMasks
        );
    
        // 2. Aim Assist (Cast with MAX radius to catch all potential targets)
        RaycastHit[] hits = Physics.SphereCastAll(
            Cam.transform.position, 
            maxAimAssistRadius, 
            Cam.transform.forward, 
            GrappleMaxDistance, 
            allGrappleMasks
        );
    
        RaycastHit bestEnemyHit = new RaycastHit();
        float bestEnemyScore = -1f;
        bool foundEnemy = false;
    
        RaycastHit bestSwingHit = new RaycastHit();
        float bestSwingScore = -1f;
        bool foundSwing = false;
    
        foreach (RaycastHit hit in hits)
        {
            // --- NEW: CONE FILTERING MATH ---
            
            // Find exactly how far ALONG the camera's ray this object is
            Vector3 localHitPoint = hit.point - Cam.transform.position;
            float distanceAlongRay = Vector3.Dot(localHitPoint, Cam.transform.forward);
    
            // Ignore objects technically behind the camera
            if (distanceAlongRay < 0) continue; 
    
            // Calculate the maximum allowed radius at this specific distance
            float currentAllowedRadius = Mathf.Lerp(minAimAssistRadius, maxAimAssistRadius, distanceAlongRay / GrappleMaxDistance);
    
            // Find the exact dead-center point on the ray at this distance
            Vector3 pointOnCenterLine = Cam.transform.position + (Cam.transform.forward * distanceAlongRay);
            
            // Measure how far the hit point is from the center line
            float distanceFromCenter = Vector3.Distance(pointOnCenterLine, hit.point);
    
            // If the object is outside our dynamic cone, ignore it completely
            if (distanceFromCenter > currentAllowedRadius)
                continue;
    
            // --- OLD: SCORING LOGIC ---
    
            Vector3 directionToHit = localHitPoint.normalized;
            float alignmentScore = Vector3.Dot(Cam.transform.forward, directionToHit);
    
            bool isEnemy = ((1 << hit.collider.gameObject.layer) & assistPriority) != 0;
    
            if (isEnemy)
            {
                if (!foundEnemy || alignmentScore > bestEnemyScore)
                {
                    bestEnemyHit = hit;
                    bestEnemyScore = alignmentScore;
                    foundEnemy = true;
                }
            }
            else 
            {
                if (!foundSwing || alignmentScore > bestSwingScore)
                {
                    bestSwingHit = hit;
                    bestSwingScore = alignmentScore;
                    foundSwing = true;
                }
            }
        }
    
        // 3. Determine Final Hit (Enemy > Direct Raycast > Swingable)
        RaycastHit finalHit = new RaycastHit();
        bool hasValidHit = false;
    
        if (foundEnemy)
        {
            finalHit = bestEnemyHit;
            hasValidHit = true;
        }
        else if (foundDirect)
        {
            finalHit = directHit;
            hasValidHit = true;
        }
        else if (foundSwing)
        {
            finalHit = bestSwingHit;
            hasValidHit = true;
        }
    
        // 4. Update Visuals
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
