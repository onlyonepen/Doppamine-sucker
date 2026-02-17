public class PlayerBaseState : PlayerState
{
    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);

        manager.PBM.enabled = true;
    }
}
