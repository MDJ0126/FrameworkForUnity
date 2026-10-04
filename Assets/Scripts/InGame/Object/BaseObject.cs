using UnityEngine;

namespace Game
{
    public abstract class BaseObject : MonoBehaviour
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
