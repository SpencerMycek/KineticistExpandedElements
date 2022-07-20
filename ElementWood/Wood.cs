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

namespace KineticistElementsExpanded.ElementWood
{
    class Wood : Statics
    {

        // Fixed a mixup between Aether and Void Class skills
        
        public static KineticistTree Tree = new();

        public static KineticistTree.Element Positive = new();
        public static KineticistTree.Element WoodBlast = new();
        public static KineticistTree.Element Verdant = new();
        public static KineticistTree.Element Autumn = new();
        public static KineticistTree.Element Spring = new();
        public static KineticistTree.Element Summer = new();
        public static KineticistTree.Element Winter = new();

        public static KineticistTree.Focus WoodFocus = new();

        public static KineticistTree.Infusion PhotoKinetic = new();
        public static KineticistTree.Infusion Spore = new();
        public static KineticistTree.Infusion Toxic = new();
        public static KineticistTree.Infusion GreaterToxic = new();

        public static void Configure()
        {
            // Void for reference

            BlueprintFeatureBase wood_class_skills = CreateWoodClassSkills();

            // Form Infusions
            // Blasts
            // Substance Infusions

            // Elemental Defense

            BlueprintFeature flesh_of_wood_feature = CreateFleshofWood();

            // Misc Blast Functions

            // Elemental Focuses
            CreateWoodElementalFocus(wood_class_skills, flesh_of_wood_feature);
            CreateKineticKnightWoodFocus(wood_class_skills, flesh_of_wood_feature);
            CreateSecondElementWood();
            CreateThirdElementWood();

            // Wild Talents
        }

        #region Class Features and Misc.

        private static BlueprintFeatureBase CreateWoodClassSkills()
        {
            var feature = Helper.CreateBlueprintFeature("WoodClassSkills", "Wood Class Skills",
                WoodClassSkillsDescription, null, null, 0)
                .SetComponents
                (
                Helper.CreateAddClassSkill(StatType.SkillLoreNature)
                );
            return feature;
        }

        private static void CreateWoodBlastsSelection()
        {

        }
        #endregion

        #region Elemental Focus Selection

        private static void CreateWoodElementalFocus(BlueprintFeatureBase class_skills, BlueprintFeatureBase flesh_of_wood)
        {
            var progression = Helper.CreateBlueprintProgression("ElementalFocusWood", "Wood",
                ElementalFocusWoodDescription, null, null,
                FeatureGroup.KineticElementalFocus)
                .SetComponents(Helper.CreatePrerequisiteNoArchetype(Tree.BloodKineticist, Tree.Class));

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            // Can be any Wood basic: Positive or Wood
            var entry1 = Helper.CreateLevelEntry(1, Positive.Selection, class_skills);
            var entry2 = Helper.CreateLevelEntry(2, flesh_of_wood);
            Helper.AddEntries(progression, entry1, entry2);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusFirst.GetBlueprint()).m_AllFeatures, progression.ToRef());

            WoodFocus.First = progression.ToRef3();
        }

        private static void CreateKineticKnightWoodFocus(BlueprintFeatureBase class_skills, BlueprintFeatureBase emptiness)
        {
            var progression = Helper.CreateBlueprintProgression("KineticKnightElementalFocusWood", "Wood",
                ElementalFocusWoodDescription, null, null,
                FeatureGroup.KineticElementalFocus)
                .SetComponents(new AddEquipmentEntity { EquipmentEntity = new EquipmentEntityLink { AssetId = "aecc5905323948449b4cd3bfe36e5daf" } });

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            // Can be any void basic: Positive or Wood
            var entry1 = Helper.CreateLevelEntry(1, Positive.Selection, class_skills);
            var entry2 = Helper.CreateLevelEntry(4, emptiness);
            Helper.AddEntries(progression, entry1, entry2);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusKnight.GetBlueprint()).m_AllFeatures, progression.ToRef());

            WoodFocus.Knight = progression.ToRef3();
        }

        private static void CreateSecondElementWood()
        {
            var progression = Helper.CreateBlueprintProgression("SecondaryElementWood", "Wood",
                ElementalFocusWoodDescription, null, null,
                FeatureGroup.KineticElementalFocus);
            progression.HideInCharacterSheetAndLevelUp = true;

            progression.SetComponents
                (
                Helper.CreateActivateTrigger
                    (
                    Helper.CreateConditionsChecker(Operation.Or,
                        Helper.CreateHasFact(new FactOwner(), AnyRef.Get(WoodFocus.First).To<BlueprintUnitFactReference>()),
                        Helper.CreateHasFact(new FactOwner(), AnyRef.Get(WoodFocus.Knight).To<BlueprintUnitFactReference>())),
                    Helper.CreateActionList
                        (
                        Helper.CreateAddFact(new FactOwner(),
                            AnyRef.Get(Positive.BlastFeature).To<BlueprintUnitFactReference>()),
                        Helper.CreateAddFact(new FactOwner(),
                            AnyRef.Get(WoodBlast.BlastFeature).To<BlueprintUnitFactReference>()),
                        Helper.CreateAddFact(new FactOwner(),
                            AnyRef.Get(Verdant.BlastFeature).To<BlueprintUnitFactReference>())
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
            var entry1 = Helper.CreateLevelEntry(7, Positive.Selection);
            Helper.AddEntries(progression, entry1);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusSecond.GetBlueprint()).m_AllFeatures, progression.ToRef());

            WoodFocus.Second = progression.ToRef3();
        }

        private static void CreateThirdElementWood()
        {
            var progression = Helper.CreateBlueprintProgression("ThirdElementWood", "Wood",
                ElementalFocusWoodDescription, null, null,
                FeatureGroup.KineticElementalFocus);
            progression.HideInCharacterSheetAndLevelUp = true;

            progression.SetComponents
                (
                Helper.CreateActivateTrigger
                    (
                    Helper.CreateConditionsChecker(Operation.Or,
                        Helper.CreateHasFact(new FactOwner(), AnyRef.Get(WoodFocus.First).To<BlueprintUnitFactReference>()),
                        Helper.CreateHasFact(new FactOwner(), AnyRef.Get(WoodFocus.Knight).To<BlueprintUnitFactReference>())),
                    Helper.CreateActionList
                        (
                        Helper.CreateAddFact(new FactOwner(),
                            AnyRef.Get(Verdant.BlastFeature).To<BlueprintUnitFactReference>())
                        )
                    ),
                Helper.CreateAddFacts(AnyRef.Get(Tree.CompositeBuff).To<BlueprintUnitFactReference>()),
                Helper.CreatePrerequisiteNoFeature(AnyRef.Get(WoodFocus.Second).To<BlueprintFeatureReference>())
                );

            progression.m_Classes = new BlueprintProgression.ClassWithLevel
            {
                AdditionalLevel = 0,
                m_Class = Tree.Class
            }.ObjToArray();

            // Can be any void basic: Positive or Wood
            var entry1 = Helper.CreateLevelEntry(15, Positive.Selection);
            Helper.AddEntries(progression, entry1);

            Helper.AppendAndReplace(ref ((BlueprintFeatureSelection)Tree.FocusThird.GetBlueprint()).m_AllFeatures, progression.ToRef());

            WoodFocus.Third = progression.ToRef3();
        }


        #endregion

        #region Flesh of Wood

        public static BlueprintFeature CreateFleshofWood()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("5b77d7cc65b8ab74688e74a37fc2f553"); // Barkskin

            #region Resource

            var resource = Helper.CreateBlueprintAbilityResource("FleshofWoodResource", null,
                FleshofWoodDescription, null, false, 6, 1, 1, 0, 3, 1, 1, true, 0, false, 0,
                classRef: Tree.Class);

            #endregion
            #region Effect Feature

            var effect_feature = Helper.CreateBlueprintFeature("FlashofWoodEffectFeature", null,
                null, null, icon, FeatureGroup.None);
            effect_feature.Ranks = 7;
            effect_feature.HideInUI = true;
            effect_feature.HideInCharacterSheetAndLevelUp = true;
            effect_feature.IsClassFeature = true;
            effect_feature.SetComponents
                (
                Helper.CreateAddFacts()
                );

            #endregion
            #region Effect Buff

            var effect_buff = Helper.CreateBlueprintBuff("FleshofWoodEffectBuff", null,
                null, null, icon);
            effect_buff.Flags(hidden: true, stayOnDeath: true, removeOnRest: true);
            effect_buff.Stacking = StackingType.Stack;
            effect_buff.IsClassFeature = true;
            effect_buff.SetComponents
                (
                Helper.CreateAddFacts(effect_feature.ToRef2())
                );

            #endregion
            #region Buff

            var buff = Helper.CreateBlueprintBuff("FleshofWoodBuff", null,
                null, null, icon);
            buff.Flags(hidden: true, stayOnDeath: true);
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = true;
            buff.SetComponents
                (
                new AddContextStatBonus
                {
                    Descriptor = ModifierDescriptor.NaturalArmorEnhancement,
                    Stat = StatType.AC,
                    Multiplier = 1,
                    HasMinimal = true,
                    Minimal = 1,
                    Value = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        ValueRank = AbilityRankType.Default
                    }
                },
                Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, min: 1, max: 7, feature: effect_feature.ToRef()),
                Helper.CreateRecalculateOnFactsChange(effect_feature.ToRef2())
                );

            #endregion
            #region Ability

            var ability = Helper.CreateBlueprintAbility("FleshofWoodAbility", "Flesh of Wood",
                FleshofWoodDescription, null, icon, AbilityType.Special, UnitCommand.CommandType.Free,
                AbilityRange.Personal).TargetSelf(CastAnimationStyle.Omni);
            ability.AvailableMetamagic = Metamagic.Heighten;
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(actions: effect_buff.CreateContextActionApplyBuff(permanent: true)),
                Helper.CreateAbilityResourceLogic(resource.ToRef(), true, false, 1),
                Helper.CreateAbilityAcceptBurnOnCast(1)
                );

            #endregion

            var feature = Helper.CreateBlueprintFeature("FleshofWoodFeature", "Flesh of Wood",
                FleshofWoodDescription, null, icon, FeatureGroup.None);
            feature.IsClassFeature = true;
            feature.SetComponents
                (
                Helper.CreateAddFacts(buff.ToRef2(), ability.ToRef2()),
                // Prereqs Positive/Wood Feature, Respective Blade features
                Helper.CreatePrerequisiteFeature(Positive.BlastFeature, any: true),
                Helper.CreatePrerequisiteFeature(Positive.BladeFeature, any: true),
                Helper.CreatePrerequisiteFeature(WoodBlast.BlastFeature, any: true),
                Helper.CreatePrerequisiteFeature(WoodBlast.BladeFeature, any: true),
                Helper.CreateAddAbilityResources(false, 0, true, false, resource.ToRef())
                );

            return feature;
        }

        #endregion

        #region Positive Blast

        public static void CreatePositiveBlast()
        {
            // Variants
            // Ability
            CreatePositiveBlastAbility();
            // Feature
            CreatePositiveBlastFeature();
            // Progression
            CreatePositiveBlastProgression();
        }

        #region Positive Variants

        private static BlueprintAbility CreatePositiveBlastVariant_base()
        {
            UnityEngine.Sprite icon = Helper.StealIcon("6670f0f21a1d7f04db2b8b115e8e6abf"); // ChannelEnergyPaladinHeal

            var ability = Helper.CreateBlueprintAbility("PositiveBlastAbility", "Positive Blast",
                PositiveBlastDescription, null, icon, AbilityType.Special,
                UnitCommand.CommandType.Standard, AbilityRange.Close, duration: null, savingThrow: null);
            ability.SetComponents
                (
                Kineticist.Blast.RunActionDealDamage(out var actions, e: DamageEnergyType.PositiveEnergy, isAOE: false, half: false),
                Kineticist.Blast.RankConfigDice(twice: false, half: false),
                Kineticist.Blast.CalculateSharedValue(),
                Kineticist.Blast.RankConfigBonus(half_bonus: true),
                Kineticist.Blast.DCForceDex(),
                Kineticist.Blast.BurnCost(actions, infusion: 0, blast: 0, talent: 0),
                Kineticist.Blast.Projectile(Resource.Projectile.SunBeam00, false, AbilityProjectileType.Simple, 0, 5),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            //((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
            //((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;

            return ability;
        }

        #region Kinetic Blade: Positive

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

        public static void CreatePositiveBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.StealIcon("6670f0f21a1d7f04db2b8b115e8e6abf"); // ChannelEnergyPaladinHeal

            var ability = Helper.CreateBlueprintAbility("PositiveBlastBase", "Positive Blast",
                PositiveBlastDescription, null, icon, AbilityType.Special,
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

            Positive.BaseAbility = ability.ToRef();
        }

        public static void CreatePositiveBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("PositiveBlastFeature", "Positive Blast",
                PositiveBlastDescription, null, null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(AnyRef.Get(Positive.BaseAbility).To<BlueprintUnitFactReference>())
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;

            Positive.BlastFeature = feature.ToRef();
        }

        public static void CreatePositiveBlastProgression()
        {
            var progression = Helper.CreateBlueprintProgression("PositiveBlastProgression", "Positive Blast",
                PositiveBlastDescription, null, null, 0)
                .SetComponents
                (
                Helper.CreateAddFacts(Kineticist.ref_compositeBlastBuff),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.Get(Kineticist.ref_infusion_kineticBlade).To<BlueprintUnitFactReference>(),
                    AnyRef.Get(Positive.BladeFeature).To<BlueprintUnitFactReference>())
                );

            var entry = Helper.CreateLevelEntry(1, AnyRef.Get(Positive.BlastFeature).To<BlueprintFeatureReference>());
            Helper.AddEntries(progression, entry);

            Positive.Progession = progression.ToRef3();
        }

        #endregion

        #region Wood Blast

        public static void CreateWoodBlast()
        {
            // Variants
            // Ability
            CreateWoodBlastAbility();
            // Feature
            CreateWoodBlastFeature();
            // Progression
            CreateWoodBlastProgression();
        }

        #region Wood Variants

        #region Kinetic Blade: Wood
        #endregion

        #endregion

        public static void CreateWoodBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.StealIcon("eba7737aef48d304fb6788d748a2df69"); // WoodGolemSplintering

            var ability = Helper.CreateBlueprintAbility("WoodBlastBase", "Wood Blast",
                WoodBlastDescription, null, icon, AbilityType.Special,
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

            WoodBlast.BaseAbility = ability.ToRef();
        }

        public static void CreateWoodBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("WoodBlastFeature", "Wood Blast",
                WoodBlastDescription, null, null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(AnyRef.Get(WoodBlast.BaseAbility).To<BlueprintUnitFactReference>())
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;

            WoodBlast.BlastFeature = feature.ToRef();
        }

        public static void CreateWoodBlastProgression()
        {
            var progression = Helper.CreateBlueprintProgression("WoodBlastProgression", "Wood Blast",
                WoodBlastDescription, null, null, 0)
                .SetComponents
                (
                Helper.CreateAddFacts(Kineticist.ref_compositeBlastBuff),
                Helper.CreateAddFeatureIfHasFact(
                    AnyRef.Get(Kineticist.ref_infusion_kineticBlade).To<BlueprintUnitFactReference>(),
                    AnyRef.Get(WoodBlast.BladeFeature).To<BlueprintUnitFactReference>())
                );

            var entry = Helper.CreateLevelEntry(1, AnyRef.Get(WoodBlast.BlastFeature).To<BlueprintFeatureReference>());
            Helper.AddEntries(progression, entry);

            WoodBlast.Progession = progression.ToRef3();
        }

        #endregion

        #region Composite Blasts
        // TODO
        //  Positive Admixture
        //  Verdant
        //  Autumn
        //  Spring
        //  Summer
        //  Winter

        #region Verdant

        public static void CreateVerdantBlast()
        {
            // Variants
            // Ability
            CreateVerdantBlastAbility();
            // Feature
            CreateVerdantBlastFeature();

        }

        #region Verdant Variants

        #region Kinetic Blade: Verdant
        #endregion

        #endregion

        public static void CreateVerdantBlastAbility(params BlueprintAbility[] variants)
        {
            UnityEngine.Sprite icon = Helper.StealIcon("eba7737aef48d304fb6788d748a2df69"); // WoodGolemSplintering

            var ability = Helper.CreateBlueprintAbility("VerdantBlastBase", "Verdant Blast",
                VerdantBlastDescription, null, icon, AbilityType.Special,
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

            Verdant.BaseAbility = ability.ToRef();
        }

        public static void CreateVerdantBlastFeature()
        {
            var feature = Helper.CreateBlueprintFeature("VerdantBlastFeature", "Verdant Blast",
                VerdantBlastDescription, null, null, FeatureGroup.KineticBlast);
            feature.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(AnyRef.Get(Verdant.BaseAbility).To<BlueprintUnitFactReference>())
                );
            feature.HideInUI = true;
            feature.IsClassFeature = true;

            Verdant.BlastFeature = feature.ToRef();
        }

        #endregion

        #region Positive Variants

        #region Kinetic Blade: Positive
        #endregion

        #endregion

        #endregion

        #region Infusions
        // TODO
        //  Dazzling
        //  Photokinetic <-- New
        //  pushing
        //  entangling
        //  foxfire
        //  impale <-- Dark Codex
        //  toxic <-- New? Codex?
        //  spore <-- New?
        //  deadly earth
        //  toxic greater New+?
        #endregion

        #region Wild Talents
        // TODO
        //  Bonus Wild Talent Feats
        //  Roots
        //  Wood Healer
        //  Woodland Step
        //  Merciful foliage
        //  Brachiation
        //  thorn flesh
        //  warp wood
        //  greensight
        //  healing burst
        //  herbal antivenom
        //  plant disguise
        //  shape wood
        //  plant puppet
        //  wild growth
        //  woodland step (greater)
        //  kinetic revivification
        //  green tongue
        //  ^^ greater
        //  tree step
        //  wood soldiers
        //  forest siege
        #endregion

        #region Area Effects
        // TODO
        //  Wood
        //  Positive
        //  Verdant
        //  Autumn
        //  Spring
        //  Summer
        //  Winter
        #endregion
    }
}
