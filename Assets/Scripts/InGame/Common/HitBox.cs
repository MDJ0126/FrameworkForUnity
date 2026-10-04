using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 충돌 판정 확인 클래스
    /// </summary>
    public class HitBox : MonoBehaviour
    {
        public delegate void OnHitEvent(HitBox otherHitBox);
        private event OnHitEvent _onHit = null;
        public event OnHitEvent OnHit
        {
            add 
            {
                _onHit -= value;
                _onHit += value;
            }
            remove 
            {
                _onHit -= value; 
            }
        }

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

        /// <summary>
        /// 무시할 콜라이더 리스트
        /// </summary>
        private List<Collider> _ignores = new();

        /// <summary>
        /// 확인된 콜라이더 리스트
        /// </summary>
        private List<Collider> _colliders = new();

        /// <summary>
        /// 무시할 콜라이더 추가
        /// </summary>
        /// <param name="colliders"></param>
        public void AddIgnoreCollider(params Collider[] colliders)
        {
            foreach (Collider collider in colliders)
            {
                _ignores.Add(collider);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HitBox hitBox = other.GetComponent<HitBox>();
            if (hitBox == null)
            {
                return;
            }

            if (_ignores.Exists(c => c.Equals(other)))
            {
                return;
            }

            if (!_colliders.Exists(c => c.Equals(other)))
            {
                _onHit?.Invoke(hitBox);
                _colliders.Add(other);
            }
        }

        //private void OnTriggerStay(Collider other)
        //{

        //}

        private void OnTriggerExit(Collider other)
        {
            _colliders.Remove(other);
        }
    }
}