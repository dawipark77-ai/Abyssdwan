#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 직업 시스템 초기 설정 도구
/// 워리어 및 기본 직업 에셋을 자동 생성
/// </summary>
public class CharacterClassSetupTool : EditorWindow
{
    [MenuItem("Tools/Game Setup/직업 시스템 설정")]
    public static void ShowWindow()
    {
        GetWindow<CharacterClassSetupTool>("직업 시스템 설정");
    }

    [MenuItem("Tools/Game Setup/워리어 직업 생성")]
    public static void CreateWarriorClass()
    {
        CreateWarriorClassAsset();
        CreateOrUpdateDatabase();
        Debug.Log("✅ 워리어 직업이 성공적으로 생성되었습니다!");
    }

    [MenuItem("Tools/Game Setup/도적(Thief) 직업 생성")]
    public static void CreateThiefClass()
    {
        CreateThiefClassAsset();
        CreateOrUpdateDatabase();
        Debug.Log("✅ 도적(Thief) 직업이 성공적으로 생성되었습니다!");
    }

    [MenuItem("Tools/Game Setup/위저드(Wizard) 직업 생성")]
    public static void CreateWizardClass()
    {
        CreateWizardClassAsset();
        CreateOrUpdateDatabase();
        Debug.Log("✅ 위저드(Wizard) 직업이 성공적으로 생성되었습니다!");
    }

    private void OnGUI()
    {
        GUILayout.Label("직업 시스템 설정 도구", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "이 도구는 기본 직업 에셋을 생성합니다.\n" +
            "Resources/Classes 폴더에 직업 에셋이 생성됩니다.", 
            MessageType.Info);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("🗡️ 워리어 직업 생성", GUILayout.Height(40)))
        {
            CreateWarriorClassAsset();
            CreateOrUpdateDatabase();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("🗡️ 도적(Thief) 직업 생성", GUILayout.Height(40)))
        {
            CreateThiefClassAsset();
            CreateOrUpdateDatabase();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("🔮 위저드(Wizard) 직업 생성", GUILayout.Height(40)))
        {
            CreateWizardClassAsset();
            CreateOrUpdateDatabase();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("📚 데이터베이스 업데이트", GUILayout.Height(30)))
        {
            CreateOrUpdateDatabase();
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "워리어 보정치:\n" +
            "• 공격: +5\n" +
            "• 방어: +4\n" +
            "• 마력: -3\n" +
            "• 민첩: -2\n" +
            "• 행운: -1\n" +
            "• HP: +30% (기본 100 → 130)\n" +
            "• MP: -10% (기본 10 → 9)",
            MessageType.None);

        EditorGUILayout.Space(5);

        EditorGUILayout.HelpBox(
            "도적(Thief) 보정치:\n" +
            "• 공격: +3\n" +
            "• 방어: -2\n" +
            "• 마력: -1\n" +
            "• 민첩: +5\n" +
            "• 행운: +3\n" +
            "• HP: -15% (기본 100 → 85)\n" +
            "• MP: +3% (기본 10 → 10)",
            MessageType.None);

        EditorGUILayout.Space(5);

        EditorGUILayout.HelpBox(
            "위저드(Wizard) 보정치:\n" +
            "• 공격: -3\n" +
            "• 방어: -3\n" +
            "• 마력: +6\n" +
            "• 민첩: -1\n" +
            "• 행운: ±0\n" +
            "• HP: -25% (기본 100 → 75)\n" +
            "• MP: +40% (기본 10 → 14)",
            MessageType.None);
    }

    /// <summary>
    /// 워리어 직업 에셋 생성
    /// </summary>
    private static void CreateWarriorClassAsset()
    {
        // Resources/Classes 폴더 생성
        string classesPath = "Assets/Resources/Classes";
        if (!AssetDatabase.IsValidFolder(classesPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            AssetDatabase.CreateFolder("Assets/Resources", "Classes");
        }

        // 기존 워리어 에셋 확인
        string warriorPath = $"{classesPath}/Class_Warrior.asset";
        CharacterClass warrior = AssetDatabase.LoadAssetAtPath<CharacterClass>(warriorPath);

        if (warrior == null)
        {
            // 새로 생성
            warrior = ScriptableObject.CreateInstance<CharacterClass>();
            AssetDatabase.CreateAsset(warrior, warriorPath);
        }

        // 워리어 스탯 설정
        warrior.className = "Warrior";
        warrior.description = "근접 전투에 특화된 전사. 높은 공격력과 방어력을 가지지만 마법 능력이 부족합니다.";
        
        // 스탯 보정치: 공격+5, 방어+4, 마력-3, 민첩-2, 행운-1
        warrior.attackBonus = 5;
        warrior.defenseBonus = 4;
        warrior.magicBonus = -3;
        warrior.agilityBonus = -2;
        warrior.luckBonus = -1;
        
        // HP/MP 배율: HP +30%, MP -10%
        warrior.hpMultiplier = 1.3f;    // 기본 100 → 130
        warrior.mpMultiplier = 0.9f;    // 기본 10 → 9

        EditorUtility.SetDirty(warrior);
        AssetDatabase.SaveAssets();

        Debug.Log($"[CharacterClassSetupTool] 워리어 직업 생성됨: {warriorPath}");
        Debug.Log($"  {warrior}");
    }

    /// <summary>
    /// 도적(Thief) 직업 에셋 생성
    /// </summary>
    private static void CreateThiefClassAsset()
    {
        // Resources/Classes 폴더 생성
        string classesPath = "Assets/Resources/Classes";
        if (!AssetDatabase.IsValidFolder(classesPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            AssetDatabase.CreateFolder("Assets/Resources", "Classes");
        }

        // 기존 도적 에셋 확인
        string thiefPath = $"{classesPath}/Class_Thief.asset";
        CharacterClass thief = AssetDatabase.LoadAssetAtPath<CharacterClass>(thiefPath);

        if (thief == null)
        {
            // 새로 생성
            thief = ScriptableObject.CreateInstance<CharacterClass>();
            AssetDatabase.CreateAsset(thief, thiefPath);
        }

        // 도적 스탯 설정
        thief.className = "Thief";
        thief.description = "민첩과 행운에 특화된 도적. 빠른 공격과 회피 능력을 가지지만 방어력이 약합니다.";
        
        // 스탯 보정치: 공격+3, 방어-2, 마력-1, 민첩+5, 행운+3
        thief.attackBonus = 3;
        thief.defenseBonus = -2;
        thief.magicBonus = -1;
        thief.agilityBonus = 5;
        thief.luckBonus = 3;
        
        // HP/MP 배율: HP -15%, MP +3%
        thief.hpMultiplier = 0.85f;   // 기본 100 → 85
        thief.mpMultiplier = 1.03f;   // 기본 10 → 10 (반올림)

        EditorUtility.SetDirty(thief);
        AssetDatabase.SaveAssets();

        Debug.Log($"[CharacterClassSetupTool] 도적(Thief) 직업 생성됨: {thiefPath}");
        Debug.Log($"  {thief}");
    }

    /// <summary>
    /// 위저드(Wizard) 직업 에셋 생성
    /// </summary>
    private static void CreateWizardClassAsset()
    {
        // Resources/Classes 폴더 생성
        string classesPath = "Assets/Resources/Classes";
        if (!AssetDatabase.IsValidFolder(classesPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            AssetDatabase.CreateFolder("Assets/Resources", "Classes");
        }

        // 기존 위저드 에셋 확인
        string wizardPath = $"{classesPath}/Class_Wizard.asset";
        CharacterClass wizard = AssetDatabase.LoadAssetAtPath<CharacterClass>(wizardPath);

        if (wizard == null)
        {
            // 새로 생성
            wizard = ScriptableObject.CreateInstance<CharacterClass>();
            AssetDatabase.CreateAsset(wizard, wizardPath);
        }

        // 위저드 스탯 설정
        wizard.className = "Wizard";
        wizard.description = "강력한 마법에 특화된 마법사. 높은 마력과 MP를 가지지만 물리 공격과 방어력이 약합니다.";
        
        // 스탯 보정치: 공격-3, 방어-3, 마력+6, 민첩-1, 행운±0
        wizard.attackBonus = -3;
        wizard.defenseBonus = -3;
        wizard.magicBonus = 6;
        wizard.agilityBonus = -1;
        wizard.luckBonus = 0;
        
        // HP/MP 배율: HP -25%, MP +40%
        wizard.hpMultiplier = 0.75f;   // 기본 100 → 75
        wizard.mpMultiplier = 1.4f;    // 기본 10 → 14

        EditorUtility.SetDirty(wizard);
        AssetDatabase.SaveAssets();

        Debug.Log($"[CharacterClassSetupTool] 위저드(Wizard) 직업 생성됨: {wizardPath}");
        Debug.Log($"  {wizard}");
    }

    /// <summary>
    /// CharacterClassDatabase 생성/업데이트
    /// </summary>
    private static void CreateOrUpdateDatabase()
    {
        string dbPath = "Assets/Resources/CharacterClassDatabase.asset";
        CharacterClassDatabase database = AssetDatabase.LoadAssetAtPath<CharacterClassDatabase>(dbPath);

        if (database == null)
        {
            database = ScriptableObject.CreateInstance<CharacterClassDatabase>();
            AssetDatabase.CreateAsset(database, dbPath);
            Debug.Log("[CharacterClassSetupTool] 새 CharacterClassDatabase 생성됨");
        }

        // Classes 폴더에서 모든 직업 에셋 찾기
        database.allClasses.Clear();
        string[] guids = AssetDatabase.FindAssets("t:CharacterClass", new[] { "Assets/Resources/Classes" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CharacterClass charClass = AssetDatabase.LoadAssetAtPath<CharacterClass>(path);
            if (charClass != null)
            {
                database.allClasses.Add(charClass);
                Debug.Log($"  추가된 직업: {charClass.className}");
            }
        }

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CharacterClassSetupTool] 데이터베이스 업데이트 완료. 총 {database.allClasses.Count}개 직업");
    }
}
#endif


