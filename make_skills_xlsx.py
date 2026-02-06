from pathlib import Path

skills = [
    {"Skill ID / Name": "01 Strong Slash", "Damage Type": "Physical", "Scale Stat": "Attack", "Cost": "HP 10% / MP 0", "Multiplier": "2.0 ~ 2.5", "Self Damage": "0% (chance 0%)", "Target Curse Type": "", "Target Curse Chance": "0%", "Self Curse Type": "", "Self Curse Chance": "0%"},
    {"Skill ID / Name": "02 Fireball", "Damage Type": "Magic", "Scale Stat": "Magic", "Cost": "HP 0% / MP 5", "Multiplier": "1.8 ~ 2.3", "Self Damage": "10% (chance 0%)", "Target Curse Type": "", "Target Curse Chance": "0%", "Self Curse Type": "", "Self Curse Chance": "0%"},
    {"Skill ID / Name": "03 Slash", "Damage Type": "Physical", "Scale Stat": "Attack", "Cost": "HP 5% / MP 0", "Multiplier": "1.5 ~ 1.8", "Self Damage": "0% (chance 0%)", "Target Curse Type": "", "Target Curse Chance": "0%", "Self Curse Type": "", "Self Curse Chance": "0%"},
    {"Skill ID / Name": "04 Meditation", "Damage Type": "Magic", "Scale Stat": "Magic", "Cost": "HP 0% / MP 0", "Multiplier": "0 ~ 0", "Self Damage": "0% (chance 0%)", "Target Curse Type": "", "Target Curse Chance": "0%", "Self Curse Type": "", "Self Curse Chance": "0%"},
    {"Skill ID / Name": "05 Magic Bolt", "Damage Type": "Magic", "Scale Stat": "Magic", "Cost": "HP 0% / MP 3", "Multiplier": "1.2 ~ 1.3", "Self Damage": "10% (chance 5%)", "Target Curse Type": "", "Target Curse Chance": "0%", "Self Curse Type": "", "Self Curse Chance": "0%"},
    {"Skill ID / Name": "06 Quickhand", "Damage Type": "Physical", "Scale Stat": "Agility", "Cost": "HP 0% / MP 0", "Multiplier": "0.6 ~ 0.8", "Self Damage": "0% (chance 0%)", "Target Curse Type": "", "Target Curse Chance": "0%", "Self Curse Type": "", "Self Curse Chance": "0%"},
    {"Skill ID / Name": "07 Shield Wall", "Damage Type": "Physical", "Scale Stat": "Attack", "Cost": "HP 0% / MP 0", "Multiplier": "0 ~ 0", "Self Damage": "0% (chance 0%)", "Target Curse Type": "", "Target Curse Chance": "0%", "Self Curse Type": "", "Self Curse Chance": "0%"},
]

output_path = Path("SkillDataList.xlsx")

from openpyxl import Workbook

wb = Workbook()
ws = wb.active
ws.title = "Skills"
headers = list(skills[0].keys())
ws.append(headers)
for row in skills:
    ws.append([row[h] for h in headers])
wb.save(output_path)
print(f"Saved: {output_path.resolve()}")







