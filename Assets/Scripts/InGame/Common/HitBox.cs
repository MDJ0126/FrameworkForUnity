using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 충돌 판정 확인 클래스
    /// </summary>
    public class HitBox : MonoBehaviour
    {
        public delegate void OnHitEvent(HitBox myHitBox, HitBox otherHitBox);
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

        /// <summary>
        /// 충돌 판정 타입
        /// </summary>
        public enum eHitBoxRole
        {
            /// <summary>
            /// 피격 판정용
            /// </summary>
            Hurt,
            /// <summary>
            /// 공격 판정용
            /// </summary>
            Attack,
        }

        #region Inspector

        /// <summary>
        /// 판정 타입
        /// </summary>
        public eHitBoxRole Role = eHitBoxRole.Hurt;

        [Header("Debug")]
        public bool isHitDebugEnabled = false;
        public Color hitDebugColor = Color.yellow;

        [Tooltip("표시 유지 시간(게임 시간, 초). 0이면 한 프레임 표시한다.")]
        [Min(0f)] public float hitDebugDuration = 2f;

        [Tooltip("근사 감지 위치에 표시할 정육면체 표식의 전체 크기(월드 단위).")]
        [Min(0.01f)] public float hitDebugSize = 0.2f;

        public bool isHitDebugDepthTest = false;

        #endregion

        public Pawn Owner { get; private set; } = null;

        private Transform _transform;

        public Transform Transform
        {
            get
            {
                if (_transform == null)
                {
                    _transform = transform;
                }
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

        private void Awake()
        {
            Owner = GetComponentInParent<Pawn>();
        }

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

            if (hitBox.Role == Role)
            {
                return;
            }

            if (_ignores.Exists(c => c.Equals(other)))
            {
                return;
            }

            if (!_colliders.Exists(c => c.Equals(other)))
            {
                DrawHitDebug(other);
                _onHit?.Invoke(this, hitBox);
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

        #region ## DEBUG ##

        private Collider _debugCollider;

        /// <summary>
        /// 유효한 HitBox 감지 위치를 게임 화면에 정육면체 표식으로 표시한다.
        /// </summary>
        private void DrawHitDebug(Collider other)
        {
            if (!(isHitDebugEnabled || GameConfig.IsCollisionDebugEnabled))
            {
                return;
            }

            if (_debugCollider == null)
            {
                _debugCollider = GetComponent<Collider>();
            }

            // Trigger에는 접촉점 정보가 없으므로 상대 콜라이더에 대한 최근접 위치로 근사한다.
            Vector3 origin = _debugCollider != null ? _debugCollider.bounds.center : Transform.position;
            Vector3 point = other.ClosestPoint(origin);
            float duration = hitDebugDuration;
            if (float.IsNaN(duration) || float.IsInfinity(duration) || duration < 0f)
            {
                duration = 0f;
            }

            float size = hitDebugSize;
            if (float.IsNaN(size) || float.IsInfinity(size) || size <= 0f)
            {
                size = 0.2f;
            }

            PhysicsQueryDebugRenderer.DrawCube(point, size, hitDebugColor, duration, isHitDebugDepthTest);
        }

        #endregion
    }
}