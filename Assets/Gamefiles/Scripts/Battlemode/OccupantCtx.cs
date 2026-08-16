 // Future pass this to other tiles to move into those tiles, then clear self

 using System.Collections.Generic;
 using Sirenix.OdinInspector;
 using UnityEngine;

 public abstract class OccupantCtx
    {
        public GameObject obj;
        public abstract BattlePositionOccupantConfig cfg { get; set; }
    }

    public class BattlerOccupantCtx : OccupantCtx
    {
        public int maxHp;
        public int currentHp;
        public int maxArmor;
        public int currentArmor;
        public int currentDMG;
        [SerializeReference] public List<BattlemodeEffectCtx> currentEffects = new();
        [SerializeReference] public List<BattlemodeEffectCtx> specialEffects = new();

        public override BattlePositionOccupantConfig cfg { get => _config; set => _config = value as BattlerConfig;}
        public BattlerConfig _config;
        public BattlerOccupantCtx(BattlerConfig config)
        {
            _config = config;
            maxHp = config.maxHP;
            maxArmor = config.maxArmor;
            
            // Future: Save current health for next battles in expiditions
            currentHp = maxHp;
            currentArmor = maxArmor;
            
            currentDMG = config.damage;

            foreach (var cfgSpecialEffect in config.specialEffects)
                specialEffects.Add(cfgSpecialEffect.GenerateEffectCtx());
        }

        public void ActedUponByAction(BattleTracker.QueuedPlayerAction queuedActorAction, BattlemodeActionCtx attackersActionCtx)
        {
            attackersActionCtx.status = BattlemodeActionCtx.Status.BeingHit;
            
            List<BattlemodeEffectCtx> effectsToAddBeforeResolve = null;
            List<BattlemodeEffectCtx> effectsToAddAfterResolve = null;
            
            if(queuedActorAction.queuedActionCtx == null) Debug.LogError("[TARGET] No action context to be targeted with.");
            foreach (var effectToApply in queuedActorAction.queuedActionCtx?.cfg.effectsToApplyToTarget)
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

        public void PreResolveActingEffects(BattlemodeAction[] preResolvedActions)
        {
            if (this is not BattlerOccupantCtx occupantCtx) return;
            
            // Resolve Effects that resolve before mutation
            foreach (var action in preResolvedActions)
            {
                foreach (var effect in occupantCtx.currentEffects)
                {
                    if (effect.cfg.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.BeforeActing
                        && (action.actionCtx != null))
                    action.actionCtx = effect.ResolveEffect(occupantCtx, action.actionCtx);
                }

                foreach (var spEffect in occupantCtx.specialEffects)
                {
                    if(spEffect.cfg.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.BeforeActing
                        && (action.actionCtx != null))
                    spEffect.ResolveEffect(occupantCtx, action.actionCtx);
                }
            }
        }
        
        public void PostResolveActingEffects(BattlemodeAction[] postActedActions)
        {
            if (this is not BattlerOccupantCtx occupantCtx) return;
            
            // Resolve Effects that resolve before mutation
            foreach (var action in postActedActions)
            {
                foreach (var effect in occupantCtx.currentEffects)
                {
                    if (effect.cfg.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.AfterActing
                        && (action.actionCtx != null))
                        action.actionCtx = effect.ResolveEffect(occupantCtx, action.actionCtx);
                }

                foreach (var spEffect in occupantCtx.specialEffects)
                {
                    if(spEffect.cfg.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.AfterActing
                       && (action.actionCtx != null))
                        action.actionCtx = spEffect.ResolveEffect(occupantCtx, action.actionCtx);
                }
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
                    occupantCtx.currentEffects.Add(effectToAdd);
            
            // Resolve Effects that resolve before mutation
            foreach (var effect in currentEffects)
                if(effect.cfg.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.BeforeHitByAction)
                    battleActionCtx = effect.ResolveEffect(occupantCtx, battleActionCtx);
            
            // Resolve SP Effects that resolve before mutation
            foreach (var spEffect in specialEffects)
                if(spEffect.cfg.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.BeforeHitByAction)
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
                if(spEffect.cfg.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.AfterHitByAction)
                    spEffect.ResolveEffect(occupantCtx, battleActionCtx);
            
            // Resolve Effects that resolve after mutation
            foreach (var effect in currentEffects)
                if(effect.cfg.resolveOccurance == BattlemodeEffectConfig.ResolveOccurance.AfterHitByAction)
                    effect.ResolveEffect(occupantCtx, battleActionCtx);
            
            // Adding effects after resolve
            if (effectsToAddAfterResolve != null)
                foreach (var effectToAdd in effectsToAddAfterResolve)
                    occupantCtx.currentEffects.Add(effectToAdd);
            
            // Removing marked for removal, Only removes status effects, not special effects
            for (var index = currentEffects.Count - 1; index >= 0; index--)
                if (currentEffects[index].markedForRemoval)
                    currentEffects.RemoveAt(index);
        }

        void MutateValues(BattlemodeActionCtx ctx)
        {
            Debug.Log("[HIT] Mutating values");
            Debug.Log("[HIT] Old HP: " + currentHp + " Old Armor: " + currentArmor);

            var dmgMult = ctx.deltaDmgMultiplier / 100f;
            var healMult = ctx.deltaHealMultiplier / 100f;
            
            Debug.Log("[HIT] Damage multiplier: " + dmgMult + " Heal multiplier: " + healMult);
            
            int dmg = Mathf.CeilToInt(ctx.dmg * dmgMult);
            int heal = Mathf.CeilToInt(ctx.heal * healMult);
            
            Debug.Log("[HIT] [Damage: " + dmg + " Heal: " + heal+ "] [Old DMG: " + ctx.dmg + " Old Heal: " + ctx.heal + "]");

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

            Debug.Log("[HIT] New HP: " + currentHp + " New Armor: " + currentArmor);
        }
    }

    public class CharacterOccupantCtx : BattlerOccupantCtx
    {
        public int maxAP;
        public int currentAP;
        public int maxHandSize;
        public override BattlePositionOccupantConfig cfg { get => _config; set => _config = value as CharacterConfig;}
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
        public int currentIntentions;
        public int currentPredicteds;
        
        public EnemyOccupantCtx(EnemyConfig config) : base(config)
        {
            currentIntentions = config.intentions;
            currentPredicteds = config.defaultPredicted;
        }
    }