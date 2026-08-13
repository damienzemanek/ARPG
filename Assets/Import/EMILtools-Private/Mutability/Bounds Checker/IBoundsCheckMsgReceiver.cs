using UnityEngine;

public interface IBoundsCheckMsgReceiver<TColliderSpace, TContext>
    where TColliderSpace : Component
{
    public virtual void OnEnterBounds(TColliderSpace collidedWith, BoundsChecker<TContext> sender, TContext ctx) { }
    public virtual void OnExitBounds(TColliderSpace collidedWith, BoundsChecker<TContext> sender, TContext ctx) { }
    public virtual void OnStayBounds(TColliderSpace collidedWith, BoundsChecker<TContext> sender, TContext ctx) { }
}


