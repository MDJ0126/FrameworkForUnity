using UnityEngine;

namespace Game
{
    public class RoarSkill : Skill
    {
        public override void Execute()
        {
            Collider[] results = new Collider[10];
            GameObject[] ignoreObjects = { owner.gameObject };
            PhysicsQueryHelper.OverlapSphereNonAlloc(owner.Transform.position, 3f, results, LayerMask.GetMask("Pawn"), ignoreObjects: ignoreObjects, debug: new PhysicsQueryHelper.PhysicsQueryDebug
            {
                drawMode = PhysicsQueryHelper.eDebugDrawMode.ForDuration,
            });

            foreach (Collider result in results)
            {
                if (!result) continue;
                Pawn pawn = result.GetComponentInParent<Pawn>();
                if (pawn)
                {
                    pawn.Damaged(owner, new DamageInfo { damage = 10 });
                }
            }

            Debug.Log("Executed Roar Skill.");
        }
    }
}