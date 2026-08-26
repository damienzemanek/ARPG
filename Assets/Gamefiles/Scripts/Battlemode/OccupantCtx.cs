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
        }

        public void ActedUponByAction(BattleTracker.QueuedAction queuedActorAction, BattlemodeActionCtx attackersActionCtx)
        {
            attackersActionCtx.status = BattlemodeActionCtx.Status.BeingHit;
            
            List<BattlemodeEffectStrategyInstance> effectsToAddBeforeResolve = null;
            List<BattlemodeEffectStrategyInstance> effectsToAddAfterResolve = null;
            
            if(queuedActorAction.actionCtx == null) Debug.LogError("[TARGET] No action context to be targeted with.");
            foreach (var effectToApply in queuedActorAction.actionCtx.cfg.effectsToApplyToTarget)
            {
                var newEffectCtx = effectToApply.CreateNewEffectInstance();

                if (effectToApply.addOccurance == BattlemodeEffectConfigInstance.AddOccurance.BeforeAction)
                {
                    effectsToAddBeforeResolve ??= new List<BattlemodeEffectStrategyInstance>();
                    effectsToAddBeforeResolve.Add(newEffectCtx);
                }
                else
                {
                    effectsToAddAfterResolve ??= new List<BattlemodeEffectStrategyInstance>();
                    effectsToAddAfterResolve.Add(newEffectCtx);
                }
            }
            
            
            attackersActionCtx = ResolveBeforeHitEffects(attackersActionCtx, effectsToAddBeforeResolve);
            MutateValues(attackersActionCtx);
            ResolveAfterHitEffects(attackersActionCtx, effectsToAddAfterResolve);
            
        }

        #region Start Of Battle

        public void ResolveStartOfBattleOpponentEffects(List<BattleTracker.QueuedAction> opponentPreResolvedActions)
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

        public void ResolveAfterTurnEndsEnemyEffects(List<BattleTracker.QueuedAction> preResolvedActions)
        {
            if(this is not BattlerOccupantCtx battlerOccupantCtx) return;
            
            foreach (var queuedAction in preResolvedActions)
            {
                foreach (var effect in battlerOccupantCtx.currentEffects)
                    effect.ResolveEffectAfterTurnEnds(battlerOccupantCtx);
                
                foreach (var spEffect in battlerOccupantCtx.specialEffects)
                    spEffect.ResolveEffectAfterTurnEnds(battlerOccupantCtx);
            }
        }
        
        public void ResolveAfterTurnEndsPlayerEffects(BattlemodeAction[] preResolvedActions)
        {
            if(this is not BattlerOccupantCtx battlerOccupantCtx) return;
            
            foreach (var action in preResolvedActions)
            {
                foreach (var effect in battlerOccupantCtx.currentEffects)
                    effect.ResolveEffectAfterTurnEnds(battlerOccupantCtx);
                
                foreach (var spEffect in battlerOccupantCtx.specialEffects)
                    spEffect.ResolveEffectAfterTurnEnds(battlerOccupantCtx);
            }
        }


        #endregion

        #region Before / After Acting (Each Works for both player and opponent)

                
        public void ResolveBeforeActingEffects(BattlemodeAction[] preResolvedActions)
        {
            if (this is not BattlerOccupantCtx occupantCtx) return;
            
            // Resolve Effects that resolve before mutation
            foreach (var action in preResolvedActions)
            {
                if(action == null) continue;
                if(action.actionCtx == null) continue;
                
                foreach (var effect in occupantCtx.currentEffects)
                    effect.ResolveEffectBeforeActing(occupantCtx, action.actionCtx);
                
                foreach (var spEffect in occupantCtx.specialEffects)
                    spEffect.ResolveEffectBeforeActing(occupantCtx, action.actionCtx);
            }
        }
        
        public void ResolveAfterActingEffects(BattlemodeActionCtx actionCtx)
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

        #endregion

        
        BattlemodeActionCtx ResolveBeforeHitEffects(BattlemodeActionCtx battleActionCtx, 
            List<BattlemodeEffectStrategyInstance> effectsToAddBeforeResolve)
        {
            var occupantCtx = this;

            // Adding effects before resolve
            if (effectsToAddBeforeResolve != null)
                foreach (var effectToAdd in effectsToAddBeforeResolve)
                {
                    if (occupantCtx.currentEffects.Any(e => e.GetType() == effectToAdd.GetType()))
                        occupantCtx.AddStacksToAlreadyExistingEffect(effectToAdd);
                    else 
                        occupantCtx.currentEffects.Add(effectToAdd);
                }
            
            // Resolve Effects that resolve before mutation
            foreach (var effect in currentEffects)
                effect.ResolveEffectBeforeHitByAction(occupantCtx, battleActionCtx);
            
            // Resolve SP Effects that resolve before mutation
            foreach (var spEffect in specialEffects)
                spEffect.ResolveEffectBeforeHitByAction(occupantCtx, battleActionCtx);
            
            return battleActionCtx;
        }

        void ResolveAfterHitEffects(
            BattlemodeActionCtx battleActionCtx, 
            List<BattlemodeEffectStrategyInstance> effectsToAddAfterResolve)
        {
            var occupantCtx = this;
            
            // Resolve SP Effects that resolve after mutation
            foreach (var spEffect in specialEffects)
                spEffect.ResolveEffectAfterHitByAction(occupantCtx, battleActionCtx);
            
            // Resolve Effects that resolve after mutation
            foreach (var effect in currentEffects)
                effect.ResolveEffectAfterHitByAction(occupantCtx, battleActionCtx);

            
            // Adding effects after resolve
            if (effectsToAddAfterResolve != null)
                foreach (var effectToAdd in effectsToAddAfterResolve)
                {
                    if (occupantCtx.currentEffects.Any(e => e.GetType() == effectToAdd.GetType()))
                        occupantCtx.AddStacksToAlreadyExistingEffect(effectToAdd);
                    else 
                        occupantCtx.currentEffects.Add(effectToAdd);
                }
            
            // Removing marked for removal, Only removes status effects, not special effects
            for (var index = currentEffects.Count - 1; index >= 0; index--)
                if (currentEffects[index].ctx.markedForRemoval)
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


        public void AddStacksToAlreadyExistingEffect(BattlemodeEffectStrategyInstance effectToAdd)
        {
            var ctx = effectToAdd.ctx;

            foreach (var stackCtx in ctx.stackCtxs)
            {
                if(stackCtx.stacks <= 0) continue;
                
                foreach (var effect in currentEffects)
                {
                    if (effect.GetType() != effectToAdd.GetType()) continue;
                    effect.GetStackCtx(stackCtx.instanceEffectTime).stacks += effectToAdd.GetStackCtx(stackCtx.instanceEffectTime).stacks;
                }

                foreach (var spEffect in specialEffects)
                {
                    if (spEffect.GetType() != effectToAdd.GetType()) continue;
                    spEffect.GetStackCtx(stackCtx.instanceEffectTime).stacks += effectToAdd.GetStackCtx(stackCtx.instanceEffectTime).stacks;
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