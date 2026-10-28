using Fusion;
using UnityEngine;
public class DuelProjectile : NetworkBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 2.5f;
    [SerializeField] private float hitRadius = 0.15f;
    private float remainingLife;
    public override void Spawned()
    {
        remainingLife = lifetime;
    }
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;
        DuelMatch match = DuelMatch.Instance;
        if (match != null && match.IsOver)
        {
            Runner.Despawn(Object);
            return;
        }
        remainingLife -= Runner.DeltaTime;
        if (remainingLife <= 0)
        {
            Runner.Despawn(Object);
            return;
        }
        Vector3 movement =
        transform.forward * speed * Runner.DeltaTime;
        bool hitSomething = Physics.SphereCast(
        transform.position,
        hitRadius,
        movement.normalized,
        out RaycastHit hit,
        movement.magnitude,
        Physics.DefaultRaycastLayers,
        QueryTriggerInteraction.Ignore
        );
        if (hitSomething)
        {
            DuelHealth target =
            hit.collider.GetComponentInParent<DuelHealth>();
            if (target != null)
            {
                if (target.Object.StateAuthority ==
                Object.StateAuthority)
                {
                    transform.position += movement;
                    return;
                }
                target.RPC_ReceiveDamage();
                Runner.Despawn(Object);
                return;
            }
            Runner.Despawn(Object);
            return;
        }
        transform.position += movement;
    }
}