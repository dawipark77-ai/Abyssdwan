using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 모든 직업 데이터를 관리하는 데이터베이스
/// Singleton 패턴으로 어디서든 접근 가능
/// </summary>
[CreateAssetMenu(fileName = "CharacterClassDatabase", menuName = "Game/Character Class Database")]
public class CharacterClassDatabase : ScriptableObject
{
    private static CharacterClassDatabase _instance;
    public static CharacterClassDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<CharacterClassDatabase>("CharacterClassDatabase");
                if (_instance == null)
                {
                    Debug.LogError("[CharacterClassDatabase] Resources 폴더에서 CharacterClassDatabase를 찾을 수 없습니다!");
                    Debug.LogError("[CharacterClassDatabase] Assets/Resources/CharacterClassDatabase.asset 파일이 존재하는지 확인하세요!");
                }
                else
                {
                    // 로드 후 Dictionary 빌드
                    _instance.BuildDictionary();
                    Debug.Log($"[CharacterClassDatabase] 데이터베이스 로드 완료. 총 {_instance.allClasses?.Count ?? 0}개 직업");
                }
            }
            return _instance;
        }
    }

    [Header("직업 목록")]
    [Tooltip("게임에서 사용 가능한 모든 직업")]
    public List<CharacterClass> allClasses = new List<CharacterClass>();

    // 이름으로 빠르게 검색하기 위한 Dictionary (런타임 캐시)
    private Dictionary<string, CharacterClass> _classDict;
    
    private void OnEnable()
    {
        BuildDictionary();
    }

    /// <summary>
    /// Dictionary 빌드 (빠른 검색용)
    /// </summary>
    public void BuildDictionary()
    {
        _classDict = new Dictionary<string, CharacterClass>();
        
        if (allClasses == null)
        {
            Debug.LogWarning("[CharacterClassDatabase] allClasses가 null입니다!");
            return;
        }
        
        Debug.Log($"[CharacterClassDatabase] Dictionary 빌드 시작. 총 {allClasses.Count}개 직업");
        
        foreach (var charClass in allClasses)
        {
            if (charClass != null && !string.IsNullOrEmpty(charClass.className))
            {
                if (!_classDict.ContainsKey(charClass.className))
                {
                    _classDict.Add(charClass.className, charClass);
                    Debug.Log($"[CharacterClassDatabase] 직업 등록: {charClass.className}");
                }
                else
                {
                    Debug.LogWarning($"[CharacterClassDatabase] 중복된 직업 이름: {charClass.className}");
                }
            }
            else
            {
                Debug.LogWarning("[CharacterClassDatabase] null이거나 이름이 없는 직업 발견!");
            }
        }
        
        Debug.Log($"[CharacterClassDatabase] Dictionary build complete. Total classes: {_classDict.Count}");
    }

    /// <summary>
    /// 직업 이름으로 직업 데이터 검색
    /// </summary>
    public CharacterClass GetClassByName(string className)
    {
        if (_classDict == null) BuildDictionary();
        
        if (_classDict.TryGetValue(className, out CharacterClass charClass))
        {
            return charClass;
        }
        
        Debug.LogWarning($"[CharacterClassDatabase] '{className}' 직업을 찾을 수 없습니다.");
        return null;
    }

    /// <summary>
    /// 인덱스로 직업 데이터 가져오기 (0부터 시작)
    /// </summary>
    public CharacterClass GetClassByIndex(int index)
    {
        if (index >= 0 && index < allClasses.Count)
        {
            return allClasses[index];
        }
        
        Debug.LogWarning($"[CharacterClassDatabase] 인덱스 {index}는 범위를 벗어났습니다. (총 {allClasses.Count}개)");
        return null;
    }

    /// <summary>
    /// 전체 직업 수
    /// </summary>
    public int ClassCount => allClasses.Count;

    /// <summary>
    /// 직업 존재 여부 확인
    /// </summary>
    public bool HasClass(string className)
    {
        if (_classDict == null) BuildDictionary();
        return _classDict.ContainsKey(className);
    }
}


