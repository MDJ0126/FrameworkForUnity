using UnityEngine;

namespace Game
{
    public class RobotKyleAnimationController : CharacterAnimationController
    {
        private RobotKyle _roboyKlye;

        public bool IsAttacking { get; private set; } = false;

        protected override void Awake()
        {
            base.Awake();
            _roboyKlye = GetComponentInParent<RobotKyle>();
        }

        /// <summary>
        /// 공격 시작
        /// </summary>
        public void StartAttack()
        {
            if (IsAttacking) return;

            IsAttacking = true;
            boneAnimator.SetTrigger(AnimHash.Attack);
            _roboyKlye.Movement.isMoveEnable = false;

            if (_roboyKlye.Movement.IsGrounded)
            {
                _roboyKlye.Movement.StopMove();
            }
        }

        /// <summary>
        /// 공격 처리
        /// </summary>
        private void OnTakeDamage()
        {

        }

        /// <summary>
        /// 공격 애니메이션 종료 이벤트
        /// </summary>
        private void OnAttackAnimationEnd()
        {
            IsAttacking = false;
            _roboyKlye.Movement.isMoveEnable = true;
        }

        /// <summary>
        /// 피격 받음
        /// </summary>
        public void OnHitted()
        {
            boneAnimator.SetTrigger(AnimHash.Hit);
        }
    }
}