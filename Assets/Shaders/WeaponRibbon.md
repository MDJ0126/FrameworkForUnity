# 무기 검기

`TestSword` 프리팹에는 청백색 검기가 연결되어 있다. 기존 공격 애니메이션의 데미지 판정 시작·종료 이벤트에 따라 생성하며, 종료 후 0.16초 동안 잔상이 사라진다. 텍스처와 외부 아트 에셋은 필요하지 않다.

- **색상:** 프리팹의 `Weapon Ribbon → Ribbon Color`를 변경한다. Alpha는 투명도, HDR Intensity는 밝기를 조절한다. Bloom을 자동으로 추가하지 않으므로 화면에서 번지는 광채는 현재 카메라의 후처리 설정에 따른다.
- **잔상 길이:** `Lifetime`을 늘리면 오래 남고 줄이면 짧게 사라진다. 기본값은 0.16초다.
- **칼날 범위:** 기본 검은 메시의 가장 긴 축과 손잡이에서 칼끝까지의 비율 `Blade Start Fraction`으로 범위를 정한다. 다른 모양의 무기는 칼날 밑부분과 끝에 빈 GameObject를 만들고 `Blade Start`와 `Blade End`에 연결한다.
- **다른 근접 무기:** `WeaponRibbon` 컴포넌트를 추가하고 머티리얼 `WeaponRibbon.mat`, 칼날 위치, `MeleeWeapon → Weapon Ribbon`을 연결한다. 공격 시작·종료 연동은 `MeleeWeapon`이 처리한다.
- **검증:** Unity 메뉴 `Sample Project → 검기 검증`으로 실제 `TestSword` 프리팹의 연결, 잔상 수명, 월드 위치 유지, 순간이동과 비활성화 정리를 검사한다. 게임 플레이 중에는 실행하지 않는다.

검기는 시각 효과만 표시하며 데미지 판정 범위를 변경하지 않는다. 생성된 메시와 GameObject는 무기와 함께 정리한다.
