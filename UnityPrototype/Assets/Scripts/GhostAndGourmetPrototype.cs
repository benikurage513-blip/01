using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GhostAndGourmet
{
    public class GhostAndGourmetPrototype : MonoBehaviour
    {
        private const int MaxFurnitureSelection = 3;
        private const float LowHpThreshold = 0.2f;

        [Header("Data Seeds")]
        [SerializeField] private List<DifficultyDefinition> difficulties = new List<DifficultyDefinition>();
        [SerializeField] private List<FurnitureDefinition> furnitureCatalog = new List<FurnitureDefinition>();
        [SerializeField] private List<CardDefinition> cardCatalog = new List<CardDefinition>();
        [SerializeField] private List<RelicDefinition> relicCatalog = new List<RelicDefinition>();
        [SerializeField] private List<EventDefinition> eventCatalog = new List<EventDefinition>();

        [Header("Prototype Defaults")]
        [SerializeField] private string selectedDifficultyId = "routine";
        [SerializeField] private List<string> selectedFurnitureIds = new List<string> { "tea-table", "moon-lamp", "vinyl-player" };

        private readonly List<object> rewardOptions = new List<object>();
        private readonly Vector2 defaultWindowSize = new Vector2(1320f, 840f);

        private RunState run;
        private Vector2 hubScroll;
        private Vector2 expeditionScroll;
        private Vector2 logScroll;
        private Vector2 handScroll;
        private GUIStyle cachedHeaderStyle;
        private GUIStyle cachedCardStyle;
        private GUIStyle cachedMutedLabelStyle;

        public void SeedDefaultsIfEmpty()
        {
            if (difficulties.Count == 0)
            {
                difficulties = new List<DifficultyDefinition>
                {
                    new DifficultyDefinition
                    {
                        Id = "picnic",
                        DisplayName = "Picnic",
                        FloorCount = 10,
                        EnemyHpScale = 0.8f,
                        EnemyAttackScale = 0.7f,
                        ReverseRuleUses = 2,
                        Summary = "短めの探索。逆転ルールの余裕があり、基礎挙動の確認向け。"
                    },
                    new DifficultyDefinition
                    {
                        Id = "routine",
                        DisplayName = "Routine",
                        FloorCount = 15,
                        EnemyHpScale = 1f,
                        EnemyAttackScale = 1f,
                        ReverseRuleUses = 1,
                        Summary = "標準難度。拠点タグとカード連携を試しやすい基準設定。"
                    },
                    new DifficultyDefinition
                    {
                        Id = "nightmare",
                        DisplayName = "Nightmare",
                        FloorCount = 20,
                        EnemyHpScale = 1.5f,
                        EnemyAttackScale = 1.3f,
                        ReverseRuleUses = 1,
                        ApplyNightmareDebuffAfterReverse = true,
                        Summary = "長い探索。逆転ルール使用後に永続デバフが入る高難度。"
                    }
                };
            }

            if (furnitureCatalog.Count == 0)
            {
                furnitureCatalog = new List<FurnitureDefinition>
                {
                    new FurnitureDefinition { Id = "tea-table", DisplayName = "Tea Table", Sweet = 1, Summary = "回復寄りの小家具。Sweet を 1 加算。" },
                    new FurnitureDefinition { Id = "moon-lamp", DisplayName = "Moon Lamp", Mystic = 1, Summary = "初手を支える照明。Mystic を 1 加算。" },
                    new FurnitureDefinition { Id = "vinyl-player", DisplayName = "Vinyl Player", Cool = 1, Summary = "ビルド調整向け。Cool を 1 加算。" },
                    new FurnitureDefinition { Id = "parfait-cart", DisplayName = "Parfait Cart", Sweet = 2, Summary = "回復効率を大きく伸ばす大型家具。" },
                    new FurnitureDefinition { Id = "mirror-wardrobe", DisplayName = "Mirror Wardrobe", Mystic = 1, Cool = 1, Summary = "手札の安定性と報酬調整を両立。" },
                    new FurnitureDefinition { Id = "ice-sofa", DisplayName = "Ice Sofa", Cool = 2, Summary = "リロール回数を押し上げる家具。" }
                };
            }

            if (cardCatalog.Count == 0)
            {
                cardCatalog = new List<CardDefinition>
                {
                    new CardDefinition
                    {
                        Id = "marshmallow-mark",
                        DisplayName = "Marshmallow Mark",
                        Role = CardRole.Definition,
                        EffectType = CardEffectType.ApplyFlagAndHeal,
                        Cost = 1,
                        FlagId = "Marshmallow",
                        BaseValue = 2,
                        RulesText = "敵に Marshmallow を付与し、少量回復する。"
                    },
                    new CardDefinition
                    {
                        Id = "sugar-veil",
                        DisplayName = "Sugar Veil",
                        Role = CardRole.Definition,
                        EffectType = CardEffectType.ApplyFlagAndShield,
                        Cost = 1,
                        FlagId = "Glazed",
                        BaseValue = 2,
                        RulesText = "敵に Glazed を付与し、ブロックを得る。"
                    },
                    new CardDefinition
                    {
                        Id = "chill-script",
                        DisplayName = "Chill Script",
                        Role = CardRole.Definition,
                        EffectType = CardEffectType.ApplyFlagAndAttackDown,
                        Cost = 1,
                        FlagId = "Chilled",
                        BaseValue = 2,
                        RulesText = "敵に Chilled を付与し、次回攻撃を弱める。"
                    },
                    new CardDefinition
                    {
                        Id = "sweet-bite",
                        DisplayName = "Sweet Bite",
                        Role = CardRole.Execution,
                        EffectType = CardEffectType.DealDamageWithFlagBonus,
                        Cost = 2,
                        FlagId = "Marshmallow",
                        BaseValue = 6,
                        Multiplier = 2f,
                        HealRatio = 0.1f,
                        RulesText = "6 ダメージ。Marshmallow があれば威力 2 倍、与ダメージの 10% 回復。"
                    },
                    new CardDefinition
                    {
                        Id = "mirror-pierce",
                        DisplayName = "Mirror Pierce",
                        Role = CardRole.Execution,
                        EffectType = CardEffectType.DealDamageWithFlatBonus,
                        Cost = 1,
                        FlagId = "Glazed",
                        BaseValue = 4,
                        BonusValue = 6,
                        RulesText = "4 ダメージ。Glazed があれば追加で 6 ダメージ。"
                    },
                    new CardDefinition
                    {
                        Id = "frost-crack",
                        DisplayName = "Frost Crack",
                        Role = CardRole.Execution,
                        EffectType = CardEffectType.DealDamageAndStunIfFlag,
                        Cost = 2,
                        FlagId = "Chilled",
                        BaseValue = 5,
                        RulesText = "5 ダメージ。Chilled があれば敵の次回行動を停止。"
                    },
                    new CardDefinition
                    {
                        Id = "tea-break",
                        DisplayName = "Tea Break",
                        Role = CardRole.Execution,
                        EffectType = CardEffectType.HealAndDraw,
                        Cost = 1,
                        BaseValue = 3,
                        DrawAmount = 1,
                        RulesText = "回復しつつ 1 枚ドロー。"
                    },
                    new CardDefinition
                    {
                        Id = "double-serving",
                        DisplayName = "Double Serving",
                        Role = CardRole.Execution,
                        EffectType = CardEffectType.ScaleDamageFromDefinitionsInHand,
                        Cost = 2,
                        BaseValue = 6,
                        BonusValue = 3,
                        RulesText = "手札の Definition 枚数に応じて伸びる一撃。"
                    }
                };
            }

            if (relicCatalog.Count == 0)
            {
                relicCatalog = new List<RelicDefinition>
                {
                    new RelicDefinition
                    {
                        Id = "sugar-ribbon",
                        DisplayName = "Sugar Ribbon",
                        EffectType = RelicEffectType.HealAtBattleStart,
                        Value = 2,
                        RulesText = "各戦闘開始時に 2 回復。"
                    },
                    new RelicDefinition
                    {
                        Id = "occult-spoon",
                        DisplayName = "Occult Spoon",
                        EffectType = RelicEffectType.IncreaseMaxCost,
                        Value = 1,
                        RulesText = "最大コストを 1 増やす。"
                    },
                    new RelicDefinition
                    {
                        Id = "cold-record",
                        DisplayName = "Cold Record",
                        EffectType = RelicEffectType.ApplyFlagAtBattleStart,
                        FlagId = "Chilled",
                        RulesText = "各戦闘開始時に敵へ Chilled を付与。"
                    }
                };
            }

            if (eventCatalog.Count == 0)
            {
                eventCatalog = new List<EventDefinition>
                {
                    new EventDefinition
                    {
                        Title = "Spilled Pudding",
                        Description = "甘い香りが拠点の空気をほどき、回復寄りの感性が育つ。",
                        SweetDelta = 1
                    },
                    new EventDefinition
                    {
                        Title = "Mirror Chat",
                        Description = "鏡越しの会話で定義の糸口を掴み、Mystic が伸びる。",
                        MysticDelta = 1
                    },
                    new EventDefinition
                    {
                        Title = "Cool Night Walk",
                        Description = "夜風の散歩で視界が冴え、リロールの余裕が増える。",
                        CoolDelta = 1
                    },
                    new EventDefinition
                    {
                        Title = "Sugared Secret",
                        Description = "姉の囁きから新たな定義を得る。",
                        GrantedDefinitionId = "marshmallow-mark"
                    }
                };
            }

            if (string.IsNullOrWhiteSpace(selectedDifficultyId) || difficulties.All(diff => diff.Id != selectedDifficultyId))
            {
                selectedDifficultyId = difficulties[1].Id;
            }

            if (selectedFurnitureIds == null || selectedFurnitureIds.Count == 0)
            {
                selectedFurnitureIds = new List<string> { "tea-table", "moon-lamp", "vinyl-player" };
            }

            selectedFurnitureIds = selectedFurnitureIds
                .Where(id => furnitureCatalog.Any(item => item.Id == id))
                .Distinct()
                .Take(MaxFurnitureSelection)
                .ToList();
        }

        private void Reset()
        {
            SeedDefaultsIfEmpty();
        }

        private void OnValidate()
        {
            SeedDefaultsIfEmpty();
        }

        private void Start()
        {
            SeedDefaultsIfEmpty();
        }

        private void OnGUI()
        {
            EnsureStyles();
            EnsureWindowSize();

            GUILayout.BeginArea(new Rect(12f, 12f, Screen.width - 24f, Screen.height - 24f));
            GUILayout.BeginHorizontal();

            DrawHubPanel();
            DrawExpeditionPanel();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (run != null && run.Mode == RunMode.Reward)
            {
                DrawRewardOverlay();
            }

            if (run != null && run.Mode == RunMode.RestChoice)
            {
                DrawRestOverlay();
            }
        }

        private void EnsureStyles()
        {
            if (cachedHeaderStyle == null)
            {
                cachedHeaderStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true
                };
            }

            if (cachedCardStyle == null)
            {
                cachedCardStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.UpperLeft,
                    wordWrap = true,
                    padding = new RectOffset(12, 12, 10, 10)
                };
            }

            if (cachedMutedLabelStyle == null)
            {
                cachedMutedLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    wordWrap = true,
                    normal = { textColor = new Color(0.78f, 0.78f, 0.78f) }
                };
            }
        }

        private void EnsureWindowSize()
        {
            if (Application.isEditor && !Application.isPlaying && (Screen.width < 400 || Screen.height < 300))
            {
                return;
            }

            if (Screen.width < defaultWindowSize.x || Screen.height < defaultWindowSize.y)
            {
                // IMGUI prototype keeps working in smaller windows; no resize request needed.
            }
        }

        private void DrawHubPanel()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(Mathf.Max(360f, Screen.width * 0.34f)), GUILayout.ExpandHeight(true));
            hubScroll = GUILayout.BeginScrollView(hubScroll);

            GUILayout.Label("Ghost & Gourmet", cachedHeaderStyle);
            GUILayout.Label("仕様書 v2 をもとにした Unity 雛形。2.3 の重み付けロジックは未実装で、報酬は均等抽選です。", cachedMutedLabelStyle);
            GUILayout.Space(12f);

            GUILayout.Label("難易度", cachedHeaderStyle);
            foreach (DifficultyDefinition difficulty in difficulties)
            {
                GUILayout.BeginVertical(cachedCardStyle);
                bool selected = selectedDifficultyId == difficulty.Id;
                GUILayout.Label($"{difficulty.DisplayName} / {difficulty.FloorCount} Floors");
                GUILayout.Label($"HP x{difficulty.EnemyHpScale:0.0} / ATK x{difficulty.EnemyAttackScale:0.0} / 逆転 {difficulty.ReverseRuleUses} 回");
                GUILayout.Label(difficulty.Summary, cachedMutedLabelStyle);
                if (GUILayout.Button(selected ? "選択中" : "この難易度を選ぶ"))
                {
                    selectedDifficultyId = difficulty.Id;
                }
                GUILayout.EndVertical();
                GUILayout.Space(6f);
            }

            GUILayout.Space(8f);
            GUILayout.Label("家具を 3 つまで選択", cachedHeaderStyle);
            foreach (FurnitureDefinition furniture in furnitureCatalog)
            {
                bool selected = selectedFurnitureIds.Contains(furniture.Id);
                GUILayout.BeginVertical(cachedCardStyle);
                GUILayout.Label($"{furniture.DisplayName}  Sweet {furniture.Sweet} / Mystic {furniture.Mystic} / Cool {furniture.Cool}");
                GUILayout.Label(furniture.Summary, cachedMutedLabelStyle);
                if (GUILayout.Button(selected ? "外す" : "選択"))
                {
                    ToggleFurnitureSelection(furniture.Id);
                }
                GUILayout.EndVertical();
                GUILayout.Space(6f);
            }

            (int sweet, int mystic, int cool) tags = ComputeHomeTags();
            GUILayout.Space(8f);
            GUILayout.Label("タグ累積", cachedHeaderStyle);
            GUILayout.Label($"Sweet {tags.sweet}  Mystic {tags.mystic}  Cool {tags.cool}");
            GUILayout.Label($"Sweet: 回復効率 +{tags.sweet * 5}%  Mystic: 初期ドロー +{tags.mystic}  Cool: リロール +{tags.cool}", cachedMutedLabelStyle);

            GUILayout.Space(12f);
            if (GUILayout.Button("探索を開始", GUILayout.Height(36f)))
            {
                StartRun();
            }

            GUI.enabled = run != null;
            if (GUILayout.Button("拠点へ戻る", GUILayout.Height(32f)))
            {
                ResetToHub();
            }
            GUI.enabled = true;

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawExpeditionPanel()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            expeditionScroll = GUILayout.BeginScrollView(expeditionScroll);

            GUILayout.Label("探索と戦闘", cachedHeaderStyle);

            if (run == null)
            {
                GUILayout.Label("左側で難易度と家具を選び、探索を開始してください。Unity で開くとこのまま Play で確認できます。", cachedMutedLabelStyle);
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
                return;
            }

            GUILayout.BeginHorizontal(GUI.skin.box);
            DrawStatusPill($"Floor {run.CurrentFloor}/{run.Difficulty.FloorCount}");
            DrawStatusPill($"HP {run.Hp}/{run.MaxHp}");
            DrawStatusPill($"Cost {run.Cost}/{run.MaxCost}");
            DrawStatusPill($"Reverse {run.ReverseUsesLeft}");
            DrawStatusPill($"Reroll {run.RerollsLeft}");
            DrawStatusPill($"Deck {run.Deck.Count}");
            GUILayout.EndHorizontal();

            DrawMapStrip();
            GUILayout.Space(8f);

            switch (run.Mode)
            {
                case RunMode.Map:
                    DrawNodeSelection();
                    break;
                case RunMode.Battle:
                    DrawBattlePanel();
                    break;
                case RunMode.Cleared:
                    GUILayout.BeginVertical(cachedCardStyle);
                    GUILayout.Label("探索クリア");
                    GUILayout.Label("難易度・家具・カードバランスの検証用に再度ランを始められます。", cachedMutedLabelStyle);
                    GUILayout.EndVertical();
                    break;
                case RunMode.Defeat:
                    GUILayout.BeginVertical(cachedCardStyle);
                    GUILayout.Label("探索失敗");
                    GUILayout.Label("難易度を下げるか、拠点タグ配分を見直して再挑戦できます。", cachedMutedLabelStyle);
                    GUILayout.EndVertical();
                    break;
                case RunMode.Reward:
                    DrawBattlePanel();
                    GUILayout.Space(8f);
                    GUILayout.Label("報酬選択中です。", cachedMutedLabelStyle);
                    break;
                case RunMode.RestChoice:
                    GUILayout.BeginVertical(cachedCardStyle);
                    GUILayout.Label("休息ノード");
                    GUILayout.Label("回復 / 強化 / 削除の選択を開いています。", cachedMutedLabelStyle);
                    GUILayout.EndVertical();
                    break;
            }

            GUILayout.Space(12f);
            DrawLogPanel();

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawStatusPill(string text)
        {
            GUILayout.Box(text, GUILayout.Height(26f));
        }

        private void DrawMapStrip()
        {
            GUILayout.Label("ルート進行", cachedHeaderStyle);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < run.Map.Count; i++)
            {
                string label = i + 1 == run.CurrentFloor ? $"[{i + 1}]" : (i + 1).ToString();
                GUILayout.Box(label, GUILayout.Width(36f), GUILayout.Height(24f));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawNodeSelection()
        {
            GUILayout.Label("次のノードを選択", cachedHeaderStyle);
            foreach (MapNodeState node in run.Map[run.CurrentFloor - 1])
            {
                GUILayout.BeginVertical(cachedCardStyle);
                GUILayout.Label($"{GetNodeTypeLabel(node.Type)} : {node.Label}");
                GUILayout.Label(GetNodeDescription(node), cachedMutedLabelStyle);
                if (GUILayout.Button("進む"))
                {
                    ResolveNode(node);
                }
                GUILayout.EndVertical();
                GUILayout.Space(6f);
            }
        }

        private void DrawBattlePanel()
        {
            GUILayout.Label("バトル", cachedHeaderStyle);
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(cachedCardStyle, GUILayout.Width(320f));
            GUILayout.Label("少女 / Execution");
            GUILayout.Label($"HP {run.Hp}/{run.MaxHp}");
            GUILayout.Label($"Cost {run.Cost}/{run.MaxCost}");
            GUILayout.Label($"Block {run.Block}");
            GUILayout.Label($"Draw {run.DrawPile.Count} / Discard {run.DiscardPile.Count}");
            GUILayout.Label($"Relics {(run.RelicIds.Count == 0 ? "None" : string.Join(", ", run.RelicIds))}", cachedMutedLabelStyle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(cachedCardStyle, GUILayout.Width(320f));
            if (run.Enemy == null)
            {
                GUILayout.Label("不思議なお姉さん / Definition");
                GUILayout.Label("次のノード選択待ち。", cachedMutedLabelStyle);
            }
            else
            {
                GUILayout.Label(run.Enemy.Name);
                GUILayout.Label($"HP {run.Enemy.Hp}/{run.Enemy.MaxHp}");
                GUILayout.Label($"ATK {Mathf.Max(0, run.Enemy.Attack + run.Enemy.TemporaryAttackModifier)}");
                GUILayout.Label($"Flags {(run.Enemy.Flags.Count == 0 ? "None" : string.Join(", ", run.Enemy.Flags))}");
                GUILayout.Label(run.Enemy.SkipNextAttack ? "次の攻撃は停止中" : "次の攻撃は有効", cachedMutedLabelStyle);
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Space(8f);
            GUILayout.BeginHorizontal();
            bool reverseReady = run.Mode == RunMode.Battle &&
                                run.ReverseUsesLeft > 0 &&
                                run.Hp <= Mathf.CeilToInt(run.MaxHp * LowHpThreshold);
            GUI.enabled = reverseReady;
            if (GUILayout.Button("逆転ルール", GUILayout.Height(32f)))
            {
                TriggerReverseRule();
            }
            GUI.enabled = run.Mode == RunMode.Battle;
            if (GUILayout.Button("ターン終了", GUILayout.Height(32f)))
            {
                EndTurn();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);
            GUILayout.Label("手札", cachedHeaderStyle);
            handScroll = GUILayout.BeginScrollView(handScroll, GUILayout.Height(240f));
            GUILayout.BeginHorizontal();
            for (int i = 0; i < run.Hand.Count; i++)
            {
                CardState card = run.Hand[i];
                GUILayout.BeginVertical(cachedCardStyle, GUILayout.Width(220f), GUILayout.Height(210f));
                GUILayout.Label($"{card.GetDisplayTitle()} / {card.Role}");
                GUILayout.Label($"Cost {card.Cost}");
                GUILayout.Label(card.RulesText, cachedMutedLabelStyle);
                bool canPlay = run.Mode == RunMode.Battle && (run.ReverseActive || card.Cost <= run.Cost);
                GUI.enabled = canPlay;
                if (GUILayout.Button("使う"))
                {
                    PlayCard(i);
                }
                GUI.enabled = true;
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }

        private void DrawLogPanel()
        {
            GUILayout.Label("ログ", cachedHeaderStyle);
            logScroll = GUILayout.BeginScrollView(logScroll, GUILayout.Height(220f));
            foreach (string line in run.Log)
            {
                GUILayout.Box(line, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndScrollView();
        }

        private void DrawRewardOverlay()
        {
            Rect area = CenteredRect(760f, 420f);
            GUILayout.BeginArea(area, GUI.skin.window);
            GUILayout.Label("報酬を選択", cachedHeaderStyle);
            GUILayout.Label("3 択 + リロール。2.3 の重み付けロジックは実装していません。", cachedMutedLabelStyle);
            GUILayout.Space(8f);

            GUILayout.BeginHorizontal();
            foreach (object reward in rewardOptions)
            {
                GUILayout.BeginVertical(cachedCardStyle, GUILayout.Width(230f), GUILayout.Height(240f));
                switch (reward)
                {
                    case CardState card:
                        GUILayout.Label($"{card.GetDisplayTitle()} / {card.Role}");
                        GUILayout.Label(card.RulesText, cachedMutedLabelStyle);
                        if (GUILayout.Button("獲得"))
                        {
                            ClaimReward(card);
                        }
                        break;
                    case RelicDefinition relic:
                        GUILayout.Label($"{relic.DisplayName} / Relic");
                        GUILayout.Label(relic.RulesText, cachedMutedLabelStyle);
                        if (GUILayout.Button("獲得"))
                        {
                            ClaimReward(relic);
                        }
                        break;
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUI.enabled = run.RerollsLeft > 0;
            if (GUILayout.Button("リロール", GUILayout.Height(32f)))
            {
                RerollRewards();
            }
            GUI.enabled = true;
            if (GUILayout.Button("今回は見送る", GUILayout.Height(32f)))
            {
                CloseRewardAndAdvance();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawRestOverlay()
        {
            Rect area = CenteredRect(700f, 320f);
            GUILayout.BeginArea(area, GUI.skin.window);
            GUILayout.Label("休息ノード", cachedHeaderStyle);
            GUILayout.Label("仕様書 2.2 に合わせ、回復 / 強化 / 削除を選べる形にしています。", cachedMutedLabelStyle);
            GUILayout.Space(8f);

            GUILayout.BeginVertical(cachedCardStyle);
            if (GUILayout.Button("HP を回復"))
            {
                int healAmount = Mathf.RoundToInt(10 * (1f + run.Sweet * 0.05f));
                HealPlayer(healAmount);
                PushLog($"休息で {healAmount} 回復した。");
                FinishRestChoice();
            }

            GUI.enabled = run.Deck.Count > 0;
            if (GUILayout.Button("ランダムなカードを強化"))
            {
                UpgradeRandomCard();
                FinishRestChoice();
            }

            if (GUILayout.Button("ランダムなカードを削除"))
            {
                RemoveRandomCard();
                FinishRestChoice();
            }
            GUI.enabled = true;
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private Rect CenteredRect(float width, float height)
        {
            return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        }

        private void ToggleFurnitureSelection(string furnitureId)
        {
            if (selectedFurnitureIds.Contains(furnitureId))
            {
                selectedFurnitureIds.Remove(furnitureId);
                return;
            }

            if (selectedFurnitureIds.Count >= MaxFurnitureSelection)
            {
                selectedFurnitureIds.RemoveAt(0);
            }

            selectedFurnitureIds.Add(furnitureId);
        }

        private (int sweet, int mystic, int cool) ComputeHomeTags()
        {
            int sweet = 0;
            int mystic = 0;
            int cool = 0;

            foreach (FurnitureDefinition furniture in furnitureCatalog.Where(item => selectedFurnitureIds.Contains(item.Id)))
            {
                sweet += furniture.Sweet;
                mystic += furniture.Mystic;
                cool += furniture.Cool;
            }

            return (sweet, mystic, cool);
        }

        private DifficultyDefinition GetSelectedDifficulty()
        {
            DifficultyDefinition difficulty = difficulties.FirstOrDefault(item => item.Id == selectedDifficultyId);
            return difficulty ?? difficulties.First();
        }

        private void StartRun()
        {
            DifficultyDefinition difficulty = GetSelectedDifficulty();
            (int sweet, int mystic, int cool) tags = ComputeHomeTags();
            rewardOptions.Clear();

            int maxHp = 44 + tags.sweet * 2;
            int maxCost = 3 + Mathf.FloorToInt(tags.mystic / 2f);
            int drawPerTurn = 4 + tags.mystic;

            run = new RunState
            {
                Difficulty = difficulty,
                CurrentFloor = 1,
                Mode = RunMode.Map,
                MaxHp = maxHp,
                Hp = maxHp,
                MaxCost = maxCost,
                Cost = maxCost,
                DrawPerTurn = drawPerTurn,
                ReverseUsesLeft = difficulty.ReverseRuleUses,
                RerollsLeft = 1 + tags.cool,
                Sweet = tags.sweet,
                Mystic = tags.mystic,
                Cool = tags.cool,
                Deck = CreateStarterDeck(),
                Map = GenerateMap(difficulty.FloorCount)
            };

            PushLog("探索を開始した。");
        }

        private void ResetToHub()
        {
            run = null;
            rewardOptions.Clear();
        }

        private List<CardState> CreateStarterDeck()
        {
            return new List<CardState>
            {
                CloneCard("marshmallow-mark"),
                CloneCard("marshmallow-mark"),
                CloneCard("sugar-veil"),
                CloneCard("chill-script"),
                CloneCard("sweet-bite"),
                CloneCard("sweet-bite"),
                CloneCard("mirror-pierce"),
                CloneCard("frost-crack"),
                CloneCard("tea-break"),
                CloneCard("double-serving")
            };
        }

        private List<MapNodeState[]> GenerateMap(int totalFloors)
        {
            List<MapNodeState[]> map = new List<MapNodeState[]>();
            NodeType[] nodePool = { NodeType.Battle, NodeType.Battle, NodeType.Elite, NodeType.Rest, NodeType.Event };

            for (int floor = 1; floor <= totalFloors; floor++)
            {
                if (floor == totalFloors)
                {
                    map.Add(new[]
                    {
                        new MapNodeState
                        {
                            Id = $"floor-{floor}-boss",
                            Type = NodeType.Elite,
                            Depth = floor,
                            IsBoss = true,
                            Label = "Final Haunt"
                        }
                    });
                    continue;
                }

                int branchCount = UnityEngine.Random.Range(2, 4);
                MapNodeState[] nodes = new MapNodeState[branchCount];
                for (int branch = 0; branch < branchCount; branch++)
                {
                    NodeType type = nodePool[UnityEngine.Random.Range(0, nodePool.Length)];
                    nodes[branch] = new MapNodeState
                    {
                        Id = $"floor-{floor}-node-{branch + 1}",
                        Type = type,
                        Depth = floor,
                        Label = GetNodeTypeLabel(type)
                    };
                }
                map.Add(nodes);
            }

            return map;
        }

        private void ResolveNode(MapNodeState node)
        {
            if (run == null || run.Mode != RunMode.Map)
            {
                return;
            }

            switch (node.Type)
            {
                case NodeType.Battle:
                case NodeType.Elite:
                    StartBattle(node);
                    break;
                case NodeType.Rest:
                    run.Mode = RunMode.RestChoice;
                    run.PendingRestChoice = true;
                    run.PendingRestNodeId = node.Id;
                    PushLog("休息ノードに到着。回復・強化・削除から選ぶ。");
                    break;
                case NodeType.Event:
                    ResolveEventNode();
                    break;
            }
        }

        private void StartBattle(MapNodeState node)
        {
            float floorFactor = 1f + (run.CurrentFloor - 1) * 0.12f;
            float eliteFactor = node.Type == NodeType.Elite ? 1.45f : 1f;
            int enemyHp = Mathf.RoundToInt((22 + run.CurrentFloor * 5) * run.Difficulty.EnemyHpScale * floorFactor * eliteFactor);
            int enemyAttack = Mathf.Max(4, Mathf.RoundToInt((6 + run.CurrentFloor * 1.2f) * run.Difficulty.EnemyAttackScale * eliteFactor));

            run.Mode = RunMode.Battle;
            run.Block = 0;
            run.ReverseActive = false;
            run.Enemy = new EnemyState
            {
                Name = node.IsBoss ? "Midnight Custard" : node.Type == NodeType.Elite ? "Haunted Banquet" : "Wandering Wisp",
                Hp = enemyHp,
                MaxHp = enemyHp,
                Attack = enemyAttack,
                SourceNode = node
            };

            run.DrawPile = Shuffle(run.Deck.Select(card => card.Clone()).ToList());
            run.DiscardPile.Clear();
            run.Hand.Clear();
            DrawCards(run.DrawPerTurn);
            ApplyBattleStartRelics();
            PushLog($"{run.Enemy.Name} が現れた。");
        }

        private void ResolveEventNode()
        {
            if (eventCatalog.Count == 0)
            {
                AdvanceFloor();
                return;
            }

            EventDefinition chosen = eventCatalog[UnityEngine.Random.Range(0, eventCatalog.Count)];
            run.Sweet += chosen.SweetDelta;
            run.Mystic += chosen.MysticDelta;
            run.Cool += chosen.CoolDelta;
            run.DrawPerTurn = 4 + run.Mystic;
            run.RerollsLeft += chosen.CoolDelta;

            if (!string.IsNullOrWhiteSpace(chosen.GrantedDefinitionId))
            {
                run.Deck.Add(CloneCard(chosen.GrantedDefinitionId));
            }

            PushLog($"{chosen.Title}: {chosen.Description}");
            AdvanceFloor();
        }

        private void FinishRestChoice()
        {
            run.PendingRestChoice = false;
            run.PendingRestNodeId = string.Empty;
            run.Mode = RunMode.Map;
            AdvanceFloor();
        }

        private void UpgradeRandomCard()
        {
            if (run.Deck.Count == 0)
            {
                PushLog("強化対象のカードがない。");
                return;
            }

            int index = UnityEngine.Random.Range(0, run.Deck.Count);
            CardState card = run.Deck[index];
            card.UpgradeLevel += 1;

            switch (card.EffectType)
            {
                case CardEffectType.ApplyFlagAndHeal:
                case CardEffectType.ApplyFlagAndShield:
                case CardEffectType.ApplyFlagAndAttackDown:
                case CardEffectType.HealAndDraw:
                    card.BaseValue += 1;
                    break;
                case CardEffectType.DealDamageWithFlagBonus:
                case CardEffectType.DealDamageWithFlatBonus:
                case CardEffectType.DealDamageAndStunIfFlag:
                case CardEffectType.ScaleDamageFromDefinitionsInHand:
                    card.BaseValue += 2;
                    break;
            }

            card.Cost = Mathf.Max(0, card.Cost - (card.UpgradeLevel == 1 ? 0 : 1));
            PushLog($"{card.GetDisplayTitle()} を強化した。");
        }

        private void RemoveRandomCard()
        {
            if (run.Deck.Count == 0)
            {
                PushLog("削除対象のカードがない。");
                return;
            }

            int index = UnityEngine.Random.Range(0, run.Deck.Count);
            string removed = run.Deck[index].GetDisplayTitle();
            run.Deck.RemoveAt(index);
            PushLog($"{removed} をデッキから削除した。");
        }

        private void ApplyBattleStartRelics()
        {
            foreach (string relicId in run.RelicIds)
            {
                RelicDefinition relic = relicCatalog.FirstOrDefault(item => item.Id == relicId);
                if (relic == null)
                {
                    continue;
                }

                switch (relic.EffectType)
                {
                    case RelicEffectType.HealAtBattleStart:
                        HealPlayer(relic.Value);
                        break;
                    case RelicEffectType.ApplyFlagAtBattleStart:
                        AddEnemyFlag(relic.FlagId);
                        break;
                }
            }
        }

        private void BeginRewards(bool elite)
        {
            rewardOptions.Clear();
            rewardOptions.AddRange(RollRewards(elite));
            run.PendingReward = true;
            run.Mode = RunMode.Reward;
        }

        private IEnumerable<object> RollRewards(bool elite)
        {
            List<object> options = new List<object>();
            List<CardState> cardPool = cardCatalog.Select(card => CloneCard(card.Id)).ToList();
            List<RelicDefinition> relicPool = relicCatalog.ToList();

            // Spec 2.3: reroll only. No deck-ratio weighting is applied here by request.
            int cardCount = elite ? 2 : 3;
            options.AddRange(PickDistinct(cardPool.Cast<object>().ToList(), cardCount));

            bool includeRelic = elite || UnityEngine.Random.value < 0.35f;
            if (includeRelic && relicPool.Count > 0)
            {
                options.AddRange(PickDistinct(relicPool.Cast<object>().ToList(), 1));
            }

            while (options.Count < 3 && cardPool.Count > 0)
            {
                options.AddRange(PickDistinct(cardPool.Cast<object>().ToList(), 1, options));
            }

            return PickDistinct(options, 3);
        }

        private List<object> PickDistinct(List<object> source, int count, List<object> exclusions = null)
        {
            List<object> pool = new List<object>(source);
            if (exclusions != null)
            {
                pool.RemoveAll(item => exclusions.Any(exclusion => GetRewardId(exclusion) == GetRewardId(item)));
            }

            List<object> picked = new List<object>();
            while (pool.Count > 0 && picked.Count < count)
            {
                int index = UnityEngine.Random.Range(0, pool.Count);
                picked.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return picked;
        }

        private void ClaimReward(object reward)
        {
            switch (reward)
            {
                case CardState card:
                    run.Deck.Add(card.Clone());
                    PushLog($"{card.GetDisplayTitle()} をデッキに追加した。");
                    break;
                case RelicDefinition relic:
                    run.RelicIds.Add(relic.Id);
                    ApplyPassiveRelic(relic);
                    PushLog($"{relic.DisplayName} を獲得した。");
                    break;
            }

            CloseRewardAndAdvance();
        }

        private void ApplyPassiveRelic(RelicDefinition relic)
        {
            if (relic.EffectType == RelicEffectType.IncreaseMaxCost)
            {
                run.MaxCost += relic.Value;
                run.Cost = Mathf.Min(run.MaxCost, run.Cost + relic.Value);
            }
        }

        private void CloseRewardAndAdvance()
        {
            run.PendingReward = false;
            rewardOptions.Clear();
            run.Mode = RunMode.Map;
            AdvanceFloor();
        }

        private void RerollRewards()
        {
            if (run.RerollsLeft <= 0)
            {
                return;
            }

            run.RerollsLeft -= 1;
            rewardOptions.Clear();
            rewardOptions.AddRange(RollRewards(run.Enemy != null && run.Enemy.SourceNode.Type == NodeType.Elite));
            PushLog("報酬をリロールした。");
        }

        private void PlayCard(int handIndex)
        {
            if (run == null || run.Mode != RunMode.Battle || handIndex < 0 || handIndex >= run.Hand.Count)
            {
                return;
            }

            CardState card = run.Hand[handIndex];
            if (!run.ReverseActive && card.Cost > run.Cost)
            {
                PushLog("コストが足りない。");
                return;
            }

            if (!run.ReverseActive)
            {
                run.Cost -= card.Cost;
            }

            run.Hand.RemoveAt(handIndex);
            ResolveCard(card);
            run.DiscardPile.Add(card);

            if (run.Enemy != null && run.Enemy.Hp <= 0)
            {
                bool elite = run.Enemy.SourceNode.Type == NodeType.Elite;
                string enemyName = run.Enemy.Name;
                run.Enemy = null;
                PushLog($"{enemyName} を撃退した。");
                BeginRewards(elite);
            }
        }

        private void ResolveCard(CardState card)
        {
            switch (card.EffectType)
            {
                case CardEffectType.ApplyFlagAndHeal:
                    AddEnemyFlag(card.FlagId);
                    HealPlayer(card.BaseValue);
                    PushLog($"{card.GetDisplayTitle()} で {card.FlagId} を付与し、回復した。");
                    break;
                case CardEffectType.ApplyFlagAndShield:
                    AddEnemyFlag(card.FlagId);
                    run.Block += card.BaseValue;
                    PushLog($"{card.GetDisplayTitle()} で {card.FlagId} を付与し、Block を得た。");
                    break;
                case CardEffectType.ApplyFlagAndAttackDown:
                    AddEnemyFlag(card.FlagId);
                    if (run.Enemy != null)
                    {
                        run.Enemy.TemporaryAttackModifier -= card.BaseValue;
                    }
                    PushLog($"{card.GetDisplayTitle()} で {card.FlagId} を付与し、敵の攻撃を弱めた。");
                    break;
                case CardEffectType.DealDamageWithFlagBonus:
                {
                    int damage = card.BaseValue;
                    int heal = 0;
                    if (HasEnemyFlag(card.FlagId))
                    {
                        damage = Mathf.RoundToInt(damage * card.Multiplier);
                        heal = Mathf.CeilToInt(damage * card.HealRatio);
                    }

                    DealDamage(damage);
                    if (heal > 0)
                    {
                        HealPlayer(heal);
                    }

                    if (card.ConsumeFlag)
                    {
                        ConsumeEnemyFlag(card.FlagId);
                    }

                    PushLog($"{card.GetDisplayTitle()} で {damage} ダメージ。");
                    break;
                }
                case CardEffectType.DealDamageWithFlatBonus:
                {
                    int damage = card.BaseValue + (HasEnemyFlag(card.FlagId) ? card.BonusValue : 0);
                    DealDamage(damage);
                    if (card.ConsumeFlag)
                    {
                        ConsumeEnemyFlag(card.FlagId);
                    }
                    PushLog($"{card.GetDisplayTitle()} で {damage} ダメージ。");
                    break;
                }
                case CardEffectType.DealDamageAndStunIfFlag:
                    DealDamage(card.BaseValue);
                    if (HasEnemyFlag(card.FlagId) && run.Enemy != null)
                    {
                        run.Enemy.SkipNextAttack = true;
                    }
                    if (card.ConsumeFlag)
                    {
                        ConsumeEnemyFlag(card.FlagId);
                    }
                    PushLog($"{card.GetDisplayTitle()} で {card.BaseValue} ダメージ。");
                    break;
                case CardEffectType.HealAndDraw:
                    HealPlayer(card.BaseValue);
                    DrawCards(card.DrawAmount);
                    PushLog($"{card.GetDisplayTitle()} で回復し、{card.DrawAmount} 枚引いた。");
                    break;
                case CardEffectType.ScaleDamageFromDefinitionsInHand:
                {
                    int definitionsInHand = run.Hand.Count(item => item.Role == CardRole.Definition);
                    int damage = Mathf.Max(card.BaseValue, definitionsInHand * card.BonusValue);
                    DealDamage(damage);
                    PushLog($"{card.GetDisplayTitle()} で {damage} ダメージ。");
                    break;
                }
            }
        }

        private void EndTurn()
        {
            if (run == null || run.Mode != RunMode.Battle || run.Enemy == null)
            {
                return;
            }

            if (run.Enemy.SkipNextAttack || run.ReverseActive)
            {
                PushLog("敵の行動を停止した。");
            }
            else
            {
                int rawDamage = Mathf.Max(0, run.Enemy.Attack + run.Enemy.TemporaryAttackModifier);
                int reducedDamage = Mathf.Max(0, rawDamage - run.Block);
                run.Block = Mathf.Max(0, run.Block - rawDamage);
                run.Hp = Mathf.Max(0, run.Hp - reducedDamage);
                PushLog($"{run.Enemy.Name} の攻撃で {reducedDamage} ダメージ。");
            }

            run.Enemy.SkipNextAttack = false;
            run.Enemy.TemporaryAttackModifier = 0;
            run.Block = 0;
            run.ReverseActive = false;

            if (run.Hp <= 0)
            {
                run.Mode = RunMode.Defeat;
                PushLog("力尽きた。");
                return;
            }

            StartPlayerTurn();
        }

        private void StartPlayerTurn()
        {
            run.Cost = run.MaxCost;
            foreach (CardState card in run.Hand)
            {
                run.DiscardPile.Add(card);
            }
            run.Hand.Clear();
            DrawCards(run.DrawPerTurn);
            PushLog("新しいターンが始まった。");
        }

        private void TriggerReverseRule()
        {
            if (run == null || run.Mode != RunMode.Battle || run.ReverseUsesLeft <= 0)
            {
                return;
            }

            if (run.Hp > Mathf.CeilToInt(run.MaxHp * LowHpThreshold))
            {
                return;
            }

            run.ReverseUsesLeft -= 1;
            run.ReverseActive = true;
            PushLog("逆転ルール発動。そのターンのコストを無視し、敵行動を停止する。");

            if (run.Difficulty.ApplyNightmareDebuffAfterReverse && !run.NightmareDebuffApplied)
            {
                run.NightmareDebuffApplied = true;
                run.MaxHp = Mathf.Max(20, run.MaxHp - 6);
                run.Hp = Mathf.Min(run.Hp, run.MaxHp);
                PushLog("Nightmare デバフで最大 HP が 6 減少した。");
            }
        }

        private void DrawCards(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                if (run.DrawPile.Count == 0)
                {
                    if (run.DiscardPile.Count == 0)
                    {
                        return;
                    }

                    run.DrawPile = Shuffle(run.DiscardPile.Select(card => card.Clone()).ToList());
                    run.DiscardPile.Clear();
                }

                CardState nextCard = run.DrawPile[run.DrawPile.Count - 1];
                run.DrawPile.RemoveAt(run.DrawPile.Count - 1);
                run.Hand.Add(nextCard);
            }
        }

        private void DealDamage(int amount)
        {
            if (run.Enemy == null)
            {
                return;
            }

            run.Enemy.Hp = Mathf.Max(0, run.Enemy.Hp - amount);
        }

        private void HealPlayer(int amount)
        {
            int total = Mathf.RoundToInt(amount * (1f + run.Sweet * 0.05f));
            run.Hp = Mathf.Min(run.MaxHp, run.Hp + total);
        }

        private void AddEnemyFlag(string flagId)
        {
            if (run.Enemy == null || string.IsNullOrWhiteSpace(flagId))
            {
                return;
            }

            if (!run.Enemy.Flags.Contains(flagId))
            {
                run.Enemy.Flags.Add(flagId);
            }
        }

        private bool HasEnemyFlag(string flagId)
        {
            return run.Enemy != null && run.Enemy.Flags.Contains(flagId);
        }

        private void ConsumeEnemyFlag(string flagId)
        {
            if (run.Enemy == null || string.IsNullOrWhiteSpace(flagId))
            {
                return;
            }

            run.Enemy.Flags.Remove(flagId);
        }

        private void AdvanceFloor()
        {
            run.Enemy = null;

            if (run.CurrentFloor >= run.Difficulty.FloorCount)
            {
                run.Mode = RunMode.Cleared;
                PushLog("探索を踏破した。");
                return;
            }

            run.CurrentFloor += 1;
            run.Mode = RunMode.Map;
        }

        private void PushLog(string message)
        {
            if (run == null)
            {
                return;
            }

            run.Log.Insert(0, message);
            if (run.Log.Count > 16)
            {
                run.Log.RemoveAt(run.Log.Count - 1);
            }
        }

        private CardState CloneCard(string cardId)
        {
            CardDefinition definition = cardCatalog.First(item => item.Id == cardId);
            return new CardState
            {
                Id = definition.Id,
                DisplayName = definition.DisplayName,
                Role = definition.Role,
                EffectType = definition.EffectType,
                Cost = definition.Cost,
                FlagId = definition.FlagId,
                BaseValue = definition.BaseValue,
                BonusValue = definition.BonusValue,
                Multiplier = definition.Multiplier,
                HealRatio = definition.HealRatio,
                DrawAmount = definition.DrawAmount,
                ConsumeFlag = definition.ConsumeFlag,
                RulesText = definition.RulesText
            };
        }

        private string GetNodeTypeLabel(NodeType type)
        {
            switch (type)
            {
                case NodeType.Battle:
                    return "戦闘ノード";
                case NodeType.Elite:
                    return "エリートノード";
                case NodeType.Rest:
                    return "休息ノード";
                case NodeType.Event:
                    return "怪異ノード";
                default:
                    return "ノード";
            }
        }

        private string GetNodeDescription(MapNodeState node)
        {
            switch (node.Type)
            {
                case NodeType.Battle:
                    return "通常戦闘。勝利後に 3 択報酬。";
                case NodeType.Elite:
                    return node.IsBoss ? "最終フロアの強敵。" : "強敵戦。レリックが混ざりやすい。";
                case NodeType.Rest:
                    return "回復・強化・削除から 1 つ選択。";
                case NodeType.Event:
                    return "会話イベント。タグ強化か定義カード取得。";
                default:
                    return string.Empty;
            }
        }

        private string GetRewardId(object reward)
        {
            switch (reward)
            {
                case CardState card:
                    return card.Id;
                case RelicDefinition relic:
                    return relic.Id;
                default:
                    return string.Empty;
            }
        }

        private List<CardState> Shuffle(List<CardState> source)
        {
            List<CardState> copy = new List<CardState>(source);
            for (int i = copy.Count - 1; i > 0; i--)
            {
                int swapIndex = UnityEngine.Random.Range(0, i + 1);
                CardState temp = copy[i];
                copy[i] = copy[swapIndex];
                copy[swapIndex] = temp;
            }

            return copy;
        }
    }
}
