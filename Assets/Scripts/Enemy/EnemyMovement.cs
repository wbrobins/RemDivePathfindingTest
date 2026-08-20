using UnityEngine;

public abstract class EnemyMovement : MonoBehaviour
{
    protected EnemyBehavior enemy;

    public virtual void Initialize(EnemyBehavior enemy)
    {
        this.enemy = enemy;
    }

    public abstract void MoveTo(Vector3 position);

    public abstract void RetreatFrom(Transform target);

    public abstract void Strafe(Transform target);

    public abstract void GoToStartPoint();

    public abstract void StopMovement();
    public virtual void OnKnockbackEnd()
    {
        
    }
}
