namespace Game
{
    public abstract class Weapon : Equipment
    {
        /// <summary>
        /// 무기별 공격 처리를 시작한다.
        /// </summary>
        public virtual void StartAttack()
        {
        }

        /// <summary>
        /// 무기별 공격 처리를 종료한다.
        /// </summary>
        public virtual void EndAttack()
        {
        }
    }
}
