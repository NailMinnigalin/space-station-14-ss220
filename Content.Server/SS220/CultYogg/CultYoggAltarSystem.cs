using System.Linq;
using Content.Shared.SS220.CultYogg;

namespace Content.Server.SS220.CultYogg;

/// <summary>
/// This handles...
/// </summary>
public sealed class CultYoggAltarSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly SharedTransformSystem _sharedTransformSystem = default!;

    private ISawmill _sawmill = default!;
    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<CultYoggAltarComponent>();
        while (query.MoveNext(out var altarEntity, out var cultYoggAltarComponent))
        {
            cultYoggAltarComponent.MiGoNearby = GetMiGoNearby(altarEntity, 10);
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("CultYoggAltarSystem");
        _sawmill.Level = LogLevel.Info;
    }

    private List<EntityUid> GetMiGoNearby(EntityUid altarEntity, float range)
    {
        if (!EntityManager.TryGetComponent<TransformComponent>(altarEntity, out var altarTransformComponent))
            return new(); //Altar without position can't have MiGo nearby

        List<EntityUid> miGoNearby = new();
        foreach (var entityNearby in _lookupSystem.GetEntitiesInRange(_sharedTransformSystem.GetMapCoordinates(altarEntity, altarTransformComponent), range))
        {
            //MiGo without position can't be nearby altar
            if (EntityManager.TryGetComponent<MiGoComponent>(entityNearby, out _) && EntityManager.TryGetComponent<TransformComponent>(entityNearby, out _))
                miGoNearby.Add(entityNearby);
        }

        miGoNearby.Sort((entityA, entityB) =>
        {
            var entityATransformComponent = EntityManager.GetComponent<TransformComponent>(entityA);
            var entityBTransformComponent = EntityManager.GetComponent<TransformComponent>(entityB);

            var entityADistanceToAltar =
                (altarTransformComponent.Coordinates.Position - entityATransformComponent.Coordinates.Position)
                .Length();
            var entityBDistanceToAltar =
                (altarTransformComponent.Coordinates.Position - entityBTransformComponent.Coordinates.Position)
                .Length();

            return (int)Math.Ceiling(entityADistanceToAltar - entityBDistanceToAltar);
        });

        return miGoNearby.Take(3).ToList();
    }
}
