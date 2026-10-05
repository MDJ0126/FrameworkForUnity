using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// HitBox를 확장하여 공격의 프레임 사이 이동·회전 구간을 검사하고 궤적을 표시한다.
    /// </summary>
    [DefaultExecutionOrder(31000)]
    public class HitBoxSweep : HitBox
    {
        [Header("Attack Sweep")]
        [Tooltip("Attack의 프레임 사이를 검사한다. Box, Sphere, Capsule 및 Convex MeshCollider를 지원한다.")]
        public bool isAttackSweepEnabled = true;

        [Tooltip("이동과 회전으로 움직이는 끝점의 검사 간격(월드 단위). 작을수록 검사가 촘촘해진다.")]
        [Min(0.001f)] public float sweepStepDistance = 0.05f;

        [Tooltip("프레임당 최대 분할 수. 한도를 넘는 이동은 검사 간격이 커진다.")]
        [Range(1, 256)] public int maxSweepSteps = 64;

        [Tooltip("중간 검사 영역의 유지 시간(게임 시간, 초). 0이면 한 프레임 표시한다.")]
        [Min(0f)] public float sweepDebugDuration = 0.5f;

        public Color sweepDebugColor = Color.yellow;
        public Color sweepDebugHitColor = Color.green;

        private Vector3 _previousPosition;
        private Quaternion _previousRotation;
        private Vector3 _previousScale;
        private bool _hasPreviousPose;
        private Collider[] _sweepResults = new Collider[32];
        private readonly HashSet<Collider> _sweepContacts = new();
        private readonly HashSet<Collider> _frameSweepContacts = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            ResetAttackSweep();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _hasPreviousPose = false;
            _sweepContacts.Clear();
            _frameSweepContacts.Clear();
        }

        protected override bool HasContact(Collider other)
        {
            return base.HasContact(other) || _sweepContacts.Contains(other);
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (!_sweepContacts.Contains(other))
            {
                base.OnTriggerExit(other);
            }
        }

        /// <summary>
        /// 공격 시작이나 순간이동 후 이전 이동 구간과 접촉 이력을 초기화한다.
        /// </summary>
        public void ResetAttackSweep()
        {
            _hasPreviousPose = false;
            Contacts.Clear();
            _sweepContacts.Clear();
            _frameSweepContacts.Clear();
            if (HitCollider != null && HitCollider.enabled)
            {
                SaveSweepPose();
            }
        }

        /// <summary>
        /// 애니메이션 갱신 후 이동과 회전을 분할하여 실제 콜라이더의 겹침을 확인한다.
        /// </summary>
        private void LateUpdate()
        {
            if (Role != eHitBoxRole.Attack || !isAttackSweepEnabled || HitCollider == null || !HitCollider.enabled)
            {
                _hasPreviousPose = false;
                _sweepContacts.Clear();
                return;
            }
            if (!_hasPreviousPose || _previousScale != Transform.lossyScale)
            {
                ResetAttackSweep();
            }
            if (!TryGetSweepBounds(out Vector3 localCenter, out Vector3 halfExtents))
            {
                _hasPreviousPose = false;
                return;
            }

            Vector3 position = Transform.position;
            Quaternion rotation = Transform.rotation;
            Vector3 scale = Transform.lossyScale;
            float reach = Vector3.Scale(localCenter, scale).magnitude + halfExtents.magnitude;
            float travel = Vector3.Distance(_previousPosition, position) + Quaternion.Angle(_previousRotation, rotation) * Mathf.Deg2Rad * reach;
            float spacing = float.IsNaN(sweepStepDistance) || float.IsInfinity(sweepStepDistance) ? 0.05f : Mathf.Max(0.001f, sweepStepDistance);
            int steps = Mathf.Clamp(Mathf.CeilToInt(travel / spacing), 1, Mathf.Clamp(maxSweepSteps, 1, 256));
            bool hasMoved = travel > 0.00001f;
            _frameSweepContacts.Clear();
            Physics.SyncTransforms();
            for (int step = 0; step <= steps; step++)
            {
                float t = (float)step / steps;
                Vector3 samplePosition = Vector3.Lerp(_previousPosition, position, t);
                Quaternion sampleRotation = Quaternion.Slerp(_previousRotation, rotation, t);
                Vector3 center = samplePosition + sampleRotation * Vector3.Scale(localCenter, scale);
                int count;
                do
                {
                    count = Physics.OverlapBoxNonAlloc(center, halfExtents, _sweepResults, sampleRotation, Physics.AllLayers, QueryTriggerInteraction.Collide);
                    if (count < _sweepResults.Length)
                    {
                        break;
                    }
                    System.Array.Resize(ref _sweepResults, _sweepResults.Length * 2);
                }
                while (true);

                bool hasHit = false;
                for (int i = 0; i < count; i++)
                {
                    Collider other = _sweepResults[i];
                    if (!CanHit(other, out _) || !Physics.ComputePenetration(HitCollider, samplePosition, sampleRotation, other, other.transform.position, other.transform.rotation, out _, out _))
                    {
                        continue;
                    }
                    hasHit = true;
                    _frameSweepContacts.Add(other);
                    ProcessHit(other, other.ClosestPoint(center));
                    if (!isActiveAndEnabled || !HitCollider.enabled)
                    {
                        _hasPreviousPose = false;
                        return;
                    }
                }
                if (hasMoved || step == steps)
                {
                    DrawSweepDebug(center, halfExtents, sampleRotation, hasHit);
                }
            }
            // 이전 구간에서만 접촉한 대상도 다음 비접촉 프레임에서 해제한다.
            Contacts.RemoveAll(collider => !_frameSweepContacts.Contains(collider));
            _sweepContacts.Clear();
            _sweepContacts.UnionWith(_frameSweepContacts);
            SaveSweepPose();
        }

        /// <summary>
        /// 다음 프레임의 보간 시작 위치, 회전 및 스케일을 기록한다.
        /// </summary>
        private void SaveSweepPose()
        {
            _previousPosition = Transform.position;
            _previousRotation = Transform.rotation;
            _previousScale = Transform.lossyScale;
            _hasPreviousPose = true;
        }

        /// <summary>
        /// 콜라이더를 감싸는 회전된 박스를 후보 검색에 사용하고, 최종 겹침은 원래 형상으로 확인한다.
        /// </summary>
        private bool TryGetSweepBounds(out Vector3 localCenter, out Vector3 halfExtents)
        {
            Vector3 scale = Transform.lossyScale;
            Vector3 absoluteScale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            localCenter = Vector3.zero;
            halfExtents = Vector3.zero;
            if (HitCollider is BoxCollider box)
            {
                localCenter = box.center;
                halfExtents = Vector3.Scale(box.size * 0.5f, absoluteScale);
            }
            else if (HitCollider is MeshCollider mesh && mesh.convex && mesh.sharedMesh != null)
            {
                localCenter = mesh.sharedMesh.bounds.center;
                halfExtents = Vector3.Scale(mesh.sharedMesh.bounds.extents, absoluteScale);
            }
            else if (HitCollider is SphereCollider sphere)
            {
                localCenter = sphere.center;
                halfExtents = Vector3.one * sphere.radius * Mathf.Max(absoluteScale.x, absoluteScale.y, absoluteScale.z);
            }
            else if (HitCollider is CapsuleCollider capsule)
            {
                localCenter = capsule.center;
                float radiusScale = capsule.direction == 0 ? Mathf.Max(absoluteScale.y, absoluteScale.z)
                    : capsule.direction == 1 ? Mathf.Max(absoluteScale.x, absoluteScale.z) : Mathf.Max(absoluteScale.x, absoluteScale.y);
                halfExtents = Vector3.one * capsule.radius * radiusScale;
                halfExtents[capsule.direction] = Mathf.Max(halfExtents[capsule.direction], capsule.height * absoluteScale[capsule.direction] * 0.5f);
            }
            else
            {
                return false;
            }
            halfExtents = Vector3.Max(halfExtents, Vector3.one * 0.0001f);
            return true;
        }

        /// <summary>
        /// 중간 위치에서 후보 검색에 사용한 박스를 게임 화면에 남기고 실제 접촉 여부를 색으로 표시한다.
        /// </summary>
        private void DrawSweepDebug(Vector3 center, Vector3 halfExtents, Quaternion rotation, bool hasHit)
        {
            if (!(isHitDebugEnabled || GameConfig.IsCollisionDebugEnabled))
            {
                return;
            }
            float duration = float.IsNaN(sweepDebugDuration) || float.IsInfinity(sweepDebugDuration) ? 0f : Mathf.Max(0f, sweepDebugDuration);
            Color color = hasHit ? sweepDebugHitColor : sweepDebugColor;
            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 start = center + rotation * GetSweepCorner(corner, halfExtents);
                for (int axis = 1; axis <= 4; axis <<= 1)
                {
                    if ((corner & axis) != 0)
                    {
                        continue;
                    }
                    Vector3 end = center + rotation * GetSweepCorner(corner | axis, halfExtents);
                    PhysicsQueryDebugRenderer.DrawLine(start, end, color, duration, isHitDebugDepthTest);
                }
            }
        }

        /// <summary>
        /// 배열을 생성하지 않고 비트의 부호로 박스 꼭짓점을 계산한다.
        /// </summary>
        private static Vector3 GetSweepCorner(int corner, Vector3 halfExtents)
        {
            return new Vector3((corner & 1) == 0 ? -halfExtents.x : halfExtents.x, (corner & 2) == 0 ? -halfExtents.y : halfExtents.y, (corner & 4) == 0 ? -halfExtents.z : halfExtents.z);
        }

    }
}
