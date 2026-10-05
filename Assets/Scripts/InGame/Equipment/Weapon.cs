namespace Game
{
    public abstract class Weapon : Equipment
    {
        public HitBox hitBox;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (hitBox)
            {
                hitBox.OnHit += OnHit;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (hitBox)
            {
                hitBox.OnHit -= OnHit;
            }
        }

        private void OnHit(HitBox myHitBox, HitBox otherHitBox)
        {
            if (otherHitBox.Role == HitBox.eHitBoxRole.Hurt)
            {
                otherHitBox.Owner.Damaged(Owner, new DamageInfo { damage = 10 });
            }
        }
    }
}