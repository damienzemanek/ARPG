using UnityEngine;

public interface ICollisionCheckMsgReceiver<TColliderSpace, TContext>
    where TColliderSpace : Component
{
    public virtual void OnEnterBounds(TColliderSpace collidedWith, CollisionChecker<TContext> sender, TContext ctx) { }
    public virtual void OnExitBounds(TColliderSpace collidedWith, CollisionChecker<TContext> sender, TContext ctx) { }
    public virtual void OnStayBounds(TColliderSpace collidedWith, CollisionChecker<TContext> sender, TContext ctx) { }
}