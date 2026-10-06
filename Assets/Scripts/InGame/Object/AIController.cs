using System.Numerics;

namespace Game
{
    public abstract class AIController : PawnController
    {
        /// <summary>
        /// 빙의
        /// </summary>
        public override void Possess(Pawn pawn = null)
        {
            base.Possess(pawn);
        }

        /// <summary>
        /// 빙의 해제
        /// </summary>
        public override void Unpossess()
        {
            base.Unpossess();
        }

        /// <summary>
        /// 목적지로 이동하기
        /// </summary>
        /// <param name="destination"></param>
        public virtual void MoveTo(Vector3 destination)
        {

        }
    }
}
