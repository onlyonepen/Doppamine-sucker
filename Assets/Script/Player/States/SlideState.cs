using UnityEngine;

public class SlideState : PlayerState
{

    Vector3 SlideDir;

    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);
        manager.PBM.playerCanMove = false;

        //Transform child = manager.transform.GetChild(0);
        //child.SetParent(null); // Unparent
        manager.transform.localScale = new Vector3(1, .5f, 1); // Scale parent
        //child.SetParent(manager.transform);

        manager.GuntipDefault();
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        manager.GrapplePrediction();

        SlideLogic();

        if (Input.GetMouseButtonDown(1)) manager.ChangeState(manager.ThrowGrappleState);
    }

    public override void OnStatePhysicsUpdate()
    {
        base.OnStatePhysicsUpdate();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        //Transform child = manager.transform.getc(0);
        //child.SetParent(null); // Unparent
        manager.transform.localScale = Vector3.one; // Scale parent
        //child.SetParent(manager.transform);
    }

    private void SlideLogic()
    {
        SlideDir = new Vector3(manager.GroundNormal().x, 0, manager.GroundNormal().z);

        float slopeAngle = Vector3.Angle(manager.GroundNormal(), Vector3.up);
        float speed = Physics.gravity.magnitude * slopeAngle * manager.SlideSpeedMult;

        SlideDir = Quaternion.AngleAxis(slopeAngle, manager.transform.right) * SlideDir;
        manager.rb.AddForce(speed * SlideDir);


        float strafeOffset = Input.GetKey(KeyCode.D) ? 10 : (Input.GetKey(KeyCode.A) ? -10 : 0);

        float targetYRotation = manager.transform.eulerAngles.y + strafeOffset;
        Quaternion targetRotation = Quaternion.Euler(0, targetYRotation, 0);

        float verticalSpeed = manager.rb.linearVelocity.y;
        Vector3 horizontalVelocity = new Vector3(manager.rb.linearVelocity.x, 0, manager.rb.linearVelocity.z);
        float horizontalMagnitude = horizontalVelocity.magnitude;

        Vector3 newHorizontalDir = targetRotation * Vector3.forward;
        manager.rb.linearVelocity = (newHorizontalDir * horizontalMagnitude) + (Vector3.up * verticalSpeed);

        //Friction
        manager.rb.AddForce(-manager.rb.linearVelocity * (1 - manager.SlideFriction));


        if (Input.GetKeyUp(KeyCode.LeftControl) || manager.rb.linearVelocity.magnitude <= manager.SlideSpeedTreshold || Input.GetKeyDown(KeyCode.Space))
        {
            if (Input.GetKeyDown(KeyCode.Space)) manager.PBM.Jump();
            manager.ChangeState(manager.BaseState);
        }
    }
}
