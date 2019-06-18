using System;
using System.Collections.Generic;
using UnityEngine;

namespace Asteroids.Game.Unity.UI
{
    public sealed class UserInterfaceManager
    {
        public int ActivityStackSize => activityShowLinks.Count;

        internal readonly GameObject canvasObject;

        private readonly List<Activity> activities = new List<Activity>();

        private readonly LinkedList<Activity> activityShowLinks = new LinkedList<Activity>();

        public UserInterfaceManager(GameObject canvasObject)
        {
            this.canvasObject = canvasObject;
        }

        public void RegisterActivity(Activity activity)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));

            foreach (var a in activities)
            {
                if (a == activity) throw new Exception("Activity is already registered!");
            }

            activities.Add(activity);
        }

        public void Update()
        {
            foreach (var window in activities)
            {
                window.Update();
            }
        }

        public void OnDestroy()
        {
            foreach (var window in activities)
            {
                window.OnDestroy();
            }

            activities.Clear();
        }

        public void ShowActivity(string activityName)
        {
            foreach (var window in activities)
            {
                if (window.name == activityName)
                {
                    if (window.IsVisible) return;

                    if (activityShowLinks.Count > 0)
                    {
                        if (!activityShowLinks.Last.Value.IsCloseEnabled())
                        {
                            Debug.LogWarning("Could not close the window!");

                            return;
                        }

                        activityShowLinks.Last.Value.Hide();
                    }

                    var currentWindowLinkNode = activityShowLinks.First;

                    while (currentWindowLinkNode != null)
                    {
                        if (currentWindowLinkNode.Value == window)
                        {
                            activityShowLinks.Remove(currentWindowLinkNode);

                            break;
                        }

                        currentWindowLinkNode = currentWindowLinkNode.Next;
                    }

                    activityShowLinks.AddLast(window);

                    window.Show();

                    return;
                }
            }

            throw new Exception($"Could not find activity by name: {activityName}!");
        }

        public void HideCurrentActivity()
        {
            if (activityShowLinks.Count == 0) return;

            if (!activityShowLinks.Last.Value.IsCloseEnabled()) return;

            activityShowLinks.Last.Value.Hide();

            activityShowLinks.RemoveLast();

            if (activityShowLinks.Count > 0) activityShowLinks.Last.Value.Show();
        }

        public Activity GetActivityByName(string activityName)
        {
            foreach (var activity in activities)
            {
                if (activity.name == activityName) return activity;
            }

            return null;
        }
    }
}