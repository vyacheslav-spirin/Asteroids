using System;
using Asteroids.Game.Unity.Match.Components;

namespace Asteroids.Game.Unity.Match
{
    internal sealed partial class UnityEntity
    {
        //TODO: Can replaced to visual component editor

        public static UnityEntity CreateNewEntityInstance(UnityEntityManager unityEntityManager, string name)
        {
            var unityEntity = new UnityEntity(unityEntityManager, name, 1);

            if (name == "Laser")
            {
                unityEntity.unityComponents[0] = new UnityLaserController(unityEntity);
            }
            else //general logic for other entities (example)
            {
                unityEntity.unityComponents[0] = new UnityTransform(unityEntity, name == "Bullet");
            }

            if (unityEntity == null) throw new Exception("Could not create unity entity by name: " + name);

            return unityEntity;
        }
    }
}