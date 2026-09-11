 // Future pass this to other tiles to move into those tiles, then clear self

 using System;
 using System.Collections.Generic;
 using System.Linq;
 using Sirenix.OdinInspector;
 using UnityEngine;
 using static BattleTracker;

 public abstract class OccupantCtx
    {
        public GameObject obj;
        public abstract OccupantCfg cfg { get; set; }
        public BattleTile newTilePosition = null;
    }

    public class BattlerOccupantCtx : OccupantCtx
    {
        public bool hasFreeMove;
        
        public int maxHp;
        public int currentHp;
        public int maxArmor;
        public int currentArmor;
        public int currentDMG;
        public int currentConstitution => currentHp + currentArmor;
        public List<BattlemodeActionConfig> exhuastedActionCfgs = new();
        [SerializeReference] public List<BattlemodeEffectStrategyInstance> currentEffects = new();
        [SerializeReference] public List<BattlemodeEffectStrategyInstance> specialEffects = new();

        public override OccupantCfg cfg { get => battlerCfg; set => battlerCfg = value as BattlerConfig;}
        public BattlerConfig battlerCfg;
        public BattlerOccupantCtx(BattlerConfig config)
        {
            cfg = config;
            maxHp = config.maxHP;
            maxArmor = config.maxArmor;
            
            // Future: Save current health for next battles in expiditions
            currentHp = maxHp;
            currentArmor = maxArmor;
            
            currentDMG = config.damage;

            foreach (var cfgSpecialEffect in config.specialEffects)
                specialEffects.Add(cfgSpecialEffect.CreateNewEffectInstance());

            hasFreeMove = true;
        }

        public void ActedUponByAction(QueuedAction queuedActorAction, BattlemodeActionCtx attackersActionCtx)
        {
            attackersActionCtx.status = BattlemodeActionCtx.Status.BeingHit;
            
            List<BattlemodeEffectStrategyInstance> effectsToAddBeforeResolve = null;
            List<BattlemodeEffectStrategyInstance> effectsToAddAfterResolve = null;
            
            if(queuedActorAction.actionCtx == null) Debug.LogError("[TARGET] No action context to be targeted with.");
            Debug.Log("[HIT] Amount of effects to apply: " + queuedActorAction.actionCtx.cfg.effectsToApplyToTarget.Count);
            foreach (var effectToApply in queuedActorAction.actionCtx.cfg.effectsToApplyToTarget)
            {
                var newEffectCtx = effectToApply.CreateNewEffectInstance();

                if (effectToApply.addOccurance == BattlemodeEffectConfigInstance.AddOccurance.BeforeAction)
                {
                    effectsToAddBeforeResolve ??= new List<BattlemodeEffectStrategyInstance>();
                    effectsToAddBeforeResolve.Add(newEffectCtx);
                    Debug.Log("[HIT] Queuing Adding BeforeAction effect: " + newEffectCtx.GetType().Name);
                }
                else if (effectToApply.addOccurance == BattlemodeEffectConfigInstance.AddOccurance.AfterAction)
                {
                    effectsToAddAfterResolve ??= new List<BattlemodeEffectStrategyInstance>();
                    effectsToAddAfterResolve.Add(newEffectCtx);
                    Debug.Log("[HIT] Queuing Adding AfterAction effect: " + newEffectCtx.GetType().Name);
                }
                else Debug.LogError("[HIT] Unknown AddOccurance: " + effectToApply.addOccurance);

            }
            
            
            TargetResolveBeforeHitEffects(attackersActionCtx, effectsToAddBeforeResolve);
            MutateValues(attackersActionCtx);
            TargetResolveAfterHitEffects(attackersActionCtx, effectsToAddAfterResolve);
            TargetAddAdditionalEffects(attackersActionCtx);
            
            // Removing marked for removal, Only removes status effects, not special effects
            currentEffects.RemoveAll(e => e.stacksTotal <= 0);
        }
        

        #region Start Of Battle

        public void ResolveStartOfBattleOpponentEffects(LinkedList<QueuedAction> opponentPreResolvedActions)
        {
            if(this is not BattlerOccupantCtx battlerOccupantCtx) return;
            
            foreach (var queuedAction in opponentPreResolvedActions)
            {
                foreach (var effect in battlerOccupantCtx.currentEffects)
                    effect.ResolveEffecStartOfBattle(battlerOccupantCtx, queuedAction.actionCtx);
                
                foreach (var spEffect in battlerOccupantCtx.specialEffects)
                    spEffect.ResolveEffecStartOfBattle(battlerOccupantCtx, queuedAction.actionCtx);
            }
        }

        public void ResolveStartOfBattlePlayerEffects(BattlemodeAction[] playerPreResolvedActions)
        {
            if(this is not BattlerOccupantCtx battlerOccupantCtx) return;

            foreach (var action in playerPreResolvedActions)
            {
                if(action == null) continue;
                if(action.actionCtx == null) continue;
                
                foreach (var effect in battlerOccupantCtx.currentEffects)
                    effect.ResolveEffecStartOfBattle(battlerOccupantCtx, action.actionCtx);


                foreach (var spEffect in battlerOccupantCtx.specialEffects)
                    spEffect.ResolveEffecStartOfBattle(battlerOccupantCtx, action.actionCtx);

            }
        }

        #endregion

        #region After Turn Ends

        public void ResolveAfterTurnEndsEffects()
        {
            if(this is not BattlerOccupantCtx battlerOccupantCtx) return;
            
            foreach (var effect in battlerOccupantCtx.currentEffects)
                effect.ResolveEffectAfterTurnEnds(battlerOccupantCtx);
                
            foreach (var spEffect in battlerOccupantCtx.specialEffects)
                spEffect.ResolveEffectAfterTurnEnds(battlerOccupantCtx);
            
            battlerOccupantCtx.currentEffects.RemoveAll(e => e.stacksTotal <= 0);
        }
        

        #endregion

        #region Pre Resolves

                
        public void PreResolveBeforeActingEffectsPlayer(BattlemodeAction[] preResolvedActions)
        {
            if (this is not BattlerOccupantCtx occupantCtx) return;
            
            occupantCtx.currentEffects.Sort(
                (a, b) => b.priority.CompareTo(a.priority));
            
            // Resolve Effects that resolve before mutation
            foreach (var action in preResolvedActions)
            {
                if(action == null) continue;
                if(action.actionCtx == null) continue;
                
                foreach (var effect in occupantCtx.currentEffects)
                    effect.ResolveEffectPreBeforeActing(occupantCtx, action.actionCtx);
                
                foreach (var spEffect in occupantCtx.specialEffects)
                    spEffect.ResolveEffectPreBeforeActing(occupantCtx, action.actionCtx);
            }
        }
        
        public void PreResolveBeforeActingEffectsOpponent(QueuedAction preResolvedAction)
        {
            if (this is not BattlerOccupantCtx occupantCtx) return;
            
            if(preResolvedAction == null) return;
            if(preResolvedAction.actionCtx == null) return;
            
            occupantCtx.currentEffects.Sort(
                (a, b) => b.priority.CompareTo(a.priority));
                
            foreach (var effect in occupantCtx.currentEffects)
                effect.ResolveEffectPreBeforeActing(occupantCtx, preResolvedAction.actionCtx);
                
            foreach (var spEffect in occupantCtx.specialEffects)
                spEffect.ResolveEffectPreBeforeActing(occupantCtx, preResolvedAction.actionCtx);
        }
        
        #endregion

        #region Actor Resolves
        
        public void ActorResolveAfterActingEffects(BattlemodeActionCtx actionCtx)
        {
            if (this is not BattlerOccupantCtx battlerOccupantCtx) return;
            if (actionCtx == null)
            {
                Debug.LogError("Trying to post resolve null actionCtx effects");
                return;
            }
            
            foreach (var effect in battlerOccupantCtx.currentEffects)
                effect.ResolveEffectAfterActing(battlerOccupantCtx, actionCtx);
            
            foreach (var spEffect in battlerOccupantCtx.specialEffects)
                spEffect.ResolveEffectAfterActing(battlerOccupantCtx, actionCtx);
        }
        
        public void ActorAddEffectsAfterActing(QueuedAction queuedActionCtx)
        {
            List<BattlemodeEffectStrategyInstance> effectsToAddToActorAfterActing = null;
            List<BattlemodeEffectStrategyInstance> specialEffectsToAddToActorAfterActing = null;
            
            effectsToAddToActorAfterActing = new List<BattlemodeEffectStrategyInstance>();
            
            foreach(var effectCfg in queuedActionCtx.actionCtx.cfg.effectsToApplyToSelf)
                if (effectCfg.addOccurance == BattlemodeEffectConfigInstance.AddOccurance.AfterAction)
                {
                    var effect = effectCfg.CreateNewEffectInstance();
                    if (effect.isSpecial)
                    {
                        specialEffectsToAddToActorAfterActing ??= new List<BattlemodeEffectStrategyInstance>();
                        specialEffectsToAddToActorAfterActing.Add(effect);
                    }
                    else
                    {
                        effectsToAddToActorAfterActing ??= new List<BattlemodeEffectStrategyInstance>();
                        effectsToAddToActorAfterActing.Add(effectCfg.CreateNewEffectInstance());
                    }
                }
            
            
            if (effectsToAddToActorAfterActing != null)
                foreach (var addEffect in effectsToAddToActorAfterActing)
                {
                    if (queuedActionCtx.actingOccupantCtx.currentEffects
                        .Any(e => e.GetType() == addEffect.GetType()))
                    {
                        queuedActionCtx.actingOccupantCtx.MutateStacksToAlreadyExistingEffect(addEffect);
                        continue;
                    }
                    queuedActionCtx.actingOccupantCtx.currentEffects.Add(addEffect);
                }
        
            if (specialEffectsToAddToActorAfterActing != null)
            {
                Debug.Log("[ACTOR] Adding Special Effects: " + specialEffectsToAddToActorAfterActing.Count);
                foreach (var addSpEffect in specialEffectsToAddToActorAfterActing)
                {
                    if (queuedActionCtx.actingOccupantCtx.specialEffects
                        .Any(sp => sp.GetType() == addSpEffect.GetType()))
                    {
                        Debug.Log("[ACTOR] Special Effect always already exists, mutating...");
                        queuedActionCtx.actingOccupantCtx.MutateStacksToAlreadyExistingEffect(addSpEffect);
                        continue;
                    }
                    queuedActionCtx.actingOccupantCtx.specialEffects.Add(addSpEffect);
                }
            }
        }

        
        public void ActorAddAdditionalEffects(QueuedAction queuedActionCtx, BattlemodeActionCtx actingActionCtx)
        {
            if(actingActionCtx.additionalEffectsToApplyToActor == null || actingActionCtx.additionalEffectsToApplyToActor.Count == 0) return;
            foreach (var addEffect in actingActionCtx.additionalEffectsToApplyToActor)
            {
                if(addEffect.isSpecial)
                {
                    if (queuedActionCtx.actingOccupantCtx.specialEffects
                        .Any(e => e.GetType() == addEffect.GetType()))
                        queuedActionCtx.actingOccupantCtx.MutateStacksToAlreadyExistingEffect(addEffect);
                }
                else
                {
                    if (queuedActionCtx.actingOccupantCtx.currentEffects
                        .Any(e => e.GetType() == addEffect.GetType()))
                    {
                        queuedActionCtx.actingOccupantCtx.MutateStacksToAlreadyExistingEffect(addEffect);
                        continue;
                    }
                    queuedActionCtx.actingOccupantCtx.currentEffects.Add(addEffect);
                }
            }
        }
        
        #endregion

        #region Target Resolves
        
        void TargetResolveBeforeHitEffects(BattlemodeActionCtx battleActionCtx, 
            List<BattlemodeEffectStrategyInstance> effectsToAddBeforeResolve)
        {
            var occupantCtx = this;

            // Adding effects before resolve
            if (effectsToAddBeforeResolve != null)
                foreach (var effectToAdd in effectsToAddBeforeResolve)
                {
                    if (occupantCtx.currentEffects.Any(e => e.GetType() == effectToAdd.GetType()))
                        occupantCtx.MutateStacksToAlreadyExistingEffect(effectToAdd);
                    else 
                        occupantCtx.currentEffects.Add(effectToAdd);
                }
            
            // Resolve Effects that resolve before mutation
            foreach (var effect in currentEffects)
                effect.TargetResolveEffectBeforeHitByAction(occupantCtx, battleActionCtx);
            
            // Resolve SP Effects that resolve before mutation
            foreach (var spEffect in specialEffects)
                spEffect.TargetResolveEffectBeforeHitByAction(occupantCtx, battleActionCtx);
        }

        void TargetResolveAfterHitEffects(
            BattlemodeActionCtx battleActionCtx, 
            List<BattlemodeEffectStrategyInstance> effectsToAddAfterResolve)
        {
            Debug.Log("[HIT] Resolving effects after hit:");
            var occupantCtx = this;
            
            // Resolve SP Effects that resolve after mutation
            foreach (var spEffect in specialEffects)
                spEffect.ResolveEffectAfterHitByAction(occupantCtx, battleActionCtx);
            
            // Resolve Effects that resolve after mutation
            foreach (var effect in currentEffects)
                effect.ResolveEffectAfterHitByAction(occupantCtx, battleActionCtx);

            
            // Adding effects after resolve
            if (effectsToAddAfterResolve != null && effectsToAddAfterResolve.Count > 0)
            {
                Debug.Log($"[HIT] {effectsToAddAfterResolve.Count} Add Effects Detected, mutating or adding...");
                foreach (var effectToAdd in effectsToAddAfterResolve)
                {
                    if (!effectToAdd.isSpecial)
                    {
                        if (occupantCtx.currentEffects.Any(e => e.GetType() == effectToAdd.GetType()))
                        {
                            occupantCtx.MutateStacksToAlreadyExistingEffect(effectToAdd);
                            Debug.Log("[HIT] Effect already exists, Mutated: " + effectToAdd.GetType().Name);
                        }
                        else
                        {
                            occupantCtx.currentEffects.Add(effectToAdd);
                            Debug.Log("[HIT] Added Effect: " + effectToAdd.GetType().Name + "");
                        }
                    }
                    else
                    {
                        // Battlers will always have their special, but at 0 stacks, so we mutate
                        if (occupantCtx.specialEffects.Any(e => e.GetType() == effectToAdd.GetType()))
                        {
                            occupantCtx.MutateStacksToAlreadyExistingEffect(effectToAdd);
                            Debug.Log("[HIT] Special Effect already exists, Mutated:" + effectToAdd.GetType().Name);
                        }
                    }
                }
            }
        }
        
        public void TargetAddAdditionalEffects(BattlemodeActionCtx actionCtx)
        {
            if(actionCtx.additionalEffectsToApplyToTarget == null || actionCtx.additionalEffectsToApplyToTarget.Count == 0) return;
            foreach (var addEffect in actionCtx.additionalEffectsToApplyToTarget)
            {
                if(addEffect.isSpecial)
                {
                    if (specialEffects.Any(e => e.GetType() == addEffect.GetType()))
                        MutateStacksToAlreadyExistingEffect(addEffect);
                    Debug.Log("[HIT] Additional Special Effect Mutated: " + addEffect.GetType().Name);
                }
                else
                {
                    if (currentEffects.Any(e => e.GetType() == addEffect.GetType()))
                    {
                        MutateStacksToAlreadyExistingEffect(addEffect);
                        Debug.Log("[HIT] Additional Effect already exists, Mutated: " + addEffect.GetType().Name);
                        continue;
                    }
                    currentEffects.Add(addEffect);
                    Debug.Log("[HIT] Added Additional Effect: " + addEffect.GetType().Name);
                }
            }
        }
        
        #endregion
        
        
        
        void MutateValues(BattlemodeActionCtx ctx)
        {
            int oldHp = currentHp;
            int oldArmor = currentArmor;
            Debug.Log("[HIT] Occupant: " + cfg?.name);

            var dmgMult = ctx.deltaDmgMultiplier / 100f;
            var healMult = ctx.deltaHealMultiplier / 100f;
            var armorMult = ctx.deltaArmorMultiplier / 100f;
            
            
            float preCeileddmg = (ctx.dmg * dmgMult);
            float preCeiledheal = (ctx.heal * healMult);
            float preCeiledarmor = (ctx.armor * armorMult);
            
            
            int preCritDmg = Mathf.CeilToInt(preCeileddmg);
            if(ctx.critHit && preCeileddmg > 0) preCeileddmg *= 2;
            if(ctx.critHit) Debug.Log("[HIT] Crit Hit!");
            
            int dmg = Mathf.CeilToInt(preCeileddmg);
            int heal = Mathf.CeilToInt(preCeiledheal);
            int armor = Mathf.CeilToInt(preCeiledarmor);
            
            Debug.Log($"$[HIT] DMG Stat: [{ctx.dmg}] " +
                      $" Base DMG Mult [{ctx.cfg.dmgMultiplier}%] " +
                      $" Ctx (Actual) DMG Mult: [{dmgMult}]" +
                      $" Crit? [{ctx.critHit}] " +
                      $"PreCrit Actual DMG: [{preCritDmg}] Actual DMG (w/ CRIT if Applied): [{dmg}]");
            
            
            currentArmor = ctx.cfg.capArmorIncrease
                ? Mathf.Min(currentArmor + armor, maxArmor)
                : currentArmor + armor;
            // Note: I am going to uncap armor increase for now as a gameplay balancing choice
            // however for enemy defends when they can do nothing, that will be capped
            //if (currentArmor > maxArmor) currentArmor = maxArmor;
            
            if (currentArmor > 0 && !ctx.isArmorPiercing)
            {
                currentArmor -= dmg; 
                
                if (currentArmor < 0) // meaning dmg is greater than armor
                {
                    // currentArmor is negative so its added to currentHp
                    currentHp += currentArmor; 
                    currentArmor = 0;
                }
            }
            else
                currentHp -= dmg;


            // Apply healing and clamp to max HP
            currentHp += heal;
            if (currentHp > maxHp) currentHp = maxHp;
            if (currentHp < 0) currentHp = 0;
            if (currentArmor < 0) currentArmor = 0;
            
            Debug.Log($"[HIT] [OldHP: {oldHp} New HP: {currentHp}] isArmorPiercing? {ctx.isArmorPiercing} [Old Armor: {oldArmor} New Armor: {currentArmor}]");
        }


        public void MutateStacksToAlreadyExistingEffect(BattlemodeEffectStrategyInstance effectToAdd)
        {
            var addCtx = effectToAdd.ctx;

            foreach (var addStackCtx in addCtx.stackCtxs)
            {
                Debug.Log("[EFFECT] Stacks: " + addStackCtx.stacks + " set directly? " + addStackCtx.setStacksDirectly + " instance time: " + addStackCtx.instanceEffectTime + "");
                if(addStackCtx.stacks <= 0 && !addStackCtx.setStacksDirectly) continue;
                Debug.Log("[EFFECT] Mutating stacks on tile: " + cfg?.name + " to already existing effect: " + effectToAdd.GetType().Name + " with stacks: " + addStackCtx.stacks + "");
                
                foreach (var effect in currentEffects)
                {
                    if (effect.GetType() != effectToAdd.GetType()) continue;
                    if(!addStackCtx.setStacksDirectly)
                        effect.GetStackCtx(addStackCtx.instanceEffectTime).stacks += effectToAdd.GetStackCtx(addStackCtx.instanceEffectTime).stacks;
                    else
                        effect.GetStackCtx(addStackCtx.instanceEffectTime).stacks = effectToAdd.GetStackCtx(addStackCtx.instanceEffectTime).stacks;
                }

                foreach (var spEffect in specialEffects)
                {
                    if (spEffect.GetType() != effectToAdd.GetType()) continue;
                    if(!addStackCtx.setStacksDirectly)
                        spEffect.GetStackCtx(addStackCtx.instanceEffectTime).stacks += effectToAdd.GetStackCtx(addStackCtx.instanceEffectTime).stacks;
                    else
                        spEffect.GetStackCtx(addStackCtx.instanceEffectTime).stacks = effectToAdd.GetStackCtx(addStackCtx.instanceEffectTime).stacks;
                }
            }
        }
        
        
    }

    public class CharacterOccupantCtx : BattlerOccupantCtx
    {
        public int maxAP;
        public int currentAP;
        public int maxHandSize;
        public override OccupantCfg cfg { get => _config; set => _config = value as CharacterConfig;}
        public CharacterConfig _config;
        
        public CharacterOccupantCtx(CharacterConfig config) : base(config)
        {
            _config = config;
            maxHandSize = config.maxHandSize;
            maxAP = config.maxAP;
            currentAP = maxAP;
        }
    }
    
    public class EnemyOccupantCtx : BattlerOccupantCtx
    {
        public int currentPredicteds;
        public RandomBag<EnemyConfig.AttackPriority> attackPriority;
        public IntentUsage.IntentUsageCtx currentIntentUsageCtx;
        public LinkedList<QueuedAction> queuedActions = new();
        
        public override OccupantCfg cfg { get => enemyCfg; set => enemyCfg = value as EnemyConfig;}
        public EnemyConfig enemyCfg;
        
        public EnemyOccupantCtx(EnemyConfig config) : base(config)
        {
            currentPredicteds = config.defaultPredicted;
            currentIntentUsageCtx = new IntentUsage.IntentUsageCtx(config.intentions);
            
            List<EnemyConfig.AttackPriority> _attackPriority = new List<EnemyConfig.AttackPriority>();
            for(int i = 0; i < config.primaryAttackPriorityWeight; i++)
                _attackPriority.Add(config.PrimaryAttackPriority);
            for(int i = 0; i < config.secondaryAttackPriorityWeight; i++)
                _attackPriority.Add(config.SecondaryAttackPriority);
            attackPriority = new RandomBag<EnemyConfig.AttackPriority>(_attackPriority);
        }
    }