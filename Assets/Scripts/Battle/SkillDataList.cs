using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillDataList", menuName = "Battle/SkillDataList")]
public class SkillDataList : ScriptableObject
{
    public List<SkillData> skillList = new List<SkillData>();

    // 스킬ID로 검색
    public SkillData GetSkillByID(string id)
    {
        return skillList.Find(skill => skill.skillID == id);
    }

    // Initialize Skills
    private void OnEnable()
    {
        // 01 Strong Slash (강베기): HP 10%, 2.0-2.5x Dmg
        // 아이콘은 GUID를 사용하여 로드: 3955b202bf839314a830086e1903dd93
        EnsureSkill("01", "Strong Slash", "Physical", 10f, 0f, 2.0f, 2.5f, iconGUID: "3955b202bf839314a830086e1903dd93");

        // 02 Fireball (파이어볼): MP 5, 1.8-2.3x Dmg (Magic), 10% Ignite (handled in BattleManager), 10% Self-Ignite
        // 아이콘은 GUID를 사용하여 로드: c43880bdb1e15444a8fe104ce0204f40
        EnsureSkill("02", "Fireball", "Magic", 0f, 5f, 1.8f, 2.3f, 1, "Magic", 10f, 0f, iconGUID: "c43880bdb1e15444a8fe104ce0204f40"); // Self-ignite handled as special case or via selfDamageChance if generic

        // 03 Slash (베기): HP 5%, 1.5-1.8x Dmg
        EnsureSkill("03", "Slash", "Physical", 5f, 0f, 1.5f, 1.8f);

        // 04 Meditation (명상): MP +7% Recovery
        EnsureSkill("04", "Meditation", "Magic", 0f, 0f, 0f, 0f, 1, "Magic", 0f, 0f, true, false, 7f);

        // 05 Magic Bolt (마력탄): MP 3, 1.2-1.3x Dmg (Magic), 10% Chance Self Dmg 5% HP
        // 아이콘은 GUID를 사용하여 로드: 5e22fb5704628704bb694d9eefe8dc40
        EnsureSkill("05", "Magic Bolt", "Magic", 0f, 3f, 1.2f, 1.3f, 1, "Magic", 10f, 5f, iconGUID: "5e22fb5704628704bb694d9eefe8dc40");

        // 06 Quickhand (빠른손놀림): Agility 0.6-0.8x Dmg, 2 Hits
        // 아이콘은 GUID를 사용하여 로드: 427edb27ad7da094b984c5f369e757da
        EnsureSkill("06", "Quickhand", "Physical", 0f, 0f, 0.6f, 0.8f, 2, "Agility", iconGUID: "427edb27ad7da094b984c5f369e757da");

        // 07 Shield Wall (대방패): Defense +20% (Buff)
        EnsureSkill("07", "Shield Wall", "Physical", 0f, 0f, 0f, 0f, 1, "Attack", 0f, 0f, false, true, 20f);
    }

    private void EnsureSkill(string id, string name, string type, float hpCostPercent, float mpCost, float minMultiplier, float maxMultiplier,
                             int hitCount = 1, string scalingStat = "Attack", float selfDamageChance = 0f, float selfDamagePercent = 0f,
                             bool isRecovery = false, bool isDefensive = false, float effectValue = 0f, string iconPath = "", string iconGUID = "")
    {
        SkillData existing = skillList.Find(s => s.skillID == id);
        Sprite iconSprite = null;

#if UNITY_EDITOR
        // GUID를 사용한 로드 (가장 확실한 방법)
        if (!string.IsNullOrEmpty(iconGUID))
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(iconGUID);
            if (!string.IsNullOrEmpty(assetPath))
            {
                iconSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (iconSprite == null)
                {
                    // Sprite로 직접 로드되지 않으면 Texture2D로 로드 후 변환
                    Texture2D texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                    if (texture != null)
                    {
                        iconSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    }
                    else
                    {
                        // LoadAllAssetsAtPath로 서브에셋 로드 시도
                        var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);
                        foreach (var asset in assets)
                        {
                            if (asset is Sprite s)
                            {
                                iconSprite = s;
                                break;
                            }
                        }
                    }
                }
                if (iconSprite != null)
                {
                    Debug.Log($"[SkillDataList] Loaded icon for {name} using GUID: {iconGUID}");
                }
                else
                {
                    Debug.LogWarning($"[SkillDataList] Could not load icon using GUID: {iconGUID}, path: {assetPath}");
                }
            }
        }
        // 경로를 사용한 로드 (하위 호환성)
        else if (!string.IsNullOrEmpty(iconPath))
        {
            // 경로 정규화 (백슬래시를 슬래시로 변환)
            string normalizedPath = iconPath.Replace('\\', '/');
            
            // 경로가 Assets로 시작하지 않으면 추가
            if (!normalizedPath.StartsWith("Assets/"))
            {
                normalizedPath = "Assets/" + normalizedPath.TrimStart('/');
            }
            
            Debug.Log($"[SkillDataList] Attempting to load icon for {name} at path: {normalizedPath}");
            
            // 먼저 Sprite로 직접 로드 시도
            iconSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(normalizedPath);
            
            // Sprite로 직접 로드되지 않으면 (Texture2D로 임포트된 경우)
            if (iconSprite == null)
            {
                // LoadAllAssetsAtPath로 모든 서브에셋 로드 (Multiple sprite mode인 경우)
                var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(normalizedPath);
                Debug.Log($"[SkillDataList] LoadAllAssetsAtPath returned {assets?.Length ?? 0} assets");
                if (assets != null)
                {
                    foreach (var asset in assets)
                    {
                        Debug.Log($"[SkillDataList] Found asset type: {asset?.GetType()}, name: {asset?.name}");
                        if (asset is Sprite s)
                        {
                            iconSprite = s;
                            Debug.Log($"[SkillDataList] Loaded icon sprite for {name}: {s.name}");
                            break;
                        }
                    }
                }
            }
            else
            {
                Debug.Log($"[SkillDataList] Loaded icon sprite for {name}: {iconSprite.name}");
            }
            
            // 여전히 로드되지 않으면 Texture2D로 로드 후 Sprite로 변환 시도
            if (iconSprite == null)
            {
                Texture2D texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(normalizedPath);
                if (texture != null)
                {
                    // Texture2D를 Sprite로 변환
                    iconSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    Debug.Log($"[SkillDataList] Converted texture to sprite for {name}");
                }
                else
                {
                    // 파일이 실제로 존재하는지 확인
                    // Application.dataPath는 이미 Assets 폴더를 포함하므로, normalizedPath에서 Assets/를 제거해야 함
                    string pathWithoutAssets = normalizedPath.StartsWith("Assets/") ? normalizedPath.Substring(7) : normalizedPath;
                    string fullPath = System.IO.Path.Combine(UnityEngine.Application.dataPath, pathWithoutAssets);
                    // 경로 구분자 정규화
                    fullPath = fullPath.Replace('\\', '/');
                    bool fileExists = System.IO.File.Exists(fullPath);
                    Debug.LogWarning($"[SkillDataList] Could not load any Sprite or Texture2D at path: {normalizedPath}");
                    Debug.LogWarning($"[SkillDataList] Full system path: {fullPath}, File exists: {fileExists}");
                    
                    // 경로 문제 해결을 위한 추가 시도: 직접 파일 시스템에서 로드
                    if (fileExists)
                    {
                        try
                        {
                            byte[] fileData = System.IO.File.ReadAllBytes(fullPath);
                            Texture2D loadedTexture = new Texture2D(2, 2);
                            if (loadedTexture.LoadImage(fileData))
                            {
                                iconSprite = Sprite.Create(loadedTexture, new Rect(0, 0, loadedTexture.width, loadedTexture.height), new Vector2(0.5f, 0.5f));
                                Debug.Log($"[SkillDataList] Loaded icon from file system for {name}");
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"[SkillDataList] Failed to load icon from file system: {e.Message}");
                        }
                    }
                    else
                    {
                        // 대체 경로 시도: 프로젝트 루트 기준
                        string projectRoot = System.IO.Directory.GetParent(UnityEngine.Application.dataPath).FullName;
                        string altPath = System.IO.Path.Combine(projectRoot, normalizedPath);
                        altPath = altPath.Replace('\\', '/');
                        Debug.LogWarning($"[SkillDataList] Trying alternative path: {altPath}");
                        if (System.IO.File.Exists(altPath))
                        {
                            try
                            {
                                byte[] fileData = System.IO.File.ReadAllBytes(altPath);
                                Texture2D altTexture = new Texture2D(2, 2);
                                if (altTexture.LoadImage(fileData))
                                {
                                    iconSprite = Sprite.Create(altTexture, new Rect(0, 0, altTexture.width, altTexture.height), new Vector2(0.5f, 0.5f));
                                    Debug.Log($"[SkillDataList] Loaded icon from alternative path for {name}");
                                }
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogError($"[SkillDataList] Failed to load icon from alternative path: {e.Message}");
                            }
                        }
                    }
                }
            }
        }
#endif

        if (existing == null)
        {
            var newSkill = new SkillData(id, name, type, hpCostPercent, mpCost, minMultiplier, maxMultiplier, 
                                        hitCount, scalingStat, selfDamageChance, selfDamagePercent, isRecovery, isDefensive, effectValue);
            newSkill.icon = iconSprite;
            skillList.Add(newSkill);
        }
        else
        {
            existing.skillName = name;
            existing.skillType = type;
            existing.hpCostPercent = hpCostPercent;
            existing.mpCost = mpCost;
            existing.minMultiplier = minMultiplier;
            existing.maxMultiplier = maxMultiplier;
            
            existing.hitCount = hitCount;
            existing.scalingStat = scalingStat;
            existing.selfDamageChance = selfDamageChance;
            existing.selfDamagePercent = selfDamagePercent;
            existing.isRecovery = isRecovery;
            existing.isDefensive = isDefensive;
            existing.isRecovery = isRecovery;
            existing.isDefensive = isDefensive;
            existing.effectValue = effectValue;
            if (iconSprite != null) existing.icon = iconSprite;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
