using System.Collections.Generic;

namespace Game
{
    public class SkillManager
    {
        private Pawn _owner = null;

        public List<Skill> Skills { get; private set; } = new();

        /// <summary>
        /// 관리 중인 스킬이 사용할 소유 Pawn 설정
        /// </summary>
        public void SetOwner(Pawn owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// 등록된 모든 스킬의 시간 기반 로직 갱신
        /// </summary>
        public void UpdateTick(float deltaTime)
        {
            foreach (Skill skill in Skills)
            {
                skill.UpdateTick(deltaTime);
            }
        }

        /// <summary>
        /// 스킬 추가
        /// </summary>
        /// <param name="skill"></param>
        public void AddSkill(Skill skill)
        {
            skill.SetOwner(_owner);
            Skills.Add(skill);
        }

        /// <summary>
        /// 스킬 삭제
        /// </summary>
        public void RemoveAtSkill(int index)
        {
            Skills.RemoveAt(index);
        }

        /// <summary>
        /// 스킬 사용
        /// </summary>
        /// <param name="index"></param>
        public void UseSkill(int index)
        {
            Skill skill = GetSkill(index);
            if (skill != null)
            {
                skill.Execute();
            }
        }

        /// <summary>
        /// 스킬 가져오기
        /// </summary>
        /// <param name="index">스킬 인덱스</param>
        /// <returns></returns>
        public Skill GetSkill(int index)
        {
            // 요청 인덱스가 목록 범위 안일 때만 스킬을 반환한다.
            if (Skills.Count > index)
            {
                return Skills[index];
            }
            else
            {
                return null;
            }
        }
    }
}
