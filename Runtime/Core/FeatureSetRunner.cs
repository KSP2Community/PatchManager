using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KSP.Game.Flow;
using PatchManager.Core.Assets;
using UnityEngine;

namespace PatchManager.Core
{
    /// <summary>
    /// Switches Patch Manager to another feature set outside the loading flow, building the set's cache first
    /// when it is missing or stale.
    /// </summary>
    /// <remarks>
    /// Runs the same steps boot runs for its own set. Assets already loaded from the previous set stay loaded.
    /// </remarks>
    public static class FeatureSetRunner
    {
        /// <summary>
        /// Gets whether a set is being switched to.
        /// </summary>
        public static bool IsRunning { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            IsRunning = false;
        }

        /// <summary>
        /// Starts switching to a feature set.
        /// </summary>
        /// <param name="features">The set's enabled features, already closed over their requirements.</param>
        /// <param name="forceRebuild">Whether to rebuild the set's cache even when it is valid.</param>
        /// <param name="onStep">Called with each step's name as it starts, or null.</param>
        /// <param name="onDone">Called with null on success or the failure reason, or null.</param>
        public static void Run(
            IEnumerable<string> features,
            bool forceRebuild,
            Action<string> onStep,
            Action<string> onDone
        )
        {
            if (IsRunning)
            {
                onDone?.Invoke("A feature set is already being switched to.");
                return;
            }

            IsRunning = true;
            CoroutineUtil.Instance.DoCoroutine(RunSteps(features.ToList(), forceRebuild, onStep, onDone));
        }

        private static IEnumerator RunSteps(
            List<string> features,
            bool forceRebuild,
            Action<string> onStep,
            Action<string> onDone
        )
        {
            string error = null;
            var steps = new List<GenericFlowAction>();
            var index = 0;
            try
            {
                PatchingManager.ActivateSet(features);
                steps.AddRange(
                    PatchingManager.SetRunSteps(forceRebuild, next => steps.InsertRange(index + 1, next), out _)
                );
            }
            catch (Exception e)
            {
                error = e.ToString();
            }

            for (; error == null && index < steps.Count; index++)
            {
                var step = steps[index];
                onStep?.Invoke(step.Name);
                var finished = false;
                try
                {
                    step.Do(
                        _ => finished = true,
                        (_, reason) =>
                        {
                            error = reason ?? $"{step.Name} failed.";
                            finished = true;
                        }
                    );
                }
                catch (Exception e)
                {
                    error = e.ToString();
                    finished = true;
                }

                while (!finished)
                {
                    yield return null;
                }
            }

            IsRunning = false;
            onDone?.Invoke(error);
        }
    }
}
