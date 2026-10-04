using System.Collections.Generic;

namespace Game
{
    public class HUDManager : SingletonBehaviour<HUDManager>
    {
        public ObjectPool followPawnInfoPool;
        public ObjectPool followSpeechBubblePool;

        /// <summary>
        /// Pawn에 연결된 모든 추적 HUD 숨기기
        /// </summary>
        public void DetachFollowHUD(List<FollowHUD> followHUDs)
        {
            foreach (FollowHUD followHUD in followHUDs)
            {
                DetachFollowHUD(followHUD);
            }
        }

        /// <summary>
        /// 추적 HUD 하나를 숨겨 오브젝트 풀에서 재사용할 수 있게 처리
        /// </summary>
        public void DetachFollowHUD(FollowHUD followHUD)
        {
            followHUD.Hide();
        }

        /// <summary>
        /// 이름 HUD를 풀에서 가져와 Pawn의 이름 앵커에 연결
        /// </summary>
        public FollowPawnInfo AttachFollowPawnInfo(Pawn pawn)
        {
            FollowPawnInfo followPawnInfo = followPawnInfoPool.Get<FollowPawnInfo>();
            followPawnInfo.SetTarget(pawn.PawnInfoAnchor);
            followPawnInfo.SetPawn(pawn);
            followPawnInfo.Show();
            return followPawnInfo;
        }

        /// <summary>
        /// 말풍선 HUD를 풀에서 가져와 Pawn의 말풍선 앵커에 연결
        /// </summary>
        public FollowSpeechBubble AttachFollowSpeechBubble(Pawn pawn, string text)
        {
            FollowSpeechBubble followSpeechBubble = followSpeechBubblePool.Get<FollowSpeechBubble>();
            followSpeechBubble.SetTarget(pawn.balloonAnchor);
            followSpeechBubble.SetText(text);
            followSpeechBubble.Show();
            return followSpeechBubble;
        }
    }
}
