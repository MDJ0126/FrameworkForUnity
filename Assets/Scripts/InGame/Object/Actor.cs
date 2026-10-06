using UnityEngine;

namespace Game
{
    /// <summary>
    /// 레벨(월드)에 배치할 수 있는 가장 기본이 되는 오브젝트 (유니티의 GameObject와 비슷한 개념)
    /// </summary>
    public abstract class Actor : MonoBehaviour
    {
        private static int _createIndex = 0;
        public int Index { get; private set; } = 0;

        private Transform _transform;

        public Transform Transform
        {
            get
            {
                if (_transform == null)
                    _transform = transform;
                return _transform;
            }
        }

        protected virtual void Awake() 
        {
            Index = _createIndex++;
        }
        protected virtual void Start() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void Update() { }
        protected virtual void LateUpdate() { }
    }
}
