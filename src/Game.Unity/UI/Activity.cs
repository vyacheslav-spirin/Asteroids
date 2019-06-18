using UnityEngine;

namespace Asteroids.Game.Unity.UI
{
    public abstract class Activity
    {
        public readonly string name;

        public bool IsVisible => rootObject.activeSelf;

        protected readonly GameObject rootObject;

        protected Activity(string name, GameObject rootObject)
        {
            this.name = name;

            this.rootObject = rootObject;

            rootObject.SetActive(false);
        }

        public virtual bool IsCloseEnabled()
        {
            return true;
        }

        public void Show()
        {
            rootObject.SetActive(true);

            OnShow();
        }

        public void Hide()
        {
            rootObject.SetActive(false);

            OnHide();
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }

        public virtual void OnDestroy()
        {
        }

        public virtual void Update()
        {
        }
    }
}