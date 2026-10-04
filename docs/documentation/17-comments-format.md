# 주석·줄바꿈 규칙

- 짧거나 보통 길이의 호출은 한 줄. 읽기 어려울 때만 줄바꿈.
- if·else는 여러 줄 블록으로 작성하며 항상 중괄호를 사용한다. 한 줄로 작성하는 경우에도 중괄호를 생략하지 않는다.
- switch문은 한 줄 작성을 금지하며 각 case·default 본문에도 반드시 중괄호를 사용한다. 중괄호는 다음 줄에 두고 레이블, 본문, break·return을 각각 별도 줄로 작성한다.
- summary는 여는 태그·설명·닫는 태그의 3줄.
- 주석은 한국어 '~한다.' 톤으로 역할·전제·부작용·처리 이유를 설명. 빈 param/returns, 함수명 반복, 대화체, 미검증 보장은 금지.

```csharp
float distance = Vector3.Distance(target.position, _targetCamera.transform.position);

/// <summary>
/// 이전 Pawn의 구독을 해제하고 새 Pawn을 연결한다.
/// </summary>
/// <param name="pawn">연결할 Pawn. null이면 연결을 해제한다.</param>

// 풀 재사용 시 이전 이벤트가 남지 않도록 먼저 구독을 해제한다.
// TODO: 종료 중 목록 제거의 재현을 확인한다.
```

위 주석은 패턴 예시입니다. 구현이 바뀌면 주석도 함께 고칩니다.

## 조건문 예제

기본 형식은 다음과 같이 조건과 본문을 여러 줄로 작성합니다.

```csharp
if (Application.isPlaying)
{
    Destroy(addressSource);
}
else
{
    DestroyImmediate(addressSource);
}
```

한 줄로 작성하는 경우에도 `if (isReady) { Initialize(); }`처럼 중괄호를 반드시 사용합니다. `if (isReady) Initialize();`처럼 중괄호를 생략하는 형식은 사용하지 않습니다. 예제의 컴파일·실행은 미확인입니다.

## switch문 예제

각 `case/default` 본문은 실행문이 하나여도 중괄호로 감쌉니다. 레이블과 실행문을 한 줄로 합치지 않습니다.

```csharp
switch (index)
{
    case 0:
    {
        Initialize();
        break;
    }
    default:
    {
        return;
    }
}
```

형식 예제이며 컴파일·실행은 미확인입니다.
