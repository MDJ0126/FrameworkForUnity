using Game;
using UnityEngine;

/// <summary>
/// 폰 애니메이션 상태의 진입, 갱신, 종료를 처리한다.
/// </summary>
public abstract class PawnStateBehaviour : StateMachineBehaviour
{
    protected Pawn owner = null;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (owner == null)
        {
            owner = animator.GetComponentInParent<Pawn>();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

    }
}