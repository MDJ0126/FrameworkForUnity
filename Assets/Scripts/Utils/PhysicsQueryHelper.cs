using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 월드 좌표의 구, 박스, 캡슐 영역과 겹치는 콜라이더를 조회한다.
/// </summary>
public static class PhysicsQueryHelper
{
    /// <summary>
    /// 조회 영역의 디버그 표시 수명을 구분한다.
    /// </summary>
    public enum eDebugDrawMode
    {
        None = 0,
        ForOneFrame = 1,
        ForDuration = 2,
    }

    /// <summary>
    /// 조회 영역의 표시 수명, 결과별 색상과 선의 정밀도를 설정한다.
    /// </summary>
    [Serializable]
    public class PhysicsQueryDebug
    {
        public eDebugDrawMode drawMode = eDebugDrawMode.ForOneFrame;

        [Tooltip("ForDuration 모드의 게임 시간(초). 0이면 한 프레임 표시한다.")]
        [Min(0f)] public float duration = 2f;

        public Color color = Color.red;
        public Color hitColor = Color.green;

        [Tooltip("다른 물체에 가려진 선을 숨길지 설정한다.")]
        public bool depthTest = false;

        [Tooltip("원 하나를 구성하는 선의 개수. 그릴 때 8~128로 제한한다.")]
        [Range(8, 128)] public int segments = 32;
    }

    private static readonly PhysicsQueryDebug _defaultDebug = new();

    /// <summary>
    /// 구 영역과 겹치는 콜라이더를 새 배열로 반환한다.
    /// </summary>
    /// <param name="ignoreObjects">제외할 오브젝트 목록. 각 오브젝트와 모든 자식의 콜라이더를 제외하며, null 항목은 건너뛴다.</param>
    /// <param name="debug">표시 설정. 전역 디버그가 켜져 있으면 null은 기본 설정으로 표시하며, None도 한 프레임 표시한다. 호출 시점의 영역을 표시한다.</param>
    public static Collider[] OverlapSphere(Vector3 center, float radius, int layerMask = Physics.AllLayers, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, IReadOnlyList<GameObject> ignoreObjects = null, PhysicsQueryDebug debug = null)
    {
        Collider[] result = Physics.OverlapSphere(center, radius, layerMask, triggerInteraction);
        int count = FilterResults(result, result.Length, ignoreObjects);
        if (count != result.Length) Array.Resize(ref result, count);
        DrawSphere(center, radius, debug, result.Length > 0);
        return result;
    }

    /// <summary>
    /// 구 영역과 겹치는 콜라이더를 재사용 버퍼에 기록하고 개수를 반환한다.
    /// </summary>
    /// <param name="results">결과 버퍼. 반환 개수 미만의 인덱스만 유효하며, 제외 대상도 조회 버퍼 공간을 차지하므로, 제외 후 개수가 작아도 일부 결과가 누락될 수 있다.</param>
    /// <param name="ignoreObjects">제외할 오브젝트 목록. 각 오브젝트와 모든 자식의 콜라이더를 제외하며, null 항목은 건너뛴다.</param>
    /// <param name="debug">표시 설정. 전역 디버그가 켜져 있으면 null은 기본 설정으로 표시하며, None도 한 프레임 표시한다. 호출 시점의 영역을 표시한다.</param>
    public static int OverlapSphereNonAlloc(Vector3 center, float radius, Collider[] results, int layerMask = Physics.AllLayers, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, IReadOnlyList<GameObject> ignoreObjects = null, PhysicsQueryDebug debug = null)
    {
        ValidateResults(results);
        int result = Physics.OverlapSphereNonAlloc(center, radius, results, layerMask, triggerInteraction);
        result = FilterResults(results, result, ignoreObjects);
        DrawSphere(center, radius, debug, result > 0);
        return result;
    }

    /// <summary>
    /// 박스 영역과 겹치는 콜라이더를 새 배열로 반환한다.
    /// </summary>
    /// <param name="size">회전 전 각 축의 전체 크기. Transform의 스케일은 자동 적용하지 않는다.</param>
    /// <param name="rotation">박스의 월드 회전. 생략하면 기본 회전을 사용한다.</param>
    /// <param name="ignoreObjects">제외할 오브젝트 목록. 각 오브젝트와 모든 자식의 콜라이더를 제외하며, null 항목은 건너뛴다.</param>
    /// <param name="debug">표시 설정. 전역 디버그가 켜져 있으면 null은 기본 설정으로 표시하며, None도 한 프레임 표시한다. 호출 시점의 영역을 표시한다.</param>
    public static Collider[] OverlapBox(Vector3 center, Vector3 size, Quaternion? rotation = null, int layerMask = Physics.AllLayers, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, IReadOnlyList<GameObject> ignoreObjects = null, PhysicsQueryDebug debug = null)
    {
        Collider[] result = Physics.OverlapBox(center, size * 0.5f, rotation ?? Quaternion.identity, layerMask, triggerInteraction);
        int count = FilterResults(result, result.Length, ignoreObjects);
        if (count != result.Length) Array.Resize(ref result, count);
        DrawBox(center, size, rotation ?? Quaternion.identity, debug, result.Length > 0);
        return result;
    }

    /// <summary>
    /// 박스 영역과 겹치는 콜라이더를 재사용 버퍼에 기록하고 개수를 반환한다.
    /// </summary>
    /// <param name="size">회전 전 각 축의 전체 크기. Transform의 스케일은 자동 적용하지 않는다.</param>
    /// <param name="results">결과 버퍼. 반환 개수 미만의 인덱스만 유효하며, 제외 대상도 조회 버퍼 공간을 차지하므로, 제외 후 개수가 작아도 일부 결과가 누락될 수 있다.</param>
    /// <param name="rotation">박스의 월드 회전. 생략하면 기본 회전을 사용한다.</param>
    /// <param name="ignoreObjects">제외할 오브젝트 목록. 각 오브젝트와 모든 자식의 콜라이더를 제외하며, null 항목은 건너뛴다.</param>
    /// <param name="debug">표시 설정. 전역 디버그가 켜져 있으면 null은 기본 설정으로 표시하며, None도 한 프레임 표시한다. 호출 시점의 영역을 표시한다.</param>
    public static int OverlapBoxNonAlloc(Vector3 center, Vector3 size, Collider[] results, Quaternion? rotation = null, int layerMask = Physics.AllLayers, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, IReadOnlyList<GameObject> ignoreObjects = null, PhysicsQueryDebug debug = null)
    {
        ValidateResults(results);
        int result = Physics.OverlapBoxNonAlloc(center, size * 0.5f, results, rotation ?? Quaternion.identity, layerMask, triggerInteraction);
        result = FilterResults(results, result, ignoreObjects);
        DrawBox(center, size, rotation ?? Quaternion.identity, debug, result > 0);
        return result;
    }

    /// <summary>
    /// 캡슐 영역과 겹치는 콜라이더를 새 배열로 반환한다.
    /// </summary>
    /// <param name="point0">한쪽 끝 구의 월드 중심. 캡슐 표면의 끝점이 아니다.</param>
    /// <param name="point1">다른 쪽 끝 구의 월드 중심. 캡슐 표면의 끝점이 아니다.</param>
    /// <param name="ignoreObjects">제외할 오브젝트 목록. 각 오브젝트와 모든 자식의 콜라이더를 제외하며, null 항목은 건너뛴다.</param>
    /// <param name="debug">표시 설정. 전역 디버그가 켜져 있으면 null은 기본 설정으로 표시하며, None도 한 프레임 표시한다. 호출 시점의 영역을 표시한다.</param>
    public static Collider[] OverlapCapsule(Vector3 point0, Vector3 point1, float radius, int layerMask = Physics.AllLayers, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, IReadOnlyList<GameObject> ignoreObjects = null, PhysicsQueryDebug debug = null)
    {
        Collider[] result = Physics.OverlapCapsule(point0, point1, radius, layerMask, triggerInteraction);
        int count = FilterResults(result, result.Length, ignoreObjects);
        if (count != result.Length) Array.Resize(ref result, count);
        DrawCapsule(point0, point1, radius, debug, result.Length > 0);
        return result;
    }

    /// <summary>
    /// 캡슐 영역과 겹치는 콜라이더를 재사용 버퍼에 기록하고 개수를 반환한다.
    /// </summary>
    /// <param name="point0">한쪽 끝 구의 월드 중심. 캡슐 표면의 끝점이 아니다.</param>
    /// <param name="point1">다른 쪽 끝 구의 월드 중심. 캡슐 표면의 끝점이 아니다.</param>
    /// <param name="results">결과 버퍼. 반환 개수 미만의 인덱스만 유효하며, 제외 대상도 조회 버퍼 공간을 차지하므로, 제외 후 개수가 작아도 일부 결과가 누락될 수 있다.</param>
    /// <param name="ignoreObjects">제외할 오브젝트 목록. 각 오브젝트와 모든 자식의 콜라이더를 제외하며, null 항목은 건너뛴다.</param>
    /// <param name="debug">표시 설정. 전역 디버그가 켜져 있으면 null은 기본 설정으로 표시하며, None도 한 프레임 표시한다. 호출 시점의 영역을 표시한다.</param>
    public static int OverlapCapsuleNonAlloc(Vector3 point0, Vector3 point1, float radius, Collider[] results, int layerMask = Physics.AllLayers, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, IReadOnlyList<GameObject> ignoreObjects = null, PhysicsQueryDebug debug = null)
    {
        ValidateResults(results);
        int result = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, results, layerMask, triggerInteraction);
        result = FilterResults(results, result, ignoreObjects);
        DrawCapsule(point0, point1, radius, debug, result > 0);
        return result;
    }

    /// <summary>
    /// 구 영역과 겹치는 콜라이더가 하나라도 있는지 확인한다.
    /// </summary>
    /// <param name="ignoreObjects">제외할 오브젝트 목록. 각 오브젝트와 모든 자식의 콜라이더를 제외하며, null 항목은 건너뛴다.</param>
    /// <param name="debug">표시 설정. 전역 디버그가 켜져 있으면 null은 기본 설정으로 표시하며, None도 한 프레임 표시한다. 호출 시점의 영역을 표시한다.</param>
    /// <remarks>제외 목록이 있으면 Overlap 결과 배열을 생성하여 대상별로 검사한다.</remarks>
    public static bool CheckSphere(Vector3 center, float radius, int layerMask = Physics.AllLayers, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, IReadOnlyList<GameObject> ignoreObjects = null, PhysicsQueryDebug debug = null)
    {
        bool result = ignoreObjects == null || ignoreObjects.Count == 0
            ? Physics.CheckSphere(center, radius, layerMask, triggerInteraction)
            : HasIncludedCollider(Physics.OverlapSphere(center, radius, layerMask, triggerInteraction), ignoreObjects);
        DrawSphere(center, radius, debug, result);
        return result;
    }

    /// <summary>
    /// 박스 영역과 겹치는 콜라이더가 하나라도 있는지 확인한다.
    /// </summary>
    /// <param name="size">회전 전 각 축의 전체 크기. Transform의 스케일은 자동 적용하지 않는다.</param>
    /// <param name="rotation">박스의 월드 회전. 생략하면 기본 회전을 사용한다.</param>
    /// <param name="ignoreObjects">제외할 오브젝트 목록. 각 오브젝트와 모든 자식의 콜라이더를 제외하며, null 항목은 건너뛴다.</param>
    /// <param name="debug">표시 설정. 전역 디버그가 켜져 있으면 null은 기본 설정으로 표시하며, None도 한 프레임 표시한다. 호출 시점의 영역을 표시한다.</param>
    /// <remarks>제외 목록이 있으면 Overlap 결과 배열을 생성하여 대상별로 검사한다.</remarks>
    public static bool CheckBox(Vector3 center, Vector3 size, Quaternion? rotation = null, int layerMask = Physics.AllLayers, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, IReadOnlyList<GameObject> ignoreObjects = null, PhysicsQueryDebug debug = null)
    {
        bool result = ignoreObjects == null || ignoreObjects.Count == 0
            ? Physics.CheckBox(center, size * 0.5f, rotation ?? Quaternion.identity, layerMask, triggerInteraction)
            : HasIncludedCollider(Physics.OverlapBox(center, size * 0.5f, rotation ?? Quaternion.identity, layerMask, triggerInteraction), ignoreObjects);
        DrawBox(center, size, rotation ?? Quaternion.identity, debug, result);
        return result;
    }

    /// <summary>
    /// 캡슐 영역과 겹치는 콜라이더가 하나라도 있는지 확인한다.
    /// </summary>
    /// <param name="point0">한쪽 끝 구의 월드 중심. 캡슐 표면의 끝점이 아니다.</param>
    /// <param name="point1">다른 쪽 끝 구의 월드 중심. 캡슐 표면의 끝점이 아니다.</param>
    /// <param name="ignoreObjects">제외할 오브젝트 목록. 각 오브젝트와 모든 자식의 콜라이더를 제외하며, null 항목은 건너뛴다.</param>
    /// <param name="debug">표시 설정. 전역 디버그가 켜져 있으면 null은 기본 설정으로 표시하며, None도 한 프레임 표시한다. 호출 시점의 영역을 표시한다.</param>
    /// <remarks>제외 목록이 있으면 Overlap 결과 배열을 생성하여 대상별로 검사한다.</remarks>
    public static bool CheckCapsule(Vector3 point0, Vector3 point1, float radius, int layerMask = Physics.AllLayers, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, IReadOnlyList<GameObject> ignoreObjects = null, PhysicsQueryDebug debug = null)
    {
        bool result = ignoreObjects == null || ignoreObjects.Count == 0
            ? Physics.CheckCapsule(point0, point1, radius, layerMask, triggerInteraction)
            : HasIncludedCollider(Physics.OverlapCapsule(point0, point1, radius, layerMask, triggerInteraction), ignoreObjects);
        DrawCapsule(point0, point1, radius, debug, result);
        return result;
    }

    /// <summary>
    /// 결과 버퍼가 없거나 비어 있으면 잘못된 호출을 예외로 알린다.
    /// </summary>
    private static void ValidateResults(Collider[] results)
    {
        if (results == null) throw new System.ArgumentNullException(nameof(results));
        if (results.Length == 0) throw new System.ArgumentException("결과 버퍼에는 하나 이상의 공간이 필요합니다.", nameof(results));
    }

    /// <summary>
    /// 제외 대상이 아닌 결과를 버퍼 앞쪽에 모으고 제거한 범위의 참조를 비운다.
    /// </summary>
    private static int FilterResults(Collider[] results, int count, IReadOnlyList<GameObject> ignoreObjects)
    {
        if (ignoreObjects == null || ignoreObjects.Count == 0) return count;
        int includedCount = 0;
        for (int i = 0; i < count; i++)
        {
            Collider collider = results[i];
            if (collider == null || IsIgnored(collider, ignoreObjects)) continue;
            results[includedCount++] = collider;
        }

        Array.Clear(results, includedCount, count - includedCount);
        return includedCount;
    }

    /// <summary>
    /// 제외 대상이 아닌 유효한 콜라이더가 하나라도 있는지 확인한다.
    /// </summary>
    private static bool HasIncludedCollider(Collider[] results, IReadOnlyList<GameObject> ignoreObjects)
    {
        for (int i = 0; i < results.Length; i++)
        {
            if (results[i] != null && !IsIgnored(results[i], ignoreObjects)) return true;
        }
        return false;
    }

    /// <summary>
    /// 콜라이더가 제외 오브젝트 자신 또는 그 자식에 속하는지 확인한다.
    /// </summary>
    private static bool IsIgnored(Collider collider, IReadOnlyList<GameObject> ignoreObjects)
    {
        Transform target = collider.transform;
        for (int i = 0; i < ignoreObjects.Count; i++)
        {
            GameObject ignored = ignoreObjects[i];
            if (ignored != null && target.IsChildOf(ignored.transform)) return true;
        }
        return false;
    }

    /// <summary>
    /// 전역 또는 개별 설정이 활성화되면 표시 설정을 반환하고, 생략된 설정은 재사용한다.
    /// </summary>
    private static PhysicsQueryDebug GetDebugSettings(PhysicsQueryDebug debug)
    {
        bool isDebugEnabled = GameConfig.IsCollisionDebugEnabled || (debug != null && debug.drawMode != eDebugDrawMode.None);
        if (!isDebugEnabled)
        {
            return null;
        }
        return debug ?? _defaultDebug;
    }

    /// <summary>
    /// 서로 수직인 세 원으로 구 영역을 표시한다.
    /// </summary>
    internal static void DrawSphere(Vector3 center, float radius, PhysicsQueryDebug debug, bool hasHit)
    {
        debug = GetDebugSettings(debug);
        if (debug == null)
        {
            return;
        }
        DrawArc(center, Vector3.right, Vector3.up, radius, 0f, Mathf.PI * 2f, debug, hasHit);
        DrawArc(center, Vector3.right, Vector3.forward, radius, 0f, Mathf.PI * 2f, debug, hasHit);
        DrawArc(center, Vector3.up, Vector3.forward, radius, 0f, Mathf.PI * 2f, debug, hasHit);
    }

    /// <summary>
    /// 월드 회전을 적용한 박스의 열두 모서리를 표시한다.
    /// </summary>
    private static void DrawBox(Vector3 center, Vector3 size, Quaternion rotation, PhysicsQueryDebug debug, bool hasHit)
    {
        debug = GetDebugSettings(debug);
        if (debug == null)
        {
            return;
        }
        Vector3 halfExtents = size * 0.5f;

        // 각 비트를 꼭짓점의 축별 부호로 사용하여 배열 할당 없이 모서리를 연결한다.
        for (int i = 0; i < 8; i++)
        {
            Vector3 start = center + rotation * GetBoxCorner(i, halfExtents);
            for (int axis = 1; axis <= 4; axis <<= 1)
            {
                if ((i & axis) != 0) continue;
                Vector3 end = center + rotation * GetBoxCorner(i | axis, halfExtents);
                DrawLine(start, end, debug, hasHit);
            }
        }
    }

    /// <summary>
    /// 구 중심 사이의 축에 맞춰 캡슐의 원통과 양 끝 반구를 표시한다.
    /// </summary>
    internal static void DrawCapsule(Vector3 point0, Vector3 point1, float radius, PhysicsQueryDebug debug, bool hasHit)
    {
        debug = GetDebugSettings(debug);
        if (debug == null)
        {
            return;
        }
        Vector3 offset = point1 - point0;
        if (offset.sqrMagnitude < 0.000001f)
        {
            DrawSphere(point0, radius, debug, hasHit);
            return;
        }

        Vector3 axis = offset.normalized;
        // 축이 위쪽과 평행해도 외적으로 유효한 단면을 만들 수 있도록 기준축을 선택한다.
        Vector3 reference = Mathf.Abs(Vector3.Dot(axis, Vector3.up)) > 0.99f ? Vector3.right : Vector3.up;
        Vector3 right = Vector3.Cross(axis, reference).normalized;
        Vector3 forward = Vector3.Cross(axis, right).normalized;

        DrawArc(point0, right, forward, radius, 0f, Mathf.PI * 2f, debug, hasHit);
        DrawArc(point1, right, forward, radius, 0f, Mathf.PI * 2f, debug, hasHit);
        DrawArc(point0, right, -axis, radius, 0f, Mathf.PI, debug, hasHit);
        DrawArc(point0, forward, -axis, radius, 0f, Mathf.PI, debug, hasHit);
        DrawArc(point1, right, axis, radius, 0f, Mathf.PI, debug, hasHit);
        DrawArc(point1, forward, axis, radius, 0f, Mathf.PI, debug, hasHit);
        DrawLine(point0 + right * radius, point1 + right * radius, debug, hasHit);
        DrawLine(point0 - right * radius, point1 - right * radius, debug, hasHit);
        DrawLine(point0 + forward * radius, point1 + forward * radius, debug, hasHit);
        DrawLine(point0 - forward * radius, point1 - forward * radius, debug, hasHit);
    }

    /// <summary>
    /// 비트로 지정한 부호에 따라 박스의 로컬 꼭짓점을 계산한다.
    /// </summary>
    private static Vector3 GetBoxCorner(int index, Vector3 halfExtents)
    {
        return new Vector3((index & 1) == 0 ? -halfExtents.x : halfExtents.x, (index & 2) == 0 ? -halfExtents.y : halfExtents.y, (index & 4) == 0 ? -halfExtents.z : halfExtents.z);
    }

    /// <summary>
    /// 두 단위축이 만드는 평면에서 원호를 선분으로 나누어 표시한다.
    /// </summary>
    private static void DrawArc(Vector3 center, Vector3 axis0, Vector3 axis1, float radius, float startAngle, float endAngle, PhysicsQueryDebug debug, bool hasHit)
    {
        int segments = Mathf.Clamp(debug.segments, 8, 128);
        Vector3 previous = center + (axis0 * Mathf.Cos(startAngle) + axis1 * Mathf.Sin(startAngle)) * radius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, (float)i / segments);
            Vector3 next = center + (axis0 * Mathf.Cos(angle) + axis1 * Mathf.Sin(angle)) * radius;
            DrawLine(previous, next, debug, hasHit);
            previous = next;
        }
    }

    /// <summary>
    /// 조회 결과의 색상과 표시 모드에 따른 수명으로 선분을 표시한다.
    /// </summary>
    private static void DrawLine(Vector3 start, Vector3 end, PhysicsQueryDebug debug, bool hasHit)
    {
        float duration = debug.drawMode == eDebugDrawMode.ForDuration ? debug.duration : 0f;
        if (float.IsNaN(duration) || float.IsInfinity(duration) || duration < 0f) duration = 0f;
        PhysicsQueryDebugRenderer.DrawLine(start, end, hasHit ? debug.hitColor : debug.color, duration, debug.depthTest);
    }
}
