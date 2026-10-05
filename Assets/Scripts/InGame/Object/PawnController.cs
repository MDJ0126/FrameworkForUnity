using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public abstract class PawnController : SingletonBehaviour<PawnController>
    {
        public delegate void OnPossessEvent(Pawn possessedPawn);
        private event OnPossessEvent _onPossessed;
        public event OnPossessEvent OnPossessed
        {
            add
            {
                _onPossessed -= value;
                _onPossessed += value;
            }
            remove
            {
                _onPossessed -= value;
            }
        }

        public delegate void OnUnpossessEvent(Pawn unpossessedPawn);
        private event OnUnpossessEvent _onUnpossessed;
        public event OnUnpossessEvent OnUnpossessed
        {
            add
            {
                _onUnpossessed -= value;
                _onUnpossessed += value;
            }
            remove
            {
                _onUnpossessed -= value;
            }
        }

        #region Inspector

        [ReadOnly][SerializeField] protected Pawn possessTarget = null;

        [Header("Aim Settings")]
        [ReadOnly] public AimTarget aimTarget = null;
        public float aimDistance = 100f;
        public LayerMask aimMask;

        #endregion
        public bool IsPossessed { get; private set; } = false;

        private Pawn _lastPossessTarget = null;

        private PlayerInput _playerInput = null;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        /// <summary>
        /// 빙의
        /// </summary>
        public virtual void Possess(Pawn pawn = null)
        {
            if (pawn == null)
            {
                // 대상이 생략되면 에디터 토글을 위해 마지막으로 빙의했던 Pawn을 재사용한다.
                pawn = _lastPossessTarget;
            }
            possessTarget = pawn;

            if (possessTarget != null)
            {
                _lastPossessTarget = pawn;
                possessTarget.Possess(this);

                // Pawn의 이동 입력과 SpringArm 카메라를 컨트롤러에 연결한다.
                var playerCameraController = Camera.main.GetComponent<PlayerCameraController>();
                playerCameraController.Bind(pawn.GetComponentInChildren<SpringArm>());
                IsPossessed = true;
                // 마우스 회전 중 커서가 게임 화면 밖으로 나가지 않도록 잠근다.
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                _playerInput.enabled = true;
                _onPossessed?.Invoke(pawn);
            }
        }

        /// <summary>
        /// 빙의 해제
        /// </summary>
        public virtual void Unpossess()
        {
            _playerInput.enabled = false;

            // Pawn별 해제 처리를 먼저 호출한 뒤 입력과 조준 참조를 정리한다.
            possessTarget.Unpossess(this);
            aimTarget = null;
            IsPossessed = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            possessTarget = null;
            _onUnpossessed?.Invoke(_lastPossessTarget);
        }
    }
}