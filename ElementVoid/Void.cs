using KineticistElementsExpanded.Components;
using KineticistElementsExpanded.KineticLib;
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
using System.Collections.Generic;
using System.Linq;
using static Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell;

namespace KineticistElementsExpanded.ElementVoid
{
    class Void : Statics
    {

        // Spell book for blasts, to "brew" them like metamagic with infusions

        public static KineticistTree Tree = new();

        public static KineticistTree.Element Gravity = new();
        public static KineticistTree.Element Negative = new();
        public static KineticistTree.Element VoidBlast = new();
        // Either Group of blasts, or a unique component
        public static KineticistTree.Element NegativeFire = new();
        public static KineticistTree.Element NegativeCold = new();
        public static KineticistTree.Element NegativeElectric = new();

        public static KineticistTree.Focus VoidFocus = new();

        private static BlueprintFeature DampeningInfusion = null;
        private static BlueprintBuff DampeningInfusionBuff = null;

        private static BlueprintFeature EnervatingInfusion = null;
        private static BlueprintBuff EnervatingInfusionBuff = null;

        private static BlueprintFeature PullingInfusion = null;
        private static BlueprintBuff PullingInfusionBuff = null;

        private static BlueprintFeature UnnervingInfusion = null;
        private static BlueprintBuff UnnervingInfusionBuff = null;

        private static BlueprintFeature VampiricInfusion = null;
        private static BlueprintBuff VampiricInfusionBuff = null;

        private static BlueprintFeature WeighingInfusion = null;
        private static BlueprintBuff WeighingInfusionBuff = null;

        private static BlueprintFeature GraviticBoost = null;
        private static BlueprintBuff GraviticBoostBuff = null;

        private static BlueprintFeature NegativeAdmixture = null;
        private static BlueprintBuff NegativeAdmixtureBuff = null;

        private static BlueprintFeature SingularityInfusion = null;

        private static BlueprintAbility VoidHealerAbility = null;
        private static BlueprintFeature VoidHealer = null;

        public static void Configure()
        {

            var void_class_skills = CreateVoidClassSkills();

            CreateSingularityInfusion();

            CreateVoidBlastsSelection();
            CreateVoidComposites();
            CreateVoidInfusions();

            Kineticist.InfusionFeatureAddPrerequisites(SingularityInfusion, Gravity, Negative, VoidBlast);
            Kineticist.TryDarkCodexAddExtraWildTalent(SingularityInfusion.ToRef());

            var emptiness_feature = CreateEmptiness();

            Kineticist.AddBlastAbilityToBurn(Gravity.BaseAbility);
            Kineticist.AddBlastAbilityToMetakinesis(Gravity.BaseAbility);
            Kineticist.AddElementalDefenseIsPrereqFor(Gravity.BlastFeature, Gravity.BladeFeature, emptiness_feature);
            Kineticist.AddToKineticBladeInfusion(Gravity.BladeFeature, Gravity.BlastFeature);

            Kineticist.AddBlastAbilityToBurn(Negative.BaseAbility);
            Kineticist.AddBlastAbilityToMetakinesis(Negative.BaseAbility);
            Kineticist.AddElementalDefenseIsPrereqFor(Negative.BlastFeature, Negative.BladeFeature, emptiness_feature);
            Kineticist.AddToKineticBladeInfusion(Negative.BladeFeature, Negative.BlastFeature);

            Kineticist.AddBlastAbilityToBurn(VoidBlast.BaseAbility);
            Kineticist.AddBlastAbilityToMetakinesis(VoidBlast.BaseAbility);
            Kineticist.AddElementalDefenseIsPrereqFor(Negative.BlastFeature, VoidBlast.BladeFeature, emptiness_feature);
            Kineticist.AddToKineticBladeInfusion(VoidBlast.BladeFeature, VoidBlast.BlastFeature);

            CreateVoidElementalFocus(void_class_skills, emptiness_feature);
            CreateKineticKnightVoidFocus(void_class_skills, emptiness_feature);
            CreateSecondElementVoid();
            CreateThirdElementVoid();

            CreateVoidWildTalents(emptiness_feature);
        }

        #region Class Features and Misc.

        private static BlueprintFeatureBase CreateVoidClassSkills()
        {
            var feature = Helper.CreateBlueprintFeature("WoodClassSkills", "Wood Class Skills",
                VoidClassSkillsDescription, null, null, 0)
                .SetComponents(
                Helper.CreateAddClassSkill(StatType.SkillKnowledgeWorld)
                );

            return feature;
        }

        private static void CreateVoidBlastsSelection()
        {
            // Create Both Progressions
            CreateGravityBlast();
            CreateNegativeBlast();

            var selection = Helper.CreateBlueprintFeatureSelection("GravityBlastSelection", "Gravity Blast",
                GravityBlastDescription, null, null, FeatureGroup.None, SelectionMode.Default);
            selection.IsClassFeature = true;

            Helper.AppendAndReplace<BlueprintFeatureReference>(ref selection.m_AllFeatures, 
                AnyRef.Get(Gravity.Progession).To<BlueprintFeatureReference>(), 
                AnyRef.Get(Negative.Progession).To<BlueprintFeatureReference>());

            Gravity.Selection = selection.ToRef3();
            Negative.Selection = Gravity.Selection;
        }

        #endregion

        #region Elemental Focus Selection

        // Blast Selections, Class Skills, Elemental Defense
        private static void CreateVoidElementalFocus(BlueprintFeatureBase class_skills, BlueprintFeatureBase emptiness)
        {
            var progression = Helper.CreateBlueprintProgression("ElementalFocusVoid", "Void",
                ElementalFocusVoidDescription, null, null,
                FeatureGroup.KineticElementalFocus)
                .SetComponents(Helper.CreatePrerequisiteNoArchetype(Tree.BloodKineticist, Tree.Class));

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            // Can be any void basic: Gravity or Negative
            var entry1 = Helper.CreateLevelEntry(1, Gravity.Selection, class_skills);
            var entry2 = Helper.CreateLevelEntry(2, emptiness);
            Helper.AddEntries(progression, entry1, entry2);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusFirst.GetBlueprint()).m_AllFeatures, progression.ToRef());

            VoidFocus.First = progression.ToRef3();
        }

        // Blast Progression?Selection, Class Skills, Elemental Defense
        private static void CreateKineticKnightVoidFocus(BlueprintFeatureBase class_skills, BlueprintFeatureBase emptiness)
        {
            var progression = Helper.CreateBlueprintProgression("KineticKnightElementalFocusVoid", "Void",
                ElementalFocusVoidDescription, null, null,
                FeatureGroup.KineticElementalFocus)
                .SetComponents(new AddEquipmentEntity { EquipmentEntity = new EquipmentEntityLink { AssetId = "aecc5905323948449b4cd3bfe36e5daf" } });

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            // Can be any void basic: Gravity or Negative
            var entry1 = Helper.CreateLevelEntry(1, Gravity.Selection, class_skills);
            var entry2 = Helper.CreateLevelEntry(4, emptiness);
            Helper.AddEntries(progression, entry1, entry2);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusKnight.GetBlueprint()).m_AllFeatures, progression.ToRef());

            VoidFocus.Knight = progression.ToRef3();
        }


        private static void CreateSecondElementVoid()
        {
            var progression = Helper.CreateBlueprintProgression("SecondaryElementVoid", "Void",
                ElementalFocusVoidDescription, null, null,
                FeatureGroup.KineticElementalFocus);
            progression.HideInCharacterSheetAndLevelUp = true;
            
            progression.SetComponents
                (
                Helper.CreateActivateTrigger
                    (
                    Helper.CreateConditionsChecker(Operation.Or, 
                        Helper.CreateHasFact(new FactOwner(), AnyRef.Get(VoidFocus.First).To<BlueprintUnitFactReference>()),
                        Helper.CreateHasFact(new FactOwner(), AnyRef.Get(VoidFocus.Knight).To<BlueprintUnitFactReference>())),
                    Helper.CreateActionList
                        (
                        Helper.CreateAddFact(new FactOwner(), 
                            AnyRef.Get(Gravity.BlastFeature).To<BlueprintUnitFactReference>()),
                        Helper.CreateAddFact(new FactOwner(), 
                            AnyRef.Get(Negative.BlastFeature).To<BlueprintUnitFactReference>()),
                        Helper.CreateAddFact(new FactOwner(), 
                            AnyRef.Get(VoidBlast.BlastFeature).To<BlueprintUnitFactReference>())
                        )
                    ),
                Helper.CreateAddFacts(AnyRef.Get(Tree.CompositeBuff).To<BlueprintUnitFactReference>())
                );

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();
            
            // Can be any void basic: Gravity or Negative
            var entry1 = Helper.CreateLevelEntry(7, Gravity.Selection);
            Helper.AddEntries(progression, entry1);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusSecond.GetBlueprint()).m_AllFeatures, progression.ToRef());

            VoidFocus.Second = progression.ToRef3();
        }

        // Blast Progression?Selection, Class Skills, Elemental Defense, Blast Features, Second Focus
        // Bit more complicated with two elements
        private static void CreateThirdElementVoid()
        {
            var progression = Helper.CreateBlueprintProgression("ThirdElementVoid", "Void",
                ElementalFocusVoidDescription, null, null,
                FeatureGroup.KineticElementalFocus);
            progression.HideInCharacterSheetAndLevelUp = true;
            
            progression.SetComponents
                (
                Helper.CreateActivateTrigger
                    (
                    Helper.CreateConditionsChecker(Operation.Or,
                        Helper.CreateHasFact(new FactOwner(), AnyRef.Get(VoidFocus.First).To<BlueprintUnitFactReference>()),
                        Helper.CreateHasFact(new FactOwner(), AnyRef.Get(VoidFocus.Knight).To<BlueprintUnitFactReference>())),
                    Helper.CreateActionList
                        (
                        Helper.CreateAddFact(new FactOwner(),
                            AnyRef.Get(VoidBlast.BlastFeature).To<BlueprintUnitFactReference>())
                        )
                    ),
                Helper.CreateAddFacts(AnyRef.Get(Tree.CompositeBuff).To<BlueprintUnitFactReference>()),
                Helper.CreatePrerequisiteNoFeature(AnyRef.Get(VoidFocus.Second).To<BlueprintFeatureReference>())
                );

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            // Can be any void basic: Gravity or Negative
            var entry1 = Helper.CreateLevelEntry(15, Gravity.Selection);
            Helper.AddEntries(progression, entry1);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusThird.GetBlueprint()).m_AllFeatures, progression.ToRef());

            VoidFocus.Third = progression.ToRef3();
        }

        #endregion

        #region Emptiness

        public static BlueprintFeature CreateEmptiness()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("emptiness.png");

            #region Resource

            var emptiness_resource = Helper.CreateBlueprintAbilityResource("EmptinessResource", null,
                null, null, true, 20, 0, 20, 0, 0, 0, 0, false, 0, false, 0);
            
            #endregion
            #region Effect Feature

            var emptiness_effect_feature = Helper.CreateBlueprintFeature("EmptinessEffectFeature", null,
                null, null, icon, FeatureGroup.None);
            emptiness_effect_feature.Ranks = 20;
            emptiness_effect_feature.HideInUI = true;
            emptiness_effect_feature.HideInCharacterSheetAndLevelUp = true;
            emptiness_effect_feature.IsClassFeature = true;
            emptiness_effect_feature.SetComponents
                (
                Helper.CreateAddFacts()
                );

            #endregion
            #region Effect Buff

            var emptiness_effect_buff = Helper.CreateBlueprintBuff("EmptinessEffectBuff", null,
                null, null, icon);
            emptiness_effect_buff.Flags(hidden: true, stayOnDeath: true, removeOnRest: true);
            emptiness_effect_buff.Stacking = StackingType.Stack;
            emptiness_effect_buff.IsClassFeature = true;
            emptiness_effect_buff.SetComponents
                (
                Helper.CreateAddFacts(emptiness_effect_feature.ToRef2())
                );

            #endregion
            #region Buff

            var emptiness_buff = Helper.CreateBlueprintBuff("EmptinessBuff", null,
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

            var emptiness_ability = Helper.CreateBlueprintAbility("EmptinessAbility", "Emptiness",
                EmptinessDescription, null, icon, AbilityType.Special, UnitCommand.CommandType.Free,
                AbilityRange.Personal).TargetSelf(CastAnimationStyle.Omni);
            emptiness_ability.AvailableMetamagic = Metamagic.Heighten;
            emptiness_ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(actions: emptiness_effect_buff.CreateContextActionApplyBuff(permanent: true)),
                Helper.CreateAbilityResourceLogic(emptiness_resource.ToRef(), true, false, 1),
                Helper.CreateAbilityAcceptBurnOnCast(1)
                );

            #endregion

            var emptiness_feature = Helper.CreateBlueprintFeature("Emptiness", "Emptiness",
                EmptinessDescription, null, icon, FeatureGroup.None);
            emptiness_feature.IsClassFeature = true;
            emptiness_feature.SetComponents
                (
                Helper.CreateAddFacts(emptiness_buff.ToRef2(), emptiness_ability.ToRef2()),
                // Prereqs Gravity/Negative Feature, Respective Blade features
                Helper.CreatePrerequisiteFeature(Gravity.BlastFeature, any: true),
                Helper.CreatePrerequisiteFeature(Gravity.BladeFeature, any: true),
                Helper.CreatePrerequisiteFeature(Negative.BlastFeature, any: true),
                Helper.CreatePrerequisiteFeature(Negative.BladeFeature, any: true),
                Helper.CreateAddAbilityResources(false, 0, true, false, emptiness_resource.ToRef())
                );

            return emptiness_feature;
        }

        #endregion

        #region Gravity Blast

        public static void CreateGravityBlast()
        {
            // Variants
            var variant_base = CreateGravityBlastVariant_base();
            var variant_extended = CreateGravityBlastVariant_extended();
            var variant_spindle = CreateGravityBlastVariant_spindle();
            var variant_wall = CreateGravityBlastVariant_wall();
            var variant_blade = CreateGravityBlastVariant_blade();
            var variant_singularity = CreateGravityBlastVariant_singularity();
            // Ability
            CreateGravityBlastAbility(variant_base, variant_extended, variant_spindle, variant_wall, variant_blade, variant_singularity);
            // Feature
            CreateGravityBlastFeature();
            // Progression
            CreateGravityBlastProgression();
            // Helpers

            // Add to Dark Codex
        }

        #region Gravity Variants

        private static BlueprintAbility CreateGravityBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("gravityBlast.png");

            var ability = Helper.CreateBlueprintAbility("GravityBlastAbility", "Gravity Blast",
                GravityBlastDescription, null, icon, AbilityType.Special,
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
                Kineticist.ref_infusion_extendedRange.Get().m_DisplayName,
                Kineticist.ref_infusion_extendedRange.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 0),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_extendedRange),
                Kineticist.Blast.Projectile(Resource.Projectile.NegativeCommonProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Gravity.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }

        private static BlueprintAbility CreateGravityBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleGravityBlastAbility",
                Kineticist.ref_infusion_spindle.Get().m_DisplayName,
                Kineticist.ref_infusion_spindle.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 0),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_spindle),
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
            ability.m_Parent = Gravity.BaseAbility;

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
                m_AreaEffect = CreateGravityWallEffect().ToRef(),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallGravityBlastAbility",
                Kineticist.ref_infusion_wall.Get().m_DisplayName,
                Kineticist.ref_infusion_wall.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 0),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_wall),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Gravity.BaseAbility;

            return ability;
        }

        private static BlueprintAbility CreateGravityBlastVariant_singularity()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("1325e698f4a3f224b880e3b83a551228"); // Supernova

            var ability = Helper.CreateBlueprintAbility("SingularityGravityBlastAbility",
                "Singularity Infusion",
                SingularityInfusionDescription, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, CreateSingularityEffect("Gravity", p: PhysicalDamageForm.Bludgeoning)),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 0),
                Kineticist.Blast.RequiredFeat(SingularityInfusion),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Gravity.BaseAbility;

            return ability;
        }

        #region Kinetic Blade: Gravity

        private static BlueprintAbility CreateGravityBlastVariant_blade()
        {
            var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
            var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
            var icon = Helper.StealIcon("89acea313b9a9cb4d86bbbca01b90346"); // KineticBladeEarthBlastAbility
            var damage_icon = Helper.StealIcon("4fc5cf33da20b5444ad3a96c77af8d20"); // EarthBlastKineticBladeDamage

            var weapon = CreateGravityBlastBlade_weapon();

            #region buffs
            var buff = Helper.CreateBlueprintBuff("KineticBladeGravityBlastBuff", null, null, null, null, null);
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistBlade { m_Blade = weapon.ToRef() }
                );
            #endregion

            #region BlastAbility

            var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBladeGravityBlastAbility", "Gravity Blast — Kinetic Blade",
                KineticBladeDescription, out var unused, null, icon,
                group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
            blade_active_ability.m_Buff = buff.ToRef();
            blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
            blade_active_ability.SetComponents
                (
                new RestrictionCanUseKineticBlade { }
                );

            #endregion

            #region BlastBurnAbility

            var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBladeGravityBlastBurnAbility", null, null, null, icon,
                AbilityType.Special, UnitCommand.CommandType.Free, AbilityRange.Personal);
            blade_burn_ability.TargetSelf(CastAnimationStyle.Omni);
            blade_burn_ability.Hidden = true;
            blade_burn_ability.DisableLog = true;
            blade_burn_ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten;
            blade_burn_ability.SetComponents
                (
                new AbilityKineticist { Amount = 1, InfusionBurnCost = 1 },
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, kinetic_blade_enable_buff.CreateContextActionApplyBuff(asChild: true)),
                new AbilityKineticBlade { }
                );

            #endregion

            #region BlastKineticBladeDamage

            var blade_damage_ability = Helper.CreateBlueprintAbility("GravityBlastKineticBladeDamage", "Telekinetic Blast",
                GravityBlastDescription, null, damage_icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            blade_damage_ability.TargetEnemy(CastAnimationStyle.Omni);
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            blade_damage_ability.Hidden = true;
            blade_damage_ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(kinetic_blade_hide_feature.ToRef2()),
                new AbilityDeliveredByWeapon { },
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning),
                Kineticist.Blast.RankConfigDice(false, false),
                Kineticist.Blast.RankConfigBonus(false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1),
                Kineticist.Blast.Projectile(Resource.Projectile.Kinetic_EarthBlast00_Projectile, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                );
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            #endregion

            weapon.SetComponents
                (
                new WeaponKineticBlade { m_ActivationAbility = blade_burn_ability.ToRef(), m_Blast = blade_damage_ability.ToRef() }
                );

            var blade_feat = Helper.CreateBlueprintFeature("GravityKineticBladeFeature", null, null, null, icon, FeatureGroup.None);
            blade_feat.HideInUI = true;
            blade_feat.HideInCharacterSheetAndLevelUp = true;
            blade_feat.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                );

            Gravity.BladeFeature = blade_feat.ToRef();
            Gravity.BladeDamageAbility = blade_damage_ability.ToRef();
            return blade_damage_ability;
        }

        private static BlueprintItemWeapon CreateGravityBlastBlade_weapon()
        {
            //var icon = Helper.StealIcon("43ff6714-3efb-86d4-f894-b10577329050"); // Air Kinetic Blade Weapon

            var weapon = Helper.CreateBlueprintItemWeapon("GravityKineticBladeWeapon", null, null, Kineticist.ref_kinetic_blast_physical_blade_type,
                damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero },
                form: null,
                secondWeapon: null, false, null, 10);
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreateGravityBlastBlade_enchantment().ToRef() };

            weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
            weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
            weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
            weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
            weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

            // Components are later
            return weapon;
        }

        private static BlueprintWeaponEnchantment CreateGravityBlastBlade_enchantment()
        {
            var first_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = 0,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };
            var first_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, feature: Tree.BlastFeature, min: 0, max: 20);
            var second_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, customProperty: Tree.MainStatProp, min: 0, max: 20);
            var second_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.DamageBonus,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Shared,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };

            var enchant = Helper.CreateBlueprintWeaponEnchantment("GravityKineticBladeEnchantment", "Gravity Blast — Kinetic Blade",
                null, "Gravity Blast", null, null, 0);
            enchant.SetComponents
                (
                first_context_calc,
                first_rank_conf,
                second_rank_conf,
                second_context_calc
                );
            enchant.WeaponFxPrefab = new PrefabLink { AssetId = "30f3331e77343eb4f8f0bc51a0fcf454" };

            return enchant;
        }

        #endregion

        #endregion

        public static void CreateGravityBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("gravityBlast.png");

            var ability = Helper.CreateBlueprintAbility("GravityBlastBase", "Gravity Blast", 
                GravityBlastDescription, null, icon, AbilityType.Special, 
                UnitCommand.CommandType.Standard, AbilityRange.Close, 
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.Get(Tree.FocusFirst).To<BlueprintUnitFactReference>()), 
                Kineticist.Blast.BurnCost(null, 0, 0, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            Gravity.BaseAbility = ability.ToRef();
        }

        public static void CreateGravityBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("GravityBlastFeature", "Gravity Blast",
                GravityBlastDescription, null, null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(AnyRef.Get(Gravity.BaseAbility).To<BlueprintUnitFactReference>())
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;

            Gravity.BlastFeature = feature.ToRef();
        }

        public static void CreateGravityBlastProgression()
        {
            var progression = Helper.CreateBlueprintProgression("GravityBlastProgression", "Gravity Blast",
                GravityBlastDescription, null, null, 0)
                .SetComponents
                (
                Helper.CreateAddFacts(Kineticist.ref_compositeBlastBuff),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.Get(Kineticist.ref_infusion_kineticBlade).To<BlueprintUnitFactReference>(), 
                    AnyRef.Get(Gravity.BladeFeature).To<BlueprintUnitFactReference>())
                );

            var entry = Helper.CreateLevelEntry(1, AnyRef.Get(Gravity.BlastFeature).To<BlueprintFeatureReference>());
            Helper.AddEntries(progression, entry);

            Gravity.Progession = progression.ToRef3();
        }

        #endregion

        #region Negative Blast

        public static void CreateNegativeBlast()
        {
            // Variants
            var variant_base = CreateNegativeBlastVariant_base();
            var variant_extended = CreateNegativeBlastVariant_extended();
            var variant_spindle = CreateNegativeBlastVariant_spindle();
            var variant_wall = CreateNegativeBlastVariant_wall();
            var variant_blade = CreateNegativeBlastVariant_blade();
            var variant_singularity = CreateNegativeBlastVariant_singularity();
            // Ability
            CreateNegativeBlastAbility(variant_base, variant_extended, variant_spindle, variant_wall, variant_blade, variant_singularity);
            // Feature
            CreateNegativeBlastFeature();
            // Progression
            CreateNegativeBlastProgression();
            // Helpers

            // Add to Dark Codex
        }

        #region Negative Variants

        private static BlueprintAbility CreateNegativeBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("negativeBlast.png");

            var ability = Helper.CreateBlueprintAbility("NegativeBlastAbility", "Negative Blast",
                NegativeBlastDescription, null, icon, AbilityType.Special,
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
                Kineticist.ref_infusion_extendedRange.Get().m_DisplayName,
                Kineticist.ref_infusion_extendedRange.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, e: DamageEnergyType.NegativeEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 0),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_extendedRange),
                Kineticist.Blast.Projectile(Resource.Projectile.UmbralStrike00, false, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Negative.BaseAbility;

            return ability;
        }

        private static BlueprintAbility CreateNegativeBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleNegativeBlastAbility",
                Kineticist.ref_infusion_spindle.Get().m_DisplayName,
                Kineticist.ref_infusion_spindle.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, e: DamageEnergyType.NegativeEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 0),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_spindle),
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
            ability.m_Parent = Negative.BaseAbility;

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
                m_AreaEffect = CreateNegativeWallEffect().ToRef(),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallNegativeBlastAbility",
                Kineticist.ref_infusion_wall.Get().m_DisplayName,
                Kineticist.ref_infusion_wall.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 0),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_wall),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Negative.BaseAbility;

            return ability;
        }

        private static BlueprintAbility CreateNegativeBlastVariant_singularity()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("1325e698f4a3f224b880e3b83a551228"); // Supernova

            var ability = Helper.CreateBlueprintAbility("SingularityNegativeBlastAbility",
                "Singularity Infusion",
                SingularityInfusionDescription, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, CreateSingularityEffect("Negative", e: DamageEnergyType.NegativeEnergy)),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 0),
                Kineticist.Blast.RequiredFeat(SingularityInfusion),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Gravity.BaseAbility;

            return ability;
        }


        #region Kinetic Blade: Negative

        private static BlueprintAbility CreateNegativeBlastVariant_blade()
        {
            var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
            var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
            var icon = Helper.StealIcon("89acea313b9a9cb4d86bbbca01b90346"); // KineticBladeEarthBlastAbility
            var damage_icon = Helper.StealIcon("4fc5cf33da20b5444ad3a96c77af8d20"); // EarthBlastKineticBladeDamage

            var weapon = CreateNegativeBlastBlade_weapon();

            #region buffs
            var buff = Helper.CreateBlueprintBuff("KineticBladeNegativeBlastBuff", null, null, null, null, null);
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistBlade { m_Blade = weapon.ToRef() }
                );
            #endregion

            #region BlastAbility

            var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBladeNegativeBlastAbility", "Negative Blast — Kinetic Blade",
                KineticBladeDescription, out var unused, null, icon,
                group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
            blade_active_ability.m_Buff = buff.ToRef();
            blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
            blade_active_ability.SetComponents
                (
                new RestrictionCanUseKineticBlade { }
                );

            #endregion

            #region BlastBurnAbility

            var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBladeNegativeBlastBurnAbility", null, null, null, icon,
                AbilityType.Special, UnitCommand.CommandType.Free, AbilityRange.Personal);
            blade_burn_ability.TargetSelf(CastAnimationStyle.Omni);
            blade_burn_ability.Hidden = true;
            blade_burn_ability.DisableLog = true;
            blade_burn_ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten;
            blade_burn_ability.SetComponents
                (
                new AbilityKineticist { Amount = 1, InfusionBurnCost = 1 },
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, kinetic_blade_enable_buff.CreateContextActionApplyBuff(asChild: true)),
                new AbilityKineticBlade { }
                );

            #endregion

            #region BlastKineticBladeDamage

            var blade_damage_ability = Helper.CreateBlueprintAbility("NegativeBlastKineticBladeDamage", "Telekinetic Blast",
                GravityBlastDescription, null, damage_icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            blade_damage_ability.TargetEnemy(CastAnimationStyle.Omni);
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            blade_damage_ability.Hidden = true;
            blade_damage_ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(kinetic_blade_hide_feature.ToRef2()),
                new AbilityDeliveredByWeapon { },
                Kineticist.Blast.RunActionDealDamage(out var actions, e: DamageEnergyType.NegativeEnergy),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1),
                Kineticist.Blast.Projectile(Resource.Projectile.UmbralStrike00, false, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                );
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            #endregion

            weapon.SetComponents
                (
                new WeaponKineticBlade { m_ActivationAbility = blade_burn_ability.ToRef(), m_Blast = blade_damage_ability.ToRef() }
                );

            var blade_feat = Helper.CreateBlueprintFeature("NegativeKineticBladeFeature", null, null, null, icon, FeatureGroup.None);
            blade_feat.HideInUI = true;
            blade_feat.HideInCharacterSheetAndLevelUp = true;
            blade_feat.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                );

            Negative.BladeFeature = blade_feat.ToRef();
            Negative.BladeDamageAbility = blade_damage_ability.ToRef();
            return blade_damage_ability;
        }

        private static BlueprintItemWeapon CreateNegativeBlastBlade_weapon()
        {
            //var icon = Helper.StealIcon("43ff6714-3efb-86d4-f894-b10577329050"); // Air Kinetic Blade Weapon

            var weapon = Helper.CreateBlueprintItemWeapon("NegativeKineticBladeWeapon", null, null, Kineticist.ref_kinetic_blast_energy_blade_type,
                damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero },
                form: null,
                secondWeapon: null, false, null, 10);
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreateNegativeBlastBlade_enchantment().ToRef() };

            weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
            weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
            weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
            weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
            weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

            // Components are later
            return weapon;
        }

        private static BlueprintWeaponEnchantment CreateNegativeBlastBlade_enchantment()
        {
            var first_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = 0,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };
            var first_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, feature: Tree.BlastFeature, min: 0, max: 20);
            var second_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, customProperty: Tree.MainStatProp, min: 0, max: 20);
            var second_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.DamageBonus,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Shared,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };

            var enchant = Helper.CreateBlueprintWeaponEnchantment("NegativeKineticBladeEnchantment", "Negative Blast — Kinetic Blade",
                null, "Gravity Blast", null, null, 0);
            enchant.SetComponents
                (
                first_context_calc,
                first_rank_conf,
                second_rank_conf,
                second_context_calc
                );
            enchant.WeaponFxPrefab = new PrefabLink { AssetId = "30f3331e77343eb4f8f0bc51a0fcf454" };

            return enchant;
        }

        #endregion

        #endregion

        public static void CreateNegativeBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("negativeBlast.png");

            var ability = Helper.CreateBlueprintAbility("NegativeBlastBase", "Negative Blast",
                NegativeBlastDescription, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.Get(Tree.FocusFirst).To<BlueprintUnitFactReference>()),
                Kineticist.Blast.BurnCost(null, 0, 0, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            Negative.BaseAbility = ability.ToRef();
        }

        public static void CreateNegativeBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("NegativeBlastFeature", "Negative Blast",
                NegativeBlastDescription, null, null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(AnyRef.Get(Negative.BaseAbility).To<BlueprintUnitFactReference>())
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;

            Negative.BlastFeature = feature.ToRef();
        }

        public static void CreateNegativeBlastProgression()
        {
            var progression = Helper.CreateBlueprintProgression("NegativeBlastProgression", "Negative Blast",
                NegativeBlastDescription, null, null, 0)
                .SetComponents
                (
                Helper.CreateAddFacts(Kineticist.ref_compositeBlastBuff),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.Get(Kineticist.ref_infusion_kineticBlade).To<BlueprintUnitFactReference>(),
                    AnyRef.Get(Negative.BladeFeature).To<BlueprintUnitFactReference>())
                );

            var entry = Helper.CreateLevelEntry(1, AnyRef.Get(Negative.BlastFeature).To<BlueprintFeatureReference>());
            Helper.AddEntries(progression, entry);

            Negative.Progession = progression.ToRef3();
        }


        #endregion

        #region Composite Blasts

        private static void CreateVoidComposites()
        {
            CreateVoidBlast();
            CreateGraviticBoost();
            CreateNegativeAdmixture();

            AddMixturesToComposite();
        }

        #region Void Blast

        // TODO,
        //      Form Infusions

        public static void CreateVoidBlast()
        {
            // Variants
            var variant_base = CreateVoidBlastVariant_base();
            var variant_extended = CreateVoidBlastVariant_extended();
            var variant_spindle = CreateVoidBlastVariant_spindle();
            var variant_wall = CreateVoidBlastVariant_wall();
            var variant_blade = CreateVoidBlastVariant_blade();
            var variant_singularity = CreateVoidBlastVariant_singularity();
            // Ability
            CreateVoidBlastAbility(variant_base, variant_extended, variant_spindle, variant_wall, variant_blade, variant_singularity);
            // Feature
            CreateVoidBlastFeature();
            // Progression - Not needed due to Composite Blast
            // CreateVoidBlastProgression();
            // Helpers

            // Add to Dark Codex
        }

        #region Void Variants

        private static BlueprintAbility CreateVoidBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("voidBlast.png");

            var ability = Helper.CreateBlueprintAbility("VoidBlastAbility", "Void Blast",
                VoidBlastDescription, null, icon, AbilityType.Special,
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
                Kineticist.ref_infusion_extendedRange.Get().m_DisplayName,
                Kineticist.ref_infusion_extendedRange.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Long, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning, e: DamageEnergyType.NegativeEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_extendedRange),
                Kineticist.Blast.Projectile(Resource.Projectile.NegativeCommonProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = VoidBlast.BaseAbility;

            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }

        private static BlueprintAbility CreateVoidBlastVariant_spindle()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion

            var ability = Helper.CreateBlueprintAbility("SpindleVoidBlastAbility",
                Kineticist.ref_infusion_spindle.Get().m_DisplayName,
                Kineticist.ref_infusion_spindle.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning, e: DamageEnergyType.NegativeEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 2, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_spindle),
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
            ability.m_Parent = VoidBlast.BaseAbility;

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
                m_AreaEffect = CreateVoidWallEffect().ToRef(),
                OnUnit = false
            };

            var ability = Helper.CreateBlueprintAbility("WallVoidBlastAbility",
                Kineticist.ref_infusion_wall.Get().m_DisplayName,
                Kineticist.ref_infusion_wall.Get().m_Description, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 2),
                Kineticist.Blast.RequiredFeat(Kineticist.ref_infusion_wall),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = VoidBlast.BaseAbility;

            return ability;
        }

        private static BlueprintAbility CreateVoidBlastVariant_singularity()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("1325e698f4a3f224b880e3b83a551228"); // Supernova

            var ability = Helper.CreateBlueprintAbility("SingularityVoidBlastAbility",
                "Singularity Infusion",
                SingularityInfusionDescription, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, CreateSingularityEffect("Void", p: PhysicalDamageForm.Bludgeoning, e: DamageEnergyType.NegativeEnergy)),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(null, infusion: 3, blast: 2),
                Kineticist.Blast.RequiredFeat(SingularityInfusion),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.CanTargetPoint = true;
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;
            ability.m_Parent = Gravity.BaseAbility;

            return ability;
        }


        #region Kinetic Blade: Void

        private static BlueprintAbility CreateVoidBlastVariant_blade()
        {
            var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
            var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
            var icon = Helper.StealIcon("89acea313b9a9cb4d86bbbca01b90346"); // KineticBladeEarthBlastAbility
            var damage_icon = Helper.StealIcon("4fc5cf33da20b5444ad3a96c77af8d20"); // EarthBlastKineticBladeDamage

            var weapon = CreateVoidBlastBlade_weapon();

            #region buffs
            var buff = Helper.CreateBlueprintBuff("KineticBladeVoidBlastBuff", null, null, null, null, null);
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistBlade { m_Blade = weapon.ToRef() }
                );
            #endregion

            #region BlastAbility

            var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBladeVoidBlastAbility", "Void Blast — Kinetic Blade",
                KineticBladeDescription, out var unused, null, icon,
                group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
            blade_active_ability.m_Buff = buff.ToRef();
            blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
            blade_active_ability.SetComponents
                (
                new RestrictionCanUseKineticBlade { }
                );

            #endregion

            #region BlastBurnAbility

            var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBladeVoidBlastBurnAbility", null, null, null, icon,
                AbilityType.Special, UnitCommand.CommandType.Free, AbilityRange.Personal);
            blade_burn_ability.TargetSelf(CastAnimationStyle.Omni);
            blade_burn_ability.Hidden = true;
            blade_burn_ability.DisableLog = true;
            blade_burn_ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten;
            blade_burn_ability.SetComponents
                (
                Kineticist.Blast.BurnCost(null, infusion: 1, blast: 2, talent: 0),
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, kinetic_blade_enable_buff.CreateContextActionApplyBuff(asChild: true)),
                new AbilityKineticBlade { }
                );

            #endregion

            #region BlastKineticBladeDamage

            var blade_damage_ability = Helper.CreateBlueprintAbility("VoidBlastKineticBladeDamage", "Void Blast",
                VoidBlastDescription, null, damage_icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            blade_damage_ability.TargetEnemy(CastAnimationStyle.Omni);
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            blade_damage_ability.Hidden = true;
            blade_damage_ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(kinetic_blade_hide_feature.ToRef2()),
                new AbilityDeliveredByWeapon { },
                Kineticist.Blast.RunActionDealDamage(out var actions, p: PhysicalDamageForm.Bludgeoning, e: DamageEnergyType.NegativeEnergy),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.RankConfigBonus(half_bonus: false),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 1, blast: 2),
                Kineticist.Blast.Projectile(Resource.Projectile.Kinetic_EarthBlast00_Projectile, true, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                );
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            #endregion

            weapon.SetComponents
                (
                new WeaponKineticBlade { m_ActivationAbility = blade_burn_ability.ToRef(), m_Blast = blade_damage_ability.ToRef() }
                );

            var blade_feat = Helper.CreateBlueprintFeature("VoidKineticBladeFeature", null, null, null, icon, FeatureGroup.None);
            blade_feat.HideInUI = true;
            blade_feat.HideInCharacterSheetAndLevelUp = true;
            blade_feat.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                );

            VoidBlast.BladeFeature = blade_feat.ToRef();
            VoidBlast.BladeDamageAbility = blade_damage_ability.ToRef();
            return blade_damage_ability;
        }

        private static BlueprintItemWeapon CreateVoidBlastBlade_weapon()
        {
            //var icon = Helper.StealIcon("43ff6714-3efb-86d4-f894-b10577329050"); // Air Kinetic Blade Weapon

            var weapon = Helper.CreateBlueprintItemWeapon("VoidKineticBladeWeapon", null, null, Kineticist.ref_kinetic_blast_physical_blade_type,
                damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero },
                form: null,
                secondWeapon: null, false, null, 10);
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreateVoidBlastBlade_enchantment().ToRef() };

            weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
            weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
            weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
            weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
            weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

            // Components are later
            return weapon;
        }

        private static BlueprintWeaponEnchantment CreateVoidBlastBlade_enchantment()
        {
            var first_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = 0,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };
            var first_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, feature: Tree.BlastFeature, min: 0, max: 20);
            var second_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, customProperty: Tree.MainStatProp, min: 0, max: 20);
            var second_context_calc = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.DamageBonus,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Shared,
                        Value = 0,
                        ValueRank = AbilityRankType.Default,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };

            var enchant = Helper.CreateBlueprintWeaponEnchantment("VoidKineticBladeEnchantment", "Void Blast — Kinetic Blade",
                null, "Void Blast", null, null, 0);
            enchant.SetComponents
                (
                first_context_calc,
                first_rank_conf,
                second_rank_conf,
                second_context_calc
                );
            enchant.WeaponFxPrefab = new PrefabLink { AssetId = "30f3331e77343eb4f8f0bc51a0fcf454" };

            return enchant;
        }

        #endregion

        #endregion

        public static void CreateVoidBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("voidBlast.png");

            var ability = Helper.CreateBlueprintAbility("VoidBlastBase", "Void Blast",
                VoidBlastDescription, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close,
                duration: null, savingThrow: null);
            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(AnyRef.Get(Tree.FocusFirst).To<BlueprintUnitFactReference>()),
                Kineticist.Blast.BurnCost(null, 0, 2, 0)
                );
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            VoidBlast.BaseAbility = ability.ToRef();
        }

        public static void CreateVoidBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("VoidBlastFeature", "Void Blast",
                VoidBlastDescription, null, null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFacts(AnyRef.Get(VoidBlast.BaseAbility).To<BlueprintUnitFactReference>()),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.Get(Kineticist.ref_infusion_kineticBlade).To<BlueprintUnitFactReference>(),
                    AnyRef.Get(VoidBlast.BladeFeature).To<BlueprintUnitFactReference>())
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;

            VoidBlast.BlastFeature = feature.ToRef();
            VoidBlast.Parent1 = Gravity;
            VoidBlast.Parent2 = Negative;
        }

        /* Not needed due to composite stuffs
        public static void CreateGravityBlastProgression()
        {
            var progression = Helper.CreateBlueprintProgression("VoidBlastProgression", "Void Blast",
                VoidBlastDescription, null, null, 0)
                .SetComponents
                (
                Helper.CreateAddFacts(Kineticist.ref_compositeBlastBuff),
                //Helper.CreateAddFeatureIfHasFact(Kineticist.ref_infusion_kineticBlade, GravityBladeFeature.ToRef2()),
                Helper.CreateAddFeatureIfHasFact(AnyRef.Get(Gravity.BlastFeature).To<BlueprintUnitFactReference>())
                );

            var entry = Helper.CreateLevelEntry(1, AnyRef.Get(Gravity.BlastFeature).To<BlueprintFeatureReference>());
            Helper.AddEntries(progression, entry);

            Gravity.Progession = progression.ToRef3();
        }
        */

        #endregion

        #region Gravitic Boost

        public static void CreateGraviticBoost()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("0e449a987c784b6f8b13319936667053"); // RitualGreaterChannelNegativeEnergyAbility

            var ability = Helper.CreateBlueprintActivatableAbility("GraviticBoostAbility", "Gravitic Boost",
                GraviticBoostDescription, out var buff, null, icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.SubstanceInfusion, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AbilityUniqueGraviticBoost
                {
                    m_AbilityList = Tree.BaseBasicPhysical
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

            var feature = Helper.CreateBlueprintFeature("GraviticBoostFeature", "Gravitic Boost",
                GraviticBoostDescription, null, icon, FeatureGroup.None);
            feature.SetComponents
                (
                Helper.CreateAddFacts(ability.ToRef2())
                );

            GraviticBoost = feature;
            GraviticBoostBuff = buff;

            Kineticist.AddElementsToInfusion(feature, buff, Tree.GetAll(basic: true, composites: false, basicEnergy: false).ToList().ToArray());
        }

        #endregion

        #region Negative Admixture

        public static void CreateNegativeAdmixture()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("0e449a987c784b6f8b13319936667053"); // RitualGreaterChannelNegativeEnergyAbility

            var ability = Helper.CreateBlueprintActivatableAbility("NegativeAdmixtureAbility", "Negative Admixture",
                NegativeAdmixtureDescription, out var buff, null, icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.SubstanceInfusion, true, true);
            ability.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;

            buff.Flags(stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AbilityUniqueNegativeAdmixture
                {
                    m_AbilityList = Tree.BaseBasicEnergy
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

            var feature = Helper.CreateBlueprintFeature("NegativeAdmixtureFeature", "Negative Admixture",
                NegativeAdmixtureDescription, null, icon, FeatureGroup.None);
            feature.SetComponents
                (
                Helper.CreateAddFacts(ability.ToRef2())
                );

            NegativeAdmixture = feature;
            NegativeAdmixtureBuff = buff;

            Kineticist.AddElementsToInfusion(feature, buff, Tree.GetAll(basic: true, composites: false, basicPhysical: false).ToList().ToArray());
        }

        #endregion

        private static void AddMixturesToComposite()
        {
            var inner_gravity_checker = new ConditionsChecker
            {
                Operation = Operation.Or,
                Conditions = Tree.GetAll(basic: true, basicEnergy: false)
                        .Select(s => Helper.CreateContextConditionHasFact(
                            AnyRef.Get(s.BlastFeature).To<BlueprintUnitFactReference>())).ToArray()
            };
            var inner_gravity_conditional = new Conditional
            {
                ConditionsChecker = inner_gravity_checker,
                IfFalse = null,
                IfTrue = Helper.CreateActionList(Helper.CreateContextActionAddFeature(GraviticBoost.ToRef()))
            };
            var outer_gravity_conditional = Helper.CreateConditional(Helper.CreateContextConditionHasFact(AnyRef.Get(Gravity.BlastFeature).To<BlueprintUnitFactReference>()),
                ifFalse: null, ifTrue: inner_gravity_conditional);

            var inner_negative_checker = new ConditionsChecker
            {
                Operation = Operation.Or,
                Conditions = Tree.GetAll(basic: true, basicPhysical: false)
                        .Select(s => Helper.CreateContextConditionHasFact(
                            AnyRef.Get(s.BlastFeature).To<BlueprintUnitFactReference>())).ToArray()
            };
            var inner_negative_conditional = new Conditional
            {
                ConditionsChecker = inner_negative_checker,
                IfFalse = null,
                IfTrue = Helper.CreateActionList(Helper.CreateContextActionAddFeature(NegativeAdmixture.ToRef()))
            };
            var outer_negative_conditional = Helper.CreateConditional(Helper.CreateContextConditionHasFact(AnyRef.Get(Negative.BlastFeature).To<BlueprintUnitFactReference>()),
                ifFalse: null, ifTrue: inner_negative_conditional);

            var composite_action = Tree.CompositeBuff.Get().GetComponent<AddFactContextActions>();
            Helper.AppendAndReplace(ref composite_action.Activated.Actions, outer_gravity_conditional, outer_negative_conditional);

        }

        #endregion

        #region Infusions
        // TODO
        //  Grappling <- Exists Already
        //  Pushing <- Exists Already
        //  Singularity <- Theres an aeon ability to mimic (FX: Supernova) 3 area effects with delay
        //  Vampiric - Gonna be weird

        public static void CreateVoidInfusions()
        {
            CreateDampeningInfusion();
            CreateEnervatingInfusion();
            CreatePullingInfusion();
            CreateUnnervingInfusion();
            CreateVampiricInfusion();
            CreateWeighingInfusion();

            Kineticist.AddElementsToInfusion(DampeningInfusion, DampeningInfusionBuff, Negative, VoidBlast);
            Kineticist.AddElementsToInfusion(EnervatingInfusion, EnervatingInfusionBuff, Negative, VoidBlast);
            Kineticist.AddElementsToInfusion(PullingInfusion, PullingInfusionBuff, Gravity, VoidBlast);
            Kineticist.AddElementsToInfusion(UnnervingInfusion, UnnervingInfusionBuff, Negative, VoidBlast);
            Kineticist.AddElementsToInfusion(VampiricInfusion, VampiricInfusionBuff, Negative, VoidBlast);
            Kineticist.AddElementsToInfusion(WeighingInfusion, WeighingInfusionBuff, Gravity, VoidBlast);

            Kineticist.TryDarkCodexAddExtraWildTalent
                (
                DampeningInfusion.ToRef(), 
                EnervatingInfusion.ToRef(), 
                PullingInfusion.ToRef(), 
                UnnervingInfusion.ToRef(),
                VampiricInfusion.ToRef(),
                WeighingInfusion.ToRef()
                );
            Helper.AppendAndReplace(ref Kineticist.infusion_selection.m_AllFeatures,
                DampeningInfusion.ToRef(),
                EnervatingInfusion.ToRef(),
                PullingInfusion.ToRef(),
                UnnervingInfusion.ToRef(),
                VampiricInfusion.ToRef(),
                WeighingInfusion.ToRef()
                );
        }

        private static void CreateDampeningInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("037460f7ae3e21943b237007f2b1a5d5"); // Dazzling Infusion Icon
            var dazzled_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("df6d1025da07524429afbae248845ecc"); // DazzledBuff

            var ability = Helper.CreateBlueprintActivatableAbility("DampeningInfusionAbility", "Dampening Infusion",
                DampeningInfusionDescription, out var buff, null, icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
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

            var feature = Helper.CreateBlueprintFeature("DampeningInfusionFeature", "Dampening Infusion",
                DampeningInfusionDescription, null, icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreateAddFacts(ability.ToRef2())
                );

            DampeningInfusion = feature;
            DampeningInfusionBuff = buff;
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

            var ability = Helper.CreateBlueprintActivatableAbility("EnervatingInfusionAbility", "Enervating Infusion",
                EnervatingInfusionDescription, out var buff, null, icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
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

            var feature = Helper.CreateBlueprintFeature("EnervatingInfusionFeature", "Enervating Infusion",
                EnervatingInfusionDescription, null, icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 14),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            EnervatingInfusion = feature;
            EnervatingInfusionBuff = buff;
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

            var ability = Helper.CreateBlueprintActivatableAbility("PullingInfusionAbility", "Pulling Infusion",
                PullingInfusionDescription, out var buff, null, icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
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

            var feature = Helper.CreateBlueprintFeature("PullingInfusionFeature", "Pulling Infusion",
                PullingInfusionDescription, null, icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 1),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            PullingInfusion = feature;
            PullingInfusionBuff = buff;
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

            var ability = Helper.CreateBlueprintActivatableAbility("UnnervingInfusionAbility", "Unnerving Infusion",
                UnnervingInfusionDescription, out var buff, null, icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
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

            var feature = Helper.CreateBlueprintFeature("UnnervingInfusionFeature", "Unnerving Infusion",
                UnnervingInfusionDescription, null, icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 6),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            UnnervingInfusion = feature;
            UnnervingInfusionBuff = buff;
        }

        private static void CreateVampiricInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("30c81aff8e5293d418759d10f193f347"); // VampiricInfusionAbility

            var effect_buff = Helper.CreateBlueprintBuff("VoidVampiricInfusionEffectBuff", "Vampiric Infusion — Void Healer",
                "Your next Void Healer ability burn cost is reduced to zero.", null, icon, null);
            effect_buff.Flags(removeOnRest: true);
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

            var ability = Helper.CreateBlueprintActivatableAbility("VoidVampiricInfusionAbility", "Vampiric Infusion",
                VampiricInfusionDescription, out var buff, null, icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
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

            var feature = Helper.CreateBlueprintFeature("VoidVampiricInfusionFeature", "Vampiric Infusion",
                VampiricInfusionDescription, null, icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 10),
                Helper.CreatePrerequisiteFeature(VoidHealer.ToRef()),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            VampiricInfusion = feature;
            VampiricInfusionBuff = buff;
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

            var ability = Helper.CreateBlueprintActivatableAbility("WeighingInfusionAbility", "Weighing Infusion",
                WeighingInfusionDescription, out var buff, null, icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.WithUnitCommand,
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

            var feature = Helper.CreateBlueprintFeature("WeighingInfusionFeature", "Weighing Infusion",
                WeighingInfusionDescription, null, icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 4),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            WeighingInfusion = feature;
            WeighingInfusionBuff = buff;
        }

        private static void CreateSingularityInfusion()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("1325e698f4a3f224b880e3b83a551228"); // Supernova
            var feature = Helper.CreateBlueprintFeature("SingularityInfusion", "Singularity",
                SingularityInfusionDescription, null, icon, FeatureGroup.KineticBlastInfusion);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 8),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(Tree.FocusFirst).To<BlueprintFeatureReference>())
                );

            Helper.AppendAndReplace(ref Kineticist.infusion_selection.m_AllFeatures, feature.ToRef());
            SingularityInfusion = feature;
        }

        #endregion

        #region Wild Talents

        private static void CreateVoidWildTalents(BlueprintFeature emptiness)
        {
            AddToSkilledKineticist();
            AddToExpandedDefense(emptiness);

            (var wild_0, var wild_1, var wild_2, var wild_3) = CreateWildTalentBonusFeatVoid();

            var corpse_puppet = CreateCorpsePuppet();
            var curse_breaker = CreateCurseBreaker();
            var gravity_control = CreateGravityControl();
            var gravity_control_greater = CreateGravityControlGreater(gravity_control);
            var undead_grip = CreateUndeadGrip();
            var void_healer = CreateVoidHealer();

            Kineticist.TryDarkCodexAddExtraWildTalent(corpse_puppet.ToRef(), curse_breaker.ToRef(), gravity_control.ToRef(), gravity_control_greater.ToRef(), undead_grip.ToRef(), wild_0.ToRef(), wild_1.ToRef(), wild_2.ToRef(), wild_3.ToRef(), void_healer.ToRef());
            Kineticist.AddToWildTalents(corpse_puppet.ToRef(), curse_breaker.ToRef(), gravity_control.ToRef(), gravity_control_greater.ToRef(), undead_grip.ToRef(), void_healer.ToRef());
        }

        private static void AddToSkilledKineticist()
        {
            var buff = Helper.CreateBlueprintBuff("SkilledKineticistVoidBuff", "Skilled Kineticist", null, null, null, null);
            buff.Flags(true, true);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                Helper.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel,
                    ContextRankProgression.Div2, max: 20, classes: new BlueprintCharacterClassReference[1] { Tree.Class }),
                Helper.CreateAddContextStatBonus(new ContextValue { ValueType = ContextValueType.Rank, Value = 0, ValueRank = AbilityRankType.Default, ValueShared = AbilitySharedValue.Damage },
                StatType.SkillKnowledgeWorld)
                );

            var condition = Helper.CreateContextConditionHasFact(AnyRef.Get(VoidFocus.First).To<BlueprintUnitFactReference>());
            var conditional = Helper.CreateConditional(condition,
                ifTrue: buff.CreateContextActionApplyBuff(0, DurationRate.Rounds, false, false, false, true, true));

            var factContextAction = Kineticist.ref_skilled_kineticist.Get().GetComponent<AddFactContextActions>();
            Helper.AppendAndReplace(ref factContextAction.Activated.Actions, conditional);
        }
        private static void AddToExpandedDefense(BlueprintFeature emptiness)
        {
            var selection = AnyRef.Get(Kineticist.ref_expanded_defense).To<BlueprintFeatureSelectionReference>().Get();
            Helper.AppendAndReplace(ref selection.m_AllFeatures, emptiness.ToRef());
        }

        private static (BlueprintFeatureSelection wild_0, BlueprintFeatureSelection wild_1, BlueprintFeatureSelection wild_2, BlueprintFeatureSelection wild_3) CreateWildTalentBonusFeatVoid()
        {
            var spell_pen = Helper.ToRef<BlueprintFeatureReference>("ee7dc126939e4d9438357fbd5980d459"); // SpellPenetration
            var spell_pen_greater = Helper.ToRef<BlueprintFeatureReference>("1978c3f91cfbbc24b9c9b0d017f4beec"); // GreaterSpellPenetration
            var precise_shot = Helper.ToRef<BlueprintFeatureReference>("8f3d1e6b4be006f4d896081f2f889665"); // PreciseShot
            var trip = Helper.ToRef<BlueprintFeatureReference>("0f15c6f70d8fb2b49aa6cc24239cc5fa"); // ImprovedTrip
            var trip_greater = Helper.ToRef<BlueprintFeatureReference>("4cc71ae82bdd85b40b3cfe6697bb7949"); // SpellPenetration

            var wild_0 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatVoid", void_wild_talent_name,
                void_wild_talent_description, null, null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_0.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.First).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.Second).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.Third).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteNoFeature(trip, false),
                Helper.CreatePrerequisiteNoFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.Knight).To<BlueprintFeatureReference>(), true)
                );
            wild_0.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_0.m_AllFeatures, spell_pen, precise_shot, trip);

            var wild_1 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatVoid1", void_wild_talent_name,
                void_wild_talent_description, null, null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_1.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.First).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.Second).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.Third).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteNoFeature(trip, false),
                Helper.CreatePrerequisiteFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.Knight).To<BlueprintFeatureReference>(), true)
                );
            wild_1.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_1.m_AllFeatures, spell_pen_greater, precise_shot, trip);

            var wild_2 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatVoid2", void_wild_talent_name,
                void_wild_talent_description, null, null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_2.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.First).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.Second).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.Third).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(trip, false),
                Helper.CreatePrerequisiteNoFeature(spell_pen, false),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.Knight).To<BlueprintFeatureReference>(), true)
                );
            wild_2.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_2.m_AllFeatures, spell_pen, precise_shot, trip_greater);

            var wild_3 = Helper.CreateBlueprintFeatureSelection("WildTalentBonusFeatVoid3", void_wild_talent_name,
                void_wild_talent_description, null, null, FeatureGroup.KineticWildTalent, SelectionMode.Default);
            wild_3.SetComponents
                (
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.First).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.Second).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.Third).To<BlueprintFeatureReference>(), true),
                Helper.CreatePrerequisiteFeature(trip, false),
                Helper.CreatePrerequisiteFeature(spell_pen, false),
                new PrerequisiteSelectionPossible
                {
                    m_ThisFeature = wild_3.ToRef3()
                },
                Helper.CreatePrerequisiteFeature(AnyRef.Get(VoidFocus.Knight).To<BlueprintFeatureReference>(), true)
                );
            wild_3.IgnorePrerequisites = true;
            Helper.AppendAndReplace(ref wild_3.m_AllFeatures, spell_pen_greater, precise_shot, trip_greater);


            Helper.AppendAndReplace(ref Kineticist.wild_talent_selection.m_AllFeatures, wild_0.ToRef(), wild_1.ToRef(), wild_2.ToRef(), wild_3.ToRef());
            return (wild_0, wild_1, wild_2, wild_3);
        }

        private static BlueprintFeature CreateCorpsePuppet()
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

            var ability = Helper.CreateBlueprintAbility("CorpsePuppetAbility", "Corpse Puppet",
                CorpsePuppetDescription, null, icon, AbilityType.Special, UnitCommand.CommandType.Standard,
                AbilityRange.Close, null, null).TargetPoint(CastAnimationStyle.Kineticist);
            ability.CanTargetSelf = true;
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(actions: context_spawn),
                Helper.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, max: 20, classes: new BlueprintCharacterClassReference[] { Tree.Class }),
                Kineticist.Blast.SpellDescriptor((SpellDescriptor)2048),
                Kineticist.Blast.BurnCost(null, infusion: 0, blast: 0, talent: 1)
                );

            var feature = Helper.CreateBlueprintFeature("CorpsePuppetFeature", "Corpse Puppet",
                CorpsePuppetDescription, null, icon, FeatureGroup.KineticWildTalent);
            feature.IsClassFeature = true;
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true, 
                    AnyRef.Get(VoidFocus.First).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Second).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Third).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Knight).To<BlueprintFeatureReference>()),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 8),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            return feature;
        }

        private static BlueprintFeature CreateCurseBreaker()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("b48674cef2bff5e478a007cf57d8345b"); // RemoveCurse

            var buff = Helper.CreateBlueprintBuff("CurseBreakerBuff", "Curse Breaker",
                CurseBreakerDescription, null, icon, null);
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

            var ability = Helper.CreateBlueprintAbility("CurseBreakerAbility", "Curse Breaker",
                CurseBreakerDescription, null, icon, AbilityType.Special, UnitCommand.CommandType.Standard,
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

            var feature = Helper.CreateBlueprintFeature("CurseBreakerFeature", "Curse Breaker",
                CurseBreakerDescription, null, icon, FeatureGroup.KineticWildTalent);
            feature.IsClassFeature = true;
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.Get(VoidFocus.First).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Second).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Third).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Knight).To<BlueprintFeatureReference>()),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 8),
                Helper.CreateAddFacts(ability.ToRef2()),
                Helper.CreateAddFactContextActions(new GameAction[] { buff.CreateContextActionApplyBuff(permanent: true) })
                );

            return feature;
        }

        private static BlueprintFeature CreateUndeadGrip()
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

            var ability = Helper.CreateBlueprintAbility("UndeadGripAbility", "Undead Grip",
                UndeadGripDescription, null, icon, AbilityType.Special, UnitCommand.CommandType.Standard,
                AbilityRange.Medium, null, null).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.SetComponents
                (
                new SpellComponent { School = SpellSchool.Enchantment },
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Will, conditional),
                Helper.CreateAbilityTargetHasFact(false, undead_type),
                Kineticist.Blast.BurnCost(null, infusion: 0, blast: 0, talent: 1)
                );

            var feature = Helper.CreateBlueprintFeature("UndeadGripFeature", "Undead Grip",
                UndeadGripDescription, null, icon, FeatureGroup.KineticWildTalent);
            feature.IsClassFeature = true;
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.Get(VoidFocus.First).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Second).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Third).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Knight).To<BlueprintFeatureReference>()),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 6),
                Helper.CreateAddFacts(ability.ToRef2())
                );


            return feature;
        }

        private static BlueprintFeature CreateVoidHealer()
        {
            UnityEngine.Sprite icon = Helper.CreateSprite("voidHealer.png");
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

            var ability = Helper.CreateBlueprintAbility("VoidHealerAbility", "Void Healer",
                VoidHealerDescription, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard,
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

            var feature = Helper.CreateBlueprintFeature("VoidHealerFeature", "Void Healer",
                VoidHealerDescription, null, icon, FeatureGroup.KineticWildTalent);
            feature.IsClassFeature = true;
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.Get(VoidFocus.First).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Second).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Third).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Knight).To<BlueprintFeatureReference>()),
                Helper.CreateAddFacts(ability.ToRef2()),
                Helper.CreatePrerequisiteNoFeature(feature.ToRef())
                );

            VoidHealerAbility = ability;
            VoidHealer = feature;

            return feature;
        }

        #region Gravity Control

        private static BlueprintFeature CreateGravityControl()
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

            var buff = Helper.CreateBlueprintBuff("GravityControlBuff", "Gravity Control",
                GravityControlDescription, null, icon);
            buff.Flags(null, true, null, null);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                ac_bonus,
                no_difficultTerrain
                );

            var ability = Helper.CreateBlueprintAbility("GravityControlAbility", "Gravity Control",
                GravityControlDescription, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Move, AbilityRange.Personal);
            ability.TargetSelf(CastAnimationStyle.Kineticist);
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, buff.CreateContextActionApplyBuff(1, DurationRate.Rounds, false, false, true, false, false))
                );

            var feature = Helper.CreateBlueprintFeature("GravityControlFeature", "Gravity Control",
                GravityControlDescription, null, icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.Get(VoidFocus.First).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Second).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Third).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Knight).To<BlueprintFeatureReference>()),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 6),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            return feature;
        }

        private static BlueprintFeature CreateGravityControlGreater(BlueprintFeature gravity_control)
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

            var ability = Helper.CreateBlueprintActivatableAbility("GravityControlGreaterAbility", "Gravity Control, Greater",
                GravityControlGreaterDescription, out var buff, null, icon, UnitCommand.CommandType.Move, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
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
            var feature = Helper.CreateBlueprintFeature("GravityControlGreaterFeature", "Gravity Control, Greater",
                GravityControlGreaterDescription, null, icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeature(gravity_control.ToRef()),
                Helper.CreatePrerequisiteFeaturesFromList(true,
                    AnyRef.Get(VoidFocus.First).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Second).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Third).To<BlueprintFeatureReference>(),
                    AnyRef.Get(VoidFocus.Knight).To<BlueprintFeatureReference>()),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 10),
                remove_lesser,
                Helper.CreateAddFacts(ability.ToRef2())
                );

            gravity_control.AddComponents(Helper.CreatePrerequisiteNoFeature(feature.ToRef()));

            return feature;
        }

        #endregion

        #endregion

        #region Area Effects
        // TODO
        //  Composites
        //  Gravitic
        //  Void Blast
        //  Neg Admixtures

        private static BlueprintAbilityAreaEffect CreateGravityWallEffect()
        {
            var wall_infusion = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c684335918896ce4ab13e96cec929796"); // WallInfusion
            var unique = new UniqueAreaEffect { m_Feature = wall_infusion.ToRef2() };
            var prefab = new PrefabLink { AssetId = "1f26aacd3ad314e4c820d4fe2ac3fd46" }; // EarthBlastWallEffect PrefabLink

            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, Helper.CreateContextValue(AbilityRankType.DamageDice), Helper.CreateContextValue(AbilitySharedValue.Damage));

            var context_dealDamage = Helper.CreateContextActionDealDamage(PhysicalDamageForm.Bludgeoning,
                dice, false, false, false, true, false, AbilitySharedValue.Damage);
            ActionList action_list = new() { Actions = new GameAction[1] { context_dealDamage } };

            var area_effect = Helper.CreateBlueprintAbilityAreaEffect("WallGravityBlastArea", null, true, true,
                AreaEffectShape.Wall, new Feet { m_Value = 60 },
                prefab, unitEnter: action_list);
            area_effect.m_Tags = AreaEffectTags.DestroyableInCutscene;
            area_effect.IgnoreSleepingUnits = false;
            area_effect.AffectDead = false;
            area_effect.AggroEnemies = true;
            area_effect.AffectEnemies = true;
            area_effect.SpellResistance = false;

            var context1 = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, stat: StatType.Constitution,
                type: AbilityRankType.DamageBonus, customProperty: Tree.MainStatProp, min: 0, max: 20);
            var context2 = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, stat: StatType.Constitution,
                type: AbilityRankType.DamageDice, customProperty: Tree.MainStatProp, min: 0, max: 20,
                feature: Tree.BlastFeature);

            var calc_shared = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };

            var calc_ability_params = new ContextCalculateAbilityParamsBasedOnClass
            {
                UseKineticistMainStat = true,
                StatType = StatType.Charisma,
                m_CharacterClass = Tree.Class
            };

            area_effect.AddComponents(unique, context1, context2, calc_shared, calc_ability_params);

            return area_effect;
        }

        private static BlueprintAbilityAreaEffect CreateNegativeWallEffect()
        {
            var wall_infusion = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c684335918896ce4ab13e96cec929796"); // WallInfusion
            var unique = new UniqueAreaEffect { m_Feature = wall_infusion.ToRef2() };
            var prefab = new PrefabLink { AssetId = "4ffc8d2162a215e44a1a728752b762eb" }; // AirBlastWallEffect PrefabLink

            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, Helper.CreateContextValue(AbilityRankType.DamageDice), Helper.CreateContextValue(AbilitySharedValue.Damage));

            var context_dealDamage = Helper.CreateContextActionDealDamage(DamageEnergyType.NegativeEnergy,
                dice, false, false, false, false, false, AbilitySharedValue.Damage);
            ActionList action_list = new() { Actions = new GameAction[1] { context_dealDamage } };

            var area_effect = Helper.CreateBlueprintAbilityAreaEffect("WallNegativeBlastArea", null, true, true,
                AreaEffectShape.Wall, new Feet { m_Value = 60 },
                prefab, unitEnter: action_list);
            area_effect.m_Tags = AreaEffectTags.DestroyableInCutscene;
            area_effect.IgnoreSleepingUnits = false;
            area_effect.AffectDead = false;
            area_effect.AggroEnemies = true;
            area_effect.AffectEnemies = true;
            area_effect.SpellResistance = true;

            var context1 = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, progression: ContextRankProgression.Div2, stat: StatType.Constitution,
                type: AbilityRankType.DamageBonus, customProperty: Tree.MainStatProp, min: 0, max: 20);
            var context2 = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, stat: StatType.Constitution,
                type: AbilityRankType.DamageDice, customProperty: Tree.MainStatProp, min: 0, max: 20,
                feature: Tree.BlastFeature);

            var calc_shared = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };

            var calc_ability_params = new ContextCalculateAbilityParamsBasedOnClass
            {
                UseKineticistMainStat = true,
                StatType = StatType.Charisma,
                m_CharacterClass = Tree.Class
            };

            area_effect.AddComponents(unique, context1, context2, calc_shared, calc_ability_params);

            return area_effect;
        }

        private static BlueprintAbilityAreaEffect CreateVoidWallEffect()
        {
            var wall_infusion = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c684335918896ce4ab13e96cec929796"); // WallInfusion
            var unique = new UniqueAreaEffect { m_Feature = wall_infusion.ToRef2() };
            var prefab = new PrefabLink { AssetId = "1f26aacd3ad314e4c820d4fe2ac3fd46" }; // EarthBlastWallEffect PrefabLink

            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, Helper.CreateContextValue(AbilityRankType.DamageDice), Helper.CreateContextValue(AbilitySharedValue.Damage));

            var context_dealDamage_physical = Helper.CreateContextActionDealDamage(PhysicalDamageForm.Bludgeoning,
                dice, false, false, false, true, false, AbilitySharedValue.Damage);
            var context_dealDamage_energy = Helper.CreateContextActionDealDamage(DamageEnergyType.NegativeEnergy,
                dice, false, false, false, true, false, AbilitySharedValue.Damage);
            ActionList action_list = new() { Actions = new GameAction[2] { context_dealDamage_physical, context_dealDamage_energy } };

            var area_effect = Helper.CreateBlueprintAbilityAreaEffect("WallVoidBlastArea", null, true, true,
                AreaEffectShape.Wall, new Feet { m_Value = 60 },
                prefab, unitEnter: action_list);
            area_effect.m_Tags = AreaEffectTags.DestroyableInCutscene;
            area_effect.IgnoreSleepingUnits = false;
            area_effect.AffectDead = false;
            area_effect.AggroEnemies = true;
            area_effect.AffectEnemies = true;
            area_effect.SpellResistance = false;

            var context1 = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, stat: StatType.Constitution,
                type: AbilityRankType.DamageBonus, customProperty: Tree.MainStatProp, min: 0, max: 20);
            var context2 = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, stat: StatType.Constitution,
                type: AbilityRankType.DamageDice, customProperty: Tree.MainStatProp, min: 0, max: 20,
                feature: Tree.BlastFeature);

            var calc_shared = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageBonus,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };

            var calc_ability_params = new ContextCalculateAbilityParamsBasedOnClass
            {
                UseKineticistMainStat = true,
                StatType = StatType.Charisma,
                m_CharacterClass = Tree.Class
            };

            area_effect.AddComponents(unique, context1, context2, calc_shared, calc_ability_params);

            return area_effect;
        }

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

                var area_effect = Helper.CreateBlueprintAbilityAreaEffect("SingularityInfusionEffectArea" + name + radius/5, null, true, true,
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

                last_area = area_effect.ToRef();
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
