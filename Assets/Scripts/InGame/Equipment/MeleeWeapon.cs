namespace Game
{
    /// <summary>
    /// 근접 무기 클래스
    /// </summary>
    public abstract class MeleeWeapon : Weapon
    {
        public WeaponRibbon weaponRibbon;

        /// <summary>
        /// 데미지 판정과 함께 새로운 검기 궤적을 시작한다.
        /// </summary>
        public override void StartAttack()
        {
            base.StartAttack();
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
        }
    }
}
