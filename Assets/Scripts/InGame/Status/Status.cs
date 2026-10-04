namespace Game
{
    [System.Serializable]
    public struct Status
    {
        public int damage;
        public int defence;
        public int maxHp;

        /// <summary>
        /// 두 스테이터스의 공격력, 방어력, 최대 체력을 항목별로 더하기
        /// </summary>
        public static Status operator +(Status a, Status b)
        {
            // 값 형식인 a의 복사본을 변경하여 합산 결과로 반환한다.
            a.damage += b.damage;
            a.defence += b.defence;
            a.maxHp += b.maxHp;
            return a;
        }

        /// <summary>
        /// 왼쪽 스테이터스에서 오른쪽 스테이터스의 각 항목 빼기
        /// </summary>
        public static Status operator -(Status a, Status b)
        {
            // 값 형식인 a의 복사본을 변경하여 차감 결과로 반환한다.
            a.damage -= b.damage;
            a.defence -= b.defence;
            a.maxHp -= b.maxHp;
            return a;
        }
    }
}
