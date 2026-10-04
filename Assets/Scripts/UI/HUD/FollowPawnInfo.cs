using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class FollowPawnInfo : FollowHUD
    {
        private const float GAUGE_ANIMATION_TIME = 0.2f;

        #region Inspector

        public TMP_Text nameText;
        public Image fillImage;

        #endregion

        private Pawn _pawn = null;

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_pawn)
            {
                _pawn.StatusInfo.OnChangedHp -= OnChangedHp;
                //_pawn.BaseStatus.OnChangedMp -= OnChangedMp;
            }
        }

        /// <summary>
        /// Pawn 세팅
        /// </summary>
        public void SetPawn(Pawn pawn)
        {
            _pawn = pawn;

            nameText.text = pawn.Name;

            pawn.StatusInfo.OnChangedHp += OnChangedHp;
            //pawn.BaseStatus.OnChangedMp += OnChangedMp;

            UpdateHpGauge((float)pawn.StatusInfo.hp / pawn.StatusInfo.baseStatus.maxHp);
        }

        /// <summary>
        /// 체력 변화 이벤트
        /// </summary>
        /// <param name="statusInfo"></param>
        private void OnChangedHp(StatusInfo statusInfo)
        {
            UpdateHpGauge(statusInfo.HpRatio);
        }

        /// <summary>
        /// 체력 게이지 업데이트
        /// </summary>
        /// <param name="value"></param>
        private void UpdateHpGauge(float value)
        {
            if (this.gameObject.activeInHierarchy)
            {
                StartCoroutine(UpdateHpGaugeCo());
            }
            else
            {
                fillImage.fillAmount = value;
            }

            IEnumerator UpdateHpGaugeCo()
            {
                float start = fillImage.fillAmount;
                float end = value;

                float duration = 0f;
                while (duration < GAUGE_ANIMATION_TIME)
                {
                    duration += Time.deltaTime;
                    float t = duration / GAUGE_ANIMATION_TIME;
                    fillImage.fillAmount = Mathf.Lerp(start, end, t);
                    yield return null;
                }
                fillImage.fillAmount = end;
            }
        }
    }
}
