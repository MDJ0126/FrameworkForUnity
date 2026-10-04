namespace Game
{
    public class StatusInfo
    {
        public delegate void OnChanegdData(StatusInfo statusInfo);
        private event OnChanegdData _onChangedHp;
        public event OnChanegdData OnChangedHp
        {
            add
            {
                _onChangedHp -= value;
                _onChangedHp += value;
            }
            remove
            {
                _onChangedHp -= value;
            }
        }

        public Status baseStatus;
        public int hp = 0;
        public float HpRatio => (float)hp / baseStatus.maxHp;
        public bool IsDead => hp <= 0;

        /// <summary>
        /// 현재 체력을 기본 스테이터스의 최대 체력으로 초기화
        /// </summary>
        public void Initialize()
        {
            hp = baseStatus.maxHp;
        }

        /// <summary>
        /// 체력 변경 이벤트 호출 (현재 구현에서는 hp 필드를 직접 대입하지 않음)
        /// </summary>
        public void SetHp(int hp)
        {
            this.hp = hp;
            _onChangedHp?.Invoke(this);
        }

        /// <summary>
        /// 현재 체력에 지정 값을 더한 결과로 SetHp 호출
        /// </summary>
        public void AddHp(int addHp)
        {
            SetHp(hp + addHp);
        }

        /// <summary>
        /// 현재 체력에 전달받은 damage를 더한 결과로 SetHp 호출
        /// </summary>
        public void Damaged(int damage)
        {
            SetHp(hp - damage);
        }
    }
}
