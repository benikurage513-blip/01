using System;
using System.Collections.Generic;
using UnityEngine;

namespace GhostAndGourmet
{
    public enum NodeType
    {
        Battle,
        Elite,
        Rest,
        Event
    }

    public enum CardRole
    {
        Definition,
        Execution
    }

    public enum CardEffectType
    {
        ApplyFlagAndHeal,
        ApplyFlagAndShield,
        ApplyFlagAndAttackDown,
        DealDamageWithFlagBonus,
        DealDamageWithFlatBonus,
        DealDamageAndStunIfFlag,
        HealAndDraw,
        ScaleDamageFromDefinitionsInHand
    }

    public enum RelicEffectType
    {
        HealAtBattleStart,
        IncreaseMaxCost,
        ApplyFlagAtBattleStart
    }

    public enum RunMode
    {
        Hub,
        Map,
        Battle,
        Reward,
        RestChoice,
        Cleared,
        Defeat
    }

    [Serializable]
    public class DifficultyDefinition
    {
        public string Id = "routine";
        public string DisplayName = "Routine";
        public int FloorCount = 15;
        public float EnemyHpScale = 1f;
        public float EnemyAttackScale = 1f;
        public int ReverseRuleUses = 1;
        public bool ApplyNightmareDebuffAfterReverse;
        [TextArea(2, 4)] public string Summary = string.Empty;
    }

    [Serializable]
    public class FurnitureDefinition
    {
        public string Id = "tea-table";
        public string DisplayName = "Tea Table";
        public int Sweet;
        public int Mystic;
        public int Cool;
        [TextArea(2, 4)] public string Summary = string.Empty;
    }

    [Serializable]
    public class CardDefinition
    {
        public string Id = "marshmallow-mark";
        public string DisplayName = "Marshmallow Mark";
        public CardRole Role = CardRole.Definition;
        public CardEffectType EffectType = CardEffectType.ApplyFlagAndHeal;
        public int Cost = 1;
        public string FlagId = string.Empty;
        public int BaseValue;
        public int BonusValue;
        public float Multiplier = 1f;
        public float HealRatio;
        public int DrawAmount;
        public bool ConsumeFlag = true;
        [TextArea(2, 4)] public string RulesText = string.Empty;
    }

    [Serializable]
    public class RelicDefinition
    {
        public string Id = "sugar-ribbon";
        public string DisplayName = "Sugar Ribbon";
        public RelicEffectType EffectType = RelicEffectType.HealAtBattleStart;
        public string FlagId = string.Empty;
        public int Value;
        [TextArea(2, 4)] public string RulesText = string.Empty;
    }

    [Serializable]
    public class EventDefinition
    {
        public string Title = "Spilled Pudding";
        [TextArea(2, 4)] public string Description = string.Empty;
        public int SweetDelta;
        public int MysticDelta;
        public int CoolDelta;
        public string GrantedDefinitionId = string.Empty;
    }

    [Serializable]
    public class CardState
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public CardRole Role;
        public CardEffectType EffectType;
        public int Cost;
        public string FlagId = string.Empty;
        public int BaseValue;
        public int BonusValue;
        public float Multiplier = 1f;
        public float HealRatio;
        public int DrawAmount;
        public bool ConsumeFlag = true;
        public string RulesText = string.Empty;
        public int UpgradeLevel;

        public CardState Clone()
        {
            return (CardState)MemberwiseClone();
        }

        public string GetDisplayTitle()
        {
            return UpgradeLevel > 0 ? $"{DisplayName} +{UpgradeLevel}" : DisplayName;
        }
    }

    [Serializable]
    public class MapNodeState
    {
        public string Id = string.Empty;
        public string Label = string.Empty;
        public NodeType Type;
        public int Depth;
        public bool IsBoss;
    }

    [Serializable]
    public class EnemyState
    {
        public string Name = string.Empty;
        public int Hp;
        public int MaxHp;
        public int Attack;
        public int TemporaryAttackModifier;
        public bool SkipNextAttack;
        public MapNodeState SourceNode = new MapNodeState();
        public List<string> Flags = new List<string>();
    }

    [Serializable]
    public class RunState
    {
        public DifficultyDefinition Difficulty = new DifficultyDefinition();
        public int CurrentFloor = 1;
        public RunMode Mode = RunMode.Map;
        public int MaxHp;
        public int Hp;
        public int MaxCost;
        public int Cost;
        public int Block;
        public int DrawPerTurn;
        public int ReverseUsesLeft;
        public int RerollsLeft;
        public bool ReverseActive;
        public bool NightmareDebuffApplied;
        public int Sweet;
        public int Mystic;
        public int Cool;
        public List<MapNodeState[]> Map = new List<MapNodeState[]>();
        public List<CardState> Deck = new List<CardState>();
        public List<CardState> DrawPile = new List<CardState>();
        public List<CardState> DiscardPile = new List<CardState>();
        public List<CardState> Hand = new List<CardState>();
        public List<string> RelicIds = new List<string>();
        public List<string> Log = new List<string>();
        public EnemyState Enemy;
        public bool PendingReward;
        public bool PendingRestChoice;
        public string PendingRestNodeId = string.Empty;
    }
}
