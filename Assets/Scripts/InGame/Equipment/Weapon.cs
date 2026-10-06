using System.Collections.Generic;

namespace Game
{
    public abstract class Weapon : Equipment
    {
        public HitBox hitBox;
        private List<Pawn> _hittedPawns = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            if (hitBox)
            {
                hitBox.OnHit += OnHit;
            }
            hitBox.enabled = false;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (hitBox)
            {
                hitBox.OnHit -= OnHit;
            }
        }

        public virtual void StartAttack()
        {
            _hittedPawns.Clear();
            hitBox.enabled = true;
        }

        public virtual void EndAttack()
        {
            hitBox.enabled = false;
        }

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