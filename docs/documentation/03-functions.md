# Function 작성

1. 하나의 함수는 하나의 목적을 맡습니다.
2. 입력 범위, 실패 결과, 상태 변경·이벤트를 정합니다.
3. 필요한 부모 호출을 유지합니다.
4. 주석은 계약과 이유를 설명합니다.

Get은 조회, Set은 변경, Add/Remove는 등록·제거, Try는 성공 여부, UpdateTick은 시간 갱신입니다.

## 짧은 예제

제안 코드이며 실행은 미확인입니다.

```csharp
/// <summary>
/// 유효한 인덱스의 스킬을 조회한다.
/// </summary>
public bool TryGetSkill(int index, out Skill skill)
{
    skill = null;
    if (index < 0 || index >= Skills.Count)
    {
        return false;
    }
    skill = Skills[index];
    return true;
}
```

현재 GetSkill은 음수 검사가 없습니다. SetHp는 대입 없이 이벤트만 발행합니다. 함수명만으로 동작을 판단하지 않습니다. 호출은 가능하면 한 줄로 씁니다. 자세한 형식은 [주석·줄바꿈](17-comments-format.md)을 따릅니다.
