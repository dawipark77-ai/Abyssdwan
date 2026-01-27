using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillUI : MonoBehaviour
{
    public BattleManager battleManager;
    public SkillDataList skillDataList;
    public Button strongSlashButton;

    void Start()
    {
        if (strongSlashButton != null)
        {
            strongSlashButton.onClick.AddListener(OnStrongSlashClicked);
        }
    }

    void OnStrongSlashClicked()
    {
        if (battleManager == null || skillDataList == null) return;

        SkillData skill = skillDataList.GetSkillByID("01");
        if (skill != null)
        {
            battleManager.UsePlayerSkill(skill);
        }
    }
}
