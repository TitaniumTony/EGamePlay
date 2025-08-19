using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameUtils;
using ECS;
using EGamePlay.Combat;
using System.Net;
using System;
using ECSGame;
using System.Threading;
using System.ComponentModel;
using ECSUnity;
using CombatEntity = EGamePlay.Combat.CombatEntity;

namespace EGamePlay
{
    public class SpellPreviewSystem : AComponentSystem<Game, SpellPreviewComponent>,
        IAwake<Game, SpellPreviewComponent>,
        IDestroy<Game, SpellPreviewComponent>
    {
        public void Awake(Game game, SpellPreviewComponent component)
        {

        }

        public void Destroy(Game game, SpellPreviewComponent component)
        {

        }

        public static void Update(Game game, SpellPreviewComponent component)
        {
            if (component.OwnerEntity == null)
            {
                component.OwnerEntity = game.MyActor.CombatEntity;
                return;
            }
            var abilityComp = component.OwnerEntity.GetComponent<SkillComponent>();
            if (Input.GetKeyDown(KeyCode.Q))
            {
                component.PreviewingSkill = abilityComp.InputSkills[KeyCode.Q];
                CastSkillDirectly(game, component.PreviewingSkill);
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                component.PreviewingSkill = abilityComp.InputSkills[KeyCode.W];
                CastSkillDirectly(game, component.PreviewingSkill);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                component.PreviewingSkill = abilityComp.InputSkills[KeyCode.E];
                CastSkillDirectly(game, component.PreviewingSkill);
            }
            //#if !EGAMEPLAY_EXCEL
            if (Input.GetKeyDown(KeyCode.R))
            {
                component.PreviewingSkill = abilityComp.InputSkills[KeyCode.R];
                CastSkillDirectly(game, component.PreviewingSkill);
            }
            //if (Input.GetKeyDown(KeyCode.T))
            //{
            //    component.PreviewingSkill = abilityComp.InputSkills[KeyCode.T];
            //    CastSkillDirectly(game, component.PreviewingSkill);
            //}
            if (Input.GetKeyDown(KeyCode.Y))
            {
                component.PreviewingSkill = abilityComp.InputSkills[KeyCode.Y];
                CastSkillDirectly(game, component.PreviewingSkill);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                component.PreviewingSkill = abilityComp.InputSkills[KeyCode.A];
                CastSkillDirectly(game, component.PreviewingSkill);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                component.PreviewingSkill = abilityComp.InputSkills[KeyCode.S];
                CastSkillDirectly(game, component.PreviewingSkill);
            }
            //#endif
            if (Input.GetMouseButtonDown((int)UnityEngine.UIElements.MouseButton.RightMouse))
            {
                CancelPreview(game);
            }
            if (component.Previewing)
            {
            }
        }

        public static void EnterPreview(Game game)
        {
            var component = game.GetComponent<SpellPreviewComponent>();
            CancelPreview(game);
            component.Previewing = true;
            var targetSelectType = SkillTargetSelectType.Custom;
            var affectTargetType = SkillAffectTargetType.EnemyTeam;
            var skillId = component.PreviewingSkill.Config.Id;
            if (component.PreviewingSkill.Config.TargetSelect == "手动指定") targetSelectType = SkillTargetSelectType.PlayerSelect;
            if (component.PreviewingSkill.Config.TargetSelect == "碰撞检测") targetSelectType = SkillTargetSelectType.CollisionSelect;
            if (component.PreviewingSkill.Config.TargetSelect == "条件指定") targetSelectType = SkillTargetSelectType.ConditionSelect;
            if (component.PreviewingSkill.Config.TargetGroup == "自身") affectTargetType = SkillAffectTargetType.Self;
            if (component.PreviewingSkill.Config.TargetGroup == "己方") affectTargetType = SkillAffectTargetType.SelfTeam;
            if (component.PreviewingSkill.Config.TargetGroup == "敌方") affectTargetType = SkillAffectTargetType.EnemyTeam;
            if (targetSelectType == SkillTargetSelectType.PlayerSelect)
            {
                TargetSelectManager.Instance.TargetLimitType = TargetLimitType.EnemyTeam;
                if (affectTargetType == SkillAffectTargetType.SelfTeam) TargetSelectManager.Instance.TargetLimitType = TargetLimitType.SelfTeam;
                TargetSelectManager.Instance.Show(x => OnSelectedTarget(game, x));
            }
            if (targetSelectType == SkillTargetSelectType.CollisionSelect)
            {
                if (skillId == 1004) DirectRectSelectManager.Instance.Show((a, b) => OnInputDirect(game, a, b));
                else if (skillId == 1005) DirectRectSelectManager.Instance.Show((a, b) => OnInputDirect(game, a, b));
                else if (skillId == 1008) DirectRectSelectManager.Instance.Show((a, b) => OnInputDirect(game, a, b));
                else PointSelectManager.Instance.Show(x => OnInputPoint(game, x));
            }
            if (targetSelectType == SkillTargetSelectType.ConditionSelect)
            {
                if (skillId == 1006)
                {
                    SelectTargetsWithDistance(game, component.PreviewingSkill, 5);
                }
            }
        }

        public static void CancelPreview(Game game)
        {
            var component = game.GetComponent<SpellPreviewComponent>();
            component.Previewing = false;
            TargetSelectManager.Instance?.Hide();
            PointSelectManager.Instance?.Hide();
            DirectRectSelectManager.Instance?.Hide();
        }

        private static void OnSelectedSelf(Game game)
        {
            var component = game.GetComponent<SpellPreviewComponent>();
            var combatEntity = StaticClient.Game.MyActor.CombatEntity;
            SpellSystem.SpellWithTarget(component.OwnerEntity, component.PreviewingSkill, combatEntity);
        }

        private static void OnSelectedTarget(Game game, GameObject selectObject)
        {
            var component = game.GetComponent<SpellPreviewComponent>();
            CancelPreview(game);
            var combatEntity = game.Object2Entities[selectObject]; ;
            SpellSystem.SpellWithTarget(component.OwnerEntity, component.PreviewingSkill, combatEntity);
        }

        private static void OnInputPoint(Game game, Vector3 point)
        {
            var component = game.GetComponent<SpellPreviewComponent>();
            SpellSystem.SpellWithPoint(component.OwnerEntity, component.PreviewingSkill, point);
        }

        private static void OnInputDirect(Game game, float direction, Vector3 point)
        {
            OnInputPoint(game, point);
        }

        public static void SelectTargetsWithDistance(Game game, Ability spellSkill, float distance)
        {
            var component = game.GetComponent<SpellPreviewComponent>();
            if (component.OwnerEntity.SpellAbility.TryMakeAction(out var action))
            {
                //foreach (var item in StaticClient.Context.Object2Entities.Values)
                //{
                //    if (item.IsHero)
                //    {
                //        continue;
                //    }
                //    if (Vector3.Distance(TransformSystem.GetPosition(item.Actor), TransformSystem.GetPosition(StaticClient.Game.MyActor)) < distance)
                //    {
                //        action.SkillTargets.Add(item);
                //    }
                //}

                if (action.SkillTargets.Count == 0)
                {
                    SpellActionSystem.FinishAction(action);
                    return;
                }

                action.SkillAbility = spellSkill;
                SpellActionSystem.Execute(action, false);
            }
        }

        /// <summary>
        /// 跳过预览直接释放技能：根据技能类型自动选择合适的释放方式
        /// </summary>
        private static void CastSkillDirectly(Game game, Ability skill)
        {
            if (skill == null) return;

            var component = game.GetComponent<SpellPreviewComponent>();
            var caster = component.OwnerEntity;

            if (caster == null) return;

            var targetSelectType = SkillTargetSelectType.Custom;
            var affectTargetType = SkillAffectTargetType.EnemyTeam;
            var skillId = skill.Config.Id;

            // 解析技能配置
            if (skill.Config.TargetSelect == "手动指定") targetSelectType = SkillTargetSelectType.PlayerSelect;
            if (skill.Config.TargetSelect == "碰撞检测") targetSelectType = SkillTargetSelectType.CollisionSelect;
            if (skill.Config.TargetSelect == "条件指定") targetSelectType = SkillTargetSelectType.ConditionSelect;
            if (skill.Config.TargetGroup == "自身") affectTargetType = SkillAffectTargetType.Self;
            if (skill.Config.TargetGroup == "己方") affectTargetType = SkillAffectTargetType.SelfTeam;
            if (skill.Config.TargetGroup == "敌方") affectTargetType = SkillAffectTargetType.EnemyTeam;

            // 根据技能类型直接释放
            if (affectTargetType == SkillAffectTargetType.Self)
            {
                // 自身目标技能
                SpellSystem.SpellWithTarget(caster, skill, caster);
            }
            else if (targetSelectType == SkillTargetSelectType.PlayerSelect)
            {
                // 手动指定目标技能 - 自动选择最近的敌人
                var target = FindNearestTarget(game, caster, affectTargetType);
                if (target != null)
                {
                    SpellSystem.SpellWithTarget(caster, skill, target);
                }
            }
            else if (targetSelectType == SkillTargetSelectType.CollisionSelect)
            {
                // 碰撞检测技能 - 使用玩家前方位置
                var forwardPoint = GetForwardPoint(caster);
                SpellSystem.SpellWithPoint(caster, skill, forwardPoint);
            }
            else if (targetSelectType == SkillTargetSelectType.ConditionSelect)
            {
                // 条件指定技能 - 特殊处理
                if (skillId == 1006)
                {
                    SelectTargetsWithDistance(game, skill, 5);
                }
                else
                {
                    // 默认使用最近目标
                    var target = FindNearestTarget(game, caster, affectTargetType);
                    if (target != null)
                    {
                        SpellSystem.SpellWithTarget(caster, skill, target);
                    }
                }
            }
            else
            {
                // 默认情况 - 使用最近目标
                var target = FindNearestTarget(game, caster, affectTargetType);
                if (target != null)
                {
                    SpellSystem.SpellWithTarget(caster, skill, target);
                }
            }
        }

        /// <summary>
        /// 查找最近的目标
        /// </summary>
        private static CombatEntity FindNearestTarget(Game game, CombatEntity caster, SkillAffectTargetType affectTargetType)
        {
            CombatEntity nearestTarget = null;
            float minDistance = float.MaxValue;

            foreach (var kv in game.Object2Entities)
            {
                var entity = kv.Value;
                if (entity == null || entity == caster) continue;

                // 根据目标类型过滤
                bool isValidTarget = false;
                if (affectTargetType == SkillAffectTargetType.EnemyTeam && !entity.IsHero)
                {
                    isValidTarget = true; // 敌方单位
                }
                else if (affectTargetType == SkillAffectTargetType.SelfTeam && entity.IsHero)
                {
                    isValidTarget = true; // 友方单位
                }

                if (!isValidTarget) continue;

                var distance = Vector3.Distance(caster.Position, entity.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestTarget = entity;
                }
            }

            return nearestTarget;
        }

        /// <summary>
        /// 获取施法者前方的点位
        /// </summary>
        private static Vector3 GetForwardPoint(CombatEntity caster)
        {
            var forward = caster.Rotation * Vector3.forward;
            return caster.Position + forward * 3f; // 前方3米处
        }
    }
}