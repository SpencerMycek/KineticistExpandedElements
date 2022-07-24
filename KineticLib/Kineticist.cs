using JetBrains.Annotations;
using KineticistElementsExpanded.Components;
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

        // This adds blasts from the provided elements to Burn, Metakinesis, KineticBladeInfusion
        public static void ElementsBlastSetup(params KineticistTree.Element[] elements)
        {
            foreach (var element in elements)
            {
                AddBlastAbilityToBurn(element.BaseAbility);
                AddBlastAbilityToMetakinesis(element.BaseAbility);
                AddToKineticBladeInfusion(element.BladeFeature, element.BlastFeature);
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

            Helper.AppendAndReplace(ref hasFact.m_Facts, elements.Select(s => AnyRef.Get(s.BladeBuff).To<BlueprintUnitFactReference>()).ToList());
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
                Helper.Print($"Dark Codex not installed: {ex.Message}");
            }
        }

        // Adds a list of features (refs) to the Wild Talent Selection of the kineticist class
        public static void AddToWildTalents(params BlueprintFeatureReference[] feat_ref)
        {
            Helper.AppendAndReplace(ref wild_talent_selection.m_AllFeatures, feat_ref);
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
            var prereq_list = Helper.CreatePrerequisiteFeaturesFromList(features: feature_list.ToArray());

            inf_feature.AddComponents(prereq_list);
        }

        // Adds the given blast to the composite buff with criteria of given two blasts
        public static void AddCompositeToBuff(KineticistTree Tree, KineticistTree.Element composite, KineticistTree.Element param1, KineticistTree.Element param2)
        {
            var inner_checker = new ConditionsChecker
            {
                Operation = Operation.Or,
                Conditions = Helper.CreateContextConditionCasterHasFact(AnyRef.Get(param1.BlastFeature).To<BlueprintUnitFactReference>()).ObjToArray()
            };
            var inner_conditional = new Conditional
            {
                ConditionsChecker = inner_checker,
                IfFalse = null,
                IfTrue = Helper.CreateActionList(Helper.CreateContextActionAddFeature(composite.BlastFeature))
            };
            var outer_conditional = Helper.CreateConditional(Helper.CreateContextConditionHasFact(AnyRef.Get(param2.BlastFeature).To<BlueprintUnitFactReference>()),
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
                Conditions = Tree.GetAll(basic: basic, basicEnergy: energy, basicPhysical: phyisical)
                        .Select(s => Helper.CreateContextConditionHasFact(
                            AnyRef.Get(s.BlastFeature).To<BlueprintUnitFactReference>())).ToArray()
            };
            var inner_conditional = new Conditional
            {
                ConditionsChecker = inner_checker,
                IfFalse = null,
                IfTrue = Helper.CreateActionList(Helper.CreateContextActionAddFeature(composite.InfusionFeature))
            };
            var outer_conditional = Helper.CreateConditional(Helper.CreateContextConditionHasFact(AnyRef.Get(param1.BlastFeature).To<BlueprintUnitFactReference>()),
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
                return projectile;
            }

            /// <summary>
            /// Alternative projectile. Requires attack roll, if weapon is not null.
            /// </summary>
            public static AbilityDeliverChainAttack ChainProjectile(string projectile_guid, [CanBeNull] BlueprintItemWeaponReference weapon, float delay = 0f)
            {
                var result = new AbilityDeliverChainAttack
                {
                    TargetsCount = Helper.CreateContextValue(AbilityRankType.DamageDice),
                    TargetType = TargetType.Enemy,
                    Weapon = weapon,
                    Projectile = projectile_guid.ToRef<BlueprintProjectileReference>(),
                    DelayBetweenChain = delay
                };
                return result;
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

            var area_effect = Helper.CreateBlueprintAbilityAreaEffect("Wall"+name+"BlastArea", null, true, true,
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

            return area_effect.ToRef();
        }
    }
}
