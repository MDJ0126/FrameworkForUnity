using Game;
using System.Collections;

public abstract class GameMode : SingletonBehaviour<GameMode>
{
    #region Inspector

    public Pawn defaultPawn;

    // HUD Class

    public PlayerController playerController;

    // GameState Class

    // PlayerState Class

    // Spectator Class

    #endregion

    /// <summary>
    /// 씬 초기화가 한 프레임 끝난 뒤 기본 Pawn에 빙의
    /// </summary>
    protected virtual IEnumerator Start()
    {
        // Pawn과 컨트롤러의 Awake가 모두 끝난 다음 빙의하도록 한 프레임 대기한다.
        yield return null;
        //playerController.SetGameMode(this);
        playerController.Possess(defaultPawn);
    }
}
