namespace Asteroids.Game.Unity.Match.Components
{
    internal abstract class UnityComponent
    {
        protected readonly UnityEntity unityEntity;

        internal UnityComponent(UnityEntity unityEntity)
        {
            this.unityEntity = unityEntity;
        }

        internal virtual void OnCreate()
        {
        }

        internal virtual void OnDestroy()
        {
        }

        internal virtual void TickChanged()
        {
        }

        internal virtual void Update()
        {
        }
    }
}