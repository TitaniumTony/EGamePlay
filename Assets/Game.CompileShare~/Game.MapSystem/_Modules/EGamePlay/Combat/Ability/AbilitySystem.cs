using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameUtils;
using ECS;
using EGamePlay.Combat;
using System;
using ET;
using ECSGame;

namespace EGamePlay
{
    public class AbilitySystem : AEntitySystem<Ability>,
        IAwake<Ability>,
        IEnable<Ability>,
        IDisable<Ability>,
        IUpdate<Ability>
    {
        public void Awake(Ability entity)
        {
            //ConfigObject = initData as AbilityConfigObject;
            entity.Config = ConfigHelper.Get<AbilityConfig>(entity.EcsNode, entity.ConfigObject.Id);

            if (entity.Config.TargetGroup == "己方")
            {
                entity.ConfigObject.AffectTargetType = SkillAffectTargetType.SelfTeam;
            }
            else if (entity.Config.TargetGroup == "敌方")
            {
                entity.ConfigObject.AffectTargetType = SkillAffectTargetType.EnemyTeam;
            }
            else if (entity.Config.TargetGroup == "自身")
            {
                entity.ConfigObject.AffectTargetType = SkillAffectTargetType.Self;
            }
            else
            {
                Log.Error($"技能目标阵营错误 {entity.Config.TargetGroup}");
            }

            if (entity.IsSkill)
            {
                if (entity.Config.TargetSelect == "碰撞检测")
                {
                    entity.ConfigObject.TargetSelectType = SkillTargetSelectType.CollisionSelect;
                }
                else if (entity.Config.TargetSelect == "条件指定")
                {
                    entity.ConfigObject.TargetSelectType = SkillTargetSelectType.ConditionSelect;
                }
                else if (entity.Config.TargetSelect == "手动指定")
                {
                    entity.ConfigObject.TargetSelectType = SkillTargetSelectType.PlayerSelect;
                }
                else
                {
                    Log.Error($"目标选取类型错误 {entity.Config.TargetSelect}");
                }
            }

            entity.Name = entity.Config.Name;

            foreach (var item in entity.ConfigObject.Effects)
            {
                var abilityEffect = entity.AddChild<AbilityEffect>(x => x.EffectConfig = item);
                AddEffect(entity, abilityEffect);

                if (abilityEffect.EffectConfig is DamageEffect)
                {
                    entity.DamageAbilityEffect = abilityEffect;
                }
                if (abilityEffect.EffectConfig is CureEffect)
                {
                    entity.CureAbilityEffect = abilityEffect;
                }
            }

            foreach (var item in entity.ConfigObject.TriggerActions)
            {
                var abilityTrigger = entity.AddChild<AbilityTrigger>(x => x.TriggerConfig = item);
                entity.AbilityTriggers.Add(abilityTrigger);
            }

            LoadExecution(entity);
        }

        public void Enable(Ability entity)
        {
            foreach (var item in entity.AbilityEffects)
            {
                item.Enable = true;
            }
            foreach (var item in entity.AbilityTriggers)
            {
                item.Enable = true;
            }
        }

        public void Disable(Ability entity)
        {
            foreach (var item in entity.AbilityEffects)
            {
                item.Enable = false;
            }
            foreach (var item in entity.AbilityTriggers)
            {
                item.Enable = false;
            }
        }

        public void Update(Ability entity)
        {

        }

        public static void AddEffect(Ability entity, AbilityEffect abilityEffect)
        {
            entity.AbilityEffects.Add(abilityEffect);
        }

        public static AbilityEffect GetEffect(Ability entity, int index = 0)
        {
            return entity.AbilityEffects[index];
        }

        public static AbilityTrigger GetTrigger(Ability entity, int index = 0)
        {
            return entity.AbilityTriggers[index];
        }

        /// <summary>
        /// 挂载能力，技能、被动、buff等都通过这个接口挂载
        /// </summary>
        public static Ability AttachAbility(CombatEntity entity, object configObject)
        {
            var component = entity.GetComponent<AbilityComponent>();
            var ability = entity.AddChild<Ability>(x => x.ConfigObject = configObject as AbilityConfigObject);
            ability.AddComponent<AbilityLevelComponent>();
            component.IdAbilities.Add(ability.Id, ability);
            return ability;
        }

        public static void RemoveAbility(CombatEntity entity, Ability ability)
        {
            var component = entity.GetComponent<AbilityComponent>();
            component.IdAbilities.Remove(ability.Id);
            EndAbility(ability);
        }

        public static void LoadExecution(Ability entity)
        {
            entity.ExecutionObject = AssetUtils.LoadObject<ExecutionObject>($"{AbilityManagerObject.ExecutionResFolder}/Execution_{entity.ConfigObject.Id}");
            if (entity.ExecutionObject == null)
            {
                return;
            }
        }

        public static void TryActivateAbility(Ability entity)
        {
            ActivateAbility(entity);
        }

        public static void ActivateAbility(Ability entity)
        {
            entity.Enable = true;
            //entity.GetComponent<AbilityEffectComponent>().Enable = true;
            //entity.GetComponent<AbilityTriggerComponent>().Enable = true;
        }

        public static void DeactivateAbility(Ability entity)
        {
            entity.Enable = false;
            //entity.GetComponent<AbilityEffectComponent>().Enable = false;
            //entity.GetComponent<AbilityTriggerComponent>().Enable = false;
        }

        public static void EndAbility(Ability entity)
        {
            DeactivateAbility(entity);
            EcsObject.Destroy(entity);
        }
    }
}