using UnityEngine;
using System;

/// <summary>
/// 게임 상태 머신: Exploration, Battle, Menu 상태 전환 관리
/// </summary>
public class GameStateController : MonoBehaviour
{
    public static GameStateController Instance;
    
    public enum GameState
    {
        Exploration,  // 탐색 모드 (1인칭 이동 가능)
        Battle,       // 전투 모드
        Menu,         // 메뉴 모드 (스테이터스/장비/스킬 창)
        Paused        // 일시정지
    }
    
    [Header("Current State")]
    public GameState currentState = GameState.Exploration;
    
    [Header("References")]
    public BattleManager battleManager;
    public DebugHotkeySystem debugSystem;
    
    // 이벤트
    public event Action<GameState, GameState> OnStateChanged; // (oldState, newState)
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        SetState(GameState.Exploration);
    }
    
    void Update()
    {
        // ESC 키로 메뉴 열기/닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Menu)
            {
                SetState(GameState.Exploration);
            }
            else if (currentState == GameState.Exploration || currentState == GameState.Battle)
            {
                SetState(GameState.Menu);
            }
        }
    }
    
    /// <summary>
    /// 상태 전환
    /// </summary>
    public void SetState(GameState newState)
    {
        if (currentState == newState) return;
        
        GameState oldState = currentState;
        currentState = newState;
        
        // 상태별 처리
        switch (newState)
        {
            case GameState.Exploration:
                EnterExplorationMode();
                break;
            case GameState.Battle:
                EnterBattleMode();
                break;
            case GameState.Menu:
                EnterMenuMode();
                break;
            case GameState.Paused:
                EnterPausedMode();
                break;
        }
        
        OnStateChanged?.Invoke(oldState, newState);
        Debug.Log($"[GameState] Changed: {oldState} → {newState}");
    }
    
    private void EnterExplorationMode()
    {
        // 전투 UI 숨기기
        if (battleManager != null)
        {
            if (battleManager.actionPanel != null)
                battleManager.actionPanel.SetActive(false);
            if (battleManager.skillPanel != null)
                battleManager.skillPanel.SetActive(false);
        }
        
        // 커서 잠금 해제 (1인칭 이동 활성화)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 시간 정상화
        Time.timeScale = 1f;
    }
    
    private void EnterBattleMode()
    {
        // 전투 UI 표시
        if (battleManager != null)
        {
            if (battleManager.actionPanel != null)
                battleManager.actionPanel.SetActive(true);
        }
        
        // 커서 표시 (버튼 클릭용)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    private void EnterMenuMode()
    {
        // 커서 표시
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 시간 정상화 (메뉴에서도 배경은 움직일 수 있음)
        Time.timeScale = 1f;
    }
    
    private void EnterPausedMode()
    {
        // 커서 표시
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 시간 정지
        Time.timeScale = 0f;
    }
    
    // 편의 메서드
    public void StartBattle()
    {
        SetState(GameState.Battle);
        if (battleManager != null)
        {
            battleManager.StartBattle();
        }
    }
    
    public void EndBattle()
    {
        SetState(GameState.Exploration);
    }
    
    public void OpenMenu()
    {
        SetState(GameState.Menu);
    }
    
    public void CloseMenu()
    {
        if (currentState == GameState.Battle)
            SetState(GameState.Battle);
        else
            SetState(GameState.Exploration);
    }
    
    public void TogglePause()
    {
        if (currentState == GameState.Paused)
        {
            // 이전 상태로 복귀
            SetState(GameState.Exploration);
        }
        else
        {
            SetState(GameState.Paused);
        }
    }
}

