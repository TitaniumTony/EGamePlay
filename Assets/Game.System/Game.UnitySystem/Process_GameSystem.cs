using ECS;
using ECSUnity;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using EGamePlay.Combat;
using EGamePlay;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

namespace ECSGame
{
    public class Process_GameSystem
    {
        public static void RpgInit(EcsNode ecsNode, Assembly assembly)
        {
            ConsoleLog.Debug($"Process_GameSystem Init");

            var allTypes = assembly.GetTypes();
            var typeList = new List<Type>();
            typeList.AddRange(allTypes);

            ecsNode.AddSystems(typeList.ToArray());

            EcsNodeSystem.Create(ecsNode);
            ecsNode.AddComponent<EntityObjectComponent>();
            ecsNode.AddComponent<ReloadComponent>(x => x.SystemAssembly = assembly);
            ecsNode.AddComponent<ConfigManageComponent>(x => x.ConfigsCollector = StaticClient.ConfigsCollector);
            ecsNode.Init();

            var game = GameSystem.Create(ecsNode);
            game.AddComponent<PlayerInputComponent>();
            game.AddComponent<SpellPreviewComponent>();
            game.Init();

            StaticClient.Game = game;

            var canvasTrans = GameObject.Find("Hero").transform.Find("Canvas");
            var healthImage = canvasTrans.Find("Image").GetComponent<Image>();
            var actor = ActorSystem.CreateHero(game, ecsNode.NewInstanceId());
            actor.AddComponent<ModelViewComponent>(x => x.ModelTrans = GameObject.Find("Hero").transform);
            actor.AddComponent<AnimationComponent>();
            actor.AddComponent<HealthViewComponent>(x =>
            {
                x.CanvasTrans = canvasTrans;
                x.HealthBarImage = healthImage;
            });
            actor.GetComponent<CollisionComponent>().Layer = 1;
            actor.AddComponent<ActorCombatComponent>();
            actor.CombatEntity.IsHero = true;
            actor.Init();
            game.MyActor = actor;
            game.Object2Entities.Add(GameObject.Find("Hero"), actor.CombatEntity);
            StaticClient.Hero = actor.CombatEntity;

            var enemiesTrans = GameObject.Find("Enemies").transform;
            for (int i = 0; i < enemiesTrans.childCount; i++)
            {
                var monsterTrans = enemiesTrans.GetChild(i);
                canvasTrans = monsterTrans.Find("Canvas");
                healthImage = canvasTrans.Find("Image").GetComponent<Image>();

                actor = ActorSystem.CreateMonster(game, ecsNode.NewInstanceId());
                TransformSystem.ChangePosition(actor, monsterTrans.position);
                actor.AddComponent<ModelViewComponent>(x => x.ModelTrans = monsterTrans);
                actor.AddComponent<AnimationComponent>();
                actor.AddComponent<HealthViewComponent>(x =>
                {
                    x.CanvasTrans = canvasTrans;
                    x.HealthBarImage = healthImage;
                });
                actor.GetComponent<CollisionComponent>().Layer = 2;
                actor.AddComponent<ActorCombatComponent>();
                actor.AddComponent<AIComponent>();
                actor.Init();
                game.Object2Entities.Add(monsterTrans.gameObject, actor.CombatEntity);

                if (monsterTrans.name == "Monster")
                {
                    StaticClient.Boss = actor.CombatEntity;
                }
            }
        }

        /// <summary>
        /// 自动战斗初始化：不加载输入/预览，仅加载必要系统，并从场景对象创建我方/敌方Actor。
        /// 支持两种结构：Allies(多友方)/Hero(单友方) + Enemies。
        /// </summary>
        public static void AutoBattleInit(EcsNode ecsNode, Assembly assembly)
        {
            ConsoleLog.Debug($"Process_GameSystem AutoBattleInit");

            var allTypes = assembly.GetTypes();
            ecsNode.AddSystems(allTypes);

            EcsNodeSystem.Create(ecsNode);
            ecsNode.AddComponent<EntityObjectComponent>();
            ecsNode.AddComponent<ReloadComponent>(x => x.SystemAssembly = assembly);
            ecsNode.AddComponent<ConfigManageComponent>(x => x.ConfigsCollector = StaticClient.ConfigsCollector);
            ecsNode.Init();

            var game = GameSystem.Create(ecsNode);
            game.AddComponent<AutoBattleComponent>();
            game.Init();
            StaticClient.Game = game;

            // Allies 集合
            var alliesRoot = GameObject.Find("Allies");
            if (alliesRoot != null)
            {
                var alliesTrans = alliesRoot.transform;
                for (int i = 0; i < alliesTrans.childCount; i++)
                {
                    var allyTrans = alliesTrans.GetChild(i);
                    var cTrans = allyTrans.Find("Canvas");
                    var hImg = cTrans != null ? cTrans.Find("Image")?.GetComponent<Image>() : null;

                    var ally = ActorSystem.CreateHero(game, ecsNode.NewInstanceId());
                    TransformSystem.ChangePosition(ally, allyTrans.position);
                    ally.AddComponent<ModelViewComponent>(x => x.ModelTrans = allyTrans);
                    ally.AddComponent<AnimationComponent>();
                    if (cTrans != null && hImg != null)
                    {
                        ally.AddComponent<HealthViewComponent>(x => { x.CanvasTrans = cTrans; x.HealthBarImage = hImg; });
                    }
                    ally.GetComponent<CollisionComponent>().Layer = 1;
                    ally.AddComponent<ActorCombatComponent>();
                    ally.CombatEntity.IsHero = true;
                    ally.Init();
                    game.Object2Entities.Add(allyTrans.gameObject, ally.CombatEntity);
                }
            }
            else
            {
                // 兼容单 Hero
                var heroGo = GameObject.Find("Hero");
                if (heroGo != null)
                {
                    var cTrans = heroGo.transform.Find("Canvas");
                    var hImg = cTrans != null ? cTrans.Find("Image")?.GetComponent<Image>() : null;

                    var hero = ActorSystem.CreateHero(game, ecsNode.NewInstanceId());
                    TransformSystem.ChangePosition(hero, heroGo.transform.position);
                    hero.AddComponent<ModelViewComponent>(x => x.ModelTrans = heroGo.transform);
                    hero.AddComponent<AnimationComponent>();
                    if (cTrans != null && hImg != null)
                    {
                        hero.AddComponent<HealthViewComponent>(x => { x.CanvasTrans = cTrans; x.HealthBarImage = hImg; });
                    }
                    hero.GetComponent<CollisionComponent>().Layer = 1;
                    hero.AddComponent<ActorCombatComponent>();
                    hero.CombatEntity.IsHero = true;
                    hero.Init();
                    game.Object2Entities.Add(heroGo, hero.CombatEntity);
                }
            }

            // Enemies 集合
            var enemiesRoot = GameObject.Find("Enemies");
            if (enemiesRoot != null)
            {
                var enemiesTrans = enemiesRoot.transform;
                for (int i = 0; i < enemiesTrans.childCount; i++)
                {
                    var monsterTrans = enemiesTrans.GetChild(i);
                    var cTrans = monsterTrans.Find("Canvas");
                    var hImg = cTrans != null ? cTrans.Find("Image")?.GetComponent<Image>() : null;

                    var enemy = ActorSystem.CreateMonster(game, ecsNode.NewInstanceId());
                    TransformSystem.ChangePosition(enemy, monsterTrans.position);
                    enemy.AddComponent<ModelViewComponent>(x => x.ModelTrans = monsterTrans);
                    enemy.AddComponent<AnimationComponent>();
                    if (cTrans != null && hImg != null)
                    {
                        enemy.AddComponent<HealthViewComponent>(x => { x.CanvasTrans = cTrans; x.HealthBarImage = hImg; });
                    }
                    enemy.GetComponent<CollisionComponent>().Layer = 2;
                    enemy.AddComponent<ActorCombatComponent>();
                    enemy.Init();
                    game.Object2Entities.Add(monsterTrans.gameObject, enemy.CombatEntity);
                }
            }
        }

        //public static void MiniInit(EcsNode ecsNode, Assembly assembly, ReferenceCollector configsCollector, AbilityConfigObject abilityConfig)
        //{
        //    ConsoleLog.Debug($"Process_GameSystem Init");

        //    var allTypes = assembly.GetTypes();
        //    var typeList = new List<Type>();
        //    typeList.AddRange(allTypes);

        //    ecsNode.AddSystems(typeList.ToArray());

        //    EcsNodeSystem.Create(ecsNode);
        //    ecsNode.Init();

        //    ecsNode.AddComponent<ReloadComponent>(x => x.SystemAssembly = assembly);
        //    ecsNode.AddComponent<ConfigManageComponent>(x => x.ConfigsCollector = configsCollector);

        //    var game = GameSystem.Create(ecsNode);
        //    game.AddComponent<PlayerInputComponent>();
        //    game.Init();

        //    StaticClient.Game = game;

        //    var combatContext = game.AddChild<CombatContext>();
        //    StaticClient.Context = combatContext;

        //    //创建怪物战斗实体
        //    var monster = combatContext.AddChild<CombatEntity>();
        //    //创建英雄战斗实体
        //    var hero = combatContext.AddChild<CombatEntity>();
        //    //给英雄挂载技能并加载技能执行体
        //    var heroSkillAbility = SkillSystem.Attach(hero, abilityConfig);

        //    ConsoleLog.Debug($"1 monster.CurrentHealth={monster.GetComponent<HealthPointComponent>().Value}");
        //    //使用英雄技能攻击怪物
        //    SpellSystem.SpellWithTarget(hero, heroSkillAbility, monster);
        //    ConsoleLog.Debug($"2 monster.CurrentHealth={monster.GetComponent<HealthPointComponent>().Value}");
        //    //--示例结束--
        //}

        public static void Reload(EcsNode ecsNode, Assembly assembly)
        {
            ConsoleLog.Debug($"Process_GameSystem Reload");

            ecsNode.GetComponent<ReloadComponent>().SystemAssembly = assembly;

            var allTypes = assembly.GetTypes();
            var typeList = new List<Type>();
            typeList.AddRange(allTypes);

            ecsNode.AddSystems(typeList.ToArray());

            EventSystem.Reload(ecsNode);
        }
    }

    /// <summary>
    /// 自动战斗组件，挂在 Game 上保存友方/敌方队列与出手节奏。
    /// </summary>
    public class AutoBattleComponent : EcsComponent
    {
        public List<CombatEntity> Allies = new List<CombatEntity>();
        public List<CombatEntity> Enemies = new List<CombatEntity>();
        public int AllyIndex = 0;
        public Dictionary<long, float> NextReadyTime = new Dictionary<long, float>();
        public float DefaultCooldown = 2.0f; // 秒
        public float HitDelay = 0.5f; // 命中延时
        public List<(CombatEntity caster, CombatEntity target, Ability skill, float time)> PendingCasts = new List<(CombatEntity, CombatEntity, Ability, float)>();
    }

    /// <summary>
    /// 自动战斗系统：轮转我方单位，自动选择目标并释放技能，跳过预览阶段。
    /// </summary>
    public class AutoBattleSystem : AComponentSystem<Game, AutoBattleComponent>, IAwake<Game, AutoBattleComponent>, IUpdate<Game>
    {
        public void Awake(Game entity, AutoBattleComponent component)
        {
            ConsoleLog.Debug("AutoBattleSystem Awake called");
            // 注意：此时 Object2Entities 可能还是空的，因为角色在后面才创建
            // 实际的单位收集会在 Update 中进行
        }

        public void Update(Game entity)
        {
            var component = entity.GetComponent<AutoBattleComponent>();
            if (component == null) return;

            // 首次收集单位（延迟初始化）
            if (component.Allies.Count == 0 && component.Enemies.Count == 0 && entity.Object2Entities.Count > 0)
            {
                foreach (var kv in entity.Object2Entities)
                {
                    var ce = kv.Value;
                    if (ce == null) continue;
                    if (ce.IsHero) component.Allies.Add(ce); else component.Enemies.Add(ce);
                    component.NextReadyTime[ce.Id] = 0f;
                }
                ConsoleLog.Debug($"AutoBattle units collected: {component.Allies.Count} allies, {component.Enemies.Count} enemies");
                return; // 第一帧只收集单位，下一帧开始战斗
            }

            float now = Time.time;

            // 处理待施法列表
            if (component.PendingCasts.Count > 0)
            {
                for (int i = component.PendingCasts.Count - 1; i >= 0; i--)
                {
                    var (caster, target, skill, time) = component.PendingCasts[i];
                    if (now >= time)
                    {
                        CastSkillDirectly(caster, target, skill);
                        component.PendingCasts.RemoveAt(i);
                    }
                }
            }

            // 清理死亡
            component.Enemies.RemoveAll(e => e == null || HealthSystem.CheckDead(e));
            component.Allies.RemoveAll(a => a == null || HealthSystem.CheckDead(a));
            if (component.Allies.Count == 0 || component.Enemies.Count == 0) return;

            // 找可出手的友方
            for (int tries = 0; tries < component.Allies.Count; tries++)
            {
                var idx = component.AllyIndex % component.Allies.Count;
                var caster = component.Allies[idx];
                component.AllyIndex = (component.AllyIndex + 1) % component.Allies.Count;
                if (caster == null || HealthSystem.CheckDead(caster)) continue;

                if (component.NextReadyTime.TryGetValue(caster.Id, out var ready) && now < ready) continue;

                // 获取可用技能（优先主动技能）
                var skill = GetAvailableSkill(caster);
                if (skill == null) continue;

                // 选择目标
                var target = SelectTarget(caster, skill, component.Enemies);
                if (target == null) continue;

                // 播放攻击动画（如果有）
                PlayAttackAnimation(caster);

                // 排队施法（延迟）
                component.PendingCasts.Add((caster, target, skill, now + component.HitDelay));
                component.NextReadyTime[caster.Id] = now + component.DefaultCooldown;
                break;
            }
        }

        private static Ability GetAvailableSkill(CombatEntity caster)
        {
            var skillComp = caster.GetComponent<SkillComponent>();
            if (skillComp == null) return null;

            // 优先使用主动技能（Q/W/E/R）
            foreach (var kv in skillComp.InputSkills)
            {
                var skill = kv.Value;
                if (skill != null && skill.Enable) return skill;
            }

            // 如果没有主动技能，使用默认技能（从 IdSkills 中选择第一个可用的）
            foreach (var kv in skillComp.IdSkills)
            {
                var skill = kv.Value;
                if (skill != null && skill.Enable) return skill;
            }

            return null;
        }

        private static CombatEntity SelectTarget(CombatEntity caster, Ability _, List<CombatEntity> enemies)
        {
            // 简单策略：选择最近的敌人
            CombatEntity nearestEnemy = null;
            float minDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy == null || HealthSystem.CheckDead(enemy)) continue;

                var distance = Vector3.Distance(caster.Position, enemy.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            return nearestEnemy;
        }

        private static void PlayAttackAnimation(CombatEntity caster)
        {
            var animComp = caster.Actor?.GetComponent<AnimationComponent>();
            if (animComp != null)
            {
                var clip = animComp.AttackAnimation != null ? animComp.AttackAnimation : animComp.SkillAnimation;
                if (clip != null)
                {
                    AnimationSystem.PlayFade(caster.Actor, clip);
                }
            }
        }

        /// <summary>
        /// 跳过预览直接施法：根据技能类型自动选择合适的施法方式
        /// </summary>
        private static void CastSkillDirectly(CombatEntity caster, CombatEntity target, Ability skill)
        {
            if (caster == null || target == null || skill == null) return;
            if (HealthSystem.CheckDead(caster) || HealthSystem.CheckDead(target)) return;

            // 直接调用 SpellSystem 的方法，跳过预览
            if (skill.Config?.TargetSelect == "手动指定" || skill.Config?.TargetSelect == null)
            {
                // 目标指向技能
                SpellSystem.SpellWithTarget(caster, skill, target);
            }
            else if (skill.Config?.TargetSelect == "碰撞检测")
            {
                // 位置指向技能，使用目标位置
                SpellSystem.SpellWithPoint(caster, skill, target.Position);
            }
            else
            {
                // 其他类型，默认使用目标指向
                SpellSystem.SpellWithTarget(caster, skill, target);
            }
        }
    }
}
