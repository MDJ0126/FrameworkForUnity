namespace Game
{
    public class HealBuff : Buff
    {
        /// <summary>
        /// 버프 시간을 갱신하고 소유 Pawn의 체력 10 회복
        /// </summary>
        public override void UpdateTick(float deltaTime)
        {
            base.UpdateTick(deltaTime);
            owner.StatusInfo.hp += 10;
        }
    }
}
