using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using VRC.SDKBase.Editor.Api;


[assembly: InternalsVisibleTo("VRC.SDK3A.Editor")]
[assembly: InternalsVisibleTo("VRC.SDK3.Editor")]

namespace VRC.SDKBase
{
    internal static class VRCMultiPlatformBuild
    {
        private const string SESSION_STATE_PREFIX = "VRCSdkControlPanel";
        private const string MPB_KEY = SESSION_STATE_PREFIX + ".MultiPlatformBuild";
        private const string MPB_PLATFORMS_LIST_KEY = SESSION_STATE_PREFIX + MPB_KEY + ".PlatformsList";
        private const string MPB_INITIAL_PLATFORM_KEY = SESSION_STATE_PREFIX + MPB_KEY + ".InitialPlatform";
        private const string MPB_NEXT_PLATFORM_KEY = SESSION_STATE_PREFIX + MPB_KEY + ".NextPlatform";
        private const string MPB_BUILT_COUNT_KEY = SESSION_STATE_PREFIX + MPB_KEY + ".BuiltCount";
        private const string MPB_STATE_KEY = SESSION_STATE_PREFIX + MPB_KEY + ".State";
        private const string MPB_PROGRESS_KEY = SESSION_STATE_PREFIX + MPB_KEY + ".Progress";
        private const string MPB_CONTENT_IDENTIFIER = SESSION_STATE_PREFIX + MPB_KEY + ".ContentIdentifier";
        
        // MultiPlatformBuild
        public static bool MPB
        {
            get => SessionState.GetBool(MPB_KEY, false);
            set => SessionState.SetBool(MPB_KEY, value);
        }
        
        public static List<BuildTarget> MPBPlatformsList
        {
            get
            {
                var loaded = SessionState.GetString(MPB_PLATFORMS_LIST_KEY, string.Empty);
                if (string.IsNullOrWhiteSpace(loaded)) return new List<BuildTarget>();
                return loaded.Split('|').Select(s => (BuildTarget) int.Parse(s)).ToList();
            }
            set
            {
                var serialized = string.Join("|", value.Select(t => ((int) t).ToString()));
                SessionState.SetString(MPB_PLATFORMS_LIST_KEY, serialized);
            }
        }

        public static BuildTarget MPBInitialPlatform
        {
            get => (BuildTarget) SessionState.GetInt(MPB_INITIAL_PLATFORM_KEY, (int) BuildTarget.NoTarget);
            set => SessionState.SetInt(MPB_INITIAL_PLATFORM_KEY, (int) value);
        }
        
        public static BuildTarget MPBNextPlatform
        {
            get => (BuildTarget) SessionState.GetInt(MPB_NEXT_PLATFORM_KEY, (int) BuildTarget.NoTarget);
            set => SessionState.SetInt(MPB_NEXT_PLATFORM_KEY, (int) value);
        }
        
        public enum MultiPlatformBuildState
        {
            Idle,
            Building,
            Switching
        }

        public static MultiPlatformBuildState MPBState
        {
            get => (MultiPlatformBuildState) SessionState.GetInt(MPB_STATE_KEY, (int) MultiPlatformBuildState.Idle);
            set => SessionState.SetInt(MPB_STATE_KEY, (int) value);
        }
        
        public static int MPBBuiltCount
        {
            get => SessionState.GetInt(MPB_BUILT_COUNT_KEY, 0);
            set => SessionState.SetInt(MPB_BUILT_COUNT_KEY, value);
        }
        
        public static int MPBProgress
        {
            get => SessionState.GetInt(MPB_PROGRESS_KEY, -1);
            set => SessionState.SetInt(MPB_PROGRESS_KEY, value);
        }
        
        public static string MPBContentIdentifier
        {
            get => SessionState.GetString(MPB_CONTENT_IDENTIFIER, string.Empty);
            set => SessionState.SetString(MPB_CONTENT_IDENTIFIER, value);
        }
        
        [MenuItem("VRChat SDK/Utilities/Clear Multi Platform Build State")]
        public static void ClearMPBState()
        {
            SessionState.EraseBool(MPB_KEY);
            SessionState.EraseString(MPB_PLATFORMS_LIST_KEY);
            SessionState.EraseInt(MPB_INITIAL_PLATFORM_KEY);
            SessionState.EraseInt(MPB_NEXT_PLATFORM_KEY);
            SessionState.EraseInt(MPB_STATE_KEY);
            SessionState.EraseInt(MPB_PROGRESS_KEY);
            SessionState.EraseInt(MPB_BUILT_COUNT_KEY);
            SessionState.EraseString(MPB_CONTENT_IDENTIFIER);
        }
        
        private const int TOTAL_PER_PLATFORM_STEPS = 2;
        
        internal static int StartMPB(List<BuildTarget> platforms)
        {
            if (!MPB)
            {
                MPBPlatformsList = platforms;
            }
            return StartMPB();
        }
        
        internal static int StartMPB()
        {
            if (!MPB)
            {
                MPB = true;
                if (MPBPlatformsList.Count == 0)
                {
                    // Inject default order if it is not specified
                    MPBPlatformsList = new List<BuildTarget> { BuildTarget.StandaloneWindows64, BuildTarget.Android, BuildTarget.iOS };
                }
                // Save the initial target to return to
                MPBInitialPlatform = VRC_EditorTools.GetCurrentBuildTargetEnum();
            }
            
            if (Progress.Exists(MPBProgress))
            {
                Progress.Report(MPBProgress, MPBBuiltCount * TOTAL_PER_PLATFORM_STEPS, MPBPlatformsList.Count * TOTAL_PER_PLATFORM_STEPS, $"Building for {VRC_EditorTools.GetCurrentBuildTarget()}");
            }
            else
            {
                MPBProgress = Progress.Start("Multi Platform Build",
                    $"Building for {string.Join(", ", MPBPlatformsList.Select(VRC_EditorTools.GetTargetName))}", Progress.Options.Unmanaged | Progress.Options.Synchronous | Progress.Options.Sticky);
            }

            // Only kick off the Publishing process if we're on the platform that is in the list
            // And we're actually intending to build it right now
            // Otherwise we'll be switching to a different platform before we start
            if (MPBPlatformsList.Contains(VRC_EditorTools.GetCurrentBuildTargetEnum()) && 
                ((MPBState == MultiPlatformBuildState.Idle && MPBInitialPlatform != MPBPlatformsList.Last()) || MPBState == MultiPlatformBuildState.Switching)
            )
            {
                MPBState =
                    MultiPlatformBuildState.Building;
                
                return Progress.Start($"Publishing for {VRC_EditorTools.GetCurrentBuildTarget()}", null,
                    Progress.Options.Sticky | Progress.Options.Unmanaged | Progress.Options.Synchronous,
                    MPBProgress);
            }

            return -1;
        }

        internal static bool ShouldContinueMPB(out bool isMPBFinished)
        {
            isMPBFinished = false;
            if (!MPB) return false;
            
            if (MPBState == MultiPlatformBuildState.Building)
            {
                return false;
            }

            // If already building, don't start a new build
            if (MPBState != MultiPlatformBuildState.Switching) return false;
            
            // If not on the expected platform - clear MPB state
            if (VRC_EditorTools.GetCurrentBuildTargetEnum() !=
                MPBNextPlatform)
            {
                if (Progress.Exists(MPBProgress))
                {
                    Progress.Report(MPBProgress, MPBPlatformsList.Count * TOTAL_PER_PLATFORM_STEPS, MPBPlatformsList.Count * TOTAL_PER_PLATFORM_STEPS, "Canceled");
                    Progress.Finish(MPBProgress, Progress.Status.Canceled);
                }
                ClearMPBState();
                return false;
            }
            
            // Continue if we haven't reached initial platform
            if (MPBInitialPlatform != VRC_EditorTools.GetCurrentBuildTargetEnum())
            {
                return true;
            }
            // If the initial platform is last in the list (which can happen for Avatar builds) - ensure the build is made
            if (MPBInitialPlatform == MPBPlatformsList.Last())
            {
                return true;
            }
            
            // If we arrived at the platform we started on - exit and clear MPB state
            isMPBFinished = true;
            ReportMPBDone();
            return false;

        }

        internal static void ReportMPBUploadStart(int progressId)
        {
            Progress.Report(MPBProgress, MPBBuiltCount * TOTAL_PER_PLATFORM_STEPS + 1, MPBPlatformsList.Count * TOTAL_PER_PLATFORM_STEPS, $"Uploading for {VRC_EditorTools.GetCurrentBuildTarget()}");
            Progress.Report(progressId, 1, TOTAL_PER_PLATFORM_STEPS, $"Uploading for {VRC_EditorTools.GetCurrentBuildTarget()}");
        }

        internal static void ReportMPBUploadFinish(int progressId)
        {
            Progress.Report(progressId, TOTAL_PER_PLATFORM_STEPS, TOTAL_PER_PLATFORM_STEPS, $"Built & Uploaded for {VRC_EditorTools.GetCurrentBuildTarget()}");
            Progress.Finish(progressId);
        }

        internal static void ReportMPBUploadSkipped(int progressId)
        {
            Progress.Report(progressId, TOTAL_PER_PLATFORM_STEPS, TOTAL_PER_PLATFORM_STEPS, $"{VRC_EditorTools.GetCurrentBuildTarget()} bundle already uploaded, upload skipped");
            Progress.Finish(progressId, Progress.Status.Canceled);
        }

        internal static void ReportMPBDone()
        {
            if (Progress.Exists(MPBProgress))
            {
                Progress.Report(MPBProgress, MPBPlatformsList.Count * TOTAL_PER_PLATFORM_STEPS, MPBPlatformsList.Count * TOTAL_PER_PLATFORM_STEPS, "Finished");
                Progress.Finish(MPBProgress);
            }
            ClearMPBState();
        }

        internal static async Task<bool> SetUpNextMPBTarget(CancellationToken cancellationToken = default, bool incrementBuildCount = true)
        {
            // If we just built the final target, we're done
            if (MPBInitialPlatform == VRC_EditorTools.GetCurrentBuildTargetEnum() &&
                MPBInitialPlatform == MPBPlatformsList.Last() &&
                MPBBuiltCount + 1 == MPBPlatformsList.Count)
            {
                return false;
            }
            
            // If the initial platform was not in the list of target platforms
            var shouldManuallyLoopBack = !MPBPlatformsList.Contains(MPBInitialPlatform);
            // and if we reached the end of the platforms list
            if (shouldManuallyLoopBack && MPBPlatformsList.Last() == VRC_EditorTools.GetCurrentBuildTargetEnum())
            {
                // Loop back to initial platform and exit
                MPBState = MultiPlatformBuildState.Switching;
                MPBNextPlatform = MPBInitialPlatform;
                VRCApiCache.Clear();
                EditorUserBuildSettings.selectedBuildTargetGroup = VRC_EditorTools.GetBuildTargetGroupForTarget(MPBInitialPlatform);
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(EditorUserBuildSettings.selectedBuildTargetGroup, MPBInitialPlatform);
                return true;
            }
            GetNextMPBTarget(VRC_EditorTools.GetCurrentBuildTargetEnum(), out var nextTarget, out var nextTargetGroup);
            if (nextTarget == BuildTarget.NoTarget)
            {
                Core.Logger.LogError("No valid next target found for MultiPlatformBuild, you should install the required modules for Android and iOS");
                if (Progress.Exists(MPBProgress))
                {
                    Progress.Report(MPBProgress, MPBPlatformsList.Count * TOTAL_PER_PLATFORM_STEPS, MPBPlatformsList.Count * TOTAL_PER_PLATFORM_STEPS, "Failed to switch platform");
                    Progress.Finish(MPBProgress, Progress.Status.Failed);
                }
                ClearMPBState();
                return false;
            }
            
            MPBNextPlatform = nextTarget;
            MPBState = MultiPlatformBuildState.Switching;

            // Only increment build count for platforms in the list
            if (MPBPlatformsList.Contains(VRC_EditorTools.GetCurrentBuildTargetEnum()) && incrementBuildCount)
            {
                MPBBuiltCount++;
            }
            
            // Leave time for the UI to update
            await Task.Delay(100, cancellationToken);
            
            // Clear the API cache as it sometimes persists during assembly reloads
            VRCApiCache.Clear();
            
            EditorUserBuildSettings.selectedBuildTargetGroup = nextTargetGroup;
            EditorUserBuildSettings.SwitchActiveBuildTargetAsync(nextTargetGroup, nextTarget);
            return true;
        }
        

        private static void GetNextMPBTarget(BuildTarget currentTarget, out BuildTarget nextTarget, out BuildTargetGroup nextTargetGroup)
        {
            // If somehow arrived without a correct order - exit
            if (MPBPlatformsList.Count == 0)
            {
                nextTarget = BuildTarget.NoTarget;
                nextTargetGroup = BuildTargetGroup.Unknown;
                return;
            }
            
            var currentTargetIndex = MPBPlatformsList.IndexOf(currentTarget);
            // Handle cases where we're starting on the platform not in the list
            if (currentTargetIndex == -1)
            {
                nextTarget = MPBPlatformsList[0];
            }
            else
            {
                nextTarget = MPBPlatformsList[(currentTargetIndex + 1) % MPBPlatformsList.Count];
            }

            nextTargetGroup = VRC_EditorTools.GetBuildTargetGroupForTarget(nextTarget);
            
            if (!VRC_EditorTools.IsBuildTargetSupported(nextTarget))
            {
                nextTarget = BuildTarget.NoTarget;
            }
        }
    }
}