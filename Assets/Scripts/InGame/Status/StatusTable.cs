using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    /// <summary>
    /// 인스펙터에서 설정한 스테이터스 목록을 보관한다.
    /// </summary>
    [CreateAssetMenu(fileName = "StatusTable", menuName = "Game/StatusTable")]
    public class StatusTable : SingletonScriptableObject<StatusTable>
    {
        public override string ADDRESS => "Assets/AddressablesResources/Datas/StatusTable.asset";

        [FormerlySerializedAs("_statuses")]
        [SerializeField] private List<Status> _datas = new List<Status>();

        public IReadOnlyList<Status> Datas => _datas;

        /// <summary>
        /// 지정한 인덱스의 스테이터스를 반환하며 범위를 벗어나면 기본값을 반환한다.
        /// </summary>
        public Status GetData(int index)
        {
            if (index >= 0 && index < Datas.Count)
            {
                return Datas[index];
            }
            return default;
        }
    }
}
