using JetBrains.Annotations;
using KineticistElementsExpanded.Components;
using CodexLib;
using AnyRef = CodexLib.AnyRef;
using Helper = CodexLib.Helper;
using KineticistTree = CodexLib.KineticistTree;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic.Class.Kineticist.ActivatableAbility;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View.Animation;
using BlueprintCore.Utils;
using Kingmaker.UnitLogic.Mechanics.Conditions;

namespace KineticistElementsExpanded.KineticLib
{
    public static class Kineticist
    {
        public static BlueprintCharacterClassReference ref_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391"); // Kineticist Base Class
        public static BlueprintUnitFactReference ref_elemental_focus_selection = Helper.ToRef<BlueprintUnitFactReference>("1f3a15a3ae8a5524ab8b97f469bf4e3d"); // ElementalFocusSelection
        public static BlueprintFeatureSelection wild_talent_selection = wild_talent_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("5c883ae0cd6d7d5448b7a420f51f8459"); // WildTalentSelection
        public static BlueprintFeatureSelection infusion_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea"); // InfusionSelection
        public static BlueprintFeatureReference ref_infusion_kineticBlade = Helper.ToRef<BlueprintFeatureReference>("9ff81732daddb174aa8138ad1297c787"); // KineticBladeInfusion
        public static BlueprintFeatureReference ref_infusion_extendedRange = Helper.ToRef<BlueprintFeatureReference>("cb2d9e6355dd33940b2bef49e544b0bf"); // ExtendedRangeInfusion
        public static BlueprintFeatureReference ref_infusion_spindle = Helper.ToRef<BlueprintFeatureReference>("c4f4a62a325f7c14dbcace3ce34782b5"); // SpindleInfusion
        public static BlueprintFeatureReference ref_infusion_wall = Helper.ToRef<BlueprintFeatureReference>("c684335918896ce4ab13e96cec929796"); // WallInfusion
        public static BlueprintUnitFactReference ref_compositeBlastBuff = Helper.ToRef<BlueprintUnitFactReference>("cb30a291c75def84090430fbf2b5c05e"); // CompositeBlastBuff
        public static BlueprintUnitFactReference ref_skilled_kineticist = Helper.ToRef<BlueprintUnitFactReference>("56b70109d78b0444cb3ad04be3b1ee9e"); // SkilledKineticistBuff
        public static BlueprintUnitFactReference ref_kinetic_healer = Helper.ToRef<BlueprintUnitFactReference>("3ef666973adfa8f40af6c0679bd98ba5"); // KineticHealerFeature
        public static BlueprintUnitFactReference ref_expanded_defense = Helper.ToRef<BlueprintUnitFactReference>("d741f298dfae8fc40b4615aaf83b6548"); // ExpandedDefenseSelection
        public static BlueprintWeaponTypeReference ref_kinetic_blast_physical_blade_type = Helper.ToRef<BlueprintWeaponTypeReference>("b05a206f6c1133a469b2f7e30dc970ef"); // KineticBlastPhysicalBlade
        public static BlueprintWeaponTypeReference ref_kinetic_blast_energy_blade_type = Helper.ToRef<BlueprintWeaponTypeReference>("a15b2fb1d5dc4f247882a7148d50afb0"); // KineticBlastEnergyBlade
        public static BlueprintAbility blade_whirlwind = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("80f10dc9181a0f64f97a9f7ac9f47d65"); // BladeWhirlwindAbility
        public static BlueprintArchetypeReference ref_blood_kineticist = Helper.ToRef<BlueprintArchetypeReference>("365b50dba54efb74fa24c07e9b7a838c"); // BloodKineticistArchetype

        // This adds blasts from the provided elements to Burn, Metakinesis, KineticBladeInfusion
        public static void ElementsBlastSetup(params KineticistTree.Element[] elements)
        {
            foreach (var element in elements)
            {
                AddBlastAbilityToBurn(element.BaseAbility);
                AddBlastAbilityToMetakinesis(element.BaseAbility);
                AddToKineticBladeInfusion(element.Blade.Feature, element.BlastFeature);
            }
        }

        // This adds a blast (Typically XBlastBase) to the blasts governed by Burn. Abilities in this llist are subject to elemental overflow
        public static void AddBlastAbilityToBurn(this BlueprintAbility blast)
        {
            BlueprintFeature[] BurnFeatureList =
            {
                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("57e3577a0eb53294e9d7cc649d5239a3"), // BurnFeature
                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("2fa48527ba627254ba9bf4556330a4d4"), // PsychokineticistBurnFeature
                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("a3051f965d971ed44b9c6c63bf240b79"), // OverwhelmingSoulBurnFeature
                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("42c5a9a8661db2f47aedf87fb8b27aaf")  // DarkElementalistBurnFeature
            };

            foreach (var burnFeature in BurnFeatureList)
            {
                var addKineticistPart = burnFeature.GetComponent<AddKineticistPart>();
                Helper.AppendAndReplace(ref addKineticistPart.m_Blasts, blast.ToRef());
            }
        }

        // This adds a blast (Typically XBlastBase) to the blasts subject to MetaKinesis: Empower, Maximize, Quicken, and their cheaper counterparts
        public static void AddBlastAbilityToMetakinesis(this BlueprintAbility blast)
        {
            BlueprintBuff[] Metakinesis_buff_list = new BlueprintBuff[] {
                ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("f5f3aa17dd579ff49879923fb7bc2adb"), // MetakinesisEmpowerBuff
                ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("f690edc756b748e43bba232e0eabd004"), // MetakinesisQuickenBuff
                ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("870d7e67e97a68f439155bdf465ea191"), // MetakinesisMaximizedBuff
                ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("f8d0f7099e73c95499830ec0a93e2eeb"), // MetakinesisEmpowerCheaperBuff
                ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("c4b74e4448b81d04f9df89ed14c38a95"), // MetakinesisQuickenCheaperBuff
                ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("b8f43f0040155c74abd1bc794dbec320") // MetakinesisMaximizedCheaperBuff
            };
            foreach (var metakinesis_buff in Metakinesis_buff_list)
            {
                AddKineticistBurnModifier component = metakinesis_buff.GetComponent<AddKineticistBurnModifier>();
                Helper.AppendAndReplace(ref component.m_AppliableTo, blast.ToRef());
                AutoMetamagic auto = metakinesis_buff.GetComponent<AutoMetamagic>();
                auto.Abilities.Add(blast.ToRef());
            }
        }

        // This adds a blast as a prereq for a given Element Defense
        public static void AddElementalDefenseIsPrereqFor(BlueprintFeature blast_feature, BlueprintFeature blade_feature, BlueprintFeature ed_feature)
        {
            blast_feature.IsPrerequisiteFor = Helper.ToArray(ed_feature).ToRef().ToList();
            blade_feature.IsPrerequisiteFor = Helper.ToArray(ed_feature).ToRef().ToList();
        }

        // This adds a kinetic blade "Add Facts" to the kinetic blade infusion
        public static void AddToKineticBladeInfusion(this BlueprintFeature blade_feature, BlueprintFeature blast_feature)
        {
            ref_infusion_kineticBlade.Get().AddComponents(Helper.CreateAddFeatureIfHasFact(blast_feature.ToRef2(), blade_feature.ToRef2()));
        }

        // This adds a kinetic blade to the BladeWhirlwind ability
        public static void AddBladesToKineticWhirlwind(params KineticistTree.Element[] elements)
        {
            var hasFact = blade_whirlwind.GetComponent<AbilityCasterHasFacts>();
            // TODO FIX with push to CodexLib
            //
            //
            //
            //Helper.AppendAndReplace(ref hasFact.m_Facts, elements.Select(s => AnyRef.Get(s.Blade.Buff).To<BlueprintUnitFactReference>()).ToList());
        }

        // Trys to add a list of features (refs) to Dark Codex's Extra Wild Talent feat, if it's installed
        public static void TryDarkCodexAddExtraWildTalent(params BlueprintFeatureReference[] feat_ref)
        {
            try
            {
                var extra_wild = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("bd287f6d1c5247da9b81761cab64021c"); // DarkCodex's ExtraWildTalentFeat
                Helper.AppendAndReplace(ref extra_wild.m_AllFeatures, feat_ref);
            }
            catch (Exception ex)
            {
                Helper.PrintNotification($"Dark Codex not installed: {ex.Message}");
            }
        }

        // Adds a list of features (refs) to the Wild Talent Selection of the kineticist class
        public static void AddToWildTalents(params BlueprintFeatureReference[] feat_ref)
        {
            Helper.AppendAndReplace(ref wild_talent_selection.m_AllFeatures, feat_ref);
        }


        public static void AddElementsToInfusion(KineticistTree.Infusion infusion, params KineticistTree.Element[] elements)
        {
            AddElementsToInfusion(infusion.Feature, infusion.Buff, elements);
        }
        // Adds elements to a provided infusions feature and buff
        public static void AddElementsToInfusion(BlueprintFeature inf_feature, BlueprintBuff inf_buff, params KineticistTree.Element[] elements)
        {
            InfusionBuffAddApplicableAbilities(inf_buff, elements);
            InfusionFeatureAddPrerequisites(inf_feature, elements);
        }

        // Adds the base ability of each supplied element to an infusion buff's AppliableTo and Ability lists 
        private static void InfusionBuffAddApplicableAbilities(BlueprintBuff inf_buff, params KineticistTree.Element[] elements)
        {
            List<BlueprintAbilityReference> base_list = elements.Select(o => o.BaseAbility).ToList();
            var modifier = inf_buff.GetComponent<AddKineticistBurnModifier>();
            if (modifier != null)
                Helper.AppendAndReplace(ref modifier.m_AppliableTo, base_list);

            var trigger = inf_buff.GetComponent<AddKineticistInfusionDamageTrigger>();
            if (trigger != null)
                Helper.AppendAndReplace(ref trigger.m_AbilityList, base_list);
        }

        // Adds the blast feature of each supplied element to an infusions feature as a PrereqList
        public static void InfusionFeatureAddPrerequisites(BlueprintFeature inf_feature, params KineticistTree.Element[] elements)
        {
            List<BlueprintFeatureReference> feature_list = elements.Select(o => o.BlastFeature).ToList();
            feature_list.RemoveAll(item => item == null);
            
            var prereq_list = Helper.CreatePrerequisiteFeaturesFromList(features: feature_list.ToArray());

            if (inf_feature != null)
            {
                inf_feature.AddComponents(prereq_list);
            }
        }

        // Adds the given blast to the composite buff with criteria of given two blasts
        public static void AddCompositeToBuff(KineticistTree Tree, KineticistTree.Element composite, KineticistTree.Element param1, KineticistTree.Element param2)
        {
            var inner_checker = new ConditionsChecker
            {
                Operation = Operation.Or,
                Conditions = Helper.CreateContextConditionCasterHasFact(AnyRef.ToRef<BlueprintUnitFactReference>(param1.BlastFeature)).ObjToArray()
            };
            var inner_conditional = new Conditional
            {
                ConditionsChecker = inner_checker,
                IfFalse = null,
                IfTrue = Helper.CreateActionList(Helper.CreateContextActionAddFeature(composite.BlastFeature))
            };
            var outer_conditional = Helper.CreateConditional(Helper.CreateContextConditionHasFact(AnyRef.ToRef<BlueprintUnitFactReference>(param2.BlastFeature)),
                ifFalse: null, ifTrue: inner_conditional);

            var composite_action = Tree.CompositeBuff.Get().GetComponent<AddFactContextActions>();
            Helper.AppendAndReplace(ref composite_action.Activated.Actions, outer_conditional);

        }

        // Adds the given admixture to the composite buff
        public static void AddAdmixtureToBuff(KineticistTree Tree, KineticistTree.Infusion composite, KineticistTree.Element param1, bool basic, bool energy, bool phyisical)
        {

            var inner_checker = new ConditionsChecker
            {
                Operation = Operation.Or,
                Conditions = Tree.GetAll(basic: basic, composite: !basic, onlyEnergy: energy, onlyPhysical: phyisical, archetype: true)
                        .Select(s => Helper.CreateContextConditionHasFact(
                            AnyRef.ToRef<BlueprintUnitFactReference>(s.BlastFeature))).ToArray()
            };
            var inner_conditional = new Conditional
            {
                ConditionsChecker = inner_checker,
                IfFalse = null,
                IfTrue = Helper.CreateActionList(Helper.CreateContextActionAddFeature(composite.Feature))
            };
            var outer_conditional = Helper.CreateConditional(Helper.CreateContextConditionHasFact(AnyRef.ToRef<BlueprintUnitFactReference>(param1.BlastFeature)),
                ifFalse: null, ifTrue: inner_conditional);

            var composite_action = Tree.CompositeBuff.Get().GetComponent<AddFactContextActions>();
            Helper.AppendAndReplace(ref composite_action.Activated.Actions, outer_conditional);
        }


        #region Blast Components

        public static class Blast
        {
            /// <summary>
            /// 1) make BlueprintAbility
            /// 2) set SpellResistance
            /// 3) make components with helpers (step1 to 9)
            /// 4) set m_Parent to XBlastBase with Helper.AddToAbilityVariants
            /// Logic for dealing damage. Will make a composite blast, if both p and e are set. How much damage is dealt is defined in step 2.
            /// </summary>
            public static AbilityEffectRunAction RunActionDealDamage(out ActionList actions, PhysicalDamageForm p = 0, DamageEnergyType e = (DamageEnergyType)255, SavingThrowType save = SavingThrowType.Unknown, bool isAOE = false, bool half = false)
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
            public static ContextRankConfig RankConfigDice(bool twice = false, bool half = false)
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
            public static ContextRankConfig RankConfigBonus(bool half_bonus = false)
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
            public static ContextCalculateAbilityParamsBasedOnClass DCForceDex()
            {
                var dc = new ContextCalculateAbilityParamsBasedOnClass
                {
                    StatType = StatType.Dexterity,
                    m_CharacterClass = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391") //KineticistClass
                };
                return dc;
            }

            /// <summary>
            /// Creates damage tooltip from the run-action. Defines burn cost. Blast cost is 0, except for composite blasts which is 2. Talent is not used.
            /// </summary>
            public static AbilityKineticist BurnCost(ActionList actions, int infusion = 0, int blast = 0, int talent = 0)
            {
                var comp = new AbilityKineticist
                {
                    InfusionBurnCost = infusion,
                    BlastBurnCost = blast,
                    WildTalentBurnCost = talent
                };

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
            public static AbilityShowIfCasterHasFact RequiredFeat(BlueprintFeature fact)
            {
                return Helper.CreateAbilityShowIfCasterHasFact(fact.ToRef2());
            }

            /// <summary>
            /// Defines projectile.
            /// </summary>
            public static AbilityDeliverProjectile Projectile(string projectile_guid, bool isPhysical, AbilityProjectileType type, float length, float width)
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
                projectile.Type = type;
                return projectile;
            }

            /// <summary>
            /// Element descriptor for energy blasts.
            /// </summary>
            public static SpellDescriptorComponent SpellDescriptor(SpellDescriptor descriptor)
            {
                return new SpellDescriptorComponent
                {
                    Descriptor = descriptor
                };
            }

            // <summary>
            // This is identical for all blasts or is missing completely. It seems to me as if it not used and a leftover.
            // </summary>
            public static ContextCalculateSharedValue CalculateSharedValue(double modifier = 1.0, AbilitySharedValue type = AbilitySharedValue.Damage, AbilityRankType typeDice = AbilityRankType.DamageDice, AbilityRankType typeBonus = AbilityRankType.DamageBonus, DiceType dice = DiceType.One)
            {
                var result = Helper.CreateContextCalculateSharedValue(Modifier: modifier, Value: Helper.CreateContextDiceValue(dice, typeDice, typeBonus), ValueType: type);
                return result;
            }

            /// <summary>
            /// Defines sfx for casting.
            /// Use either use either OnPrecastStart or OnStart for time.
            /// </summary>
            public static AbilitySpawnFx Sfx(AbilitySpawnFxTime time, string sfx_guid)
            {
                var sfx = new AbilitySpawnFx
                {
                    Time = time,
                    PrefabLink = new PrefabLink() { AssetId = sfx_guid }
                };
                return sfx;
            }

            public static BlueprintBuff ExpandSubstance(BlueprintBuff buff, BlueprintAbilityReference baseBlast)
            {
                Helper.AppendAndReplace(ref buff.GetComponent<AddKineticistInfusionDamageTrigger>().m_AbilityList, baseBlast);
                Helper.AppendAndReplace(ref buff.GetComponent<AddKineticistBurnModifier>().m_AppliableTo, baseBlast);
                return buff;
            }

        }

        #endregion

        #region Blade Construction
        
        public static class Blade
        {
            public static BlueprintAbility CreateKineticBlade(KineticistTree tree, string element, string prefix, bool isComposite, string prefabAssetId, string projectileUID, UnityEngine.Sprite icon, UnityEngine.Sprite damage_icon, PhysicalDamageForm p = (PhysicalDamageForm)0, DamageEnergyType e = (DamageEnergyType)255, AbilityEffectRunAction damageTypeOverride = null)
            {
                var kinetic_blade_enable_buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("426a9c07-9ee7-ac34-aa8e-0054f2218074"); // KineticBladeEnableBuff
                var kinetic_blade_hide_feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4d39ccef-7b5b-2e94-58e8-599eae3c3be0"); // KineticBladeHideFeature
                //var icon = Helper.StealIcon("89acea31-3b9a-9cb4-d86b-bbca01b90346"); // KineticBladeAirBlastAbility
                //var damage_icon = Helper.StealIcon("89cc522f-2e14-44b4-0ba1-757320c58530"); // AirBlastKineticBladeDamage

                bool isPhysical = p != (PhysicalDamageForm)0;
                bool overrideAction = damageTypeOverride != null;

                var weapon = CreateBlueprintItemWeapon(tree, element, prefix, isPhysical, isComposite, prefabAssetId);

                #region BlastAbility

                var blade_active_ability = Helper.CreateBlueprintActivatableAbility("KineticBlade"+element+"BlastAbility", out var buff, LocalizationTool.GetString(element+".Blade.Enchant"),
                    LocalizationTool.GetString("Blade.Description"), icon, group: Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityGroup.FormInfusion, deactivateWhenDead: true);
                blade_active_ability.m_ActivateOnUnitAction = Kingmaker.UnitLogic.ActivatableAbilities.AbilityActivateOnUnitActionType.Attack;
                blade_active_ability.SetComponents
                    (
                    new RestrictionCanUseKineticBlade { }
                    );

                #endregion

                #region buffs
                buff.Flags(true, true, null, null);
                buff.Stacking = StackingType.Replace;
                buff.SetComponents
                    (
                    new AddKineticistBlade { m_Blade = AnyRef.ToAny(weapon) }
                    );
                #endregion

                #region BlastBurnAbility

                var blade_burn_ability = Helper.CreateBlueprintAbility("KineticBlade"+element+"BlastBurnAbility", null, null, icon,
                    AbilityType.Special, UnitCommand.CommandType.Free, AbilityRange.Personal);
                blade_burn_ability.TargetSelf();
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

                ActionList actions = null;

                var blade_damage_ability = Helper.CreateBlueprintAbility(element+"BlastKineticBladeDamage", LocalizationTool.GetString(element + ".Blade.Prefix"),
                    LocalizationTool.GetString("Blade.Description"), damage_icon, AbilityType.Special, UnitCommand.CommandType.Standard, AbilityRange.Close);
                blade_damage_ability.TargetEnemy();
                blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
                blade_damage_ability.Hidden = true;
                blade_damage_ability.SetComponents
                    (
                    Helper.CreateAbilityShowIfCasterHasFact(kinetic_blade_hide_feature.ToRef2()),
                    new AbilityDeliveredByWeapon { },
                    overrideAction ? damageTypeOverride :
                        Kineticist.Blast.RunActionDealDamage(out actions, p, e, isAOE: false, half: false),
                    Kineticist.Blast.RankConfigDice(twice: isComposite && (p == (PhysicalDamageForm)0 || e == (DamageEnergyType)255), half: false),
                    Kineticist.Blast.RankConfigBonus(half_bonus: !isPhysical),
                    Kineticist.Blast.DCForceDex(),
                    Kineticist.Blast.BurnCost(actions, infusion: 1),
                    Kineticist.Blast.Projectile(projectileUID, isPhysical, AbilityProjectileType.Simple, 0, 5),
                    Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                    Kineticist.Blast.Sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                    );
                blade_damage_ability.AvailableMetamagic = Metamagic.Empower | Metamagic.Maximize | Metamagic.Quicken | Metamagic.Heighten;

                #endregion

                weapon.Get().SetComponents
                    (
                    new WeaponKineticBlade { m_ActivationAbility = blade_burn_ability.ToRef(), m_Blast = blade_damage_ability.ToRef() }
                    );

                var blade_feat = Helper.CreateBlueprintFeature(element+"KineticBladeFeature", null, null, icon, FeatureGroup.None);
                blade_feat.HideInUI = true;
                blade_feat.HideInCharacterSheetAndLevelUp = true;
                blade_feat.SetComponents
                    (
                    Helper.CreateAddFeatureIfHasFact(blade_active_ability.ToRef()),
                    Helper.CreateAddFeatureIfHasFact(blade_burn_ability.ToRef2())
                    );

                return blade_damage_ability;
            }

            public static BlueprintItemWeaponReference CreateBlueprintItemWeapon(KineticistTree tree, string element, string prefix, bool isPhysical, bool isComposite, string prefabAssetId)
            {
                var weapon = Helper.CreateBlueprintItemWeapon(element+"KineticBladeWeapon", LocalizationTool.GetString(element+".Blade.Prefix"), LocalizationTool.GetString("Blade.Description"), isPhysical? Kineticist.ref_kinetic_blast_physical_blade_type: Kineticist.ref_kinetic_blast_energy_blade_type,
                    damageOverride: new DiceFormula { m_Rolls = 0, m_Dice = DiceType.Zero }, price: 10);
                weapon.m_Enchantments = new BlueprintWeaponEnchantmentReference[1] { CreateBlueprintWeaponEnchantment(tree, element, prefix, prefabAssetId, isPhysical, isComposite) };
                weapon.m_EquipmentEntity = AnyRef.ToAny("");
                weapon.m_EquipmentEntityAlternatives = new KingmakerEquipmentEntityReference[0] { };
                weapon.m_VisualParameters.m_WeaponAnimationStyle = Kingmaker.View.Animation.WeaponAnimationStyle.SlashingOneHanded;
                weapon.m_VisualParameters.m_SpecialAnimation = Kingmaker.Visual.Animation.Kingmaker.UnitAnimationSpecialAttackType.None;
                weapon.m_VisualParameters.m_WeaponModel = new PrefabLink { AssetId = "7c05296dbc70bf6479e66df7d9719d1e" };
                weapon.m_VisualParameters.m_WeaponBeltModelOverride = null;
                weapon.m_VisualParameters.m_WeaponSheathModelOverride = new PrefabLink { AssetId = "f777a23c850d099428c33807f83cd3d6" };

                // Components are done later, in calling function
                return AnyRef.ToAny(weapon);
            }

            public static BlueprintWeaponEnchantmentReference CreateBlueprintWeaponEnchantment(KineticistTree tree, string element, string prefix, string prefabAssetId, bool isPhysical, bool isComposite)
            {
                var first_context_calc = new ContextCalculateSharedValue
                {
                    ValueType = AbilitySharedValue.Damage,
                    Modifier = 1.0,
                    Value = new ContextDiceValue
                    {
                        DiceType = DiceType.One,
                        DiceCountValue = new ContextValue
                        {
                            ValueType = ContextValueType.Simple,
                            Value = 0,
                        },
                        BonusValue = new ContextValue
                        {
                            ValueType = isPhysical ? ContextValueType.Rank : ContextValueType.Simple,
                            Value = 0,
                            ValueRank = isComposite ? AbilityRankType.DamageBonus : AbilityRankType.DamageDice,
                        }
                    }
                };
                var first_rank_conf = Helper.CreateContextRankConfig(
                    ContextRankBaseValueType.FeatureRank,
                    progression: isComposite ? ContextRankProgression.MultiplyByModifier : ContextRankProgression.AsIs,
                    stepLevel: 2,
                    type: AbilityRankType.DamageDice, 
                    feature: tree.KineticBlast, min: 0, max: 20);
                var second_rank_conf = Helper.CreateContextRankConfig(
                    ContextRankBaseValueType.CustomProperty, 
                    progression: isPhysical ? ContextRankProgression.AsIs : ContextRankProgression.Div2,
                    type: AbilityRankType.DamageBonus, 
                    customProperty: tree.KineticistMainStatProperty, 
                    min: 0, max: 20);
                var second_context_calc = new ContextCalculateSharedValue
                {
                    ValueType = AbilitySharedValue.DamageBonus,
                    Modifier = 1.0,
                    Value = new ContextDiceValue
                    {
                        DiceType = DiceType.Zero,
                        DiceCountValue = new ContextValue
                        {
                            ValueType = ContextValueType.Rank,
                            ValueRank = AbilityRankType.DamageDice
                        },
                        BonusValue = new ContextValue
                        {
                            ValueType = ContextValueType.Rank,
                            ValueRank = AbilityRankType.DamageBonus
                        }
                    }
                };

                var enchant = Helper.CreateBlueprintWeaponEnchantment(element+"KineticBladeEnchantment", LocalizationTool.GetString(element+".Blade.Enchant"));
                enchant.SetComponents
                    (
                    first_context_calc,
                    first_rank_conf,
                    second_rank_conf
                    );
                if (!isComposite) enchant.AddComponents(second_context_calc);
                enchant.WeaponFxPrefab = new PrefabLink { AssetId = prefabAssetId };

                return AnyRef.ToAny(enchant);
            }
        }

        #endregion

        public static BlueprintAbilityAreaEffectReference CreateWallAreaEffect(string name, string fx_id, PhysicalDamageForm p = (PhysicalDamageForm)0, DamageEnergyType e = (DamageEnergyType)255, bool twice = false)
        {
            var wall_infusion = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c684335918896ce4ab13e96cec929796"); // WallInfusion
            var unique = new UniqueAreaEffect { m_Feature = wall_infusion.ToRef2() };
            var prefab = new PrefabLink { AssetId = fx_id };

            bool isComposite = p != (PhysicalDamageForm)0 && e != (DamageEnergyType)255;

            Blast.RunActionDealDamage(out var actions, p: p, e: e);

            if (p != (PhysicalDamageForm)0 )
            {
                ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueType = ContextValueType.Shared;
                ((ContextActionDealDamage)actions.Actions[0]).Value.BonusValue.ValueShared = AbilitySharedValue.Damage;
            }

            var area_effect = Helper.CreateBlueprintAbilityAreaEffect("Wall"+name+"BlastArea", true, true,
                AreaEffectShape.Wall, new Feet { m_Value = 60 },
                prefab, unitEnter: actions);
            area_effect.m_Tags = AreaEffectTags.DestroyableInCutscene;
            area_effect.IgnoreSleepingUnits = false;
            area_effect.AffectDead = false;
            area_effect.AggroEnemies = true;
            area_effect.AffectEnemies = true;
            area_effect.SpellResistance = !isComposite && e != (DamageEnergyType)255;

            area_effect.AddComponents
                (
                unique,
                Blast.RankConfigDice(twice: twice, half: false),
                Blast.RankConfigBonus(half_bonus: !isComposite && e != (DamageEnergyType)255),
                Blast.CalculateSharedValue(),
                Blast.DCForceDex()
                );

            return AnyRef.ToAny(area_effect);
        }
    }
}
