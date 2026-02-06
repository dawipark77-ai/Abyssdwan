namespace AbyssdawnBattle
{
    public enum UsageType { Active, Passive }
    public enum DamageType { None = 0, Physical, Magic }
    public enum ScaleStat { None = 0, Attack, Defense, Magic, Agility, Luck, CurrentHPPercent, CurrentMPPercent }
    public enum StatusEffect { None, Ignite, Poison, Stun, Slow, Buff }
    public enum EffectType
    {
        None = 0,
        Damage,
        Recovery,
        BuffAttack,
        BuffDefense,
        BuffAgility,
        BuffMagic,
        BuffLuck,
        PassiveAttack,
        PassiveDefense,
        PassiveAgility,
        PassiveMagic,
        PassiveLuck,
        PassiveAccuracy,
        DebuffAttack,
        DebuffDefense,
        DebuffAgility,
        DebuffMagic,
        DebuffLuck,
        Stun,
        Silence
    }
    public enum RecoveryTarget { HP, MP, Both }
    public enum TriggerCondition { None, OnDefense, OnAttacked, OnAttack, TurnStart, LowHP }
}
