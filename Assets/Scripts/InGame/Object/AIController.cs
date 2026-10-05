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
    }
}
