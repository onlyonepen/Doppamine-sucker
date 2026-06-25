using DG.Tweening;
using Script.Player.States;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class PlayerStateManager : MonoBehaviour
{
    [ReadOnly] public string curreentState;
    [Header("Energy")]
    public bool useEnergy = true;
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
    public PlayerCameraController camController;
    public PlayerHpManager playerHp;
    public FootstepManager footstepManager;
    [Header("CameraFov")]
    public float minFov = 80f;
    public float maxFov = 100f;
    public float fovSmoothSpeed = 10f;
    public float fovChangeTreshold = 10f;
    public float MaxSpeedForFovChange = 60f;
    private float currentFov;
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
    public Transform GrappleArm;
    public float GrappleEnemyOffset = 1.5f;
    internal Vector3 initialHandPos;
    internal Quaternion initialHandRot;
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
    public float PullIntoSpeed = 40f;
    public float OvershootYAxis = 3f;

    [HideInInspector] public bool canGrapple;

    [Header("DiedState")]
    public Image redScreenOverlay;
    public float deathDuration = 1f;
    public GameObject gameOverScreen;
    
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
    public PlayerState DiedState = new DiedState();

    #endregion

    private void Start()
    {
        currentEnergy = MaxEnergy;
        currentFov = minFov;
        initialHandPos = GrappleArm.localPosition;
        initialHandRot = GrappleArm.localRotation;
        
        CurrentState = BaseState;
        CurrentState.OnStateEnter(this);
    }

    void Update()
    {
        CurrentState.OnStateUpdate();
        EnergyRegen();
        UpdateFov();
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
    
    [Header("AimAssist")]
    public float minAimAssistRadius = 0.8f; 
    public float maxAimAssistRadius = 5.0f; 

    public RaycastHit GrapplePrediction()
    {
        LayerMask assistPriority = HeavyPull | Pullable; // Enemies / Pullables
        LayerMask allGrappleMasks = Swingable | assistPriority;
        LayerMask obstacleMask = GlobalReference.Instance.TerrainLayer; 

        // --- 1. DIRECT RAYCAST ---
        RaycastHit directHitEnemy = new RaycastHit();
        bool foundDirectEnemy = false;
    
        RaycastHit directHitSwing = new RaycastHit();
        bool foundDirectSwing = false;

        // Check perfectly down the center first
        if (Physics.Raycast(Cam.transform.position, Cam.transform.forward, out RaycastHit tempDirect, GrappleMaxDistance, allGrappleMasks | obstacleMask))
        {
            int hitLayer = 1 << tempDirect.collider.gameObject.layer;

            // SWAPPED: Check if the direct hit is terrain/swingable FIRST
            if ((hitLayer & Swingable) != 0) 
            {
                directHitSwing = tempDirect;
                foundDirectSwing = true;
            }
            // Then check if the direct hit is an enemy
            else if ((hitLayer & assistPriority) != 0)
            {
                directHitEnemy = tempDirect;
                foundDirectEnemy = true;
            }
        }

        // --- 2. AIM ASSIST (SPHERECAST) ---
        RaycastHit[] hits = Physics.SphereCastAll(
            Cam.transform.position, 
            maxAimAssistRadius, 
            Cam.transform.forward, 
            GrappleMaxDistance, 
            allGrappleMasks
        );

        RaycastHit bestAssistEnemyHit = new RaycastHit();
        float bestEnemyScore = -1f;
        bool foundAssistEnemy = false;

        RaycastHit bestAssistSwingHit = new RaycastHit();
        float bestSwingScore = -1f;
        bool foundAssistSwing = false;

        foreach (RaycastHit hit in hits)
        {
            // Unity Quirk: If the SphereCast starts inside a collider, hit.point returns Vector3.zero.
            // This line prevents mathematical errors when calculating the localHitPoint.
            if (hit.point == Vector3.zero) continue;

            Vector3 localHitPoint = hit.point - Cam.transform.position;
            float distanceAlongRay = Vector3.Dot(localHitPoint, Cam.transform.forward);

            if (distanceAlongRay < 0) continue; 

            // Dynamic cone calculation
            float currentAllowedRadius = Mathf.Lerp(minAimAssistRadius, maxAimAssistRadius, distanceAlongRay / GrappleMaxDistance);
            Vector3 pointOnCenterLine = Cam.transform.position + (Cam.transform.forward * distanceAlongRay);
            float distanceFromCenter = Vector3.Distance(pointOnCenterLine, hit.point);

            if (distanceFromCenter > currentAllowedRadius)
                continue;

            // FIXED: Check for blocking obstacles safely!
            // Output the hit data, and verify we aren't just hitting the object we want to grapple.
            if (Physics.Linecast(Cam.transform.position, hit.point, out RaycastHit blockHit, obstacleMask))
            {
                if (blockHit.collider != hit.collider)
                {
                    continue; // It's blocked by a different obstacle
                }
            }

            Vector3 directionToHit = localHitPoint.normalized;
            float alignmentScore = Vector3.Dot(Cam.transform.forward, directionToHit);

            bool isEnemy = ((1 << hit.collider.gameObject.layer) & assistPriority) != 0;

            // Separate highest scoring enemy and highest scoring terrain
            if (isEnemy)
            {
                if (!foundAssistEnemy || alignmentScore > bestEnemyScore)
                {
                    bestAssistEnemyHit = hit;
                    bestEnemyScore = alignmentScore;
                    foundAssistEnemy = true;
                }
            }
            else 
            {
                if (!foundAssistSwing || alignmentScore > bestSwingScore)
                {
                    bestAssistSwingHit = hit;
                    bestSwingScore = alignmentScore;
                    foundAssistSwing = true;
                }
            }
        }

        // --- 3. PRIORITY RESOLUTION ---
        RaycastHit finalHit = new RaycastHit();
        bool hasValidHit = false;

        // SWAPPED: 1. Terrain in Direct Raycast (Intentional Traversal)
        if (foundDirectSwing)
        {
            finalHit = directHitSwing;
            hasValidHit = true;
        }
        // SWAPPED: 2. Enemy in Direct Raycast (Intentional Combat)
        else if (foundDirectEnemy)
        {
            finalHit = directHitEnemy;
            hasValidHit = true;
        }
        // 3. Enemy in Aim Assist (Forgiving Combat)
        else if (foundAssistEnemy)
        {
            finalHit = bestAssistEnemyHit;
            hasValidHit = true;
        }
        // 4. Terrain in Aim Assist (Forgiving Traversal)
        else if (foundAssistSwing)
        {
            finalHit = bestAssistSwingHit;
            hasValidHit = true;
        }

        // --- 4. VISUAL FEEDBACK ---
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
        grappleGun.localRotation = Quaternion.Euler(0f, 0f, 0f);
        //grappleGun.DOLocalRotate(Vector3.zero, .5f);
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
        return true;
        
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


    public void UpdateFov()
    {
        float currentSpeed = rb.linearVelocity.magnitude;
        float speedFactor = Mathf.InverseLerp(fovChangeTreshold, MaxSpeedForFovChange, currentSpeed);
        float logFactor = Mathf.Log10(1f + (speedFactor * 9f));
        float targetFov = Mathf.Lerp(minFov, maxFov, logFactor);
        currentFov = Mathf.Lerp(currentFov, targetFov, Time.deltaTime * fovSmoothSpeed);
        camController.changeFov(currentFov);
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
        manager.curreentState = this.ToString();
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
