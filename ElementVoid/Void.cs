using CodexLib;
using KineticistElementsExpanded.Components;
using KineticistElementsExpanded.Components.Properties;
using KineticistElementsExpanded.KineticLib;
using AnyRef = CodexLib.AnyRef;
using Helper = CodexLib.Helper;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Class.Kineticist.ActivatableAbility;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using static Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell;
using BlueprintCore.Utils;

namespace KineticistElementsExpanded.ElementVoid
{
    class Void
    {

        // Spell book for blasts, to "brew" them like metamagic with infusions

        private static KineticistTree Tree = KineticistTree.Instance;

        private static KineticistTree.Infusion NegativeAdmixture = new();
        private static KineticistTree.Infusion GraviticBoost = new();
        private static KineticistTree.Infusion GraviticBoostGreater = new();

        private static BlueprintAbility VoidHealerAbility = null;
        private static BlueprintFeature VoidHealer = null;

        public static void Configure()
        {
            var void_class_skills = CreateVoidClassSkills();

            CreateVoidInfusions();

            CreateVoidBlastsSelection();
            CreateVoidComposites();

            Kineticist.AddElementsToInfusion(Tree.Singularity, Tree.Gravity, Tree.Negative, Tree.Composite_Void);
            Kineticist.AddElementsToInfusion(Tree.Dampening, Tree.Negative, Tree.Composite_Void);
            Kineticist.AddElementsToInfusion(Tree.Enervating, Tree.Negative, Tree.Composite_Void);
            Kineticist.AddElementsToInfusion(Tree.Pulling, Tree.Gravity, Tree.Composite_Void);
            Kineticist.AddElementsToInfusion(Tree.Unnerving, Tree.Negative, Tree.Composite_Void);
            Kineticist.AddElementsToInfusion(Tree.Vampiric, Tree.Negative, Tree.Composite_Void);
            Kineticist.AddElementsToInfusion(Tree.Weighing, Tree.Gravity, Tree.Composite_Void);

            BlueprintFeature emptiness_feature = CreateEmptiness();

            Kineticist.AddElementalDefenseIsPrereqFor(Tree.Gravity.BlastFeature, Tree.Gravity.Blade.Feature, emptiness_feature);
            Kineticist.AddElementalDefenseIsPrereqFor(Tree.Negative.BlastFeature, Tree.Negative.Blade.Feature, emptiness_feature);

            Kineticist.ElementsBlastSetup(Tree.Gravity, Tree.Negative, Tree.Composite_Void);

            PushInfusions(Tree.Gravity, Tree.Composite_Void);

            Kineticist.AddAdmixtureToBuff(Tree, GraviticBoostGreater, Tree.Gravity, false, false, true);

            Kineticist.AddBladesToKineticWhirlwind(Tree.Gravity, Tree.Negative, Tree.Composite_Void);

            CreateVoidElementalFocus(void_class_skills, emptiness_feature);
            CreateKineticKnightVoidFocus(void_class_skills, emptiness_feature);
            CreateSecondElementVoid();
            CreateThirdElementVoid();

            CreateVoidWildTalents(emptiness_feature);
        }

        #region Class Features and Misc.

        private static BlueprintFeatureBase CreateVoidClassSkills()
        {
            var feature = Helper.CreateBlueprintFeature("VoidClassSkills", LocalizationTool.GetString("Void.Skills.Name"),
                LocalizationTool.GetString("Void.Skills.Description"), null, 0)
                .SetComponents(
                Helper.CreateAddClassSkill(StatType.SkillKnowledgeWorld),
                Helper.CreateAddClassSkill(StatType.SkillMobility)
                );

            return feature;
        }

        private static void CreateVoidBlastsSelection()
        {
            // Create Both Progressions
            CreateGravityBlast();
            CreateNegativeBlast();

            var selection = Helper.CreateBlueprintFeatureSelection("GravityBlastSelection", LocalizationTool.GetString("Void.Selection.Name"),
                LocalizationTool.GetString("Void.Selection.Description"), null, FeatureGroup.None, SelectionMode.Default);
            selection.IsClassFeature = true;

            Helper.AppendAndReplace<BlueprintFeatureReference>(ref selection.m_AllFeatures, 
                AnyRef.ToAny(Tree.Gravity.Progession), 
                AnyRef.ToAny(Tree.Negative.Progession));
        }

        private static void PushInfusions(params KineticistTree.Element[] elements)
        {
            var pushingInfusion_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("fbb97f35-a41b-71c4-cbc3-6c5f3995b892"); // PushingInfusionFeature
            var pushingInfusion_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("f795bede-8bae-faf4-d9d7-f404ede960ba"); // PushingInfusionBuff

            foreach (var element in elements)
            {
                var prereq = pushingInfusion_feature.GetComponent<PrerequisiteFeaturesFromList>();
                Helper.AppendAndReplace(ref prereq.m_Features, element.BlastFeature);

                var applicable = pushingInfusion_buff.GetComponent<AddKineticistBurnModifier>();
                Helper.AppendAndReplace(ref applicable.m_AppliableTo, element.BaseAbility);

                var trigger = pushingInfusion_buff.GetComponent<AddKineticistInfusionDamageTrigger>();
                Helper.AppendAndReplace(ref trigger.m_AbilityList, element.BaseAbility);
            }
        }

        #endregion

        #region Elemental Focus Selection

        private static void CreateVoidElementalFocus(BlueprintFeatureBase class_skills, BlueprintFeatureBase emptiness)
        {
            BlueprintProgression progression = Helper.CreateBlueprintProgression("ElementalFocusVoid", LocalizationTool.GetString("Void"),
                LocalizationTool.GetString("Void.Focus.Description"), null,
                FeatureGroup.KineticElementalFocus)
                .SetComponents(Helper.CreatePrerequisiteNoArchetype(Kineticist.ref_blood_kineticist, Tree.Class));

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            var entry1 = Helper.CreateLevelEntry(1, Tree.Gravity.Selection, class_skills);
            var entry2 = Helper.CreateLevelEntry(2, emptiness);
            Helper.AddEntries(progression, entry1, entry2);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusFirst.GetBlueprint()).m_AllFeatures, progression.ToRef());
        }

        private static void CreateKineticKnightVoidFocus(BlueprintFeatureBase class_skills, BlueprintFeatureBase emptiness)
        {
            BlueprintProgression progression = Helper.CreateBlueprintProgression("KineticKnightElementalFocusVoid", LocalizationTool.GetString("Void"),
                LocalizationTool.GetString("Void.Focus.Description"), null,
                FeatureGroup.KineticElementalFocus)
                .SetComponents(new AddEquipmentEntity { EquipmentEntity = new EquipmentEntityLink { AssetId = "aecc5905323948449b4cd3bfe36e5daf" } });

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            var entry1 = Helper.CreateLevelEntry(1, Tree.Gravity.Selection, class_skills);
            var entry2 = Helper.CreateLevelEntry(4, emptiness);
            Helper.AddEntries(progression, entry1, entry2);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusKnight.GetBlueprint()).m_AllFeatures, progression.ToRef());
        }

        private static void CreateSecondElementVoid()
        {
            BlueprintProgression progression = Helper.CreateBlueprintProgression("SecondaryElementVoid", LocalizationTool.GetString("Void"),
                LocalizationTool.GetString("Void.Focus.Description"), null,
                FeatureGroup.KineticElementalFocus);
            progression.HideInCharacterSheetAndLevelUp = true;
            
            progression.SetComponents
                (
                Helper.CreateActivateTrigger
                    (
                    Helper.CreateConditionsChecker(Operation.Or, 
                        Helper.CreateHasFact(new FactOwner(), AnyRef.ToAny(Tree.FocusVoid.First)),
                        Helper.CreateHasFact(new FactOwner(), AnyRef.ToAny(Tree.FocusVoid.Knight))
                        ),
                    new GameAction[]
                    {
                        Helper.CreateAddFact(new FactOwner(),AnyRef.ToAny(Tree.Gravity.BlastFeature)),
                        Helper.CreateAddFact(new FactOwner(),AnyRef.ToAny(Tree.Negative.BlastFeature)),
                        Helper.CreateAddFact(new FactOwner(),AnyRef.ToAny(Tree.Composite_Void.BlastFeature))
                     }
                    ),
                Helper.CreateAddFacts(AnyRef.ToAny(Tree.CompositeBuff))
                );

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();
            
            // Can be any void basic: Gravity or Negative
            var entry1 = Helper.CreateLevelEntry(7, Tree.Gravity.Selection);
            Helper.AddEntries(progression, entry1);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusSecond.GetBlueprint()).m_AllFeatures, progression.ToRef());
        }

        private static void CreateThirdElementVoid()
        {
            var progression = Helper.CreateBlueprintProgression("ThirdElementVoid", LocalizationTool.GetString("Void"),
                LocalizationTool.GetString("Void.Focus.Description"), null,
                FeatureGroup.KineticElementalFocus);
            progression.HideInCharacterSheetAndLevelUp = true;
            
            progression.SetComponents
                (
                Helper.CreateActivateTrigger
                    (
                    Helper.CreateConditionsChecker(Operation.Or,
                        Helper.CreateHasFact(new FactOwner(), AnyRef.ToAny(Tree.FocusVoid.First)),
                        Helper.CreateHasFact(new FactOwner(), AnyRef.ToAny(Tree.FocusVoid.Knight))
                        ),
                    new GameAction[]
                    {
                        Helper.CreateAddFact(new FactOwner(),AnyRef.ToAny(Tree.Composite_Void.BlastFeature))
                    }
                    ),
                Helper.CreateAddFacts(AnyRef.ToAny(Tree.CompositeBuff)),
                Helper.CreatePrerequisiteNoFeature(AnyRef.ToAny(Tree.FocusVoid.Second))
                );

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            var entry1 = Helper.CreateLevelEntry(15, Tree.Gravity.Selection);
            Helper.AddEntries(progression, entry1);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusThird.GetBlueprint()).m_AllFeatures, progression.ToRef());
        }

        #endregion

        #region Emptiness

        public static BlueprintFeature CreateEmptiness()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/emptiness.png");

            #region Resource

            var emptiness_resource = Helper.CreateBlueprintAbilityResource("EmptinessResource", min: 0, max: 20, baseValue: 20);
            
            #endregion
            #region Effect Feature

            var emptiness_effect_feature = Helper.CreateBlueprintFeature("EmptinessEffectFeature",
                null, null, icon, FeatureGroup.None);
            emptiness_effect_feature.Ranks = 20;
            emptiness_effect_feature.HideInUI = true;
            emptiness_effect_feature.HideInCharacterSheetAndLevelUp = true;
            emptiness_effect_feature.IsClassFeature = true;
            emptiness_effect_feature.SetComponents
                (
                new AddFacts { }
                );

            #endregion
            #region Effect Buff

            var emptiness_effect_buff = Helper.CreateBlueprintBuff("EmptinessEffectBuff",
                null, null, icon);
            emptiness_effect_buff.Flags(hidden: true, stayOnDeath: true);
            emptiness_effect_buff.m_Flags |= BlueprintBuff.Flags.RemoveOnRest;
            emptiness_effect_buff.Stacking = StackingType.Stack;
            emptiness_effect_buff.IsClassFeature = true;
            emptiness_effect_buff.SetComponents
                (
                Helper.CreateAddFacts(emptiness_effect_feature.ToRef2())
                );

            #endregion
            #region Buff

            var emptiness_buff = Helper.CreateBlueprintBuff("EmptinessBuff",
                null, null, icon);
            emptiness_buff.Flags(hidden: true, stayOnDeath: true);
            emptiness_buff.Stacking = StackingType.Replace;
            emptiness_buff.IsClassFeature = true;
            emptiness_buff.SetComponents
                (
                new AddDamageResistanceEnergy
                {
                    Type = DamageEnergyType.NegativeEnergy,
                    UseValueMultiplier = true,
                    ValueMultiplier = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 2
                    },
                    Value = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        ValueRank = AbilityRankType.StatBonus
                    }
                },
                new AddFortification
                {
                    UseContextValue = true,
                    Value = new ContextValue
                    {
                        ValueType = ContextValueType.Shared,
                        ValueShared = AbilitySharedValue.StatBonus
                    }
                },
                new SavingThrowBonusAgainstDescriptor
                {
                    SpellDescriptor = SpellDescriptor.Emotion,
                    ModifierDescriptor = ModifierDescriptor.UntypedStackable,
                    Bonus = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        ValueRank = AbilityRankType.StatBonus
                    }
                },
                Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, progression: ContextRankProgression.BonusValue, type: AbilityRankType.StatBonus, stepLevel: 1, min: 0, max: 20, feature: emptiness_effect_feature.ToRef()),
                new ContextCalculateSharedValue
                {
                    Modifier = 5.0,
                    ValueType = AbilitySharedValue.StatBonus,
                    Value = new ContextDiceValue
                    {
                        DiceType = DiceType.Zero,
                        DiceCountValue = new ContextValue
                        {
                            ValueType = ContextValueType.Simple,
                            Value = 0
                        },
                        BonusValue = new ContextValue
                        {
                            ValueType = ContextValueType.Rank,
                            ValueRank = AbilityRankType.StatBonus
                        }
                    }
                },
                Helper.CreateRecalculateOnFactsChange(emptiness_effect_feature.ToRef2())
                );

            #endregion
            #region Ability

            var emptiness_ability = Helper.CreateBlueprintAbility("EmptinessAbility", LocalizationTool.GetString("Void.Defense"),
                LocalizationTool.GetString("Void.Defense.Description"), icon, AbilityType.Special, UnitCommand.CommandType.Free,
                AbilityRange.Personal).TargetSelf(CastAnimationStyle.Omni);
            emptiness_ability.AvailableMetamagic = Metamagic.Heighten;
            emptiness_ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(actions: emptiness_effect_buff.CreateContextActionApplyBuff(permanent: true)),
                Helper.CreateAbilityResourceLogic(AnyRef.ToAny(emptiness_resource), 1),
                Helper.CreateAbilityAcceptBurnOnCast(1)
                );

            #endregion

            var emptiness_feature = Helper.CreateBlueprintFeature("Emptiness", LocalizationTool.GetString("Void.Defense"),
                LocalizationTool.GetString("Void.Defense.Description"), icon, FeatureGroup.None);
            emptiness_feature.IsClassFeature = true;
            emptiness_feature.SetComponents
                (
                Helper.CreateAddFacts(emptiness_buff.ToRef2(), emptiness_ability.ToRef()),
                Helper.CreatePrerequisiteFeature(Tree.Gravity.BlastFeature, any: true),
                Helper.CreatePrerequisiteFeature(Tree.Gravity.Blade.Feature, any: true),
                Helper.CreatePrerequisiteFeature(Tree.Negative.BlastFeature, any: true),
                Helper.CreatePrerequisiteFeature(Tree.Negative.Blade.Feature, any: true),
                Helper.CreateAddAbilityResources(emptiness_resource)
                );

            return emptiness_feature;
        }

        #endregion

        #region Gravity Blast

        public static void CreateGravityBlast()
        {
            // Variants
            var standard = CreateGravityBlastVariant_base();
            var extended = CreateGravityBlastVariant_extended();
            var spindle = CreateGravityBlastVariant_spindle();
            var wall = CreateGravityBlastVariant_wall();
            //var blade = CreateGravityBlastVariant_blade();
            var blade = Kineticist.Blade.CreateKineticBlade(Tree,
                "Gravity", "Gravity", isComposite: false,
                "30f3331e77343eb4f8f0bc51a0fcf454", Resource.Projectile.Kinetic_EarthBlast00_Projectile,
                Helper.CreateSprite(Main.ModPath + "/Icons/gravityBlast.png"),
                Helper.CreateSprite(Main.ModPath + "/Icons/gravityBlast.png"),
                p: PhysicalDamageForm.Bludgeoning);
            var singularity = CreateGravityBlastVariant_singularity();
            // Ability
            CreateGravityBlastAbility(standard, extended, spindle, wall, blade, singularity);
            // Feature
            CreateGravityBlastFeature();
            // Progression
            CreateGravityBlastProgression();

        }

        #region Gravity Variants

        private static BlueprintAbility CreateGravityBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/gravityBlast.png");

            var ability = Helper.CreateBlueprintAbility("GravityBlastAbility", LocalizationTool.GetString("Void.Gravity.Name"),
                LocalizationTool.GetString("Void.Gravity.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 0, blast: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.NegativeCommonProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateGravityBlastVariant_extended()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("cb2d9e6355dd33940b2bef49e544b0bf"); // ExtendedRangeInfusion

            var ability = Helper.CreateBlueprintAbility("ExtendedRangeGravityBlastAbility", 
                Tree.ExtendedRange.Feature.Get().m_DisplayName,
                Tree.ExtendedRange.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 0),
                Kineticist.Blast.RequiredFeat(Tree.ExtendedRange.Feature),
                Kineticist.Blast.Projectile(Resource.Projectile.NegativeCommonProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Gravity.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateGravityBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleGravityBlastAbility",
                Tree.Spindle.Feature.Get().m_DisplayName,
                Tree.Spindle.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 0),
                Kineticist.Blast.RequiredFeat(Tree.Spindle.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverChain
                {
                    m_ProjectileFirst = Resource.Projectile.NegativeCommonProjectile00.ToRef<BlueprintProjectileReference>(),
                    m_Projectile = Resource.Projectile.NegativeCommonProjectile00.ToRef<BlueprintProjectileReference>(),
                    TargetsCount = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 70,
                    },
                    Radius = new Feet { m_Value = 5 },
                    TargetDead = false,
                    m_TargetType = TargetType.Enemy,
                    m_Condition = new ConditionsChecker { Conditions = null, Operation = Operation.And}
                }
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Gravity.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }
        private static BlueprintAbility CreateGravityBlastVariant_wall()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c684335918896ce4ab13e96cec929796"); // WallInfusion

            var action = new ContextActionSpawnAreaEffect
            {
                DurationValue = Helper.CreateContextDurationValue(
                    new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    }, DiceType.Zero,
                    new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }, DurationRate.Rounds),
                m_AreaEffect = Kineticist.CreateWallAreaEffect("Gravity", "1f26aacd3ad314e4c820d4fe2ac3fd46", p: PhysicalDamageForm.Bludgeoning),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallGravityBlastAbility",
                Tree.Wall.Feature.Get().m_DisplayName,
                Tree.Wall.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 0),
                Kineticist.Blast.RequiredFeat(Tree.Wall.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Gravity.BaseAbility;

            return ability;
        }
        private static BlueprintAbility CreateGravityBlastVariant_singularity()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("1325e698f4a3f224b880e3b83a551228"); // Supernova

            var ability = Helper.CreateBlueprintAbility("SingularityGravityBlastAbility",
                LocalizationTool.GetString("Void.Singularity.Name"),
                LocalizationTool.GetString("Void.Singularity.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, CreateSingularityEffect("Gravity", p: PhysicalDamageForm.Bludgeoning)),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 0),
                Kineticist.Blast.RequiredFeat(Tree.Singularity.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Gravity.BaseAbility;

            return ability;
        }

        #endregion

        public static void CreateGravityBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/gravityBlast.png");

            var ability = Helper.CreateBlueprintAbility("GravityBlastBase", LocalizationTool.GetString("Void.Gravity.Name"),
                LocalizationTool.GetString("Void.Gravity.Description"), icon, AbilityType.Special, 
                UnitCommand.CommandType.Standard, AbilityRange.Close, 
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.ToAny(Tree.FocusFirst)), 
                Kineticist.Blast.BurnCost(null, 0, 0, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }
        }

        public static void CreateGravityBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("GravityBlastFeature", LocalizationTool.GetString("Void.Gravity.Name"),
                LocalizationTool.GetString("Void.Gravity.Description"), null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(AnyRef.ToAny(Tree.Gravity.BaseAbility))
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;
        }

        public static void CreateGravityBlastProgression()
        {
            var progression = Helper.CreateBlueprintProgression("GravityBlastProgression", LocalizationTool.GetString("Void.Gravity.Name"),
                LocalizationTool.GetString("Void.Gravity.Description"), null, 0)
                .SetComponents
                (
                Helper.CreateAddFacts(Kineticist.ref_compositeBlastBuff),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.ToAny(Tree.KineticBlade.Feature), 
                    AnyRef.ToAny(Tree.Gravity.Blade.Feature)),
                Helper.CreateAddFeatureIfHasFact(AnyRef.ToAny(Tree.Gravity.BlastFeature))
                );

            var entry = Helper.CreateLevelEntry(1, AnyRef.ToAny(Tree.Gravity.BlastFeature));
            Helper.AddEntries(progression, entry);
        }

        #endregion

        #region Negative Blast

        public static void CreateNegativeBlast()
        {
            // Variants
            var standard = CreateNegativeBlastVariant_base();
            var extended = CreateNegativeBlastVariant_extended();
            var spindle = CreateNegativeBlastVariant_spindle();
            var wall = CreateNegativeBlastVariant_wall();
            //var blade = CreateNegativeBlastVariant_blade();
            var blade = Kineticist.Blade.CreateKineticBlade(Tree, "Negative", "Negative",
                false, "907d3c215c9522d4e8a3b763f1b32935", Resource.Projectile.UmbralStrike00,
                Helper.CreateSprite(Main.ModPath + "/Icons/negativeBlast.png"),
                Helper.CreateSprite(Main.ModPath + "/Icons/negativeBlast.png"),
                e: DamageEnergyType.NegativeEnergy);
            var singularity = CreateNegativeBlastVariant_singularity();
            // Ability
            CreateNegativeBlastAbility(standard, extended, spindle, wall, blade, singularity);
            // Feature
            CreateNegativeBlastFeature();
            // Progression
            CreateNegativeBlastProgression();

        }

        #region Negative Variants

        private static BlueprintAbility CreateNegativeBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/negativeBlast.png");

            var ability = Helper.CreateBlueprintAbility("NegativeBlastAbility", LocalizationTool.GetString("Void.Negative.Name"),
                LocalizationTool.GetString("Void.Negative.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, e: DamageEnergyType.NegativeEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 0, blast: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.UmbralStrike00, false, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            return ability;
        }

        private static BlueprintAbility CreateNegativeBlastVariant_extended()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("cb2d9e6355dd33940b2bef49e544b0bf"); // ExtendedRangeInfusion

            var ability = Helper.CreateBlueprintAbility("ExtendedRangeNegativeBlastAbility",
                Tree.ExtendedRange.Feature.Get().m_DisplayName,
                Tree.ExtendedRange.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, e: DamageEnergyType.NegativeEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 0),
                Kineticist.Blast.RequiredFeat(Tree.ExtendedRange.Feature),
                Kineticist.Blast.Projectile(Resource.Projectile.UmbralStrike00, false, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Negative.BaseAbility;

            return ability;
        }

        private static BlueprintAbility CreateNegativeBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleNegativeBlastAbility",
                Tree.Spindle.Feature.Get().m_DisplayName,
                Tree.Spindle.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, e: DamageEnergyType.NegativeEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 0),
                Kineticist.Blast.RequiredFeat(Tree.Spindle.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverChain
                {
                    m_ProjectileFirst = Resource.Projectile.UmbralStrike00.ToRef<BlueprintProjectileReference>(),
                    m_Projectile = Resource.Projectile.UmbralStrike00.ToRef<BlueprintProjectileReference>(),
                    TargetsCount = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 70,
                    },
                    Radius = new Feet { m_Value = 5 },
                    TargetDead = false,
                    m_TargetType = TargetType.Enemy,
                    m_Condition = new ConditionsChecker { Conditions = null, Operation = Operation.And }
                }
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Negative.BaseAbility;

            return ability;
        }

        private static BlueprintAbility CreateNegativeBlastVariant_wall()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c684335918896ce4ab13e96cec929796"); // WallInfusion

            var action = new ContextActionSpawnAreaEffect
            {
                DurationValue = Helper.CreateContextDurationValue(
                    new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    }, DiceType.Zero,
                    new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }, DurationRate.Rounds),
                m_AreaEffect = Kineticist.CreateWallAreaEffect("Negative", "4ffc8d2162a215e44a1a728752b762eb", e: DamageEnergyType.NegativeEnergy),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallNegativeBlastAbility",
                Tree.Wall.Feature.Get().m_DisplayName,
                Tree.Wall.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 0),
                Kineticist.Blast.RequiredFeat(Tree.Wall.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Negative.BaseAbility;

            return ability;
        }

        private static BlueprintAbility CreateNegativeBlastVariant_singularity()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("1325e698f4a3f224b880e3b83a551228"); // Supernova

            var ability = Helper.CreateBlueprintAbility("SingularityNegativeBlastAbility",
                LocalizationTool.GetString("Void.Singularity.Name"),
                LocalizationTool.GetString("Void.Singularity.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, CreateSingularityEffect("Negative", e: DamageEnergyType.NegativeEnergy)),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 0),
                Kineticist.Blast.RequiredFeat(Tree.Singularity.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Negative.BaseAbility;

            return ability;
        }

        #endregion

        public static void CreateNegativeBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/negativeBlast.png");

            var ability = Helper.CreateBlueprintAbility("NegativeBlastBase", LocalizationTool.GetString("Void.Negative.Name"),
                LocalizationTool.GetString("Void.Negative.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.ToAny(Tree.FocusFirst)),
                Kineticist.Blast.BurnCost(null, 0, 0, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }
        }

        public static void CreateNegativeBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("NegativeBlastFeature", LocalizationTool.GetString("Void.Negative.Name"),
                LocalizationTool.GetString("Void.Negative.Description"), null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(AnyRef.ToAny(Tree.Negative.BaseAbility))
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;
        }

        public static void CreateNegativeBlastProgression()
        {
            var progression = Helper.CreateBlueprintProgression("NegativeBlastProgression", LocalizationTool.GetString("Void.Negative.Name"),
                LocalizationTool.GetString("Void.Negative.Description"), null, 0)
                .SetComponents
                (
                Helper.CreateAddFacts(Kineticist.ref_compositeBlastBuff),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.ToAny(Tree.KineticBlade.Feature),
                    AnyRef.ToAny(Tree.Negative.Blade.Feature)),
                Helper.CreateAddFeatureIfHasFact(AnyRef.ToAny(Tree.Negative.BlastFeature))
                );

            var entry = Helper.CreateLevelEntry(1, AnyRef.ToAny(Tree.Negative.BlastFeature));
            Helper.AddEntries(progression, entry);
        }


        #endregion

        #region Composite Blasts

        private static void CreateVoidComposites()
        {
            CreateVoidBlast();
            CreateGraviticBoost();
            CreateGraviticBoostGreater();
            CreateNegativeAdmixture();

            AddMixturesToComposite();
        }

        #region Void Blast

        public static void CreateVoidBlast()
        {
            // Variants
            var standard = CreateVoidBlastVariant_base();
            var extended = CreateVoidBlastVariant_extended();
            var spindle = CreateVoidBlastVariant_spindle();
            var wall = CreateVoidBlastVariant_wall();
            //var blade = CreateVoidBlastVariant_blade();
            var blade = Kineticist.Blade.CreateKineticBlade(Tree,
                "Void", "Void", isComposite: true,
                "30f3331e77343eb4f8f0bc51a0fcf454", Resource.Projectile.Kinetic_EarthBlast00_Projectile,
                Helper.CreateSprite(Main.ModPath + "/Icons/VoidBlast.png"),
                Helper.CreateSprite(Main.ModPath + "/Icons/VoidBlast.png"),
                p: PhysicalDamageForm.Bludgeoning, e: DamageEnergyType.NegativeEnergy);
            var singularity = CreateVoidBlastVariant_singularity();
            // Ability
            CreateVoidBlastAbility(standard, extended, spindle, wall, blade, singularity);
            // Feature
            CreateVoidBlastFeature();
            // Progression - Not needed due to Composite Blast
            // CreateVoidBlastProgression();

        }

        #region Void Variants

        private static BlueprintAbility CreateVoidBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/VoidBlast.png");

            var ability = Helper.CreateBlueprintAbility("VoidBlastAbility", LocalizationTool.GetString("Void.Void.Name"),
                LocalizationTool.GetString("Void.Void.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning, e: DamageEnergyType.NegativeEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 0, blast: 2),
                Kineticist.Blast.Projectile(Resource.Projectile.NegativeCommonProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }

        private static BlueprintAbility CreateVoidBlastVariant_extended()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("cb2d9e6355dd33940b2bef49e544b0bf"); // ExtendedRangeInfusion

            var ability = Helper.CreateBlueprintAbility("ExtendedRangeVoidBlastAbility",
                Tree.ExtendedRange.Feature.Get().m_DisplayName,
                Tree.ExtendedRange.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning, e: DamageEnergyType.NegativeEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.ExtendedRange.Feature),
                Kineticist.Blast.Projectile(Resource.Projectile.NegativeCommonProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Void.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }

        private static BlueprintAbility CreateVoidBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleVoidBlastAbility",
                Tree.Spindle.Feature.Get().m_DisplayName,
                Tree.Spindle.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning, e: DamageEnergyType.NegativeEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.Spindle.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverChain
                {
                    m_ProjectileFirst = Resource.Projectile.NegativeCommonProjectile00.ToRef<BlueprintProjectileReference>(),
                    m_Projectile = Resource.Projectile.NegativeCommonProjectile00.ToRef<BlueprintProjectileReference>(),
                    TargetsCount = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 70,
                    },
                    Radius = new Feet { m_Value = 5 },
                    TargetDead = false,
                    m_TargetType = TargetType.Enemy,
                    m_Condition = new ConditionsChecker { Conditions = null, Operation = Operation.And }
                }
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Void.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }

        private static BlueprintAbility CreateVoidBlastVariant_wall()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c684335918896ce4ab13e96cec929796"); // WallInfusion

            var action = new ContextActionSpawnAreaEffect
            {
                DurationValue = Helper.CreateContextDurationValue(
                    new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    }, DiceType.Zero,
                    new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }, DurationRate.Rounds),
                m_AreaEffect = Kineticist.CreateWallAreaEffect("Void", "1f26aacd3ad314e4c820d4fe2ac3fd46", p: PhysicalDamageForm.Bludgeoning, e: DamageEnergyType.NegativeEnergy),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallVoidBlastAbility",
                Tree.Wall.Feature.Get().m_DisplayName,
                Tree.Wall.Feature.Get().m_Description, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.Wall.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Void.BaseAbility;

            return ability;
        }

        private static BlueprintAbility CreateVoidBlastVariant_singularity()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("1325e698f4a3f224b880e3b83a551228"); // Supernova

            var ability = Helper.CreateBlueprintAbility("SingularityVoidBlastAbility",
                LocalizationTool.GetString("Void.Singularity.Name"),
                LocalizationTool.GetString("Void.Singularity.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, CreateSingularityEffect("Void", p: PhysicalDamageForm.Bludgeoning, e: DamageEnergyType.NegativeEnergy)),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 2),
                Kineticist.Blast.RequiredFeat(Tree.Singularity.Feature),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Tree.Composite_Void.BaseAbility;

            return ability;
        }

        #endregion

        public static void CreateVoidBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/VoidBlast.png");

            var ability = Helper.CreateBlueprintAbility("VoidBlastBase", LocalizationTool.GetString("Void.Void.Name"),
                LocalizationTool.GetString("Void.Void.Description"), icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.ToAny(Tree.FocusFirst)),
                Kineticist.Blast.BurnCost(null, 0, 2, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            Tree.Composite_Void.BaseAbility = ability.ToRef();
        }

        public static void CreateVoidBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("VoidBlastFeature", LocalizationTool.GetString("Void.Void.Name"),
                LocalizationTool.GetString("Void.Void.Description"), null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.ToAny(Tree.Composite_Void.BaseAbility)),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.ToAny(Kineticist.ref_infusion_kineticBlade),
                    AnyRef.ToAny(Tree.Composite_Void.Blade.Feature))
                );
            feature.HideInCharacterSheetAndLevelUp = true;
            feature.HideInUI = true;
            feature.IsClassFeature = true;
        }

        #endregion

        #region Gravitic Boost

        public static void CreateGraviticBoost()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("0e449a987c784b6f8b13319936667053"); // RitualGreaterChannelNegativeEnergyAbility

            var ability = Helper.CreateBlueprintActivatableAbility("GraviticBoostAbility", out var buff, LocalizationTool.GetString("Void.Admixture.Gravity.Name"),
                LocalizationTool.GetString("Void.Admixture.Gravity.Description"), icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.None, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AbilityUniqueGraviticBoost
                {
                    m_AbilityList = Tree.GetAll(basic: true, onlyPhysical: true, archetype: true).Select(s => s.BaseAbility).ToArray()
                },
                new AddKineticistBurnModifier
                {
                    BurnType = KineticistBurnType.Blast,
                    Value = 2
                },
                new RecalculateOnStatChange
                {
                    Stat = StatType.Unknown,
                    UseKineticistMainStat = true
                },
                new ContextCalculateAbilityParamsBasedOnClass
                {
                    UseKineticistMainStat = true,
                    StatType = StatType.Charisma,
                    m_CharacterClass = Tree.Class
                }
                );

            var feature = Helper.CreateBlueprintFeature("GraviticBoostFeature", LocalizationTool.GetString("Void.Admixture.Gravity.Name"),
                LocalizationTool.GetString("Void.Admixture.Gravity.Description"), icon, FeatureGroup.None);
            feature.SetComponents
                (
                Helper.CreateAddFacts(ability.ToRef())
                );

            GraviticBoost.Feature = feature.ToRef();
            GraviticBoost.Buff = buff.ToRef();

            Kineticist.AddElementsToInfusion(feature, buff, Tree.GetAll(basic: true, onlyPhysical: true, archetype: true).ToList().ToArray());
        }
        public static void CreateGraviticBoostGreater()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("0e449a987c784b6f8b13319936667053"); // RitualGreaterChannelNegativeEnergyAbility

            var ability = Helper.CreateBlueprintActivatableAbility("GraviticBoostGreaterAbility", out var buff, LocalizationTool.GetString("Void.Admixture.Gravity.Greater.Name"),
                LocalizationTool.GetString("Void.Admixture.Gravity.Greater.Description"), icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.None, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AbilityUniqueGraviticBoost
                {
                    m_AbilityList = Tree.GetAll(composite: true, onlyPhysical: true, archetype: true).Select(s => s.BaseAbility).ToArray()
                },
                new AddKineticistBurnModifier
                {
                    BurnType = KineticistBurnType.Blast,
                    Value = 3
                },
                new RecalculateOnStatChange
                {
                    Stat = StatType.Unknown,
                    UseKineticistMainStat = true
                },
                new ContextCalculateAbilityParamsBasedOnClass
                {
                    UseKineticistMainStat = true,
                    StatType = StatType.Charisma,
                    m_CharacterClass = Tree.Class
                }
                );

            var feature = Helper.CreateBlueprintFeature("GraviticBoostGreaterFeature", LocalizationTool.GetString("Void.Admixture.Gravity.Greater.Name"),
                LocalizationTool.GetString("Void.Admixture.Gravity.Greater.Description"), icon, FeatureGroup.None);
            feature.SetComponents
                (
                Helper.CreateAddFacts(ability.ToRef())
                );

            GraviticBoostGreater.Feature = feature.ToRef();
            GraviticBoostGreater.Buff = buff.ToRef();

            Kineticist.AddElementsToInfusion(feature, buff, Tree.GetAll(composite: true, onlyPhysical: true, archetype: true).ToList().ToArray());
        }

        #endregion

        #region Negative Admixture

        public static void CreateNegativeAdmixture()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/negativeAdmixture.png");

            var ability = Helper.CreateBlueprintActivatableAbility("NegativeAdmixtureAbility", out var buff, LocalizationTool.GetString("Void.Admixture.Negative.Name"),
                LocalizationTool.GetString("Void.Admixture.Negative.Description"), icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.None, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AbilityUniqueNegativeAdmixture
                {
                    m_AbilityList = Tree.GetAll(basic: true, onlyEnergy: true, archetype: true).Select(s=> s.BaseAbility).ToArray()
                },
                new AddKineticistBurnModifier
                {
                    BurnType = KineticistBurnType.Infusion,
                    Value = 1
                },
                new RecalculateOnStatChange
                {
                    Stat = StatType.Unknown,
                    UseKineticistMainStat = true
                },
                new ContextCalculateAbilityParamsBasedOnClass
                {
                    UseKineticistMainStat = true,
                    StatType = StatType.Charisma,
                    m_CharacterClass = Tree.Class
                }
                );

            var feature = Helper.CreateBlueprintFeature("NegativeAdmixtureFeature", LocalizationTool.GetString("Void.Admixture.Negative.Name"),
                LocalizationTool.GetString("Void.Admixture.Negative.Description"), icon, FeatureGroup.None);
            feature.SetComponents
                (
                Helper.CreateAddFacts(ability.ToRef())
                );

            NegativeAdmixture.Feature = feature.ToRef();
            NegativeAdmixture.Buff = buff.ToRef();

            Kineticist.AddElementsToInfusion(feature, buff, Tree.GetAll(basic: true, onlyEnergy: true, archetype: true).ToList().ToArray());
        }

        #endregion

        private static void AddMixturesToComposite()
        {
            var inner_gravity_checker = new ConditionsChecker
            {
                Operation = Operation.Or,
                Conditions = Tree.GetAll(basic: true, onlyPhysical: true, archetype: true)
                        .Select(s => Helper.CreateContextConditionHasFact(
                            AnyRef.ToAny(s.BlastFeature))).ToArray()
            };
            var inner_gravity_conditional = new Conditional
            {
                ConditionsChecker = inner_gravity_checker,
                IfFalse = null,
                IfTrue = Helper.CreateActionList(Helper.CreateContextActionAddFeature(GraviticBoost.Feature))
            };
            var outer_gravity_conditional = Helper.CreateConditional(Helper.CreateContextConditionHasFact(AnyRef.ToAny(Tree.Gravity.BlastFeature)),
                ifFalse: null, ifTrue: inner_gravity_conditional);

            var inner_negative_checker = new ConditionsChecker
            {
                Operation = Operation.Or,
                Conditions = Tree.GetAll(basic: true, onlyEnergy: true, archetype: true)
                        .Select(s => Helper.CreateContextConditionHasFact(
                            AnyRef.ToAny(s.BlastFeature))).ToArray()
            };
            var inner_negative_conditional = new Conditional
            {
                ConditionsChecker = inner_negative_checker,
                IfFalse = null,
                IfTrue = Helper.CreateActionList(Helper.CreateContextActionAddFeature(NegativeAdmixture.Feature))
            };
            var outer_negative_conditional = Helper.CreateConditional(Helper.CreateContextConditionHasFact(AnyRef.ToAny(Tree.Negative.BlastFeature)),
                ifFalse: null, ifTrue: inner_negative_conditional);

            var composite_action = Tree.CompositeBuff.Get().GetComponent<AddFactContextActions>();
            Helper.AppendAndReplace(ref composite_action.Activated.Actions, outer_gravity_conditional, outer_negative_conditional);

        }

        #endregion

        #region Infusions

        public static void CreateVoidInfusions()
        {
            CreateDampeningInfusion();
            CreateEnervatingInfusion();
            CreatePullingInfusion();
            CreateUnnervingInfusion();
            CreateVampiricInfusion();
            CreateWeighingInfusion();
            CreateSingularityInfusion();

            Kineticist.TryDarkCodexAddExtraWildTalent
                (
                Tree.Dampening.Feature,
                Tree.Enervating.Feature,
                Tree.Pulling.Feature,
                Tree.Unnerving.Feature,
                Tree.Vampiric.Feature,
                Tree.Weighing.Feature,
                Tree.Singularity.Feature
                );
            Helper.AppendAndReplace(ref Kineticist.infusion_selection.m_AllFeatures,
                Tree.Dampening.Feature,
                Tree.Enervating.Feature,
                Tree.Pulling.Feature,
                Tree.Unnerving.Feature,
                Tree.Vampiric.Feature,
                Tree.Weighing.Feature,
                Tree.Singularity.Feature
                );
        }

        private static void CreateDampeningInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("037460f7ae3e21943b237007f2b1a5d5"); // Dazzling Infusion Icon
            var dazzled_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("df6d1025da07524429afbae248845ecc"); // DazzledBuff

            var ability = Helper.CreateBlueprintActivatableAbility("DampeningInfusionAbility", out var buff, LocalizationTool.GetString("Void.Dampening.Name"),
                LocalizationTool.GetString("Void.Dampening.Description"), icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.SubstanceInfusion, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistInfusionDamageTrigger
                {
                    CheckSpellParent = true,
                    TriggerOnDirectDamage = true,
                    Actions = Helper.CreateActionList(new ContextActionSavingThrow 
                        {
                        Type = SavingThrowType.Will,
                        Actions = Helper.CreateActionList(Helper.CreateContextActionConditionalSaved(failed: dazzled_buff.CreateContextActionApplyBuff(1, DurationRate.Minutes, asChild: true)))
                        }),
                    m_AbilityList = null,
                    SpellDescriptorsList = (SpellDescriptor)2632353982198054912
                },
                new AddKineticistBurnModifier
                {
                    BurnType = KineticistBurnType.Infusion,
                    Value = 1
                },
                new RecalculateOnStatChange
                {
                    Stat = StatType.Unknown,
                    UseKineticistMainStat = true
                },
                new ContextCalculateAbilityParamsBasedOnClass
                {
                    UseKineticistMainStat = true,
                    StatType = StatType.Charisma,
                    m_CharacterClass = Tree.Class
                }
                );

            var feature = Helper.CreateBlueprintFeature("DampeningInfusionFeature", LocalizationTool.GetString("Void.Dampening.Name"),
                LocalizationTool.GetString("Void.Dampening.Description"), icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreateAddFacts(ability.ToRef())
                );

            Tree.Dampening.Feature = feature.ToRef();
            Tree.Dampening.Buff = buff.ToRef();
        }

        private static void CreateEnervatingInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("f34fb78eaaec141469079af124bcfa0f"); // Enervation

            var saving_throw = new ContextActionSavingThrow
            {
                Type = SavingThrowType.Fortitude,
                Actions = Helper.CreateActionList(Helper.CreateContextActionConditionalSaved(
                    succeed: null,
                    failed: new ContextActionDealDamage
                    {
                        m_Type = ContextActionDealDamage.Type.EnergyDrain,
                        EnergyDrainType = EnergyDrainType.Temporary,
                        Duration = new ContextDurationValue
                        {
                            Rate = DurationRate.Hours,
                            DiceType = DiceType.Zero,
                            DiceCountValue = new ContextValue { ValueType = ContextValueType.Simple, Value = 0 },
                            BonusValue = new ContextValue { ValueType = ContextValueType.Simple, Value = 24 }
                        },
                        Value = new ContextDiceValue
                        {
                            DiceType = DiceType.Zero,
                            DiceCountValue = new ContextValue { ValueType = ContextValueType.Simple, Value = 0 },
                            BonusValue = new ContextValue { ValueType = ContextValueType.Simple, Value = 1 }
                        }
                    }))
            };

            var ability = Helper.CreateBlueprintActivatableAbility("EnervatingInfusionAbility", out var buff, LocalizationTool.GetString("Void.Enervating.Name"),
                LocalizationTool.GetString("Void.Enervating.Description"), icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.SubstanceInfusion, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistInfusionDamageTrigger
                {
                    CheckSpellParent = true,
                    TriggerOnDirectDamage = true,
                    Actions = Helper.CreateActionList(saving_throw),
                    m_AbilityList = null,
                    SpellDescriptorsList = (SpellDescriptor)2632353982198054912
                },
                new AddKineticistBurnModifier
                {
                    BurnType = KineticistBurnType.Infusion,
                    Value = 4
                },
                new RecalculateOnStatChange
                {
                    Stat = StatType.Unknown,
                    UseKineticistMainStat = true
                },
                new ContextCalculateAbilityParamsBasedOnClass
                {
                    UseKineticistMainStat = true,
                    StatType = StatType.Charisma,
                    m_CharacterClass = Tree.Class
                }
                );

            var feature = Helper.CreateBlueprintFeature("EnervatingInfusionFeature", LocalizationTool.GetString("Void.Enervating.Name"),
                LocalizationTool.GetString("Void.Enervating.Description"), icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 14),
                Helper.CreateAddFacts(ability.ToRef())
                );

            Tree.Enervating.Feature = feature.ToRef();
            Tree.Enervating.Buff = buff.ToRef();
        }

        private static void CreatePullingInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("d131394d9d8ef384298406cfa45bb7b7"); // Pull Action

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.Pull,
                IgnoreConcealment = false,
                ReplaceStat = true,
                UseKineticistMainStat = true
            };

            var ability = Helper.CreateBlueprintActivatableAbility("PullingInfusionAbility", out var buff, LocalizationTool.GetString("Void.Pulling.Name"),
                LocalizationTool.GetString("Void.Pulling.Description"), icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.SubstanceInfusion, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistInfusionDamageTrigger
                {
                    CheckSpellParent = true,
                    TriggerOnDirectDamage = true,
                    Actions = Helper.CreateActionList(action),
                    m_AbilityList = null,
                    SpellDescriptorsList = (SpellDescriptor)2632353982198054912
                },
                new AddKineticistBurnModifier
                {
                    BurnType = KineticistBurnType.Infusion,
                    Value = 1
                },
                new RecalculateOnStatChange
                {
                    Stat = StatType.Unknown,
                    UseKineticistMainStat = true
                },
                new ContextCalculateAbilityParamsBasedOnClass
                {
                    UseKineticistMainStat = true,
                    StatType = StatType.Charisma,
                    m_CharacterClass = Tree.Class
                }
                );

            var feature = Helper.CreateBlueprintFeature("PullingInfusionFeature", LocalizationTool.GetString("Void.Pulling.Name"),
                LocalizationTool.GetString("Void.Pulling.Description"), icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 1),
                Helper.CreateAddFacts(ability.ToRef())
                );

            Tree.Pulling.Feature = feature.ToRef();
            Tree.Pulling.Buff = buff.ToRef();
        }

        private static void CreateUnnervingInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("25ec6cb6ab1845c48a95f9c20b034220"); // Shaken
            var shaken_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220"); // Shaken

            var conditional = Helper.CreateContextActionConditionalSaved
                (
                succeed: null,
                failed: shaken_buff.CreateContextActionApplyBuff(1, DurationRate.Rounds, asChild: true)
                );

            var saving_throw = new ContextActionSavingThrow
            {
                Type = SavingThrowType.Will,
                Actions = Helper.CreateActionList(conditional)
            };

            var ability = Helper.CreateBlueprintActivatableAbility("UnnervingInfusionAbility", out var buff, LocalizationTool.GetString("Void.Unnerving.Name"),
                LocalizationTool.GetString("Void.Unnerving.Description"), icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.SubstanceInfusion, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistInfusionDamageTrigger
                {
                    CheckSpellParent = true,
                    TriggerOnDirectDamage = true,
                    Actions = Helper.CreateActionList(saving_throw),
                    m_AbilityList = null,
                    SpellDescriptorsList = (SpellDescriptor)2632353982198054912
                },
                new AddKineticistBurnModifier
                {
                    BurnType = KineticistBurnType.Infusion,
                    Value = 2
                },
                new RecalculateOnStatChange
                {
                    Stat = StatType.Unknown,
                    UseKineticistMainStat = true
                },
                new ContextCalculateAbilityParamsBasedOnClass
                {
                    UseKineticistMainStat = true,
                    StatType = StatType.Charisma,
                    m_CharacterClass = Tree.Class
                }
                );

            var feature = Helper.CreateBlueprintFeature("UnnervingInfusionFeature", LocalizationTool.GetString("Void.Unnerving.Name"),
                LocalizationTool.GetString("Void.Unnerving.Description"), icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 6),
                Helper.CreateAddFacts(ability.ToRef())
                );

            Tree.Unnerving.Feature = feature.ToRef();
            Tree.Unnerving.Buff = buff.ToRef();
        }

        private static void CreateVampiricInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("30c81aff8e5293d418759d10f193f347"); // VampiricInfusionAbility

            var effect_buff = Helper.CreateBlueprintBuff("VoidVampiricInfusionEffectBuff", LocalizationTool.GetString("Void.Vampiric.Buff.Name"),
                LocalizationTool.GetString("Void.Vampiric.Buff.Description"), icon, null);
            effect_buff.m_Flags |= BlueprintBuff.Flags.RemoveOnRest;
            effect_buff.SetComponents
                (
                new AddKineticistBurnModifier
                {
                    Value = -1,
                    BurnType = KineticistBurnType.WildTalent,
                    m_AppliableTo = new BlueprintAbilityReference[1] { VoidHealerAbility.ToRef() },
                    RemoveBuffOnAcceptBurn = true
                },
                new AddAbilityUseTrigger
                {
                    Action = Helper.CreateActionList(new ContextActionRemoveSelf {}),
                    AfterCast = true,
                    ForOneSpell = true,
                    m_Ability = VoidHealerAbility.ToRef(),
                    Type = AbilityType.Spell,
                    Range = AbilityRange.Touch
                }
                );

            var onCaster = new ContextActionOnContextCaster
            {
                Actions = Helper.CreateActionList(effect_buff.CreateContextActionApplyBuff(asChild: true, permanent: true))
            };

            var ability = Helper.CreateBlueprintActivatableAbility("VoidVampiricInfusionAbility", out var buff, LocalizationTool.GetString("Void.Vampiric.Name"),
                LocalizationTool.GetString("Void.Vampiric.Description"), icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.SubstanceInfusion, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistInfusionDamageTrigger
                {
                    CheckSpellParent = true,
                    TriggerOnDirectDamage = true,
                    Actions = Helper.CreateActionList(onCaster),
                    m_AbilityList = null,
                    SpellDescriptorsList = (SpellDescriptor)2632353982198054912
                },
                new AddKineticistBurnModifier
                {
                    BurnType = KineticistBurnType.Infusion,
                    Value = 3
                },
                new RecalculateOnStatChange
                {
                    Stat = StatType.Unknown,
                    UseKineticistMainStat = true
                },
                new ContextCalculateAbilityParamsBasedOnClass
                {
                    UseKineticistMainStat = true,
                    StatType = StatType.Charisma,
                    m_CharacterClass = Tree.Class
                }
                );

            var healer = AnyRef.ToAny(Helper.GetGuid("VoidHealerFeature"));

            var feature = Helper.CreateBlueprintFeature("VoidVampiricInfusionFeature", LocalizationTool.GetString("Void.Vampiric.Name"),
                LocalizationTool.GetString("Void.Vampiric.Description"), icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 10),
                Helper.CreatePrerequisiteFeature(healer),
                Helper.CreateAddFacts(AnyRef.ToAny(ability))
                );

            Tree.Vampiric.Feature = feature.ToRef();
            Tree.Vampiric.Buff = buff.ToRef();
        }

        private static void CreateWeighingInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("0fd00984a2c0e0a429cf1a911b4ec5ca"); // Entangle
            var entangle_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("23ae079877c3326419d015daebf447db"); // EntanglingInfusionEffectBuff
            var entangle_buff_2 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("5d4db592f0fde214991ed27752cfce16"); // EntanglingInfusionSecondEffectBuff

            var conditions_checker = new ConditionsChecker { Conditions = Helper.CreateContextConditionHasBuff(entangle_buff).ObjToArray(), Operation = Operation.And };

            var conditional = new Conditional
                {
                ConditionsChecker = conditions_checker,
                IfTrue =  Helper.CreateActionList(null),
                IfFalse = Helper.CreateActionList
                    (
                    entangle_buff.CreateContextActionApplyBuff(1, DurationRate.Minutes),
                    entangle_buff_2.CreateContextActionApplyBuff(1, DurationRate.Rounds)
                    )
                };

            var save_action = Helper.CreateContextActionConditionalSaved
                (
                succeed: null,
                failed: conditional
                );

            var saving_throw = new ContextActionSavingThrow
            {
                Type = SavingThrowType.Reflex,
                Actions = Helper.CreateActionList(save_action)
            };

            var ability = Helper.CreateBlueprintActivatableAbility("WeighingInfusionAbility", out var buff, LocalizationTool.GetString("Void.Weighing.Name"),
                LocalizationTool.GetString("Void.Weighing.Description"), icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.SubstanceInfusion, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistInfusionDamageTrigger
                {
                    CheckSpellParent = true,
                    TriggerOnDirectDamage = true,
                    Actions = Helper.CreateActionList(saving_throw),
                    m_AbilityList = null,
                    SpellDescriptorsList = (SpellDescriptor)2632353982198054912
                },
                new AddKineticistBurnModifier
                {
                    BurnType = KineticistBurnType.Infusion,
                    Value = 2
                },
                new RecalculateOnStatChange
                {
                    Stat = StatType.Unknown,
                    UseKineticistMainStat = true
                },
                new ContextCalculateAbilityParamsBasedOnClass
                {
                    UseKineticistMainStat = true,
                    StatType = StatType.Charisma,
                    m_CharacterClass = Tree.Class
                }
                );

            var feature = Helper.CreateBlueprintFeature("WeighingInfusionFeature", LocalizationTool.GetString("Void.Weighing.Name"),
                LocalizationTool.GetString("Void.Weighing.Description"), icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 4),
                Helper.CreateAddFacts(ability.ToRef())
                );

            Tree.Weighing.Feature = feature.ToRef();
            Tree.Weighing.Buff = buff.ToRef();
        }

        private static void CreateSingularityInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("1325e698f4a3f224b880e3b83a551228"); // Supernova
            var feature = Helper.CreateBlueprintFeature("SingularityInfusion", LocalizationTool.GetString("Void.Singularity.Name"),
                LocalizationTool.GetString("Void.Singularity.Description"), icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 8),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusFirst))
                );

            Helper.AppendAndReplace(ref Kineticist.infusion_selection.m_AllFeatures, feature.ToRef());

            Tree.Singularity.Feature = feature.ToRef();
        }

        #endregion

        #region Wild Talents

        private static void CreateVoidWildTalents(BlueprintFeature emptiness)
        {
            AddToSkilledKineticist();
            AddToExpandedDefense(emptiness);

            (var wild_0, var wild_1, var wild_2, var wild_3) = CreateWildTalentBonusFeatVoid();

            BlueprintFeatureReference corpse_puppet = CreateCorpsePuppet();
            BlueprintFeatureReference curse_breaker = CreateCurseBreaker();
            BlueprintFeatureReference gravity_control = CreateGravityControl();
            BlueprintFeatureReference gravity_control_greater = CreateGravityControlGreater(gravity_control);
            BlueprintFeatureReference undead_grip = CreateUndeadGrip();
            BlueprintFeatureReference void_healer = CreateVoidHealer();

            Kineticist.TryDarkCodexAddExtraWildTalent(corpse_puppet, curse_breaker, gravity_control, gravity_control_greater, undead_grip, wild_0.ToRef(), wild_1.ToRef(), wild_2.ToRef(), wild_3.ToRef(), void_healer);
            Kineticist.AddToWildTalents(corpse_puppet, curse_breaker, gravity_control, gravity_control_greater, undead_grip, void_healer);
        }

        private static void AddToSkilledKineticist()
        {
            var buff = Helper.CreateBlueprintBuff("SkilledKineticistVoidBuff", LocalizationTool.GetString("SkilledKineticist"));
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                Helper.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                    ContextRankProgression.Div2, max: 20, classes: new BlueprintCharacterClassReference[1] { Tree.Class }),
                Helper.CreateAddContextStatBonus(new ContextValue { ValueType = ContextValueType.Rank, Value = 0, ValueRank = AbilityRankType.Default, ValueShared = AbilitySharedValue.Damage },
                StatType.SkillKnowledgeWorld)
                );

            var condition = Helper.CreateContextConditionHasFact(AnyRef.ToAny(Tree.FocusVoid.First));
            var conditional = Helper.CreateConditional(condition,
                ifTrue: buff.CreateContextActionApplyBuff(0, DurationRate.Rounds, false, false, false, true, true));

            var factContextAction = Kineticist.ref_skilled_kineticist.Get().GetComponent<AddFactContextActions>();
            Helper.AppendAndReplace(ref factContextAction.Activated.Actions, conditional);
        }
        private static void AddToExpandedDefense(BlueprintFeature emptiness)
        {
            var selection = AnyRef.ToAny(Kineticist.ref_expanded_defense).ToRef<BlueprintFeatureSelectionReference>().Get();
            Helper.AppendAndReplace(ref selection.m_AllFeatures, emptiness.ToRef());
        }

        private static (BlueprintFeatureSelection wild_0, BlueprintFeatureSelection wild_1, BlueprintFeatureSelection wild_2, BlueprintFeatureSelection wild_3) CreateWildTalentBonusFeatVoid()
        {
            var spell_pen = Helper.ToRef<BlueprintFeatureReference>("ee7dc126939e4d9438357fbd5980d459"); // SpellPenetration
            var spell_pen_greater = Helper.ToRef<BlueprintFeatureReference>("1978c3f91cfbbc24b9c9b0d017f4beec"); // GreaterSpellPenetration
            var precise_shot = Helper.ToRef<BlueprintFeatureReference>("8f3d1e6b4be006f4d896081f2f889665"); // PreciseShot
            var trip = Helper.ToRef<BlueprintFeatureReference>("0f15c6f70d8fb2b49aa6cc24239cc5fa"); // ImprovedTrip
            var trip_greater = Helper.ToRef<BlueprintFeatureReference>("4cc71ae82bdd85b40b3cfe6697bb7949"); // SpellPenetration

            var wild_0 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatVoid", LocalizationTool.GetString("Void.BonusWild.Name"),
                LocalizationTool.GetString("Void.BonusWild.Description"), null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_0.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.First), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.Second), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.Third), true),
                Helper.CreatePrerequisiteNoFeature(trip, false),
                Helper.CreatePrerequisiteNoFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.Knight), true)
                );
            wild_0.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_0.m_AllFeatures, spell_pen, precise_shot, trip);

            var wild_1 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatVoid1", LocalizationTool.GetString("Void.BonusWild.Name"),
                LocalizationTool.GetString("Void.BonusWild.Description"), null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_1.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.First), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.Second), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.Third), true),
                Helper.CreatePrerequisiteNoFeature(trip, false),
                Helper.CreatePrerequisiteFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.Knight), true)
                );
            wild_1.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_1.m_AllFeatures, spell_pen_greater, precise_shot, trip);

            var wild_2 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatVoid2", LocalizationTool.GetString("Void.BonusWild.Name"),
                LocalizationTool.GetString("Void.BonusWild.Description"), null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_2.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.First), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.Second), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.Third), true),
                Helper.CreatePrerequisiteFeature(trip, false),
                Helper.CreatePrerequisiteNoFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.Knight), true)
                );
            wild_2.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_2.m_AllFeatures, spell_pen, precise_shot, trip_greater);

            var wild_3 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatVoid3", LocalizationTool.GetString("Void.BonusWild.Name"),
                LocalizationTool.GetString("Void.BonusWild.Description"), null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_3.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.First), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.Second), true),
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.Third), true),
                Helper.CreatePrerequisiteFeature(trip, false),
                Helper.CreatePrerequisiteFeature(spell_pen, false),
                new PrerequisiteSelectionPossible
                {
                    m_ThisFeature = AnyRef.ToAny(wild_3)
                },
                Helper.CreatePrerequisiteFeature(AnyRef.ToAny(Tree.FocusVoid.Knight), true)
                );
            wild_3.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_3.m_AllFeatures, spell_pen_greater, precise_shot, trip_greater);


            Helper.AppendAndReplace(ref Kineticist.wild_talent_selection.m_AllFeatures, wild_0.ToRef(), wild_1.ToRef(), wild_2.ToRef(), wild_3.ToRef());
            return (wild_0, wild_1, wild_2, wild_3);
        }

        private static BlueprintFeatureReference CreateCorpsePuppet()
        {
            var summon_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("50d51854cf6a3434d96a87d050e1d09a"); // SummonedCreatureSpawnMonsterIV-VI
            UnityEngine.Sprite icon = Helper.StealIcon("4b76d32feb089ad4499c3a1ce8e1ac27");

            var summon_buff_apply = summon_buff.CreateContextActionApplyBuff(permanent: true, dispellable: false);

            var context_spawn = new ContextActionSpawnMonster
            {
                m_Blueprint = "53e228ba7fe18104c93dc4b7294a1b30".ToRef<BlueprintUnitReference>(),
                m_SummonPool = "d94c93e7240f10e41ae41db4c83d1cbe".ToRef<BlueprintSummonPoolReference>(),
                CountValue = new ContextDiceValue
                {
                    DiceType = DiceType.D4,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 1
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 2
                    }
                },
                DurationValue = new ContextDurationValue
                {
                    Rate = DurationRate.Rounds,
                    DiceType = DiceType.Zero,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        ValueRank = AbilityRankType.Default
                    }
                },
                AfterSpawn = new ActionList { Actions = new GameAction[] { summon_buff_apply } }
            };

            var ability = Helper.CreateBlueprintAbility("CorpsePuppetAbility", LocalizationTool.GetString("Void.CorpsePuppet.Name"),
                LocalizationTool.GetString("Void.CorpsePuppet.Description"), icon, AbilityType.Special, UnitCommand.CommandType.Standard,
                AbilityRange.Close, null, null).TargetPoint(CastAnimationStyle.Kineticist);
            ability.CanTargetSelf = true;
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(actions: context_spawn),
                Helper.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, max: 20, classes: new BlueprintCharacterClassReference[] { Tree.Class }),
                Kineticist.Blast.SpellDescriptor((SpellDescriptor)2048),
                Kineticist.Blast.BurnCost(null, infusion: 0, blast: 0, talent: 1)
                );

            var feature = Helper.CreateBlueprintFeature("CorpsePuppetFeature", LocalizationTool.GetString("Void.CorpsePuppet.Name"),
                LocalizationTool.GetString("Void.CorpsePuppet.Description"), icon, FeatureGroup.KineticWildTalent);
            feature.IsClassFeature = true;
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true, 
                    AnyRef.ToAny(Tree.FocusVoid.First),
                    AnyRef.ToAny(Tree.FocusVoid.Second),
                    AnyRef.ToAny(Tree.FocusVoid.Third),
                    AnyRef.ToAny(Tree.FocusVoid.Knight)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 8),
                Helper.CreateAddFacts(ability.ToRef())
                );

            return feature.ToRef();
        }

        private static BlueprintFeatureReference CreateCurseBreaker()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("b48674cef2bff5e478a007cf57d8345b"); // RemoveCurse

            var buff = Helper.CreateBlueprintBuff("CurseBreakerBuff", LocalizationTool.GetString("Void.CurseBreaker.Name"),
                LocalizationTool.GetString("Void.CurseBreaker.Description"), icon, null);
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new SavingThrowBonusAgainstDescriptor
                {
                    SpellDescriptor = SpellDescriptor.Curse,
                    ModifierDescriptor = ModifierDescriptor.UntypedStackable,
                    Value = 4
                },
                new SavingThrowBonusAgainstDescriptor
                {
                    SpellDescriptor = SpellDescriptor.Hex,
                    ModifierDescriptor = ModifierDescriptor.UntypedStackable,
                    Value = 4
                }
                );

            var context_dispel = new ContextActionDispelMagic
            {
                m_StopAfterCountRemoved = false,
                m_CountToRemove = new ContextValue
                {
                    ValueType = ContextValueType.Simple,
                    Value = 1
                },
                m_BuffType = ContextActionDispelMagic.BuffType.All,
                m_MaxSpellLevel = new ContextValue
                {
                    ValueType = ContextValueType.Simple,
                    Value = 0
                },
                m_UseMaxCasterLevel = false,
                m_CheckType = RuleDispelMagic.CheckType.DC,
                m_Skill = StatType.Unknown,
                CheckBonus = 0,
                Descriptor = (SpellDescriptor)268435456
            };

            var ability = Helper.CreateBlueprintAbility("CurseBreakerAbility", LocalizationTool.GetString("Void.CurseBreaker.Name"),
                LocalizationTool.GetString("Void.CurseBreaker.Description"), icon, AbilityType.Special, UnitCommand.CommandType.Standard,
                AbilityRange.Touch, null, null).TargetAlly(CastAnimationStyle.Kineticist);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(actions: context_dispel),
                new AbilitySpawnFx
                {
                    PrefabLink = new PrefabLink { AssetId = "930c1a4aa129b8344a40c8c401d99a04" },
                    Time = AbilitySpawnFxTime.OnApplyEffect,
                    Anchor = AbilitySpawnFxAnchor.SelectedTarget,
                    OrientationMode = AbilitySpawnFxOrientation.Copy
                },
                Kineticist.Blast.BurnCost(null, infusion: 0, blast: 0, talent: 1)
                );

            var feature = Helper.CreateBlueprintFeature("CurseBreakerFeature", LocalizationTool.GetString("Void.CurseBreaker.Name"),
                LocalizationTool.GetString("Void.CurseBreaker.Description"), icon, FeatureGroup.KineticWildTalent);
            feature.IsClassFeature = true;
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.FocusVoid.First),
                    AnyRef.ToAny(Tree.FocusVoid.Second),
                    AnyRef.ToAny(Tree.FocusVoid.Third),
                    AnyRef.ToAny(Tree.FocusVoid.Knight)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 8),
                Helper.CreateAddFacts(ability.ToRef()),
                Helper.CreateAddFactContextActions(new GameAction[] { buff.CreateContextActionApplyBuff(permanent: true) })
                );

            return feature.ToRef();
        }

        private static BlueprintFeatureReference CreateUndeadGrip()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("41e8a952da7a5c247b3ec1c2dbb73018");
            var undead_type = Helper.ToRef<BlueprintUnitFactReference>("734a29b693e9ec346ba2951b27987e33"); // UndeadType
            var hold_monstor = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("2cfcce5b62d3e6d4082ec31b58468cc8"); // HoldMonsterBuff

            var duration = new ContextDurationValue
            {
                Rate = DurationRate.Rounds,
                DiceType = DiceType.Zero,
                DiceCountValue = new ContextValue
                {
                    ValueType = ContextValueType.Simple,
                    Value = 0
                },
                BonusValue = new ContextValue
                {
                    ValueType = ContextValueType.Rank,
                    ValueRank = AbilityRankType.Default
                }
            };

            var conditional = Helper.CreateContextActionConditionalSaved(failed: hold_monstor.CreateContextActionApplyBuff(duration));

            var ability = Helper.CreateBlueprintAbility("UndeadGripAbility", LocalizationTool.GetString("Void.UndeadGrip.Name"),
                LocalizationTool.GetString("Void.UndeadGrip.Description"), icon, AbilityType.Special, UnitCommand.CommandType.Standard,
                AbilityRange.Medium, null, null).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.SetComponents
                (
                new SpellComponent { School = SpellSchool.Enchantment },
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Will, conditional),
                Helper.CreateAbilityTargetHasFact(false, undead_type),
                Kineticist.Blast.BurnCost(null, infusion: 0, blast: 0, talent: 1)
                );

            var feature = Helper.CreateBlueprintFeature("UndeadGripFeature", LocalizationTool.GetString("Void.UndeadGrip.Name"),
                LocalizationTool.GetString("Void.UndeadGrip.Description"), icon, FeatureGroup.KineticWildTalent);
            feature.IsClassFeature = true;
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.FocusVoid.First),
                    AnyRef.ToAny(Tree.FocusVoid.Second),
                    AnyRef.ToAny(Tree.FocusVoid.Third),
                    AnyRef.ToAny(Tree.FocusVoid.Knight)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 6),
                Helper.CreateAddFacts(ability.ToRef())
                );


            return feature.ToRef();
        }

        private static BlueprintFeatureReference CreateVoidHealer()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite(Main.ModPath+"/Icons/voidHealer.png");
            var negativeAffinity = Helper.ToRef<BlueprintUnitFactReference>("d5ee498e19722854198439629c1841a5"); // NegativeEnergyAffinity

            var heal = new ContextActionHealTarget
            {
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.Zero,
                    DiceCountValue = new ContextValue { Value = 0, ValueType = ContextValueType.Simple },
                    BonusValue = new ContextValue { ValueType = ContextValueType.Shared, ValueShared = AbilitySharedValue.Duration }
                }
            };

            var fx = new ContextActionSpawnFx
            {
                PrefabLink = new PrefabLink { AssetId = "e9399b6d57369ab4a9c3d88798d92f33" }
            };

            var calc_shared_duration = Kineticist.Blast.CalculateSharedValue(type: AbilitySharedValue.Duration, dice: DiceType.D6);
            calc_shared_duration.Value.BonusValue.ValueType = ContextValueType.Shared;
            calc_shared_duration.Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            var ability = Helper.CreateBlueprintAbility("VoidHealerAbility", LocalizationTool.GetString("Void.VoidHealer.Name"),
                LocalizationTool.GetString("Void.VoidHealer.Description"), icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard,
                AbilityRange.Touch, null, null).TargetAlly(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, heal, fx),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.CalculateSharedValue(),
                calc_shared_duration,
                Kineticist.Blast.BurnCost(null, infusion: 0, blast: 0, talent: 1),
                Helper.CreateAbilityTargetHasFact(false, negativeAffinity),
                new SpellComponent { School = SpellSchool.Universalist }
                );

            var feature = Helper.CreateBlueprintFeature("VoidHealerFeature", LocalizationTool.GetString("Void.VoidHealer.Name"),
                LocalizationTool.GetString("Void.VoidHealer.Description"), icon, FeatureGroup.KineticWildTalent);
            feature.IsClassFeature = true;
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.FocusVoid.First),
                    AnyRef.ToAny(Tree.FocusVoid.Second),
                    AnyRef.ToAny(Tree.FocusVoid.Third),
                    AnyRef.ToAny(Tree.FocusVoid.Knight)),
                Helper.CreateAddFacts(ability.ToRef()),
                Helper.CreatePrerequisiteNoFeature(feature.ToRef())
                );

            VoidHealerAbility = ability;
            VoidHealer = feature;

            return feature.ToRef();
        }

        #region Gravity Control

        private static BlueprintFeatureReference CreateGravityControl()
        {
            var icon = Helper.StealIcon("e4979934-bdb3-9d84-2b28-bee614606823"); // Buff Wings Mutagen

            var ac_bonus = new ACBonusAgainstAttacks
            {
                AgainstMeleeOnly = true,
                AgainstRangedOnly = false,
                OnlySneakAttack = false,
                NotTouch = false,
                IsTouch = false,
                OnlyAttacksOfOpportunity = false,
                Value = Helper.CreateContextValue(0),
                ArmorClassBonus = 3,
                Descriptor = ModifierDescriptor.Dodge,
                CheckArmorCategory = false,
                NotArmorCategory = null,
                NoShield = false
            };
            var no_difficultTerrain = new AddConditionImmunity
            {
                Condition = UnitCondition.DifficultTerrain
            };

            var buff = Helper.CreateBlueprintBuff("GravityControlBuff", LocalizationTool.GetString("Void.GravityControl.Name"),
                LocalizationTool.GetString("Void.GravityControl.Description"), icon);
            buff.Flags(null, true, null, null);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                ac_bonus,
                no_difficultTerrain
                );

            var ability = Helper.CreateBlueprintAbility("GravityControlAbility", LocalizationTool.GetString("Void.GravityControl.Name"),
                LocalizationTool.GetString("Void.GravityControl.Description"), icon, AbilityType.SpellLike, UnitCommand.CommandType.Move, AbilityRange.Personal);
            ability.TargetSelf(CastAnimationStyle.Kineticist);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, buff.CreateContextActionApplyBuff(1, DurationRate.Rounds, false, false, true, false, false))
                );

            var feature = Helper.CreateBlueprintFeature("GravityControlFeature", LocalizationTool.GetString("Void.GravityControl.Name"),
                LocalizationTool.GetString("Void.GravityControl.Description"), icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.FocusVoid.First),
                    AnyRef.ToAny(Tree.FocusVoid.Second),
                    AnyRef.ToAny(Tree.FocusVoid.Third),
                    AnyRef.ToAny(Tree.FocusVoid.Knight)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 6),
                Helper.CreateAddFacts(ability.ToRef())
                );

            return feature.ToRef();
        }

        private static BlueprintFeatureReference CreateGravityControlGreater(BlueprintFeature gravity_control)
        {
            var icon = Helper.StealIcon("e4979934-bdb3-9d84-2b28-bee614606823"); // Buff Wings Mutagen

            var ac_bonus = new ACBonusAgainstAttacks
            {
                AgainstMeleeOnly = true,
                AgainstRangedOnly = false,
                OnlySneakAttack = false,
                NotTouch = false,
                IsTouch = false,
                OnlyAttacksOfOpportunity = false,
                Value = Helper.CreateContextValue(0),
                ArmorClassBonus = 3,
                Descriptor = ModifierDescriptor.Dodge,
                CheckArmorCategory = false,
                NotArmorCategory = null,
                NoShield = false
            };
            var no_difficultTerrain = new AddConditionImmunity
            {
                Condition = UnitCondition.DifficultTerrain
            };

            var ability = Helper.CreateBlueprintActivatableAbility("GravityControlGreaterAbility", out var buff, LocalizationTool.GetString("Void.GravityControl.Greater.Name"),
                LocalizationTool.GetString("Void.GravityControl.Greater.Description"), icon, UnitCommand.CommandType.Move, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.None, false, false, false, false, false, false, true, false, false, 1);

            buff.Flags(false, false, null, null);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                ac_bonus,
                no_difficultTerrain
                );

            var remove_lesser = new RemoveFeatureOnApply
            {
                m_Feature = gravity_control.ToRef2()
            };
            var feature = Helper.CreateBlueprintFeature("GravityControlGreaterFeature", LocalizationTool.GetString("Void.GravityControl.Greater.Name"),
                LocalizationTool.GetString("Void.GravityControl.Greater.Description"), icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeature(gravity_control.ToRef()),
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.ToAny(Tree.FocusVoid.First),
                    AnyRef.ToAny(Tree.FocusVoid.Second),
                    AnyRef.ToAny(Tree.FocusVoid.Third),
                    AnyRef.ToAny(Tree.FocusVoid.Knight)),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 10),
                remove_lesser,
                Helper.CreateAddFacts(ability.ToRef())
                );

            gravity_control.AddComponents(Helper.CreatePrerequisiteNoFeature(feature.ToRef()));

            return feature.ToRef();
        }

        #endregion

        #endregion

        #region Area Effects

        private static ContextActionSpawnAreaEffect CreateSingularityEffect(string name, PhysicalDamageForm p = 0, DamageEnergyType e = (DamageEnergyType)255)
        {
            var prefab = new PrefabLink { AssetId = "3353f84806fcf7247afd2c3cad7fee16" }; // SupernovaAreaEffect PrefabLink

            bool isComposite = p != 0 && e != (DamageEnergyType)255;

            #region Fixed Values

            BlueprintAbilityAreaEffectReference last_area = null;
            var context_dice = Kineticist.Blast.RankConfigDice(twice: false, half: false);
            if (!isComposite)
            {
                context_dice.m_Progression = ContextRankProgression.DivStep;
                if (p != 0) context_dice.m_StepLevel = 4;
                if (e != (DamageEnergyType)255) context_dice.m_StepLevel = 2;
            }
            var context_bonus = Kineticist.Blast.RankConfigBonus(half_bonus: !isComposite && e != (DamageEnergyType)255);
            var context_duration = Helper.CreateContextDurationValue(
                new ContextValue { ValueType = ContextValueType.Simple, Value = 0 }, DiceType.Zero,
                new ContextValue { ValueType = ContextValueType.Simple, Value = 2, }, DurationRate.Rounds);
            var calc_shared = Kineticist.Blast.CalculateSharedValue();
            if (isComposite)
            {
                calc_shared.Value.DiceCountValue.ValueRank = AbilityRankType.DamageBonus;
                calc_shared.Value.BonusValue.ValueRank = AbilityRankType.DamageDice;
            }
            var calc_ability_params = Kineticist.Blast.DCForceDex();
            calc_ability_params.UseKineticistMainStat = true;

            #endregion

            #region Damage Setup

            ContextValue damageDice = Helper.CreateContextValue(AbilityRankType.DamageDice);
            ContextValue damageBonus = Helper.CreateContextValue(AbilityRankType.DamageBonus);
            if (isComposite || p != 0)
            {
                damageBonus = Helper.CreateContextValue(AbilitySharedValue.Damage);
            }

            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, damageDice, damageBonus);

            List<ContextAction> list = new(2);

            if (p != 0)
                list.Add(Helper.CreateContextActionDealDamage(p,
                dice, isAoE: true, halfIfSaved: true, false, isComposite, false, AbilitySharedValue.Damage));
            if (e != (DamageEnergyType)255)
                list.Add(Helper.CreateContextActionDealDamage(e,
                dice, isAoE: true, halfIfSaved: true, false, isComposite, false, AbilitySharedValue.Damage));

            #endregion  

            var damage_list = Helper.CreateActionList(list.ToArray());

            ContextActionSavingThrow savingThrow = new ContextActionSavingThrow
            {
                Type = SavingThrowType.Reflex,
                Actions = damage_list
            };

            foreach (var radius in new int[3] { 15, 11, 5 } )
            {
                AbilityUniqueSingularityRunAction action = new AbilityUniqueSingularityRunAction
                {
                    spawn = new ActionList(),
                    damage = Helper.CreateActionList(savingThrow),
                    UnitExit = new ActionList(),
                    UnitEnter = new ActionList(),
                    UnitMove = new ActionList(),
                    Round = new ActionList()
                };
                if ( last_area != null )
                {
                    ContextActionSpawnAreaEffect area = new ContextActionSpawnAreaEffect
                    {
                        DurationValue = context_duration,
                        m_AreaEffect = last_area,
                        OnUnit = false
                    };
                    action.spawn.InsertAt(area);
                }

                var area_effect = Helper.CreateBlueprintAbilityAreaEffect("SingularityInfusionEffectArea" + name + radius/5, true, true,
                    AreaEffectShape.Cylinder, new Feet { m_Value = radius },
                    prefab);
                area_effect.m_Tags = AreaEffectTags.DestroyableInCutscene;
                area_effect.IgnoreSleepingUnits = false;
                area_effect.AffectDead = false;
                area_effect.AggroEnemies = true;
                area_effect.AffectEnemies = true;
                area_effect.SpellResistance = false;
                area_effect.m_TickRoundAfterSpawn = false;

                area_effect.AddComponents(context_dice, context_bonus, calc_shared, calc_ability_params);
                area_effect.AddComponents(action);

                last_area = AnyRef.ToAny(area_effect);
            }

            return new ContextActionSpawnAreaEffect
            {
                DurationValue = context_duration,
                m_AreaEffect = last_area,
                OnUnit = false
            };
        }

        #endregion
    }
}
