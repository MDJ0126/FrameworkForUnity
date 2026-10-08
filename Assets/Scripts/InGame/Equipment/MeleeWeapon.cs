using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// 히트박스 피해 판정과 검기 궤적을 관리한다.
    /// </summary>
    public abstract class MeleeWeapon : Weapon
    {
        public HitBox hitBox;
        public WeaponRibbon weaponRibbon;
        private List<Pawn> _hittedPawns = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            if (hitBox)
            {
                hitBox.OnHit += OnHit;
                hitBox.enabled = false;
            }
        }

        /// <summary>
        /// 데미지 판정과 함께 새로운 검기 궤적을 시작한다.
        /// </summary>
        public override void StartAttack()
        {
            base.StartAttack();
            _hittedPawns.Clear();
            if (hitBox)
            {
                hitBox.enabled = true;
            }
            if (weaponRibbon)
            {
                weaponRibbon.BeginTrail();
            }
        }

        /// <summary>
        /// 판정과 검기 생성을 종료하고 남은 궤적은 자연스럽게 소멸하게 한다.
        /// </summary>
        public override void EndAttack()
        {
            base.EndAttack();
            if (hitBox)
            {
                hitBox.enabled = false;
            }
            if (weaponRibbon)
            {
                weaponRibbon.EndTrail();
            }
        }

        protected override void OnDisable()
        {
            if (weaponRibbon)
            {
                weaponRibbon.ClearTrail();
            }
            base.OnDisable();
            if (hitBox)
            {
                hitBox.OnHit -= OnHit;
            }
        }

        /// <summary>
        /// 한 공격에서 처음 맞은 피격 대상에만 피해를 적용한다.
        /// </summary>
        private void OnHit(HitBox myHitBox, HitBox otherHitBox)
        {
            if (otherHitBox.Role == HitBox.eHitBoxRole.Hurt)
            {
                if (!_hittedPawns.Contains(otherHitBox.Owner))
                {
                    _hittedPawns.Add(otherHitBox.Owner);
                    otherHitBox.Owner.Damaged(Owner, new DamageInfo { damage = Owner.StatusInfo.baseStatus.damage });
                }
            }
        }
    }
}
