using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace VRC.SDK3.Editor
{
    public class WorldBuilderSessionState
    {
        public const string SESSION_STATE_PREFIX = "VRC.SK3.Editor";
        public const string WORLD_NAME_KEY = SESSION_STATE_PREFIX + ".World.Name";
        public const string WORLD_DESC_KEY = SESSION_STATE_PREFIX + ".World.Desc";
        public const string WORLD_TAGS_KEY = SESSION_STATE_PREFIX + ".World.Tags";
        public const string WORLD_CAPACITY_KEY = SESSION_STATE_PREFIX + ".World.Capacity";
        public const string WORLD_RECOMMENDED_CAPACITY_KEY = SESSION_STATE_PREFIX + ".World.RecommendedCapacity";
        public const string WORLD_THUMBPATH_KEY = SESSION_STATE_PREFIX + ".World.ThumbPath";
        public const string WORLD_SELECTED_PLATFORMS_KEY = SESSION_STATE_PREFIX + ".World.Platforms";

        public static string WorldName
        {
            get => SessionState.GetString(WORLD_NAME_KEY, "");
            set => SessionState.SetString(WORLD_NAME_KEY, value);
        }

        public static string WorldDesc
        {
            get => SessionState.GetString(WORLD_DESC_KEY, "");
            set => SessionState.SetString(WORLD_DESC_KEY, value);
        }

        public static string WorldTags
        {
            get => SessionState.GetString(WORLD_TAGS_KEY, "");
            set => SessionState.SetString(WORLD_TAGS_KEY, value);
        }

        public static int WorldCapacity
        {
            get => SessionState.GetInt(WORLD_CAPACITY_KEY, 32);
            set => SessionState.SetInt(WORLD_CAPACITY_KEY, value);
        }
        
        public static int WorldRecommendedCapacity
        {
            get => SessionState.GetInt(WORLD_RECOMMENDED_CAPACITY_KEY, 16);
            set => SessionState.SetInt(WORLD_RECOMMENDED_CAPACITY_KEY, value);
        }

        public static string WorldThumbPath
        {
            get => SessionState.GetString(WORLD_THUMBPATH_KEY, "");
            set => SessionState.SetString(WORLD_THUMBPATH_KEY, value);
        }

        public static List<BuildTarget> WorldPlatforms
        {
            get
            {
                var loaded = SessionState.GetString(WORLD_SELECTED_PLATFORMS_KEY, string.Empty);
                if (string.IsNullOrWhiteSpace(loaded)) return new List<BuildTarget>();
                return loaded.Split('|').Select(s => (BuildTarget) int.Parse(s)).ToList();
            }
            set
            {
                var serialized = string.Join("|", value.Select(t => ((int) t).ToString()));
                SessionState.SetString(WORLD_SELECTED_PLATFORMS_KEY, serialized);
            }
        }

        public static void Clear()
        {
            SessionState.EraseString(WORLD_NAME_KEY);
            SessionState.EraseString(WORLD_DESC_KEY);
            SessionState.EraseString(WORLD_TAGS_KEY);
            SessionState.EraseInt(WORLD_CAPACITY_KEY);
            SessionState.EraseString(WORLD_THUMBPATH_KEY);
        }
    }
}