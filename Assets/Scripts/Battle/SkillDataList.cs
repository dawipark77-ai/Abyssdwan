using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SkillData = AbyssdawnBattle.SkillData;

[CreateAssetMenu(fileName = "SkillDataList", menuName = "Battle/SkillDataList")]
public class SkillDataList : ScriptableObject
{
    public List<SkillData> skillList = new List<SkillData>();

    // 스킬ID로 검색
    public SkillData GetSkillByID(string id)
    {
        if (skillList == null || skillList.Count == 0)
        {
            RefreshFromResources();
        }

        return skillList.Find(skill => skill != null && skill.skillID == id);
    }

    private void OnEnable()
    {
        if (skillList == null || skillList.Count == 0)
        {
            RefreshFromResources();
        }
    }

    public void RefreshFromResources()
    {
        var loaded = Resources.LoadAll<SkillData>("Skills");
        skillList = loaded != null
            ? loaded.OrderBy(s => s.skillID).ToList()
            : new List<SkillData>();
    }
}
