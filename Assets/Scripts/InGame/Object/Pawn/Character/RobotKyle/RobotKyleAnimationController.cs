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
            boneAnimator.SetTrigger("Attack");
            _roboyKlye.Movement.isMoveEnable = false;

            if (_roboyKlye.Movement.IsGrounded)
            {
                _roboyKlye.Movement.StopMove();
            }
        }

        /// <summary>
        /// 공격 애니메이션 종료 이벤트
        /// </summary>
        private void OnAttackAnimationEnd()
        {
            IsAttacking = false;
            _roboyKlye.Movement.isMoveEnable = true;
        }
    }
}