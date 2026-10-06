using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 액터를 상속받으며, 플레이어나 AI가 조종(빙의)할 수 있는 액터
    /// </summary>
    public abstract class Pawn : Actor
    {
        public delegate void OnDamagedEvent(Pawn attacker, Pawn target);
        private event OnDamagedEvent _onDamaged = null;
        public event OnDamagedEvent OnDamaged
        {
            add
            {
                _onDamaged -= value;
                _onDamaged += value;
            }
            remove
            {
                _onDamaged -= value;
            }
        }

        #region Inspector

        public WidgetAnchor PawnInfoAnchor;
        public WidgetAnchor balloonAnchor;
        public List<Equipment> equipmentList;

        #endregion

        public virtual string Name => nameof(Pawn);
        public StatusInfo StatusInfo { get; private set; } = new();
        public SkillManager SkillManager { get; private set; } = new();
        public BuffManager BuffManager { get; private set; } = new();
        public Movement Movement { get; private set; }
        public PawnAnimationController PawnAnimationController { get; private set; }
        protected List<FollowHUD> followHUDs = new();
        public Weapon EquippedWeapon { get; private set; } = null;

        protected override void Awake()
        {
            base.Awake();
            Movement = GetComponent<Movement>();
            PawnAnimationController = GetComponentInChildren<PawnAnimationController>();
            if (equipmentList != null)
            {
                foreach (Equipment equipment in equipmentList)
                {
                    equipment.SetOwner(this);
                    if (equipment is Weapon weapon)
                    {
                        EquippedWeapon = weapon;
                    }
                }
            }
            Initalize();
        }

        protected override void Start()
        {
            base.Start();
            SkillManager.SetOwner(this);
            BuffManager.SetOwner(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            followHUDs.Add(HUDManager.Instance.AttachFollowPawnInfo(this));
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            if (HUDManager.IsLive)
            {
                HUDManager.Instance.DetachFollowHUD(followHUDs);
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            SkillManager.UpdateTick(Time.deltaTime);
            BuffManager.UpdateTick(Time.deltaTime);
        }

        /// <summary>
        /// 최초 초기화
        /// </summary>
        protected abstract void Initalize();

        /// <summary>
        /// 플레이어 컨트롤러 빙의 시 파생 Pawn에서 사용할 처리 지점
        /// </summary>
        public virtual void Possess(PawnController pawnController)
        {

        }

        /// <summary>
        /// 플레이어 컨트롤러 빙의 해제 시 파생 Pawn에서 사용할 처리 지점
        /// </summary>
        public virtual void Unpossess(PawnController pawnController)
        {

        }

        /// <summary>
        /// 데미지를 받았을 때
        /// </summary>
        /// <param name="attacker">공격자</param>
        /// <param damageInfo="attacker">데미지 정보</param>
        public void Damaged(Pawn attacker, DamageInfo damageInfo)
        {
            DamagerProcess(attacker, damageInfo);
            _onDamaged?.Invoke(attacker, this);
        }

        /// <summary>
        /// 데미지 처리
        /// </summary>
        /// <param name="attacker">공격자</param>
        /// <param damageInfo="attacker">데미지 정보</param>
        protected abstract void DamagerProcess(Pawn attacker, DamageInfo damageInfo);
    }
}
