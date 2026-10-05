using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 공격·피격 HitBox의 Trigger 충돌을 처리하고 감지 지점을 표시한다.
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

        protected Collider HitCollider { get; private set; }
        protected List<Collider> Contacts => _colliders;

        protected virtual void Awake()
        {
            Owner = GetComponentInParent<Pawn>();
            HitCollider = GetComponent<Collider>();
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

        protected virtual void OnEnable()
        {
            _colliders.Clear();
        }

        protected virtual void OnDisable()
        {
            _colliders.Clear();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (HitCollider == null || !HitCollider.enabled || !isActiveAndEnabled)
            {
                return;
            }
            ProcessHit(other, other.ClosestPoint(HitCollider.bounds.center));
        }

        /// <summary>
        /// Trigger와 중간 검사의 제외 조건 및 접촉 중 중복 판정을 공유한다.
        /// </summary>
        protected void ProcessHit(Collider other, Vector3 point)
        {
            if (!CanHit(other, out HitBox hitBox) || HasContact(other))
            {
                return;
            }
            // 이벤트에서 판정을 꺼도 이력이 남지 않도록 호출 전에 등록한다.
            _colliders.Add(other);
            DrawHitDebug(point);
            _onHit?.Invoke(this, hitBox);
        }

        /// <summary>
        /// 같은 역할, 자신, 같은 소유자 및 명시된 제외 대상을 판정에서 제외한다.
        /// </summary>
        protected bool CanHit(Collider other, out HitBox hitBox)
        {
            hitBox = other != null ? other.GetComponent<HitBox>() : null;
            return hitBox != null && hitBox != this && hitBox.isActiveAndEnabled && hitBox.Role != Role
                && (Owner == null || hitBox.Owner != Owner) && !_ignores.Contains(other)
                && !Physics.GetIgnoreLayerCollision(gameObject.layer, other.gameObject.layer)
                && !Physics.GetIgnoreCollision(HitCollider, other);
        }

        /// <summary>
        /// 접촉 중인 대상의 중복 이벤트를 막고 파생 클래스의 추가 접촉 이력을 연결한다.
        /// </summary>
        protected virtual bool HasContact(Collider other)
        {
            return _colliders.Contains(other);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            _colliders.Remove(other);
        }

        #region ## DEBUG ##

        /// <summary>
        /// 유효한 HitBox 감지 위치를 게임 화면에 정육면체 표식으로 표시한다.
        /// </summary>
        private void DrawHitDebug(Vector3 point)
        {
            if (!(isHitDebugEnabled || GameConfig.IsCollisionDebugEnabled))
            {
                return;
            }

            // Trigger에는 접촉점 정보가 없으므로 상대 콜라이더에 대한 최근접 위치로 근사한다.
            // 중간 검사는 해당 시점의 위치에서 계산한 근사점을 전달한다.
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