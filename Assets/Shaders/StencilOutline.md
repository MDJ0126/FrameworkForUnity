# 스텐실 외곽선

- 캐릭터 또는 일반 메시 루트에 **Stencil Outline** 컴포넌트를 추가한다. 기존 머티리얼은 그대로 사용한다.
- Inspector의 **Is Outline Enabled**로 표시를 전환하고, **Outline Color / Outline Width**로 색상과 픽셀 두께를 조절한다. 컴포넌트를 비활성화해도 외곽선을 끈다.
- `PC_Renderer`, `Mobile_Renderer`에는 `StencilOutlineRendererFeature`를 연결했다. 다른 URP Renderer에는 같은 Renderer Feature를 추가하고 Shader에 `StencilOutline.shader`를 지정한다. Renderer Feature의 활성 체크는 해당 렌더러 전체 외곽선을 전환한다.
- 장비 교체로 깊은 자식 계층의 메시가 바뀌면 `RefreshRenderers()`를 호출한다. 직접 자식 변경과 컴포넌트 활성화 시에는 자동으로 수집한다.

```csharp
StencilOutline outline = character.GetComponent<StencilOutline>();
outline.SetOutlineEnabled(true);
outline.SetOutlineStyle(Color.yellow, 3f);
outline.SetOutlineEnabled(false);
```

## 렌더링 계약

URP 17 / Unity 6용 Render Graph 경로와 Compatibility Mode 경로를 구현한다. 불투명 MeshRenderer와 SkinnedMeshRenderer의 각 서브메시를 마스크 패스와 외곽선 패스로 그린다. 모든 대상 마스크를 먼저 기록하여 몸체 내부와 여러 자식 메시 사이의 외곽선을 제외한다. 깊이 검사를 사용하므로 불투명 벽 뒤에서는 표시하지 않는다. 투명 표면은 일반 URP 투명 렌더링 순서에 따라 외곽선 위에 합성된다.

스텐실 **bit 0 (값 1)**을 전용으로 사용한다. 패스 시작과 끝에서 이 비트만 초기화하고 나머지 비트를 보존한다. URP 소스의 `StencilUsage.UserMask`에 포함된 비트이며, 다른 사용자 효과와 bit 0을 공유해서는 안 된다. 카메라 타깃은 스텐실을 포함한 깊이 포맷이 필요하다.

## 적용 범위와 제한

- 불투명 3D 메시를 대상으로 한다. 렌더 큐가 2500보다 큰 머티리얼과 `_ALPHATEST_ON` 알파 컷아웃 머티리얼은 제외한다. 사용자 셰이더의 별도 알파 클리핑과 버텍스 변형은 재현하지 않는다.
- 메시의 기존 법선으로 확장한다. 하드 에지, 분리된 법선, 열린 면에서는 외곽선에 틈이나 두께 차이가 생길 수 있다. 매끄러운 캐릭터 법선을 사용하는 것이 적합하다.
- LODGroup의 단계 선택, GPU occlusion culling, 원본 머티리얼의 Cull 설정을 재현하지 않는다. 해당 기능을 사용하는 대상은 별도 통합이 필요하다.
- 대상마다 불투명 서브메시 수의 두 배에 해당하는 추가 드로우와 카메라별 스텐실 초기화 두 번이 발생한다. 모바일 기기 성능과 XR은 별도 검증이 필요하다.
- 서로 겹치는 대상은 하나의 스텐실 실루엣으로 처리한다. 대상 사이의 내부 경계선을 표시하는 용도로는 사용하지 않는다.

구현 기준: 프로젝트 `Packages/manifest.json`, URP 17.0.4의 `StencilUsage.cs`, `DistortTunnelPass_Tunnel.cs`, `RendererListRenderFeature.cs`. [Unity 6 Render Graph 패스 작성](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/render-graph-write-render-pass.html).

## 검증 결과

2026-10-04, Unity 6000.0.83f1 / URP 17.0.4 / Windows D3D11의 분리된 검증 프로젝트에서 C#와 셰이더 3개 패스를 컴파일했다. Forward·Deferred 각각의 Render Graph·Compatibility Mode에서 구 메시의 외곽선 켜기, 끄기, 다시 켜기, 컴포넌트 비활성화, 불투명 벽 뒤 가림을 GPU 픽셀로 검사하여 통과했다. 활성 상태는 빨간 외곽선 774픽셀, 비활성 및 완전 가림 상태는 0픽셀이었다.

실제 프로젝트 캐릭터의 스키닝·장비 구성과 모바일 기기·XR 출력은 **미확인**이다. 검증 코드와 출력은 Git에서 제외된 `Temp/OutlineValidation`에 보관한다.
