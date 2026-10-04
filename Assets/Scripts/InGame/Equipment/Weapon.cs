using UnityEngine;

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

        private void OnHit(HitBox otherHitBox)
        {
            Debug.Log(otherHitBox);
        }
    }
}