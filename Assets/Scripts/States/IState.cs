using UnityEngine;
namespace PlatformerGame
{
    public interface IState
    {
        IState UpdateState(Animator animator,NetworkPlayer player, PlayerStat stats);
        void Jump();
        void DoubleJump();
    }
}