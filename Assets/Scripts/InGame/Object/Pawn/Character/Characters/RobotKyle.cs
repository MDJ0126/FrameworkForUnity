namespace Game
{
    public class RobotKyle : Character
    {
        #region Inspector

        public Socket leftHandleSocket;
        public Socket rightHandleSocket;

        #endregion
        public override string Name => nameof(RobotKyle);

        public RobotKyleAnimationController RobotKyleAnimationController { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            RobotKyleAnimationController = GetComponentInChildren<RobotKyleAnimationController>();
        }

        protected override void Start()
        {
            base.Start();
            SkillManager.AddSkill(new RoarSkill());
        }

        protected override void Initalize()
        {
            Status status = StatusTable.Instance.GetData(0);
            StatusInfo.baseStatus += status;
            StatusInfo.Initialize();
        }

        protected override void DamagerProcess(Pawn attacker, DamageInfo damageInfo)
        {
            this.StatusInfo.Damaged(damageInfo.damage);
            RobotKyleAnimationController.OnHitted();
        }
    }
}