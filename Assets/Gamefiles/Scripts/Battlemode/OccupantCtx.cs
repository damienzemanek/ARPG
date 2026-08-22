 // Future pass this to other tiles to move into those tiles, then clear self

 using System;
 using System.Collections.Generic;
 using System.Linq;
 using Sirenix.OdinInspector;
 using UnityEngine;

 public abstract class OccupantCtx
    {
        public GameObject obj;
        public abstract OccupantCfg cfg { get; set; }
        public BattleTile newTilePosition = null;
    }

    public class BattlerOccupantCtx : OccupantCtx
    {
        public int maxHp;
        public int currentHp;
        public int maxArmor;
        public int currentArmor;
        public int currentDMG;
        public int currentConstitution => currentHp + currentArmor;
        [SerializeReference] public List<BattlemodeEffectCtx> currentEffects = new();
        [SerializeReference] public List<BattlemodeEffectCtx> specialEffects = new();

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
                specialEffects.Add(cfgSpecialEffect.GenerateEffectCtx());
        }

        public void ActedUponByAction(BattleTracker.QueuedAction queuedActorAction, BattlemodeActionCtx attackersActionCtx)
        {
            attackersActionCtx.status = BattlemodeActionCtx.Status.BeingHit;
            
            List<BattlemodeEffectCtx> effectsToAddBeforeResolve = null;
            List<BattlemodeEffectCtx> effectsToAddAfterResolve = null;
            
            if(queuedActorAction.actionCtx == null) Debug.LogError("[TARGET] No action context to be targeted with.");
            foreach (var effectToApply in queuedActorAction.actionCtx.cfg.effectsToApplyToTarget)
            {
                var newEffectCtx = effectToApply.GenerateEffectCtx();

                if (effectToApply.addOccurance == BattlemodeEffectConfig.AddOccurance.BeforeAction)
                {
                    effectsToAddBeforeResolve ??= new List<BattlemodeEffectCtx>();
                    effectsToAddBeforeResolve.Add(newEffectCtx);
                }
                else
                {
                    effectsToAddAfterResolve ??= new List<BattlemodeEffectCtx>();
                    effectsToAddAfterResolve.Add(newEffectCtx);
                }
            }
            
            
            attackersActionCtx = PreResolveTargetEffects(attackersActionCtx, effectsToAddBeforeResolve);
            MutateValues(attackersActionCtx);
            PostResolveTargetEffects(attackersActionCtx, effectsToAddAfterResolve);
            
        }
        
        public void ResolveStartOfBattleOpponentEffects(List<BattleTracker.QueuedAction> opponentPreResolvedActions)
        {
            if(this is not BattlerOccupantCtx battlerOccupantCtx) return;
            
            foreach (var queuedAction in opponentPreResolvedActions)
            {
                foreach (var effect in battlerOccupantCtx.currentEffects)
                {
                    if (effect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.StartOfBattle
                        && (queuedAction.actionCtx != null))
                        queuedAction.actionCtx = effect.ResolveEffect(battlerOccupantCtx, queuedAction.actionCtx);
                }

                foreach (var spEffect in battlerOccupantCtx.specialEffects)
                {
                    if(spEffect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.StartOfBattle
                       && (queuedAction.actionCtx != null))
                        spEffect.ResolveEffect(battlerOccupantCtx, queuedAction.actionCtx);
                }
            }
        }

        public void ResolveStartOfBattlePlayerEffects(BattlemodeAction[] playerPreResolvedActions)
        {
            if(this is not BattlerOccupantCtx battlerOccupantCtx) return;

            foreach (var action in playerPreResolvedActions)
            {
                foreach (var effect in battlerOccupantCtx.currentEffects)
                {
                    if (effect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.StartOfBattle
                        && (action.actionCtx != null))
                        action.actionCtx = effect.ResolveEffect(battlerOccupantCtx, action.actionCtx);
                }

                foreach (var spEffect in battlerOccupantCtx.specialEffects)
                {
                    if(spEffect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.StartOfBattle
                       && (action.actionCtx != null))
                        spEffect.ResolveEffect(battlerOccupantCtx, action.actionCtx);
                }
            }
        }
        
        

        public void ResolveEndOfTurnEnemyEffects(List<BattleTracker.QueuedAction> preResolvedActions)
        {
            if(this is not BattlerOccupantCtx battlerOccupantCtx) return;
            
            foreach (var queuedAction in preResolvedActions)
            {
                foreach (var effect in battlerOccupantCtx.currentEffects)
                {
                    if (effect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.AfterTurnEnds
                        && (queuedAction.actionCtx != null))
                        queuedAction.actionCtx = effect.ResolveEffect(battlerOccupantCtx, queuedAction.actionCtx);
                }

                foreach (var spEffect in battlerOccupantCtx.specialEffects)
                {
                    if(spEffect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.AfterTurnEnds
                       && (queuedAction.actionCtx != null))
                        spEffect.ResolveEffect(battlerOccupantCtx, queuedAction.actionCtx);
                }
            }
        }
        
        public void ResolveEndOfTurnPlayerEffects(BattlemodeAction[] preResolvedActions)
        {
            if(this is not BattlerOccupantCtx battlerOccupantCtx) return;

            foreach (var action in preResolvedActions)
            {
                foreach (var effect in battlerOccupantCtx.currentEffects)
                {
                    if (effect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.AfterTurnEnds
                        && (action.actionCtx != null))
                        action.actionCtx = effect.ResolveEffect(battlerOccupantCtx, action.actionCtx);
                }

                foreach (var spEffect in battlerOccupantCtx.specialEffects)
                {
                    if(spEffect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.AfterTurnEnds
                       && (action.actionCtx != null))
                        spEffect.ResolveEffect(battlerOccupantCtx, action.actionCtx);
                }
            }
        }

        public void PreResolveActingEffects(BattlemodeAction[] preResolvedActions)
        {
            if (this is not BattlerOccupantCtx occupantCtx) return;
            
            // Resolve Effects that resolve before mutation
            foreach (var action in preResolvedActions)
            {
                foreach (var effect in occupantCtx.currentEffects)
                {
                    if (effect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.BeforeActing
                        && (action.actionCtx != null))
                    action.actionCtx = effect.ResolveEffect(occupantCtx, action.actionCtx);
                }

                foreach (var spEffect in occupantCtx.specialEffects)
                {
                    if(spEffect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.BeforeActing
                        && (action.actionCtx != null))
                    spEffect.ResolveEffect(occupantCtx, action.actionCtx);
                }
            }
        }
        
        public void PostResolveActingEffects(BattlemodeActionCtx actionCtx)
        {
            if (this is not BattlerOccupantCtx battlerOccupantCtx) return;
            if (actionCtx == null)
            {
                Debug.LogError("Trying to post resolve null actionCtx effects");
                return;
            }
            
            foreach (var effect in battlerOccupantCtx.currentEffects)
            {
                if (effect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.AfterActing)
                    actionCtx = effect.ResolveEffect(battlerOccupantCtx, actionCtx);
            }

            foreach (var spEffect in battlerOccupantCtx.specialEffects)
            {
                if(spEffect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.AfterActing)
                    actionCtx = spEffect.ResolveEffect(battlerOccupantCtx, actionCtx);
            }
        }
        
        BattlemodeActionCtx PreResolveTargetEffects(
            BattlemodeActionCtx battleActionCtx, 
            List<BattlemodeEffectCtx> effectsToAddBeforeResolve)
        {
            var occupantCtx = this;

            // Adding effects before resolve
            if (effectsToAddBeforeResolve != null)
                foreach (var effectToAdd in effectsToAddBeforeResolve)
                {
                    if (occupantCtx.currentEffects.Any(e => e.effectStrategy.GetType() == effectToAdd.effectStrategy.GetType()))
                        occupantCtx.currentEffects.Find(e => e.effectStrategy.GetType() == effectToAdd.effectStrategy.GetType()).stacks += effectToAdd.stacks;
                    else 
                        occupantCtx.currentEffects.Add(effectToAdd);
                }
            
            // Resolve Effects that resolve before mutation
            foreach (var effect in currentEffects)
                if(effect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.BeforeHitByAction)
                    battleActionCtx = effect.ResolveEffect(occupantCtx, battleActionCtx);
            
            // Resolve SP Effects that resolve before mutation
            foreach (var spEffect in specialEffects)
                if(spEffect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.BeforeHitByAction)
                    battleActionCtx = spEffect.ResolveEffect(occupantCtx, battleActionCtx);
            
            return battleActionCtx;
        }

        void PostResolveTargetEffects(
            BattlemodeActionCtx battleActionCtx, 
            List<BattlemodeEffectCtx> effectsToAddAfterResolve)
        {
            var occupantCtx = this;
            
            // Resolve SP Effects that resolve after mutation
            foreach (var spEffect in specialEffects)
                if(spEffect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.AfterHitByAction)
                    spEffect.ResolveEffect(occupantCtx, battleActionCtx);
            
            // Resolve Effects that resolve after mutation
            foreach (var effect in currentEffects)
                if(effect.effectStrategy.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.AfterHitByAction)
                    effect.ResolveEffect(occupantCtx, battleActionCtx);
            
            // Adding effects after resolve
            if (effectsToAddAfterResolve != null)
                foreach (var effectToAdd in effectsToAddAfterResolve)
                {
                    if (occupantCtx.currentEffects.Any(e => e.effectStrategy.GetType() == effectToAdd.effectStrategy.GetType()))
                        occupantCtx.currentEffects.Find(e => e.effectStrategy.GetType() == effectToAdd.effectStrategy.GetType()).stacks += effectToAdd.stacks;
                    else 
                        occupantCtx.currentEffects.Add(effectToAdd);
                }
            
            // Removing marked for removal, Only removes status effects, not special effects
            for (var index = currentEffects.Count - 1; index >= 0; index--)
                if (currentEffects[index].markedForRemoval)
                    currentEffects.RemoveAt(index);
        }
        
        void MutateValues(BattlemodeActionCtx ctx)
        {
            Debug.Log("[HIT] Mutating values on tile: " + cfg?.name);
            Debug.Log("[HIT] Old HP: " + currentHp + " Old Armor: " + currentArmor);

            var dmgMult = ctx.deltaDmgMultiplier / 100f;
            var healMult = ctx.deltaHealMultiplier / 100f;
            var armorMult = ctx.deltaArmorMultiplier / 100f;
            
            Debug.Log("[HIT] Damage multiplier: " + dmgMult + " Heal multiplier: " + healMult);
            
            int dmg = Mathf.CeilToInt(ctx.dmg * dmgMult);
            int heal = Mathf.CeilToInt(ctx.heal * healMult);
            int armor = Mathf.CeilToInt(ctx.armor * armorMult);
            
            Debug.Log("[HIT] [Damage: " + dmg + " Heal: " + heal+ "] [Old DMG: " + ctx.dmg + " Old Heal: " + ctx.heal + "]");

            currentArmor = ctx.cfg.capArmorIncrease
                ? Mathf.Min(currentArmor + armor, maxArmor)
                : currentArmor + armor;
            // Note: I am going to uncap armor increase for now as a gameplay balancing choice
            // however for enemy defends when they can do nothing, that will be capped
            //if (currentArmor > maxArmor) currentArmor = maxArmor;
            
            if (currentArmor > 0)
            {
                currentArmor -= dmg;
                if (currentArmor < 0)
                {
                    currentHp += currentArmor; // Subtract leftover damage from HP
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
            
            Debug.Log("[HIT] New HP: " + currentHp + " New Armor: " + currentArmor);
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
        public List<BattleTracker.QueuedAction> queuedActions = new();
        
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
        
        public void ResetSavedIntention() => currentIntentUsageCtx.savedIntentIndex = -1;
        public void SaveCurrentIntention() => currentIntentUsageCtx.savedIntentIndex = currentIntentUsageCtx.intentIndex;

        public bool HasSavedIntention(out int index)
        {
            index = currentIntentUsageCtx.savedIntentIndex;
            return (index != -1);
        }
    }