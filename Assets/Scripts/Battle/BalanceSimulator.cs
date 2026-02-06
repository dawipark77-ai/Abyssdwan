using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 諛몃윴???쒕??덉씠???ㅽ겕由쏀듃: ?덈꺼???띾룄, 蹂댁뒪 ?쒖씠?? ?섏쟾 ?대━??媛?μ꽦 ?깆쓣 ?쒕??덉씠??/// </summary>
public class BalanceSimulator : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public BattleManager battleManager;
    public List<EnemyStats> enemyDatabase = new List<EnemyStats>(); // 紐⑤뱺 ???곗씠??    
    [Header("Simulation Settings")]
    public int maxFloors = 50;
    public int battlesPerFloor = 3; // 痢듬떦 ?됯퇏 ?꾪닾 ?잛닔
    public float avgBattlesPerFloor = 3f;
    
    [Header("Results")]
    [TextArea(10, 20)]
    public string simulationResults = "";
    
    [ContextMenu("Run Full Simulation")]
    public void RunFullSimulation()
    {
        simulationResults = "";
        AddLog("=== BALANCE SIMULATION START ===\n");
        
        // 1. ?덈꺼???띾룄 ?쒕??덉씠??
        SimulateLevelProgression();
        
        // 2. 蹂댁뒪 ?쒖씠??泥댄겕
        SimulateBossDifficulty();
        
        // 3. 50痢??대━???쒕??덉씠??
        SimulateDungeonClear();
        
        // 4. ?꾪닾 ?앹〈??遺꾩꽍
        SimulateCombatSurvival();
        
        // 5. ?ㅽ꺈 怨≪꽑 遺꾩꽍
        AnalyzeStatCurves();
        
        AddLog("\n=== SIMULATION COMPLETE ===");
        Debug.Log(simulationResults);
    }
    
    // ?덈꺼???띾룄 ?쒕??덉씠??
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
        
        // 50痢듦퉴吏 ?쒕??덉씠??
        for (int floor = 1; floor <= maxFloors; floor++)
        {
            int floorBattles = Mathf.RoundToInt(avgBattlesPerFloor);
            int floorEnemies = floorBattles * UnityEngine.Random.Range(1, 3); // ?꾪닾??1~2留덈━ ?됯퇏
            
            totalExp += floorEnemies * expPerEnemy;
            battlesCompleted += floorBattles;
            
            // ?덈꺼??泥댄겕 (媛꾨떒??寃쏀뿕移?怨≪꽑: ?덈꺼 * 100)
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
    
    // 蹂댁뒪 ?쒖씠??泥댄겕
    private void SimulateBossDifficulty()
    {
        AddLog("--- Boss Difficulty Check ---");
        
        if (playerStats == null || enemyDatabase == null || enemyDatabase.Count == 0)
        {
            AddLog("ERROR: Missing player/enemy data");
            return;
        }
        
        // ?뚮젅?댁뼱 湲곗? ?ㅽ꺈 (?덈꺼 10 媛??
        int playerLevel = 10;
        int playerATK = playerStats.attack + (playerLevel - 1) * 2; // ?덈꺼??+2 怨듦꺽??
        int playerDEF = playerStats.defense + (playerLevel - 1) * 1;
        int playerHP = playerStats.maxHP + (playerLevel - 1) * 10;
        
        AddLog($"Player Level {playerLevel} Stats:");
        AddLog($"  HP: {playerHP}, ATK: {playerATK}, DEF: {playerDEF}");
        
        // 蹂댁뒪 李얘린 (HP媛 ?믨굅???대쫫??Boss ?ы븿)
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
        
        // ?뚮젅?댁뼱媛 蹂댁뒪瑜??곕윭?⑤━?????꾩슂??????(???
        int playerDamagePerTurn = Mathf.Max(1, (playerATK * 2 - boss.defense) / 2);
        int turnsToKill = Mathf.CeilToInt((float)boss.maxHP / playerDamagePerTurn);
        
        // 蹂댁뒪媛 ?뚮젅?댁뼱瑜??곕윭?⑤━?????꾩슂??????
        int bossDamagePerTurn = Mathf.Max(1, (boss.attack * 2 - playerDEF) / 2);
        int turnsToDie = Mathf.CeilToInt((float)playerHP / bossDamagePerTurn);
        
        AddLog($"  Player needs ~{turnsToKill} turns to kill boss");
        AddLog($"  Boss needs ~{turnsToDie} turns to kill player");
        
        float difficultyRatio = (float)turnsToKill / turnsToDie;
        
        if (difficultyRatio > 2.0f)
        {
            AddLog($"  ?좑툘 VERY HARD - Player will likely die (Ratio: {difficultyRatio:F2})");
        }
        else if (difficultyRatio > 1.5f)
        {
            AddLog($"  ?좑툘 HARD - Challenging but possible (Ratio: {difficultyRatio:F2})");
        }
        else if (difficultyRatio > 0.7f)
        {
            AddLog($"  ??BALANCED - Fair fight (Ratio: {difficultyRatio:F2})");
        }
        else
        {
            AddLog($"  ?좑툘 EASY - Boss too weak (Ratio: {difficultyRatio:F2})");
        }
    }
    
    // 50痢??대━???쒕??덉씠??
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
                // ???좏깮 (痢듭뿉 ?곕씪 ?쒖씠???곸듅)
                EnemyStats enemy = GetEnemyForFloor(floor);
                if (enemy == null) continue;
                
                // ?꾪닾 ?쒕??덉씠??
                bool survived = SimulateSingleBattle(playerATK, playerDEF, ref playerHP, enemy);
                
                if (!survived)
                {
                    died = true;
                    deathFloor = floor;
                    break;
                }
                
                // ?덈꺼??(媛꾨떒??
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
            AddLog($"??Player died at Floor {deathFloor}, Level {currentLevel}");
        }
        else
        {
            AddLog($"??Player cleared Floor {maxFloors} at Level {currentLevel}");
        }
        
        AddLog("");
    }
    
    // ?⑥씪 ?꾪닾 ?쒕??덉씠??
    private bool SimulateSingleBattle(int playerATK, int playerDEF, ref int playerHP, EnemyStats enemy)
    {
        int enemyHP = enemy.maxHP;
        int maxTurns = 50; // 臾댄븳 猷⑦봽 諛⑹?
        int turn = 0;
        
        while (turn < maxTurns && playerHP > 0 && enemyHP > 0)
        {
            // ?뚮젅?댁뼱 ??
            int playerDamage = Mathf.Max(1, (playerATK * 2 - enemy.defense) / 2);
            enemyHP -= playerDamage;
            if (enemyHP <= 0) break;
            
            // ????
            int enemyDamage = Mathf.Max(1, (enemy.attack * 2 - playerDEF) / 2);
            playerHP -= enemyDamage;
            if (playerHP <= 0) return false;
            
            turn++;
        }
        
        return playerHP > 0;
    }
    
    // ?꾪닾 ?앹〈??遺꾩꽍
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
    
    // ?ㅽ꺈 怨≪꽑 遺꾩꽍
    private void AnalyzeStatCurves()
    {
        AddLog("--- Stat Curve Analysis ---");
        
        if (playerStats == null || enemyDatabase == null || enemyDatabase.Count == 0)
        {
            AddLog("ERROR: Missing player/enemy data");
            return;
        }
        
        // ?뚮젅?댁뼱 ?ㅽ꺈 踰붿쐞
        int minPlayerATK = playerStats.attack;
        int maxPlayerATK = playerStats.attack + (maxFloors * 2); // ?덈꺼??媛??        
        // ???ㅽ꺈 踰붿쐞
        int minEnemyHP = enemyDatabase.Min(e => e.maxHP);
        int maxEnemyHP = enemyDatabase.Max(e => e.maxHP);
        int avgEnemyHP = Mathf.RoundToInt((float)enemyDatabase.Average(e => e.maxHP));
        
        AddLog($"Player ATK Range: {minPlayerATK} ~ {maxPlayerATK}");
        AddLog($"Enemy HP Range: {minEnemyHP} ~ {maxEnemyHP} (Avg: {avgEnemyHP})");
        
        // ?곕?吏 怨꾩궛 泥댄겕
        int earlyGameDamage = Mathf.Max(1, (minPlayerATK * 2 - 5) / 2); // 珥덈컲 ??諛⑹뼱??5 媛??
        int lateGameDamage = Mathf.Max(1, (maxPlayerATK * 2 - 15) / 2); // ?꾨컲 ??諛⑹뼱??15 媛??
        int earlyGameTurns = Mathf.CeilToInt((float)avgEnemyHP / earlyGameDamage);
        int lateGameTurns = Mathf.CeilToInt((float)maxEnemyHP / lateGameDamage);
        
        AddLog($"Early Game: ~{earlyGameTurns} turns to kill avg enemy");
        AddLog($"Late Game: ~{lateGameTurns} turns to kill strongest enemy");
        
        if (lateGameTurns > 20)
        {
            AddLog("?좑툘 Late game combat may be too long");
        }
        
        AddLog("");
    }
    
    // 痢듭뿉 留욌뒗 ???좏깮
    private EnemyStats GetEnemyForFloor(int floor)
    {
        if (enemyDatabase == null || enemyDatabase.Count == 0) return null;
        
        // 痢듭뿉 ?곕씪 ???쒖씠??議곗젙 (媛꾨떒??援ы쁽)
        var availableEnemies = enemyDatabase.Where(e => e.maxHP <= floor * 10).ToList();
        if (availableEnemies.Count == 0) availableEnemies = enemyDatabase;
        
        return availableEnemies[UnityEngine.Random.Range(0, availableEnemies.Count)];
    }
    
    // ?됯퇏 寃쏀뿕移?怨꾩궛
    private int CalculateAverageExp()
    {
        if (enemyDatabase == null || enemyDatabase.Count == 0) return 10;
        
        // ?곸쓽 HP瑜?湲곕컲?쇰줈 寃쏀뿕移?異붿젙 (HP * 0.1)
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


