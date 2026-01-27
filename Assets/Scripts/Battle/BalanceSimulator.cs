using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 밸런스 시뮬레이션 스크립트: 레벨업 속도, 보스 난이도, 던전 클리어 가능성 등을 시뮬레이션
/// </summary>
public class BalanceSimulator : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public BattleManager battleManager;
    public List<EnemyStats> enemyDatabase = new List<EnemyStats>(); // 모든 적 데이터
    
    [Header("Simulation Settings")]
    public int maxFloors = 50;
    public int battlesPerFloor = 3; // 층당 평균 전투 횟수
    public float avgBattlesPerFloor = 3f;
    
    [Header("Results")]
    [TextArea(10, 20)]
    public string simulationResults = "";
    
    [ContextMenu("Run Full Simulation")]
    public void RunFullSimulation()
    {
        simulationResults = "";
        AddLog("=== BALANCE SIMULATION START ===\n");
        
        // 1. 레벨업 속도 시뮬레이션
        SimulateLevelProgression();
        
        // 2. 보스 난이도 체크
        SimulateBossDifficulty();
        
        // 3. 50층 클리어 시뮬레이션
        SimulateDungeonClear();
        
        // 4. 전투 생존율 분석
        SimulateCombatSurvival();
        
        // 5. 스탯 곡선 분석
        AnalyzeStatCurves();
        
        AddLog("\n=== SIMULATION COMPLETE ===");
        Debug.Log(simulationResults);
    }
    
    // 레벨업 속도 시뮬레이션
    private void SimulateLevelProgression()
    {
        AddLog("--- Level Progression Simulation ---");
        
        if (playerStats == null || enemyDatabase == null || enemyDatabase.Count == 0)
        {
            AddLog("ERROR: Missing player/enemy data");
            return;
        }
        
        int currentLevel = 1;
        int totalExp = 0;
        int expPerEnemy = CalculateAverageExp();
        int battlesCompleted = 0;
        
        AddLog($"Starting Level: {currentLevel}");
        AddLog($"Average EXP per enemy: {expPerEnemy}");
        
        // 50층까지 시뮬레이션
        for (int floor = 1; floor <= maxFloors; floor++)
        {
            int floorBattles = Mathf.RoundToInt(avgBattlesPerFloor);
            int floorEnemies = floorBattles * Random.Range(1, 3); // 전투당 1~2마리 평균
            
            totalExp += floorEnemies * expPerEnemy;
            battlesCompleted += floorBattles;
            
            // 레벨업 체크 (간단한 경험치 곡선: 레벨 * 100)
            int expNeeded = currentLevel * 100;
            if (totalExp >= expNeeded)
            {
                int levelsGained = totalExp / expNeeded;
                currentLevel += levelsGained;
                totalExp %= expNeeded;
                AddLog($"Floor {floor}: Level {currentLevel} reached after {battlesCompleted} battles");
            }
            
            if (floor % 10 == 0)
            {
                AddLog($"Floor {floor}: Level {currentLevel}, Total Battles: {battlesCompleted}");
            }
        }
        
        AddLog($"Final Level at Floor {maxFloors}: {currentLevel}");
        AddLog($"Total Battles: {battlesCompleted}");
        AddLog($"Battles per Floor: {(float)battlesCompleted / maxFloors:F2}");
        AddLog("");
    }
    
    // 보스 난이도 체크
    private void SimulateBossDifficulty()
    {
        AddLog("--- Boss Difficulty Check ---");
        
        if (playerStats == null || enemyDatabase == null || enemyDatabase.Count == 0)
        {
            AddLog("ERROR: Missing player/enemy data");
            return;
        }
        
        // 플레이어 기준 스탯 (레벨 10 가정)
        int playerLevel = 10;
        int playerATK = playerStats.attack + (playerLevel - 1) * 2; // 레벨당 +2 공격력
        int playerDEF = playerStats.defense + (playerLevel - 1) * 1;
        int playerHP = playerStats.maxHP + (playerLevel - 1) * 10;
        
        AddLog($"Player Level {playerLevel} Stats:");
        AddLog($"  HP: {playerHP}, ATK: {playerATK}, DEF: {playerDEF}");
        
        // 보스 찾기 (HP가 높거나 이름에 Boss 포함)
        var bosses = enemyDatabase.Where(e => 
            e.maxHP > playerHP * 2 || 
            e.enemyName.ToLower().Contains("boss") ||
            e.attack > playerDEF * 2
        ).ToList();
        
        if (bosses.Count == 0)
        {
            AddLog("No bosses found in database. Analyzing strongest enemies...");
            bosses = enemyDatabase.OrderByDescending(e => e.maxHP).Take(3).ToList();
        }
        
        foreach (var boss in bosses)
        {
            AnalyzeBossDifficulty(boss, playerATK, playerDEF, playerHP);
        }
        
        AddLog("");
    }
    
    private void AnalyzeBossDifficulty(EnemyStats boss, int playerATK, int playerDEF, int playerHP)
    {
        AddLog($"Analyzing Boss: {boss.enemyName}");
        AddLog($"  Boss HP: {boss.maxHP}, ATK: {boss.attack}, DEF: {boss.defense}");
        
        // 플레이어가 보스를 쓰러뜨리는 데 필요한 턴 수 (대략)
        int playerDamagePerTurn = Mathf.Max(1, (playerATK * 2 - boss.defense) / 2);
        int turnsToKill = Mathf.CeilToInt((float)boss.maxHP / playerDamagePerTurn);
        
        // 보스가 플레이어를 쓰러뜨리는 데 필요한 턴 수
        int bossDamagePerTurn = Mathf.Max(1, (boss.attack * 2 - playerDEF) / 2);
        int turnsToDie = Mathf.CeilToInt((float)playerHP / bossDamagePerTurn);
        
        AddLog($"  Player needs ~{turnsToKill} turns to kill boss");
        AddLog($"  Boss needs ~{turnsToDie} turns to kill player");
        
        float difficultyRatio = (float)turnsToKill / turnsToDie;
        
        if (difficultyRatio > 2.0f)
        {
            AddLog($"  ⚠️ VERY HARD - Player will likely die (Ratio: {difficultyRatio:F2})");
        }
        else if (difficultyRatio > 1.5f)
        {
            AddLog($"  ⚠️ HARD - Challenging but possible (Ratio: {difficultyRatio:F2})");
        }
        else if (difficultyRatio > 0.7f)
        {
            AddLog($"  ✓ BALANCED - Fair fight (Ratio: {difficultyRatio:F2})");
        }
        else
        {
            AddLog($"  ⚠️ EASY - Boss too weak (Ratio: {difficultyRatio:F2})");
        }
    }
    
    // 50층 클리어 시뮬레이션
    private void SimulateDungeonClear()
    {
        AddLog("--- 50 Floor Clear Simulation ---");
        
        if (playerStats == null || enemyDatabase == null || enemyDatabase.Count == 0)
        {
            AddLog("ERROR: Missing player/enemy data");
            return;
        }
        
        int playerHP = playerStats.maxHP;
        int playerMP = playerStats.maxMP;
        int playerATK = playerStats.attack;
        int playerDEF = playerStats.defense;
        int currentLevel = 1;
        bool died = false;
        int deathFloor = 0;
        
        AddLog("Starting dungeon run...");
        
        for (int floor = 1; floor <= maxFloors; floor++)
        {
            int floorBattles = Mathf.RoundToInt(avgBattlesPerFloor);
            
            for (int battle = 0; battle < floorBattles; battle++)
            {
                // 적 선택 (층에 따라 난이도 상승)
                EnemyStats enemy = GetEnemyForFloor(floor);
                if (enemy == null) continue;
                
                // 전투 시뮬레이션
                bool survived = SimulateSingleBattle(playerATK, playerDEF, ref playerHP, enemy);
                
                if (!survived)
                {
                    died = true;
                    deathFloor = floor;
                    break;
                }
                
                // 레벨업 (간단히)
                if (battle % 5 == 0)
                {
                    currentLevel++;
                    playerATK += 2;
                    playerDEF += 1;
                    playerHP += 10;
                }
            }
            
            if (died) break;
            
            if (floor % 10 == 0)
            {
                AddLog($"Floor {floor}: Level {currentLevel}, HP: {playerHP}");
            }
        }
        
        if (died)
        {
            AddLog($"❌ Player died at Floor {deathFloor}, Level {currentLevel}");
        }
        else
        {
            AddLog($"✓ Player cleared Floor {maxFloors} at Level {currentLevel}");
        }
        
        AddLog("");
    }
    
    // 단일 전투 시뮬레이션
    private bool SimulateSingleBattle(int playerATK, int playerDEF, ref int playerHP, EnemyStats enemy)
    {
        int enemyHP = enemy.maxHP;
        int maxTurns = 50; // 무한 루프 방지
        int turn = 0;
        
        while (turn < maxTurns && playerHP > 0 && enemyHP > 0)
        {
            // 플레이어 턴
            int playerDamage = Mathf.Max(1, (playerATK * 2 - enemy.defense) / 2);
            enemyHP -= playerDamage;
            if (enemyHP <= 0) break;
            
            // 적 턴
            int enemyDamage = Mathf.Max(1, (enemy.attack * 2 - playerDEF) / 2);
            playerHP -= enemyDamage;
            if (playerHP <= 0) return false;
            
            turn++;
        }
        
        return playerHP > 0;
    }
    
    // 전투 생존율 분석
    private void SimulateCombatSurvival()
    {
        AddLog("--- Combat Survival Rate Analysis ---");
        
        if (playerStats == null || enemyDatabase == null || enemyDatabase.Count == 0)
        {
            AddLog("ERROR: Missing player/enemy data");
            return;
        }
        
        int playerATK = playerStats.attack;
        int playerDEF = playerStats.defense;
        int playerHP = playerStats.maxHP;
        
        int wins = 0;
        int losses = 0;
        int simulations = 100;
        
        foreach (var enemy in enemyDatabase)
        {
            int enemyWins = 0;
            
            for (int i = 0; i < simulations; i++)
            {
                int tempHP = playerHP;
                bool survived = SimulateSingleBattle(playerATK, playerDEF, ref tempHP, enemy);
                if (survived) wins++;
                else
                {
                    losses++;
                    enemyWins++;
                }
            }
            
            float winRate = (1f - (float)enemyWins / simulations) * 100f;
            AddLog($"{enemy.enemyName}: Win Rate {winRate:F1}% ({enemy.maxHP}HP, {enemy.attack}ATK)");
        }
        
        float overallWinRate = (float)wins / (simulations * enemyDatabase.Count) * 100f;
        AddLog($"\nOverall Win Rate: {overallWinRate:F1}%");
        AddLog("");
    }
    
    // 스탯 곡선 분석
    private void AnalyzeStatCurves()
    {
        AddLog("--- Stat Curve Analysis ---");
        
        if (playerStats == null || enemyDatabase == null || enemyDatabase.Count == 0)
        {
            AddLog("ERROR: Missing player/enemy data");
            return;
        }
        
        // 플레이어 스탯 범위
        int minPlayerATK = playerStats.attack;
        int maxPlayerATK = playerStats.attack + (maxFloors * 2); // 레벨업 가정
        
        // 적 스탯 범위
        int minEnemyHP = enemyDatabase.Min(e => e.maxHP);
        int maxEnemyHP = enemyDatabase.Max(e => e.maxHP);
        int avgEnemyHP = Mathf.RoundToInt((float)enemyDatabase.Average(e => e.maxHP));
        
        AddLog($"Player ATK Range: {minPlayerATK} ~ {maxPlayerATK}");
        AddLog($"Enemy HP Range: {minEnemyHP} ~ {maxEnemyHP} (Avg: {avgEnemyHP})");
        
        // 데미지 계산 체크
        int earlyGameDamage = Mathf.Max(1, (minPlayerATK * 2 - 5) / 2); // 초반 적 방어력 5 가정
        int lateGameDamage = Mathf.Max(1, (maxPlayerATK * 2 - 15) / 2); // 후반 적 방어력 15 가정
        
        int earlyGameTurns = Mathf.CeilToInt((float)avgEnemyHP / earlyGameDamage);
        int lateGameTurns = Mathf.CeilToInt((float)maxEnemyHP / lateGameDamage);
        
        AddLog($"Early Game: ~{earlyGameTurns} turns to kill avg enemy");
        AddLog($"Late Game: ~{lateGameTurns} turns to kill strongest enemy");
        
        if (lateGameTurns > 20)
        {
            AddLog("⚠️ Late game combat may be too long");
        }
        
        AddLog("");
    }
    
    // 층에 맞는 적 선택
    private EnemyStats GetEnemyForFloor(int floor)
    {
        if (enemyDatabase == null || enemyDatabase.Count == 0) return null;
        
        // 층에 따라 적 난이도 조정 (간단한 구현)
        var availableEnemies = enemyDatabase.Where(e => e.maxHP <= floor * 10).ToList();
        if (availableEnemies.Count == 0) availableEnemies = enemyDatabase;
        
        return availableEnemies[Random.Range(0, availableEnemies.Count)];
    }
    
    // 평균 경험치 계산
    private int CalculateAverageExp()
    {
        if (enemyDatabase == null || enemyDatabase.Count == 0) return 10;
        
        // 적의 HP를 기반으로 경험치 추정 (HP * 0.1)
        return Mathf.RoundToInt((float)enemyDatabase.Average(e => e.maxHP) * 0.1f);
    }
    
    private void AddLog(string message)
    {
        simulationResults += message + "\n";
    }
    
    [ContextMenu("Quick Test - Level 10 vs Boss")]
    public void QuickTestLevel10()
    {
        simulationResults = "";
        AddLog("=== QUICK TEST: Level 10 Player vs Bosses ===\n");
        
        int playerLevel = 10;
        int playerATK = playerStats.attack + (playerLevel - 1) * 2;
        int playerDEF = playerStats.defense + (playerLevel - 1) * 1;
        int playerHP = playerStats.maxHP + (playerLevel - 1) * 10;
        
        AddLog($"Player Level {playerLevel}: HP {playerHP}, ATK {playerATK}, DEF {playerDEF}\n");
        
        var bosses = enemyDatabase.Where(e => 
            e.maxHP > playerHP || 
            e.enemyName.ToLower().Contains("boss")
        ).OrderByDescending(e => e.maxHP).Take(5).ToList();
        
        if (bosses.Count == 0)
        {
            bosses = enemyDatabase.OrderByDescending(e => e.maxHP).Take(5).ToList();
        }
        
        foreach (var boss in bosses)
        {
            AnalyzeBossDifficulty(boss, playerATK, playerDEF, playerHP);
            AddLog("");
        }
        
        Debug.Log(simulationResults);
    }
}

