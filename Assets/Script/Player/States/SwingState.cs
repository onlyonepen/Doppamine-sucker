using UnityEngine;

public class SwingState : PlayerState
{
    private Vector3 GrapplePoint;
    private SpringJoint joint;
    private float currentDist;

    private float vertInput;
    private float horiInput;

    private bool SwingDashed = false;

    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);

        manager.PBM.playerCanMove = false;
        SwingDashed = false;
        GrapplePoint = manager.RUD.GrapplePoint;
        manager.GrappleLr.enabled = true;
        manager.GrappleLr.positionCount = 2;
        manager.GrappleLr.SetPosition(1, GrapplePoint);
        InnitiateSpring();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        manager.GrappleLr.enabled = false;
        MonoBehaviour.Destroy(joint);
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        manager.GuntipPointToGrapple();

        if (Input.GetMouseButtonUp(1)) manager.ChangeState(manager.BaseState);
        if (Input.GetKeyDown(KeyCode.LeftShift)) manager.ChangeState(manager.HookIntoState);

        vertInput = Input.GetAxis("Vertical");
        horiInput = Input.GetAxis("Horizontal");
        vertInput = new Vector2(horiInput, vertInput).normalized.y;
        horiInput = new Vector2(horiInput, vertInput).normalized.x;

        drawRope();
        AirControl();
        SwingDash();

        if (Vector3.Distance(GrapplePoint, manager.transform.position) < currentDist)
        {
            RecalibateJointMaxDistance();
        }
        currentDist = Vector3.Distance(GrapplePoint, manager.transform.position);
    }

    public override void OnStatePhysicsUpdate()
    {
        base.OnStatePhysicsUpdate();

        manager.rb.AddForce(Vector3.up * 5, ForceMode.Acceleration); // lower gravity for better air control

    }

    private void InnitiateSpring()
    {
        joint = manager.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = GrapplePoint;

        float distance = Vector3.Distance(manager.transform.position, GrapplePoint);
        joint.maxDistance = distance * 1f;
        joint.minDistance = distance * 0.25f;

        joint.spring = manager.JointSpring; // Adjust these for "bounciness"
        joint.damper = manager.JointDamper; // Adjust these to stop swinging forever
        joint.massScale = manager.JointMassScale;
    }

    private void drawRope()
    {
        if (!joint) return;

        manager.GrappleLr.SetPosition(0, manager.Guntip.position);
        
    }

    private void AirControl()
    {
        float vertical = vertInput * manager.AirControlFwdForce;
        float horizontal = horiInput * manager.AirControlHorizontalForce;

        Vector3 TotalForceDir = (manager.Cam.transform.forward * vertical) + (manager.Cam.transform.right * horizontal);
        manager.rb.AddForce(TotalForceDir * Time.deltaTime, ForceMode.Force);
    }

    private void SwingDash()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !SwingDashed)
        {
            float dashForce = manager.rb.linearVelocity.magnitude * manager.SwingDashPower * currentDist * 0.001f;
            float initialVelocity = manager.rb.linearVelocity.magnitude;

            float newForce = dashForce + initialVelocity;

            newForce = Mathf.Clamp(newForce, manager.SwingDashMinPower, manager.SwingDashMaxPower);
            manager.rb.AddForce(manager.Cam.transform.forward.normalized * newForce, ForceMode.VelocityChange);
            Debug.Log("Dash for " + newForce);
            //joint.maxDistance *= 1.2f;
            SwingDashed = true;
        }
    }

    private void RecalibateJointMaxDistance()
    {
        joint.maxDistance = currentDist * 0.8f;
        joint.minDistance = currentDist * 0.25f;
    }
}