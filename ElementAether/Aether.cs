using BlueprintCore.Blueprints.Configurators.Classes;
using JetBrains.Annotations;
using KineticistElementsExpanded.Components;
using KineticistElementsExpanded.Components.Properties;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
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
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.FactLogic.AddMechanicsFeature;
using static Kingmaker.UnitLogic.Mechanics.Properties.BlueprintUnitProperty;
using static Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell;

namespace KineticistElementsExpanded.ElementAether
{
    class Aether : Statics
    {
        // TODO
        // Wall Infusions need their own Area type for correct damage
        // Infusions for Force Hook/Disintegrate
        public static void Configure()
        {
            var blast_progression = CreateFullTelekineticBlast(out var blast_feature, out var tb_blade_feature);
            var force_blast_feature = CreateAetherCompositeBlasts(out var force_blade_feature);
            var aether_class_skills = CreateAetherClassSkills();
            var force_ward_feature = CreateForceWard(blast_feature);
            AddElementalDefenseIsPrereqFor(blast_feature, tb_blade_feature, force_ward_feature);
            var first_progression_aether = CreateAetherElementalFocus(blast_progression, aether_class_skills, force_ward_feature);
            var kinetic_knight_progression_aether = CreateKineticKnightAetherFocus(blast_progression, aether_class_skills, force_ward_feature);
            var second_progression_aether = CreateSecondElementAether(blast_progression, kinetic_knight_progression_aether, blast_feature, force_blast_feature);
            var third_progression_aether = CreateThirdElementAether(blast_progression, kinetic_knight_progression_aether, blast_feature, force_blast_feature, second_progression_aether);
            CreateAetherWildTalents(
                first_progression_aether, kinetic_knight_progression_aether, second_progression_aether, third_progression_aether, blast_feature,
                force_ward_feature);
        }

        private static BlueprintFeatureBase CreateAetherClassSkills()
        {
            var feature = Helper.CreateBlueprintFeature("AetherClassSkills", "Aether Class Skills",
                AetherClassSkillsDescription, null, null, 0)
                .SetComponents(
                Helper.CreateAddClassSkill(StatType.SkillThievery),
                Helper.CreateAddClassSkill(StatType.SkillKnowledgeWorld)
                );

            return feature;
        }

        #region Elemental Focus Selection

        private static BlueprintProgression CreateAetherElementalFocus(BlueprintFeatureBase blast_progression, BlueprintFeatureBase aether_class_skills, BlueprintFeatureBase force_ward_feature)
        {
            var element_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("1f3a15a3-ae8a-5524-ab8b-97f469bf4e3d"); // First Kineticist Element Selection
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9-ec1a-d924-d889-272429eb8391"); // Kineticist Base Class
            var blood_kineticist_arch = Helper.ToRef<BlueprintArchetypeReference>("365b50db-a54e-fb74-fa24-c07e9b7a838c"); // Kineticist Archetype: Blood

            var progression = Helper.CreateBlueprintProgression("ElementalFocusAether", "Aether",
                ElementalFocusAetherDescription, ElementalFocusAetherGuid, null,
                FeatureGroup.KineticElementalFocus)
                .SetComponents(Helper.CreatePrerequisiteNoArchetype(blood_kineticist_arch, kineticist_class));

            var entry1 = Helper.CreateLevelEntry(1, blast_progression, aether_class_skills);
            var entry2 = Helper.CreateLevelEntry(2, force_ward_feature);
            Helper.AddEntries(progression, entry1, entry2);

            Helper.AppendAndReplace(ref element_selection.m_AllFeatures, progression.ToRef());
            return progression;
        }

        private static BlueprintProgression CreateKineticKnightAetherFocus(BlueprintFeatureBase blast_progression, BlueprintFeatureBase aether_class_skills, BlueprintFeatureBase force_ward_feature)
        {
            var element_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("b1f296f0-bd16-bc24-2ae3-5d0638df82eb"); // First Kineticist Element Selection - Kinetic Knight
            
            var progression = Helper.CreateBlueprintProgression("KineticKnightElementalFocusAether", "Aether",
                ElementalFocusAetherDescription, null, null,
                FeatureGroup.KineticElementalFocus)
                .SetComponents(new AddEquipmentEntity { EquipmentEntity = new EquipmentEntityLink { AssetId = "aecc5905323948449b4cd3bfe36e5daf" } });

            var entry1 = Helper.CreateLevelEntry(1, blast_progression, aether_class_skills);
            var entry2 = Helper.CreateLevelEntry(4, force_ward_feature);
            Helper.AddEntries(progression, entry1, entry2);

            Helper.AppendAndReplace(ref element_selection.m_AllFeatures, progression.ToRef());
            return progression;
        }

        private static BlueprintProgression CreateSecondElementAether(BlueprintFeatureBase blast_progression, BlueprintProgression knight_progression, BlueprintFeature blast_feature, BlueprintFeature force_blast_feature) {
            var element_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("4204bc10-b3d5-db44-0b1f-52f0c375848b"); // Second Kineticist Element Selection
            var composite_blast_buff = ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("cb30a291-c75d-ef84-0904-30fbf2b5c05e"); // Kineticist CompositeBlastBuff

            var progression = Helper.CreateBlueprintProgression("SecondaryElementAether", "Aether",
                ElementalFocusAetherDescription, null, null,
                FeatureGroup.KineticElementalFocus);
            progression.HideInCharacterSheetAndLevelUp = true;
            progression.SetComponents
                (
                Helper.CreateActivateTrigger
                    (
                    Helper.CreateConditionsChecker(Operation.Or, Helper.CreateHasFact(new FactOwner(), blast_progression.ToRef2()), Helper.CreateHasFact(new FactOwner(), knight_progression.ToRef2())),
                    Helper.CreateActionList(Helper.CreateAddFact(new FactOwner(), force_blast_feature.ToRef2()))
                    ),
                Helper.CreateAddFacts(composite_blast_buff.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blast_feature.ToRef2())
                );

            var entry1 = Helper.CreateLevelEntry(1, blast_progression);
            Helper.AddEntries(progression, entry1);

            Helper.AppendAndReplace(ref element_selection.m_AllFeatures, progression.ToRef());
            return progression;
        }

        private static BlueprintProgression CreateThirdElementAether(BlueprintFeatureBase blast_progression, BlueprintProgression knight_progression, BlueprintFeature blast_feature, BlueprintFeature force_blast_feature, BlueprintProgression second_aether) {
            var element_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("e2c17188-28fc-8434-79f1-8ab4d75ded86"); // Third Kineticist Element Selection
            var composite_blast_buff = ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("cb30a291-c75d-ef84-0904-30fbf2b5c05e"); // Kineticist CompositeBlastBuff
            var progression = Helper.CreateBlueprintProgression("ThirdElementAether", "Aether",
                ElementalFocusAetherDescription, null, null,
                FeatureGroup.KineticElementalFocus);
            progression.HideInCharacterSheetAndLevelUp = true;
            progression.SetComponents
                (
                Helper.CreateActivateTrigger
                    (
                    Helper.CreateConditionsChecker(Operation.Or, Helper.CreateHasFact(new FactOwner(), blast_progression.ToRef2()), Helper.CreateHasFact(new FactOwner(), knight_progression.ToRef2())),
                    Helper.CreateActionList(Helper.CreateAddFact(new FactOwner(), force_blast_feature.ToRef2()))
                    ),
                Helper.CreateAddFacts(composite_blast_buff.ToRef()),
                Helper.CreatePrerequisiteNoFeature(second_aether.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blast_feature.ToRef2())
                );

            var entry1 = Helper.CreateLevelEntry(1, blast_progression);
            Helper.AddEntries(progression, entry1);

            Helper.AppendAndReplace(ref element_selection.m_AllFeatures, progression.ToRef());
            return progression;
        }

        #endregion

        #region Force Ward

        private static BlueprintFeature CreateForceWard(BlueprintFeature tb_feature)
        {
            var icon = Helper.StealIcon("25fc1cc0-7ea3-4f94-eb6f-bbce473767b4");
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9-ec1a-d924-d889-272429eb8391");

            #region Effect Feature
            var fw_effect_feature = Helper.CreateBlueprintFeature("ForceWardEffectFeature", "Force Ward",
                ForceWardDescription, null, null, 0);
            fw_effect_feature.Ranks = 20;
            fw_effect_feature.ReapplyOnLevelUp = true;

            var feature_value_getter = new FeatureRankPlus1Getter()
            {
                Feature = fw_effect_feature.ToRef()
            };
            var classlvl_value_getter = new ClassLevelGetter()
            {
                ClassRef = kineticist_class
            };
            var temp_hp_progression = Helper.CreateBlueprintUnitProperty("ForceWardHPProperty")
                .SetComponents
                (
                feature_value_getter,
                classlvl_value_getter
                );
            temp_hp_progression.OperationOnComponents = MathOperation.Multiply;
            temp_hp_progression.BaseValue = 1;

            var calculateShared = new ContextCalculateSharedValue
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 0.5,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.CasterCustomProperty,
                        m_CustomProperty = temp_hp_progression.ToRef()
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 0
                    }
                }
            };
            var fw_temp_hp = new TemporaryHitPointsUnique
            {
                Value = new ContextValue
                {
                    Value = 1,
                    ValueType = ContextValueType.Shared,
                    ValueShared = AbilitySharedValue.Damage
                },
                RemoveWhenHitPointsEnd = false,
                Descriptor = ModifierDescriptor.UntypedStackable
            };

            var fw_regen = new RegenTempHpPerMinute(kineticist_class, fw_effect_feature);

            fw_effect_feature.SetComponents
                (
                fw_temp_hp,
                calculateShared,
                Helper.CreateRecalculateOnFactsChange(fw_effect_feature.ToRef2()),
                fw_regen
                );
            #endregion
            #region Resource
            var fw_resource = Helper.CreateBlueprintAbilityResource("ForceWardResource", "Force Ward",
                ForceWardDescription, null, false, 20, 0, 3, 0, 0, 0, 0, false, 0, false, 0, StatType.Constitution,
                true, 0, kineticist_class, null);
            #endregion
            #region Buff
            var fw_buff = Helper.CreateBlueprintBuff("ForceWardBuff", "FW Buff",
                null, null, null, null);
            fw_buff.Flags(true, true, null, null)
                .SetComponents
                (
                Helper.CreateAddFacts(fw_effect_feature.ToRef2())
                );
            fw_buff.Stacking = StackingType.Stack;
            var fw_buff_combat_refresh = Helper.CreateBlueprintBuff("ForceWardBuffCombatRefresh", "FW Buff Refresh",
                null, null, icon, null);
            fw_buff.Flags(true, true, null, null)
                .SetComponents
                (
                Helper.CreateAddFacts(fw_effect_feature.ToRef2())
                );
            fw_buff_combat_refresh.Stacking = StackingType.Prolong;

            #endregion
            #region Ability

            var fw_ability = Helper.CreateBlueprintAbility("ForceWardAbility", "Force Ward",
                ForceWardDescription, null, icon, AbilityType.Special, UnitCommand.CommandType.Free,
                AbilityRange.Personal, null, null)
                .SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, fw_buff.CreateContextActionApplyBuff(0, DurationRate.Rounds, false, true, false, true, true)),
                Helper.CreateAbilityResourceLogic(fw_resource.ToRef(), true, false, 1),
                Helper.CreateAbilityAcceptBurnOnCast(1)
                );

            #endregion
            #region Feature
            var fw_feature = Helper.CreateBlueprintFeature("ForceWardFeature", "Force Ward",
                ForceWardDescription, null, null, 0)
                .SetComponents
                (
                Helper.CreateAddFacts(fw_effect_feature.ToRef2(), fw_ability.ToRef2()),
                Helper.CreateAddAbilityResources(false, 0, true, false, fw_resource.ToRef()),
                Helper.CreatePrerequisiteFeaturesFromList(true, tb_feature.ToRef()),
                Helper.CreateCombatStateTrigger(fw_buff_combat_refresh.CreateContextActionApplyBuff(0, DurationRate.Rounds, false, true, false, true, true))
                );

            #endregion

            return fw_feature;
        }

        #endregion

        #region TeleKinetic Blast
        private static BlueprintFeatureBase CreateFullTelekineticBlast(out BlueprintFeature blast_feature, out BlueprintFeature tb_blade_feature)
        {
            var variant_base = CreateTelekineticBlastVariant_base();
            var variant_extended = CreateTelekineticBlastVariant_extended();
            var variant_spindle = CreateTelekineticBlastVariant_spindle();
            var variant_wall = CreateTelekineticBlastVariant_wall();
            var variant_blade = CreateTelekineticBlastVariant_blade(out tb_blade_feature);
            var blast_ability = CreateTelekineticBlastAbility(variant_base, variant_extended, variant_spindle, variant_wall, variant_blade);
            blast_feature = CreateTelekineticBlastFeature(blast_ability);
            var blast_progression = CreateTelekineticBlastProgression(blast_feature, tb_blade_feature);

            AddToKineticBladeInfusion(tb_blade_feature, blast_feature);
            return blast_progression;
        }

        #region Blast Variants
        // Todo
        // Foe Throw*
        // Many Throw*
        private static BlueprintAbility CreateTelekineticBlastVariant_base()
        {
            var icon = Helper.StealIcon("0ab1552e-2ebd-acf4-4bb7-b20f5393366d");

            var blast = Helper.CreateBlueprintAbility(
                "TelekineticBlastAbility",
                "Telekinetic Blast",
                TelekineticBlastDescription,
                null,
                icon,
                AbilityType.SpellLike,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close,
                duration: null,
                savingThrow: null)
                .SetComponents
                (
                Step1_run_damage(out var actions,
                p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing,
                isAOE: false, half: false),
                Step2_rank_dice(twice: false),
                Step3_rank_bonus(half_bonus: false),
                Step4_dc(),
                Step5_burn(actions, infusion: 0, blast: 0),
                Step7_projectile(Resource.Projectile.WindProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);

            return blast;
        }

        private static BlueprintAbility CreateTelekineticBlastVariant_extended()
        {
            var requirement = Helper.ToRef<BlueprintUnitFactReference>("cb2d9e63-55dd-3394-0b2b-ef49e544b0bf");
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("cb2d9e63-55dd-3394-0b2b-ef49e544b0bf");
            var weapon = Helper.ToRef<BlueprintItemWeaponReference>("65951e11-9584-8844-b8ab-8f46d942f6e8");
            var icon = Helper.StealIcon("cb2d9e63-55dd-3394-0b2b-ef49e544b0bf");

            var blast = Helper.CreateBlueprintAbility(
                parent.name+"Telekinetic",
                parent.m_DisplayName,
                parent.m_Description,
                null,
                icon,
                AbilityType.SpellLike,
                UnitCommand.CommandType.Standard,
                AbilityRange.Long,
                duration: null,
                savingThrow: null)
                .SetComponents
                (
                Step1_run_damage(out var actions,
                p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing,
                isAOE: false, half: false),
                Step2_rank_dice(twice: false),
                Step3_rank_bonus(half_bonus: false),
                Step4_dc(),
                Step5_burn(actions, infusion: 1, blast: 0),
                Helper.CreateAbilityShowIfCasterHasFact(requirement),
                Step7_projectile(Resource.Projectile.WindProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);

            return blast;
        }

        private static BlueprintAbility CreateTelekineticBlastVariant_spindle()
        {
            var requirement = Helper.ToRef<BlueprintUnitFactReference>("c4f4a62a-325f-7c14-dbca-ce3ce34782b5");
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c4f4a62a-325f-7c14-dbca-ce3ce34782b5");
            var weapon = Helper.ToRef<BlueprintItemWeaponReference>("65951e11-9584-8844-b8ab-8f46d942f6e8");
            var icon = Helper.StealIcon("c4f4a62a-325f-7c14-dbca-ce3ce34782b5");

            var blast = Helper.CreateBlueprintAbility(
                parent.name + "Telekinetic",
                parent.m_DisplayName,
                parent.m_Description,
                null,
                icon,
                AbilityType.SpellLike,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close,
                duration: null,
                savingThrow: null)
                .SetComponents
                (
                Step1_run_damage(out var actions,
                p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing,
                isAOE: false, half: false),
                Step2_rank_dice(twice: false),
                Step3_rank_bonus(half_bonus: false),
                Step4_dc(),
                Step5_burn(actions, infusion: 2, blast: 0),
                Helper.CreateAbilityShowIfCasterHasFact(requirement),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverChain 
                { 
                    m_ProjectileFirst = Resource.Projectile.WindProjectile00.ToRef<BlueprintProjectileReference>(), 
                    m_Projectile = Resource.Projectile.WindProjectile00.ToRef<BlueprintProjectileReference>(),
                    TargetsCount = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 70,
                        ValueRank = AbilityRankType.ProjectilesCount,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    Radius = new Feet { m_Value = 5 },
                    TargetDead = false,
                    m_TargetType = TargetType.Enemy,
                    m_Condition = new ConditionsChecker { Conditions = null, Operation = Operation.And}
                }
                ).TargetEnemy(CastAnimationStyle.Kineticist);

            return blast;
        }

        private static BlueprintAbility CreateTelekineticBlastVariant_wall()
        {
            var requirement = Helper.ToRef<BlueprintUnitFactReference>("c6843359-1889-6ce4-ab13-e96cec929796");
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c6843359-1889-6ce4-ab13-e96cec929796");
            var weapon = Helper.ToRef<BlueprintItemWeaponReference>("65951e11-9584-8844-b8ab-8f46d942f6e8");
            var icon = Helper.StealIcon("c6843359-1889-6ce4-ab13-e96cec929796");
            var area_effect = ResourcesLibrary.TryGetBlueprint<BlueprintAbilityAreaEffect>("2a90aa7f-7716-77b4-e962-4fa77697fdc6");

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
                m_AreaEffect = area_effect.ToRef(),
                OnUnit = false
            };

            var blast = Helper.CreateBlueprintAbility(
                parent.name + "Telekinetic",
                parent.m_DisplayName,
                parent.m_Description,
                null,
                icon,
                AbilityType.SpellLike,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close,
                duration: null,
                savingThrow: null)
                .SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                Step4_dc(),
                Step5_burn(null, infusion: 3, blast: 0),
                Helper.CreateAbilityShowIfCasterHasFact(requirement),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            blast.CanTargetPoint = true;

            return blast;
        }

        #endregion

        #region Telekinetic Blade
        private static BlueprintAbility CreateTelekineticBlastVariant_blade(out BlueprintFeature tb_blade_feat)
        {
            var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
            var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
            var icon = Helper.StealIcon("89cc522f-2e14-44b4-0ba1-757320c58530"); // AirBlastKineticBladeDamage

            var weapon = CreateTelekineticBlastBlade_weapon();

            #region buffs
            var buff = Helper.CreateBlueprintBuff("KineticBladeTelekineticBlastBuff", null, null, null, null, null);
            buff.Flags(true, true, null, null);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistBlade { m_Blade = weapon.ToRef() }
                );
            #endregion

            #region KineticBladeTelekineticBlastAbility

            var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBladeTelekineticBlastAbility", "Telekinetic Blast — Kinetic Blade",
                KineticBladeDescription, out var unused, null, icon,
                group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
            blade_active_ability.m_Buff = buff.ToRef();
            blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
            blade_active_ability.SetComponents
                (
                new RestrictionCanUseKineticBlade { }
                );

            #endregion

            #region KineticBladeTelekineticBlastBurnAbility

            var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBladeTelekineticBlastBurnAbility", null, null, null, icon,
                AbilityType.Special, UnitCommand.CommandType.Free, AbilityRange.Personal);
            blade_burn_ability.TargetSelf(CastAnimationStyle.Omni);
            blade_burn_ability.Hidden = true;
            blade_burn_ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten;
            blade_burn_ability.SetComponents
                (
                new AbilityKineticist { Amount = 1, InfusionBurnCost = 1 },
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, kinetic_blade_enable_buff.CreateContextActionApplyBuff(asChild: true)),
                new AbilityKineticBlade { }
                );

            #endregion

            #region TelekineticBlastKineticBladeDamage

            var blade_damage_ability = Helper.CreateBlueprintAbility("TelekineticBlastKineticBladeDamage", "Telekinetic Blast",
                TelekineticBlastDescription, null, icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            blade_damage_ability.TargetEnemy(CastAnimationStyle.Omni);
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            blade_damage_ability.Hidden = true;
            blade_damage_ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(kinetic_blade_hide_feature.ToRef2()),
                new AbilityDeliveredByWeapon { },
                Step1_run_damage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing),
                Step2_rank_dice(false, false),
                Step3_rank_bonus(false),
                Step4_dc(),
                Step5_burn(actions, infusion: 1),
                Step7_projectile(Resource.Projectile.WindProjectile00, true, AbilityProjectileType.Simple, 0, 5),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                );

            #endregion

            weapon.SetComponents
                (
                new WeaponKineticBlade { m_ActivationAbility = blade_burn_ability.ToRef(), m_Blast = blade_damage_ability.ToRef()}
                );

            // Blast Burn/Blast Ability (active)
            tb_blade_feat = Helper.CreateBlueprintFeature("TelekineticKineticBladeFeature", null, null, null, icon, FeatureGroup.None);
            tb_blade_feat.HideInUI = true;
            tb_blade_feat.HideInCharacterSheetAndLevelUp = true;
            tb_blade_feat.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                );

            return blade_damage_ability;
        }

        private static BlueprintItemWeapon CreateTelekineticBlastBlade_weapon()
        {
            //var icon = Helper.StealIcon("43ff6714-3efb-86d4-f894-b10577329050"); // Air Kinetic Blade Weapon
            var kinetic_blast_physical_blade_type = Helper.ToRef<BlueprintWeaponTypeReference>("b05a206f-6c11-33a4-69b2-f7e30dc970ef"); // Kinetic Blast Physical Blade Type
            
            var weapon = Helper.CreateBlueprintItemWeapon("AetherKineticBladeWeapon", null, null, kinetic_blast_physical_blade_type,
                damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero },
                form: null,
                secondWeapon: null, false, null, 10);
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreateTelekineticBlastBlade_enchantment().ToRef() };

            weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
            weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
            weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
            weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
            weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

            // Components are later
            return weapon;
        }

        private static BlueprintWeaponEnchantment CreateTelekineticBlastBlade_enchantment()
        {
            var kinetic_blast_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("93efbde2-764b-5504-e98e-6824cab3d27c"); // Kinetic Blast Feature
            var kineticist_main_stat_property = Helper.ToRef<BlueprintUnitPropertyReference>("f897845b-bbc0-08d4-f9c1-c4a03e22357a"); // Kineticist Main Stat Property

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
            var first_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, feature: kinetic_blast_feature.ToRef(), min: 0, max: 20);
            var second_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, customProperty: kineticist_main_stat_property, min: 0, max: 20);
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

            var enchant = Helper.CreateBlueprintWeaponEnchantment("AetherKineticBladeEnchantment", "Telekinetic Blast — Kinetic Blade",
                null, "Telekinetic Blast", null, null, 0);
            enchant.SetComponents
                (
                first_context_calc,
                first_rank_conf,
                second_rank_conf,
                second_context_calc
                );
            enchant.WeaponFxPrefab = new PrefabLink { AssetId = "19d9b36b62efe1448b00630ec53db58c" };

            return enchant;
        }

        #endregion

        private static BlueprintAbility CreateTelekineticBlastAbility(params BlueprintAbility[] variants)
        {
            var icon = Helper.StealIcon("0ab1552e-2ebd-acf4-4bb7-b20f5393366d");

            var ability = Helper.CreateBlueprintAbility("TelekineticBlastBase",
                "Telekinetic Blast", TelekineticBlastDescription,
                null, icon, AbilityType.Special, UnitCommand.CommandType.Standard,
                AbilityRange.Close, duration: null, savingThrow: null);

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            return ability;
        }

        private static BlueprintFeature CreateTelekineticBlastFeature(BlueprintAbility blast_ability)
        {
            var feature = Helper.CreateBlueprintFeature("TelekineticBlastFeature",
                "Telekinetic Blast", TelekineticBlastDescription,
                null, null, FeatureGroup.KineticBlast)
                .SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blast_ability.ToRef2())
                );
            feature.HideInUI = true;
            return feature;
        }

        private static BlueprintFeatureBase CreateTelekineticBlastProgression(BlueprintFeature blast_feature, BlueprintFeature blade_feature)
        {
            var kinetic_blade_infusion = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("9ff81732-dadd-b174-aa81-38ad1297c787"); // KineticBladeInfusion
            var composite_blast_buff = ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>("cb30a291-c75d-ef84-0904-30fbf2b5c05e");

            var progression = Helper.CreateBlueprintProgression("TelekineticBlastProgression", "Telekinetic Blast",
                TelekineticBlastDescription, null, null, 0)
                .SetComponents
                (
                Helper.CreateAddFacts(composite_blast_buff.ToRef()),
                Helper.CreateAddFeatureIfHasFact(kinetic_blade_infusion.ToRef2(), blade_feature.ToRef2()),
                Helper.CreateAddFeatureIfHasFact(blast_feature.ToRef2())
                );

            var entry = Helper.CreateLevelEntry(1, blast_feature);
            Helper.AddEntries(progression, entry);

            return progression;
        }
       
        private static void AddElementalDefenseIsPrereqFor(BlueprintFeature blast_feature, BlueprintFeature tb_blade_feature, BlueprintFeature fw_feature)
        {
            blast_feature.IsPrerequisiteFor = Helper.ToArray(fw_feature).ToRef().ToList();
            tb_blade_feature.IsPrerequisiteFor = Helper.ToArray(fw_feature).ToRef().ToList();
        }

        private static void AddToKineticBladeInfusion(BlueprintFeature blade_feature, BlueprintFeature blast_feature)
        {
            var kinetic_blade_infusion = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("9ff81732-dadd-b174-aa81-38ad1297c787"); // KineticBladeInfusion
            kinetic_blade_infusion.AddComponents(Helper.CreateAddFeatureIfHasFact(blast_feature.ToRef2(), blade_feature.ToRef2()));
            
        }

        #endregion

        #region Composite Blast

        //  Disintegrate*
        //  Force Hook*
        private static BlueprintFeature CreateAetherCompositeBlasts(out BlueprintFeature force_blade_feature)
        {
            var variant_base = CreateForceBlastVariant_base();
            var variant_extended = CreateForceBlastVariant_extended();
            var variant_spindle = CreateForceBlastVariant_spindle();
            var variant_wall = CreateForceBlastVariant_wall();
            var variant_blade = CreateForceBlastVariant_blade(out force_blade_feature);
            var force_blast_ability = CreateForceBlastAbility(variant_base, variant_extended, variant_spindle, variant_wall, variant_blade);
            var force_blast_feature = CreateForceBlastFeature(force_blast_ability);

            AddToKineticBladeInfusion(force_blade_feature, force_blast_feature);
            return force_blast_feature;
        }      

        private static BlueprintAbility CreateForceBlastAbility(params BlueprintAbility[] variants)
        {
            var icon = Helper.StealIcon("3baf0164-9a92-ae64-0927-b0f633db7c11"); // SteamBlastBase

            var ability = Helper.CreateBlueprintAbility("ForceBlastBase",
                "Force Blast", ForceBlastDescription,
                null, icon, AbilityType.Special, UnitCommand.CommandType.Standard,
                AbilityRange.Close, duration: null, savingThrow: null);

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            return ability;
        }

        private static BlueprintFeature CreateForceBlastFeature(BlueprintAbility blast_ability)
        {
            var feature = Helper.CreateBlueprintFeature("ForceBlastFeature", "Force Blast",
                ForceBlastDescription, null, null, FeatureGroup.None);
            feature.HideInCharacterSheetAndLevelUp = true;
            feature.HideInUI = true;
            feature.SetComponents
                (
                // Todo
                // Add Force Blade if KineticBladeInfusion
                Helper.CreateAddFacts(blast_ability.ToRef2())
                );

            return feature;
        }

        #region composite variants

        public static BlueprintAbility CreateForceBlastVariant_base()
        {
            var icon = Helper.StealIcon("3baf0164-9a92-ae64-0927-b0f633db7c11"); // SteamBlastBase

            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, AbilityRankType.DamageDice, AbilityRankType.DamageBonus);
            var action_damage = Helper.CreateContextActionDealDamage(DamageEnergyType.Fire, dice, sharedValue: AbilitySharedValue.DurationSecond);
            var runaction = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action_damage);

            var blast = Helper.CreateBlueprintAbility(
                "ForceBlastAbility",
                "Force Blast",
                ForceBlastDescription,
                null,
                icon,
                AbilityType.SpellLike,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close,
                duration: null,
                savingThrow: null)
                .SetComponents
                (
                runaction, // Force Damage (Force with fire, same as battering blast)
                Step2_rank_dice(twice: false),
                Step3_rank_bonus(half_bonus: false),
                Step4_dc(),
                Step5_burn(null, infusion: 0, blast: 2),
                Step7_projectile(Resource.Projectile.Kinetic_SteamLine00, true, AbilityProjectileType.Simple, 0, 5),
                Step8_spell_description(SpellDescriptor.Force),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);

            return blast;
        }

        public static BlueprintAbility CreateForceBlastVariant_extended()
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("cb2d9e63-55dd-3394-0b2b-ef49e544b0bf"); // ExtendedRangeInfusion
            var icon = Helper.StealIcon("cb2d9e63-55dd-3394-0b2b-ef49e544b0bf"); // ExtendedRangeSteamBlastAbility

            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, AbilityRankType.DamageDice, AbilityRankType.DamageBonus);
            var action_damage = Helper.CreateContextActionDealDamage(DamageEnergyType.Fire, dice, sharedValue: AbilitySharedValue.DurationSecond);
            var runaction = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action_damage);

            var blast = Helper.CreateBlueprintAbility(
                "ExtendedRangeForceBlastAbility",
                "Force Blast",
                parent.Description,
                null,
                icon,
                AbilityType.SpellLike,
                UnitCommand.CommandType.Standard,
                AbilityRange.Long,
                duration: null,
                savingThrow: null)
                .SetComponents
                (
                runaction, // Force Damage (Force with fire, same as battering blast)
                Step2_rank_dice(twice: false),
                Step3_rank_bonus(half_bonus: false),
                Step4_dc(),
                Step5_burn(null, infusion: 1, blast: 2),
                Step6_feat(parent),
                Step7_projectile(Resource.Projectile.Kinetic_SteamLine00, true, AbilityProjectileType.Simple, 0, 5),
                Step8_spell_description(SpellDescriptor.Force),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            blast.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            return blast;
        }

        public static BlueprintAbility CreateForceBlastVariant_spindle()
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c4f4a62a-325f-7c14-dbca-ce3ce34782b5"); // SpindleInfusion
            var icon = Helper.StealIcon("c4f4a62a-325f-7c14-dbca-ce3ce34782b5"); // SpindleInfusion

            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, AbilityRankType.DamageDice, AbilityRankType.DamageBonus);
            var action_damage = Helper.CreateContextActionDealDamage(DamageEnergyType.Fire, dice, sharedValue: AbilitySharedValue.DurationSecond);
            var runaction = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action_damage);

            var blast = Helper.CreateBlueprintAbility(
                "ExtendedRangeForceBlastAbility",
                "Force Blast",
                parent.Description,
                null,
                icon,
                AbilityType.SpellLike,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close,
                duration: null,
                savingThrow: null)
                .SetComponents
                (
                runaction, // Force Damage (Force with fire, same as battering blast)
                Step2_rank_dice(twice: false),
                Step3_rank_bonus(half_bonus: false),
                Step4_dc(),
                Step5_burn(null, infusion: 2, blast: 2),
                Step6_feat(parent),
                Step8_spell_description(SpellDescriptor.Force),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth),
                new AbilityDeliverChain
                {
                    m_ProjectileFirst = Resource.Projectile.Kinetic_SteamLine00.ToRef<BlueprintProjectileReference>(),
                    m_Projectile = Resource.Projectile.Kinetic_SteamLine00.ToRef<BlueprintProjectileReference>(),
                    TargetsCount = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 70,
                        ValueRank = AbilityRankType.ProjectilesCount,
                        ValueShared = AbilitySharedValue.Damage
                    },
                    Radius = new Feet { m_Value = 5 },
                    TargetDead = false,
                    m_TargetType = TargetType.Enemy,
                    m_Condition = new ConditionsChecker { Conditions = null, Operation = Operation.And }
                }
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            blast.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            return blast;
        }

        public static BlueprintAbility CreateForceBlastVariant_wall()
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c6843359-1889-6ce4-ab13-e96cec929796"); // WallInfusion
            var icon = Helper.StealIcon("c6843359-1889-6ce4-ab13-e96cec929796"); // WallInfusion
            var area_effect = ResourcesLibrary.TryGetBlueprint<BlueprintAbilityAreaEffect>("6a64cc20-d582-0dc4-cb39-07b36ce6ac13"); // WallSteamBlastArea

            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, AbilityRankType.DamageDice, AbilityRankType.DamageBonus);
            var action_damage = Helper.CreateContextActionDealDamage(DamageEnergyType.Fire, dice, sharedValue: AbilitySharedValue.DurationSecond);
            var runaction = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action_damage);

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
                m_AreaEffect = area_effect.ToRef(),
                OnUnit = false
            };

            var blast = Helper.CreateBlueprintAbility(
                "ExtendedRangeForceBlastAbility",
                "Force Blast",
                parent.Description,
                null,
                icon,
                AbilityType.SpellLike,
                UnitCommand.CommandType.Standard,
                AbilityRange.Long,
                duration: null,
                savingThrow: null)
                .SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action),
                //runaction, // Force Damage (Force with fire, same as battering blast)
                //Step2_rank_dice(twice: false),
                //Step3_rank_bonus(half_bonus: false),
                Step4_dc(),
                Step5_burn(null, infusion: 1, blast: 2),
                Step6_feat(parent),
                Step8_spell_description(SpellDescriptor.Force),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            blast.CanTargetPoint = true;
            blast.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

            return blast;
        }

        #region Force Blade

        private static BlueprintAbility CreateForceBlastVariant_blade(out BlueprintFeature force_blade_feat)
        {
            var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
            var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
            var icon = Helper.StealIcon("89cc522f-2e14-44b4-0ba1-757320c58530"); // BlueFlameBlastKineticBladeDamage

            var weapon = CreateForceBlastBlade_weapon();

            #region buffs
            var buff = Helper.CreateBlueprintBuff("KineticBladeForceBlastBuff", null, null, null, null, null);
            buff.Flags(true, true, null, null);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                new AddKineticistBlade { m_Blade = weapon.ToRef() }
                );
            #endregion

            #region KineticBladeTelekineticBlastAbility

            var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBladeForceBlastAbility", "Force Blast — Kinetic Blade",
                KineticBladeDescription, out var unused, null, icon,
                group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
            blade_active_ability.m_Buff = buff.ToRef();
            blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
            blade_active_ability.SetComponents
                (
                new RestrictionCanUseKineticBlade { }
                );

            #endregion

            #region KineticBladeTelekineticBlastBurnAbility

            var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBladeForceBlastBurnAbility", null, null, null, icon,
                AbilityType.Special, UnitCommand.CommandType.Free, AbilityRange.Personal);
            blade_burn_ability.TargetSelf(CastAnimationStyle.Omni);
            blade_burn_ability.Hidden = true;
            blade_burn_ability.AvailableMetamagic = Metamagic.Extend | Metamagic.Heighten;
            blade_burn_ability.SetComponents
                (
                new AbilityKineticist { Amount = 1, InfusionBurnCost = 1, BlastBurnCost = 2 },
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, kinetic_blade_enable_buff.CreateContextActionApplyBuff(asChild: true)),
                new AbilityKineticBlade { }
                );

            #endregion

            #region TelekineticBlastKineticBladeDamage

            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, AbilityRankType.DamageDice, AbilityRankType.DamageBonus);
            var action_damage = Helper.CreateContextActionDealDamage(DamageEnergyType.Fire, dice, sharedValue: AbilitySharedValue.DurationSecond);
            var runaction = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action_damage);

            var blade_damage_ability = Helper.CreateBlueprintAbility("ForcecBlastKineticBladeDamage", "Force Blast",
                ForceBlastDescription, null, icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
            blade_damage_ability.TargetEnemy(CastAnimationStyle.Omni);
            blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            blade_damage_ability.Hidden = true;
            blade_damage_ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(kinetic_blade_hide_feature.ToRef2()),
                new AbilityDeliveredByWeapon { },
                runaction,
                Step2_rank_dice(false, false),
                Step3_rank_bonus(false),
                Step4_dc(),
                Step5_burn(null, infusion: 0, blast: 0),
                Step7_projectile(Resource.Projectile.Kinetic_SteamLine00, true, AbilityProjectileType.Simple, 0, 5),
                Step8_spell_description(SpellDescriptor.Force),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                );

            #endregion

            weapon.SetComponents
                (
                new WeaponKineticBlade { m_ActivationAbility = blade_burn_ability.ToRef(), m_Blast = blade_damage_ability.ToRef() }
                );

            force_blade_feat = Helper.CreateBlueprintFeature("ForceKineticBladeFeature", null, null, null, icon, FeatureGroup.None);
            force_blade_feat.HideInUI = true;
            force_blade_feat.HideInCharacterSheetAndLevelUp = true;
            force_blade_feat.SetComponents
                (
                Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                );

            return blade_damage_ability;
        }

        private static BlueprintItemWeapon CreateForceBlastBlade_weapon()
        {
            //var icon = Helper.StealIcon("43ff6714-3efb-86d4-f894-b10577329050"); // Air Kinetic Blade Weapon
            var kinetic_blast_energy_blade_type = Helper.ToRef<BlueprintWeaponTypeReference>("a15b2fb1-d5dc-4f24-7882-a7148d50afb0"); // Kinetic Blast Energy Blade Type

            var weapon = Helper.CreateBlueprintItemWeapon("ForceKineticBladeWeapon", null, null, kinetic_blast_energy_blade_type,
                damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero },
                form: null,
                secondWeapon: null, false, null, 10);
            weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreateForceBlastBlade_enchantment().ToRef() };

            weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
            weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
            weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
            weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
            weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

            // Components are later
            return weapon;
        }

        private static BlueprintWeaponEnchantment CreateForceBlastBlade_enchantment()
        {
            var kinetic_blast_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("93efbde2-764b-5504-e98e-6824cab3d27c"); // Kinetic Blast Feature
            var kineticist_main_stat_property = Helper.ToRef<BlueprintUnitPropertyReference>("f897845b-bbc0-08d4-f9c1-c4a03e22357a"); // Kineticist Main Stat Property

            var first_context_calc = new ContextCalculateSharedValue
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
                        ValueType = ContextValueType.Simple,
                        Value = 0,
                        ValueRank = AbilityRankType.DamageDice,
                        ValueShared = AbilitySharedValue.Damage
                    }
                }
            };
            var first_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, type: AbilityRankType.DamageDice, feature: kinetic_blast_feature.ToRef(), min: 0, max: 20, progression: ContextRankProgression.MultiplyByModifier, stepLevel: 2);
            var second_rank_conf = Helper.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, type: AbilityRankType.DamageBonus, customProperty: kineticist_main_stat_property, min: 0, max: 20, progression: ContextRankProgression.Div2);

            var enchant = Helper.CreateBlueprintWeaponEnchantment("ForceKineticBladeEnchantment", "Force Blast — Kinetic Blade",
                null, "Force Blast", null, null, 0);
            enchant.SetComponents
                (
                first_context_calc,
                first_rank_conf,
                second_rank_conf
                );
            enchant.WeaponFxPrefab = new PrefabLink { AssetId = "fafefd27475150f499b5c7275a851f2f" };

            return enchant;
        }

        #endregion

        #endregion

        // Aetheric Boost (Buff, maybe?)
        //  Provide a buff/toggle with the same scaling as blast dice as bonus damage
        #region Aetheric Boost
        #endregion

        #endregion

        #region Wild Talents

        // TODO
        // Suffocate* // Dont think I want it, just a save or die
        // Wild Talent Bonus Feats 0,1,2,3
        //  Touchsight (Uncanny Dodge)
        //  Bonus Feats:
        //  Blind Fight, Improved Blind Flight, Greater Blind Flight Disarm? Greater? combat maneuvers?
        private static void CreateAetherWildTalents(BlueprintProgression first_prog, BlueprintProgression kinetic_prog, BlueprintProgression second_prog, BlueprintProgression third_prog, BlueprintFeature tb_feature, BlueprintFeature fw_feature)
        {
            AddToKineticHealer(first_prog, second_prog, third_prog, kinetic_prog);
            AddToExpandedDefense(fw_feature);
            CreateTelekineticInvisibility(first_prog, second_prog, third_prog, kinetic_prog);
            var tf_feat = CreateTelekineticFinesse(first_prog, second_prog, third_prog, kinetic_prog);
            CreateTelekineticManeuvers(first_prog, second_prog, third_prog, kinetic_prog, tf_feat);
            CreateTouchsiteReactive(first_prog, second_prog, third_prog, kinetic_prog);
            CreateSelfTelekinesis(first_prog, second_prog, third_prog, kinetic_prog);
            CreateSpellDeflection(first_prog, second_prog, third_prog, kinetic_prog);
        }

        private static void AddToKineticHealer(BlueprintProgression first_prog, BlueprintProgression second_prog, BlueprintProgression third_prog, BlueprintProgression kinetic_prog)
        {
            var feat = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("3ef66697-3adf-a8f4-0af6-c0679bd98ba5"); // Kinetic Healer Feature
            var feat_preq = feat.GetComponent<PrerequisiteFeaturesFromList>();
            Helper.AppendAndReplace(ref feat_preq.m_Features, first_prog.ToRef(), second_prog.ToRef(), third_prog.ToRef(), kinetic_prog.ToRef());
        }
        private static void AddToExpandedDefense(BlueprintFeature fw_feature)
        {
            var selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("d741f298-dfae-8fc4-0b46-15aaf83b6548"); // Kineticist Expanded Defense Selection
            Helper.AppendAndReplace(ref selection.m_AllFeatures, fw_feature.ToRef());
        }
        private static void CreateTelekineticInvisibility(BlueprintProgression first_prog, BlueprintProgression second_prog, BlueprintProgression third_prog, BlueprintProgression kinetic_prog)
        {
            var wild_talent_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("5c883ae0-cd6d-7d54-48b7-a420f51f8459"); // Kineticist Wild Talent Selection
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9-ec1a-d924-d889-272429eb8391"); // Kineticist Base Class
            var invis_buff_icon = Helper.StealIcon("525f980c-b29b-c224-0b93-e953974cb325"); // Invisibility Effect Buff Icon
            var invis_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("525f980c-b29b-c224-0b93-e953974cb325"); // Invisibility Effect Buff

            var ti_ability = Helper.CreateBlueprintAbility("TelekineticInvisibiltyAbility", "Telekinetic Invisibility",
                TelekineticInvisibilityDescription, null, invis_buff_icon, AbilityType.Supernatural, UnitCommand.CommandType.Standard,
                AbilityRange.Personal);
            ti_ability.TargetSelf();
            ti_ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction
                    (
                    SavingThrowType.Unknown,
                    invis_buff.CreateContextActionApplyBuff(1, DurationRate.Hours, false, false, false, true)
                    ),
                Helper.CreateAbilityAcceptBurnOnCast(0)
                );

            var ti_feat = Helper.CreateBlueprintFeature("TelekineticInvisibilityFeature", "Telekinetic Invisibility",
                TelekineticInvisibilityDescription, null, invis_buff_icon, FeatureGroup.KineticWildTalent);
            ti_feat.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true, first_prog.ToRef(), second_prog.ToRef(), third_prog.ToRef(), kinetic_prog.ToRef()),
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 6),
                Helper.CreateAddFacts(ti_ability.ToRef2())
                );

            Helper.AppendAndReplace(ref wild_talent_selection.m_AllFeatures, ti_feat.ToRef());
        }
        private static BlueprintFeature CreateTelekineticFinesse(BlueprintProgression first_prog, BlueprintProgression second_prog, BlueprintProgression third_prog, BlueprintProgression kinetic_prog)
        {
            var wild_talent_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("5c883ae0-cd6d-7d54-48b7-a420f51f8459"); // Kineticist Wild Talent Selection
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9-ec1a-d924-d889-272429eb8391"); // Kineticist Base Class
            var icon = Helper.StealIcon("d6d68c99-6016-e1c4-e85e-cd0ee0067c29"); // Ranged Legerdemain

            var tf_ability = Helper.CreateBlueprintActivatableAbility("TelekineticFinesseToggleAbility", "Telekinetic Finesse",
                TelekineticFinesseDescription, out var tf_buff, null, icon, UnitCommand.CommandType.Free, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
                Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.None, false, false, false, false, false, false, false, false, false, 1);

            var mech_feature = new AddMechanicsFeature { m_Feature = MechanicsFeatureType.RangedLegerdemain };

            tf_buff.Flags(null, true, null, null);
            tf_buff.Stacking = StackingType.Replace;
            tf_buff.SetComponents
                (
                mech_feature
                );

            var tf_feat = Helper.CreateBlueprintFeature("TelekineticFinesseFeature", "Telekinetic Finesse",
                TelekineticFinesseDescription, null, icon, FeatureGroup.KineticWildTalent);
            tf_feat.HideInCharacterSheetAndLevelUp = true;
            tf_feat.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true, first_prog.ToRef(), second_prog.ToRef(), third_prog.ToRef(), kinetic_prog.ToRef()),
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 1),
                Helper.CreateAddFacts(tf_ability.ToRef2())
                );

            Helper.AppendAndReplace(ref wild_talent_selection.m_AllFeatures, tf_feat.ToRef());
            return tf_feat;
        }
        
        #region Telekinetic Maneuvers
        private static void CreateTelekineticManeuvers(BlueprintProgression first_prog, BlueprintProgression second_prog, BlueprintProgression third_prog, BlueprintProgression kinetic_prog,  BlueprintFeature tf_feat)
        {
            var variant_trip = CreateVariant_TM_Trip();
            var variant_disarm = CreateVariant_TM_Disarm();
            var variant_bullRush = CreateVariant_TM_BullRush();
            var variant_dt_blind = CreateVariant_TM_DirtyTrick_Blind(tf_feat);
            var variant_dt_entangle = CreateVariant_TM_DirtyTrick_Entangle(tf_feat);
            var variant_dt_sickened = CreateVariant_TM_DirtyTrick_Sickened(tf_feat);
            var variant_pull = CreateVariant_TM_Pull(); ;
            CreateTeleKineticManeuversFeature(first_prog, second_prog, third_prog, kinetic_prog, variant_trip, variant_disarm, variant_bullRush, variant_dt_blind, variant_dt_entangle, variant_dt_sickened, variant_pull);
        }

        #region tm_variants
        private static BlueprintAbility CreateVariant_TM_Trip()
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("6fd05c4e-cfeb-d6f4-d873-325de442fc17");
            var icon = Helper.StealIcon("6fd05c4e-cfeb-d6f4-d873-325de442fc17");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.Trip,
                IgnoreConcealment = false,
                ReplaceStat = true,
                UseKineticistMainStat = true
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversTripAction", parent.m_DisplayName,
                parent.Description, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionSpecialAttack;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                run_action
                );

            return ability;
        }
        private static BlueprintAbility CreateVariant_TM_Disarm()
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("45d94c6d-b453-cfc4-a9b9-9b72d6afe6f6");
            var icon = Helper.StealIcon("45d94c6d-b453-cfc4-a9b9-9b72d6afe6f6");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.Disarm,
                IgnoreConcealment = false,
                ReplaceStat = true,
                UseKineticistMainStat = true
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversDisarmAction", parent.m_DisplayName,
                parent.Description, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionSpecialAttack;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                run_action
                );

            return ability;
        }
        // Grapple doesn't appear to be an action in game
        private static BlueprintAbility CreateVariant_TM_Grapple()
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("6fd05c4e-cfeb-d6f4-d873-325de442fc17");
            var icon = Helper.StealIcon("6fd05c4e-cfeb-d6f4-d873-325de442fc17");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.Trip,
                IgnoreConcealment = false,
                ReplaceStat = true,
                UseKineticistMainStat = true
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversGrappleAction", parent.m_DisplayName,
                parent.Description, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionSpecialAttack;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                run_action
                );

            return ability;
        }
        private static BlueprintAbility CreateVariant_TM_BullRush()
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("7ab6f70c-996f-e9b4-597b-8332f0a3af5f");
            var icon = Helper.StealIcon("7ab6f70c-996f-e9b4-597b-8332f0a3af5f");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.BullRush,
                IgnoreConcealment = false,
                ReplaceStat = true,
                UseKineticistMainStat = true
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversBullRushAction", parent.m_DisplayName,
                parent.Description, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionSpecialAttack;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                run_action
                );

            return ability;
        }
        private static BlueprintAbility CreateVariant_TM_DirtyTrick_Blind(BlueprintFeature tf_feat)
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("8b736419-3036-a8d4-a803-08fbe16c8187");
            var icon = Helper.StealIcon("8b736419-3036-a8d4-a803-08fbe16c8187");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.DirtyTrickBlind,
                IgnoreConcealment = false,
                ReplaceStat = true,
                NewStat = StatType.Dexterity
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversDirtyTrickBlindAction", parent.m_DisplayName,
                parent.Description, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionSpecialAttack;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(tf_feat.ToRef2()),
                run_action
                );

            return ability;
        }
        private static BlueprintAbility CreateVariant_TM_DirtyTrick_Entangle(BlueprintFeature tf_feat)
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("5f22daa9-460c-5844-992b-f751e1e8eb78");
            var icon = Helper.StealIcon("5f22daa9-460c-5844-992b-f751e1e8eb78");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.DirtyTrickEntangle,
                IgnoreConcealment = false,
                ReplaceStat = true,
                NewStat = StatType.Dexterity
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversDirtyTrickEntangleAction", parent.m_DisplayName,
                parent.Description, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionSpecialAttack;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(tf_feat.ToRef2()),
                run_action
                );

            return ability;
        }
        private static BlueprintAbility CreateVariant_TM_DirtyTrick_Sickened(BlueprintFeature tf_feat)
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("4921b86e-e42c-0b54-e87a-2f9b20521ab9");
            var icon = Helper.StealIcon("4921b86e-e42c-0b54-e87a-2f9b20521ab9");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.DirtyTrickSickened,
                IgnoreConcealment = false,
                ReplaceStat = true,
                NewStat = StatType.Dexterity
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversDirtyTrickSickenedAction", parent.m_DisplayName,
                parent.Description, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionSpecialAttack;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                Helper.CreateAbilityShowIfCasterHasFact(tf_feat.ToRef2()),
                run_action
                );

            return ability;
        }
        private static BlueprintAbility CreateVariant_TM_Pull()
        {
            var parent = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("d131394d-9d8e-f384-2984-06cfa45bb7b7");
            var icon = Helper.StealIcon("d131394d-9d8e-f384-2984-06cfa45bb7b7");

            var action = new ContextActionCombatManeuver
            {
                Type = CombatManeuver.Pull,
                IgnoreConcealment = false,
                ReplaceStat = true,
                UseKineticistMainStat = true
            };
            var run_action = Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, action);

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversPullAction", "Pull",
                TelekineticManeuversPullDescription, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Long);
            ability.TargetEnemy(CastAnimationStyle.Kineticist);
            ability.SpellResistance = true;
            ability.Hidden = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionSpecialAttack;
            ability.m_TargetMapObjects = false;
            ability.AvailableMetamagic = Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;

            ability.SetComponents
                (
                run_action
                );

            return ability;
        }
        #endregion

        private static void CreateTeleKineticManeuversFeature(BlueprintProgression first_prog, BlueprintProgression second_prog, BlueprintProgression third_prog, BlueprintProgression kinetic_prog, params BlueprintAbility[] variants)
        {
            var wild_talent_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("5c883ae0-cd6d-7d54-48b7-a420f51f8459"); // Kineticist Wild Talent Selection
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9-ec1a-d924-d889-272429eb8391"); // Kineticist Base Class

            var ability = Helper.CreateBlueprintAbility("TelekineticManeuversAbility", "Telekinetic Maneuvers", 
                TelekineticManeuversDescription, null, null, AbilityType.SpellLike, UnitCommand.CommandType.Standard,
                AbilityRange.Long, null, null);

            foreach (var v in variants)
            {
                Helper.AddToAbilityVariants(ability, v);
            }

            var feature = Helper.CreateBlueprintFeature("TelekineticManeuversFeature", "Telekinetic Maneuvers",
                TelekineticManeuversDescription, null, null, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true, first_prog.ToRef(), second_prog.ToRef(), third_prog.ToRef(), kinetic_prog.ToRef()),
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 8),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            Helper.AppendAndReplace(ref wild_talent_selection.m_AllFeatures, feature.ToRef());
        }
        
        #endregion

        private static void CreateTouchsiteReactive(BlueprintProgression first_prog, BlueprintProgression second_prog, BlueprintProgression third_prog, BlueprintProgression kinetic_prog)
        {
            var wild_talent_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("5c883ae0-cd6d-7d54-48b7-a420f51f8459"); // Kineticist Wild Talent Selection
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9-ec1a-d924-d889-272429eb8391"); // Kineticist Base Class
            var icon = Helper.StealIcon("3c08d842-e802-c3e4-eb19-d15496145709"); // Uncanny Dodge

            var ignore_flatFoot = new FlatFootedIgnore
            {
                Type = FlatFootedIgnoreType.UncannyDodge
            };
            var condition = new AddCondition
            {
                Condition = UnitCondition.AttackOfOpportunityBeforeInitiative
            };

            var feature = Helper.CreateBlueprintFeature("TouchsiteReactive", "Touchsite, Reactive",
                TouchsiteReactiveDescription, null, icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true, first_prog.ToRef(), second_prog.ToRef(), third_prog.ToRef(), kinetic_prog.ToRef()),
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 10),
                ignore_flatFoot,
                condition
                );

            Helper.AppendAndReplace(ref wild_talent_selection.m_AllFeatures, feature.ToRef());
        }
        
        #region Self Telekinesis
        private static void CreateSelfTelekinesis(BlueprintProgression first_prog, BlueprintProgression second_prog, BlueprintProgression third_prog, BlueprintProgression kinetic_prog)
        {
            var st_lesser_feat = CreateSelfTelekinesisLesser(first_prog, second_prog, third_prog, kinetic_prog);
            CreateSelfTelekinesisGreater(first_prog, second_prog, third_prog, kinetic_prog, st_lesser_feat);
        }

        private static BlueprintFeature CreateSelfTelekinesisLesser(BlueprintProgression first_prog, BlueprintProgression second_prog, BlueprintProgression third_prog, BlueprintProgression kinetic_prog)
        {
            var wild_talent_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("5c883ae0-cd6d-7d54-48b7-a420f51f8459"); // Kineticist Wild Talent Selection
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9-ec1a-d924-d889-272429eb8391"); // Kineticist Base Class
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

            var buff = Helper.CreateBlueprintBuff("SelfTelekinesisBuff", "Self Telekinesis",
                SelfTelekinesisDescription, null, icon);
            buff.Flags(null, true, null, null);
            buff.Stacking = StackingType.Replace;
            buff.SetComponents
                (
                ac_bonus,
                no_difficultTerrain
                );

            var ability = Helper.CreateBlueprintAbility("SelfTelekinesisAbility", "Self Telekinesis",
                SelfTelekinesisDescription, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Move, AbilityRange.Personal);
            ability.TargetSelf(CastAnimationStyle.Kineticist);
            ability.AnimationStyle = Kingmaker.View.Animation.CastAnimationStyle.CastActionOmni;
            ability.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, buff.CreateContextActionApplyBuff(1, DurationRate.Rounds, false, false, true, false, false))
                );

            var feature = Helper.CreateBlueprintFeature("SelfTelekinesisFeature", "Self Telekinesis", 
                SelfTelekinesisDescription, null, icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true, first_prog.ToRef(), second_prog.ToRef(), third_prog.ToRef(), kinetic_prog.ToRef()),
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 6),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            Helper.AppendAndReplace(ref wild_talent_selection.m_AllFeatures, feature.ToRef());

            return feature;
        }

        private static void CreateSelfTelekinesisGreater(BlueprintProgression first_prog, BlueprintProgression second_prog, BlueprintProgression third_prog, BlueprintProgression kinetic_prog, BlueprintFeature st_lesser_feat)
        {
            var wild_talent_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("5c883ae0-cd6d-7d54-48b7-a420f51f8459"); // Kineticist Wild Talent Selection
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9-ec1a-d924-d889-272429eb8391"); // Kineticist Base Class
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

            var ability = Helper.CreateBlueprintActivatableAbility("SelfTelekinesisGreaterAbility", "Self Telekinesis, Greater",
                SelfTelekinesisGreaterDescription, out var buff, null, icon, UnitCommand.CommandType.Move, Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivationType.Immediately,
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
                m_Feature = st_lesser_feat.ToRef2()
            };
            var feature = Helper.CreateBlueprintFeature("SelfTelekinesisGreaterFeature", "Self Telekinesis, Greater",
                SelfTelekinesisGreaterDescription, null, icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeature(st_lesser_feat.ToRef()),
                Helper.CreatePrerequisiteFeaturesFromList(true, first_prog.ToRef(), second_prog.ToRef(), third_prog.ToRef(), kinetic_prog.ToRef()),
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 10),
                remove_lesser,
                Helper.CreateAddFacts(ability.ToRef2())
                );

            Helper.AppendAndReplace(ref wild_talent_selection.m_AllFeatures, feature.ToRef());

            st_lesser_feat.AddComponents(Helper.CreatePrerequisiteNoFeature(feature.ToRef()));
        }
        
        #endregion

        private static void CreateSpellDeflection(BlueprintProgression first_prog, BlueprintProgression second_prog, BlueprintProgression third_prog, BlueprintProgression kinetic_prog)
        {
            var wild_talent_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("5c883ae0-cd6d-7d54-48b7-a420f51f8459"); // Kineticist Wild Talent Selection
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9-ec1a-d924-d889-272429eb8391"); // Kineticist Base Class
            var kineticist_class_ref = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9-ec1a-d924-d889-272429eb8391"); // Kineticist Base Class
            var icon = Helper.StealIcon("50a77710-a7c4-9144-99d0-254e76a808e5"); // Spell Resistance Buff

            var add_sr = new AddSpellResistance
            {
                AddCR = false,
                AllSpellResistancePenaltyDoNotUse = false,
                Value = new ContextValue
                {
                    ValueType = ContextValueType.Shared,
                    ValueRank = AbilityRankType.Default,
                    ValueShared = AbilitySharedValue.Damage,
                    Value = 1
                }
            };

            var classlvl_value_getter = new ClassLevelGetter()
            {
                ClassRef = kineticist_class_ref
            };
            var property = Helper.CreateBlueprintUnitProperty("SpellDeflectionProperty")
                .SetComponents
                (
                classlvl_value_getter
                );
            property.BaseValue = 1;
            property.OperationOnComponents = MathOperation.Sum;

            var value = new ContextCalculateSharedValue 
            {
                ValueType = AbilitySharedValue.Damage,
                Modifier = 1.0,
                Value = new ContextDiceValue
                {
                    DiceType = DiceType.One,
                    DiceCountValue = new ContextValue
                    {
                        ValueType = ContextValueType.CasterCustomProperty,
                        m_CustomProperty = property.ToRef()
                    },
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Simple,
                        Value = 9
                    }
                }
            };

            var buff = Helper.CreateBlueprintBuff("SpellDeflectionBuff", "Spell Deflection",
                SpellDeflectionDescription, null, icon, null);
            buff.Stacking = StackingType.Replace;
            buff.Flags(null, false, null, null);
            buff.SetComponents
                (
                add_sr, value
                );

            var variant_instant = Helper.CreateBlueprintAbility("SpellDeflectionAbilityInstant", "Spell Deflection",
                SpellDeflectionDescription, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Personal);
            variant_instant.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, 
                    buff.CreateContextActionApplyBuff(1, DurationRate.Rounds, false, false, true, false, false)),
                Helper.CreateAbilityAcceptBurnOnCast(0)
                );

            var duration_value = new ContextDurationValue
            {
                Rate = DurationRate.TenMinutes,
                DiceType = DiceType.One,
                DiceCountValue = Helper.CreateContextValue(0),
                BonusValue = new ContextValue
                {
                    m_CustomProperty = property.ToRef(),
                    Value = 1,
                    ValueRank = AbilityRankType.Default,
                    ValueShared = AbilitySharedValue.Damage,
                    ValueType = ContextValueType.CasterCustomProperty
                }
            };

            var variant_prolonged = Helper.CreateBlueprintAbility("SpellDeflectionAbilityProlonged", "Spell Deflection",
                SpellDeflectionDescription, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Personal);
            variant_prolonged.SetComponents
                (
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, 
                    buff.CreateContextActionApplyBuff(duration_value, false, true, false, false)),
                Helper.CreateAbilityAcceptBurnOnCast(1)
                );

            var ability = Helper.CreateBlueprintAbility("SpellDeflectionAbility", "Spell Deflection",
                SpellDeflectionDescription, null, icon, AbilityType.SpellLike, UnitCommand.CommandType.Standard, AbilityRange.Personal);

            ability.AddToAbilityVariants(variant_instant, variant_prolonged);

            var feature = Helper.CreateBlueprintFeature("SpellDeflectionFeature", "Spell Deflection",
                SpellDeflectionDescription, null, icon, FeatureGroup.KineticWildTalent);
            feature.SetComponents
                (
                Helper.CreatePrerequisiteFeaturesFromList(true, first_prog.ToRef(), second_prog.ToRef(), third_prog.ToRef(), kinetic_prog.ToRef()),
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 14),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            Helper.AppendAndReplace(ref wild_talent_selection.m_AllFeatures, feature.ToRef());
        }
        private static void CreateSuffocationWildTalent(BlueprintProgression first_prog, BlueprintProgression second_prog, BlueprintProgression third_prog, BlueprintProgression kinetic_prog)
        {
            var wild_talent_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("5c883ae0-cd6d-7d54-48b7-a420f51f8459"); // Kineticist Wild Talent Selection
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9-ec1a-d924-d889-272429eb8391"); // Kineticist Base Class
            //var kineticist_class_ref = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9-ec1a-d924-d889-272429eb8391"); // Kineticist Base Class
            var icon = Helper.StealIcon("b3c6cb76-d5b1-1cf4-c831-4d7b1c7b9b8b"); // Choking Bomb feature
        }
        
        #endregion

        #region Helper

        /// <summary>
        /// 1) make BlueprintAbility
        /// 2) set SpellResistance
        /// 3) make components with helpers (step1 to 9)
        /// 4) set m_Parent to XBlastBase with Helper.AddToAbilityVariants
        /// Logic for dealing damage. Will make a composite blast, if both p and e are set. How much damage is dealt is defined in step 2.
        /// </summary>
        public static AbilityEffectRunAction Step1_run_damage(out ActionList actions, PhysicalDamageForm p = 0, DamageEnergyType e = (DamageEnergyType)255, SavingThrowType save = SavingThrowType.Unknown, bool isAOE = false, bool half = false)
        {
            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, AbilityRankType.DamageDice, AbilityRankType.DamageBonus);

            List<ContextAction> list = new(2);

            bool isComposite = p != 0 && e != (DamageEnergyType)255;

            if (p != 0)
                list.Add(Helper.CreateContextActionDealDamage(p, dice, isAOE, isAOE, false, half, isComposite, AbilitySharedValue.DurationSecond, writeShare: isComposite));
            if (e != (DamageEnergyType)255)
                list.Add(Helper.CreateContextActionDealDamage(e, dice, isAOE, isAOE, false, half, isComposite, AbilitySharedValue.DurationSecond, readShare: isComposite));

            var runaction = Helper.CreateAbilityEffectRunAction(save, list.ToArray());
            actions = runaction.Actions;
            return runaction;
        }

        /// <summary>
        /// Defines damage dice. Set twice for composite blasts that are pure energy or pure physical. You shouldn't need half at all.
        /// </summary>
        public static ContextRankConfig Step2_rank_dice(bool twice = false, bool half = false)
        {
            var progression = ContextRankProgression.AsIs;
            if (half) progression = ContextRankProgression.Div2;
            if (twice) progression = ContextRankProgression.MultiplyByModifier;

            var rankdice = Helper.CreateContextRankConfig(
                baseValueType: ContextRankBaseValueType.FeatureRank,
                type: AbilityRankType.DamageDice,
                progression: progression,
                stepLevel: twice ? 2 : 0,
                feature: "93efbde2764b5504e98e6824cab3d27c".ToRef<BlueprintFeatureReference>()); //KineticBlastFeature
            return rankdice;
        }

        /// <summary>
        /// Defines bonus damage. Set half_bonus for energy blasts.
        /// </summary>
        public static ContextRankConfig Step3_rank_bonus(bool half_bonus = false)
        {
            var rankdice = Helper.CreateContextRankConfig(
                baseValueType: ContextRankBaseValueType.CustomProperty,
                progression: half_bonus ? ContextRankProgression.Div2 : ContextRankProgression.AsIs,
                type: AbilityRankType.DamageBonus,
                stat: StatType.Constitution,
                customProperty: "f897845bbbc008d4f9c1c4a03e22357a".ToRef<BlueprintUnitPropertyReference>()); //KineticistMainStatProperty
            return rankdice;
        }

        /// <summary>
        /// Simply makes the DC dex based.
        /// </summary>
        public static ContextCalculateAbilityParamsBasedOnClass Step4_dc()
        {
            var dc = new ContextCalculateAbilityParamsBasedOnClass();
            dc.StatType = StatType.Dexterity;
            dc.m_CharacterClass = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391"); //KineticistClass
            return dc;
        }

        /// <summary>
        /// Creates damage tooltip from the run-action. Defines burn cost. Blast cost is 0, except for composite blasts which is 2. Talent is not used.
        /// </summary>
        public static AbilityKineticist Step5_burn(ActionList actions, int infusion = 0, int blast = 0, int talent = 0)
        {
            var comp = new AbilityKineticist();
            comp.InfusionBurnCost = infusion;
            comp.BlastBurnCost = blast;
            comp.WildTalentBurnCost = talent;

            if (actions?.Actions == null)
                return comp;

            for (int i = 0; i < actions.Actions.Length; i++)
            {
                if (actions.Actions[i] is not ContextActionDealDamage action)
                    continue;
                comp.CachedDamageInfo.Add(new AbilityKineticist.DamageInfo() { Value = action.Value, Type = action.DamageType, Half = action.Half });
            }
            return comp;
        }

        /// <summary>
        /// Required feat for this ability to show up.
        /// </summary>
        public static AbilityShowIfCasterHasFact Step6_feat(BlueprintFeature fact)
        {
            return Helper.CreateAbilityShowIfCasterHasFact(fact.ToRef2());
        }

        /// <summary>
        /// Defines projectile.
        /// </summary>
        public static AbilityDeliverProjectile Step7_projectile(string projectile_guid, bool isPhysical, AbilityProjectileType type, float length, float width)
        {
            string weapon = isPhysical ? "65951e1195848844b8ab8f46d942f6e8" : "4d3265a5b9302ee4cab9c07adddb253f"; //KineticBlastPhysicalWeapon //KineticBlastEnergyWeapon
            //KineticBlastPhysicalBlade b05a206f6c1133a469b2f7e30dc970ef
            //KineticBlastEnergyBlade a15b2fb1d5dc4f247882a7148d50afb0

            var projectile = Helper.CreateAbilityDeliverProjectile(
                projectile_guid.ToRef<BlueprintProjectileReference>(),
                type,
                weapon.ToRef<BlueprintItemWeaponReference>(),
                length.Feet(),
                width.Feet());
            return projectile;
        }

        /// <summary>
        /// Alternative projectile. Requires attack roll, if weapon is not null.
        /// </summary>
        public static AbilityDeliverChainAttack Step7b_chain_projectile(string projectile_guid, [CanBeNull] BlueprintItemWeaponReference weapon, float delay = 0f)
        {
            var result = new AbilityDeliverChainAttack();
            result.TargetsCount = Helper.CreateContextValue(AbilityRankType.DamageDice);
            result.TargetType = TargetType.Enemy;
            result.Weapon = weapon;
            result.Projectile = projectile_guid.ToRef<BlueprintProjectileReference>();
            result.DelayBetweenChain = delay;
            return result;
        }

        /// <summary>
        /// Alternative projectile. Requires attack roll, if weapon is not null.
        /// </summary>
        public static AbilityDeliverProjectile Step7c_simple_projectile(string projectile_guid, bool isPhysical)
        {
            string weapon = isPhysical ? "65951e1195848844b8ab8f46d942f6e8" : "4d3265a5b9302ee4cab9c07adddb253f"; //KineticBlastPhysicalWeapon //KineticBlastEnergyWeapon
            //KineticBlastPhysicalBlade b05a206f6c1133a469b2f7e30dc970ef
            //KineticBlastEnergyBlade a15b2fb1d5dc4f247882a7148d50afb0

            var result = new AbilityDeliverProjectile();
            result.m_Projectiles = projectile_guid.ToRef<BlueprintProjectileReference>().ObjToArray();
            result.Type = AbilityProjectileType.Simple;
            result.m_Weapon = weapon.ToRef<BlueprintItemWeaponReference>();
            result.NeedAttackRoll = true;
            return result;
        }


        /// <summary>
        /// Element descriptor for energy blasts.
        /// </summary>
        public static SpellDescriptorComponent Step8_spell_description(SpellDescriptor descriptor)
        {
            return new SpellDescriptorComponent
            {
                Descriptor = descriptor
            };
        }

        // <summary>
        // This is identical for all blasts or is missing completely. It seems to me as if it not used and a leftover.
        // </summary>
        //public static ContextCalculateSharedValue step9_shared_value()
        //{
        //    return Helper.CreateContextCalculateSharedValue();
        //}

        /// <summary>
        /// Defines sfx for casting.
        /// Use either use either OnPrecastStart or OnStart for time.
        /// </summary>
        public static AbilitySpawnFx Step_sfx(AbilitySpawnFxTime time, string sfx_guid)
        {
            var sfx = new AbilitySpawnFx();
            sfx.Time = time;
            sfx.PrefabLink = new PrefabLink() { AssetId = sfx_guid };
            return sfx;
        }

        public static BlueprintBuff ExpandSubstance(BlueprintBuff buff, BlueprintAbilityReference baseBlast)
        {
            Helper.AppendAndReplace(ref buff.GetComponent<AddKineticistInfusionDamageTrigger>().m_AbilityList, baseBlast);
            Helper.AppendAndReplace(ref buff.GetComponent<AddKineticistBurnModifier>().m_AppliableTo, baseBlast);
            return buff;
        }

        #endregion
    }
}
