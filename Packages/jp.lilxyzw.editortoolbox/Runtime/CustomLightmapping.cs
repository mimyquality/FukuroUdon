using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace jp.lilxyzw.editortoolbox.runtime
{
    [Docs(
        "CustomLightmapping",
        "By attaching this component to any object, you can customize its behavior during light mapping."
    )]
    [DocsHowTo("You can use it just by attaching this component to any object in the scene. Some settings cannot be changed from Unity's UI, so you need to use this component.")]
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(CustomLightmapping))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(CustomLightmapping))]
    [ExecuteAlways]
    internal class CustomLightmapping : EditorOnlyBehaviour
    {
        [Tooltip("The type of light attenuation. InverseSquared is a darker but physically correct attenuation than the default. InverseSquaredNoRangeAttenuation is also physically correct but ignores the range parameter. Linear is a non-physically correct linear attenuation, Legacy is the quadratic attenuation that Unity uses by default.")]
        public FalloffType falloffType = FalloffType.InverseSquared;

        [Tooltip("Multiplier for the light intensity. Does not affect directional lights. This option applies to non-directional lights, as it is intended to be used to adjust darkened scenes by changing the type of attenuation.")]
        public float intensityMultiplier = 1.0f;

        [Tooltip("Multiplier for the light range. Does not affect directional lights. This option applies to non-directional lights, as it is intended to be used to adjust darkened scenes by changing the type of attenuation.")]
        public float rangeMultiplier = 1.0f;

        #if UNITY_EDITOR
        void RequestLights(Light[] requests, Unity.Collections.NativeArray<LightDataGI> lightsOutput)
        {
            var dir = new DirectionalLight();
            var point = new PointLight();
            var spot = new SpotLight();
            var rect = new RectangleLight();
            var disc = new DiscLight();
            var cookie = new Cookie();
            var ld = new LightDataGI();

            for(int i = 0; i < requests.Length; i++)
            {
                var l = requests[i];
                switch(l.type)
                {
                    case UnityEngine.LightType.Directional: LightmapperUtils.Extract(l, ref dir); LightmapperUtils.Extract(l, out cookie); ld.Init(ref dir, ref cookie); break;
                    case UnityEngine.LightType.Point: LightmapperUtils.Extract(l, ref point); LightmapperUtils.Extract(l, out cookie); ld.Init(ref point, ref cookie); break;
                    case UnityEngine.LightType.Spot: LightmapperUtils.Extract(l, ref spot); LightmapperUtils.Extract(l, out cookie); ld.Init(ref spot, ref cookie); break;
                    case UnityEngine.LightType.Rectangle: LightmapperUtils.Extract(l, ref rect); LightmapperUtils.Extract(l, out cookie); ld.Init(ref rect, ref cookie); break;
                    case UnityEngine.LightType.Disc: LightmapperUtils.Extract(l, ref disc); LightmapperUtils.Extract(l, out cookie); ld.Init(ref disc, ref cookie); break;
                    default: ld.InitNoBake(l.GetInstanceID()); break;
                }

                if(ld.falloff != FalloffType.Undefined)
                {
                    ld.falloff = falloffType;
                    ld.color.intensity *= intensityMultiplier;
                    ld.indirectColor.intensity *= intensityMultiplier;
                    ld.range *= rangeMultiplier;
                }
                lightsOutput[i] = ld;
            }
        }

        void SetLightsDirty()
        {
            foreach(var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                UnityEditor.Experimental.Lightmapping.SetLightDirty(l);
        }

        void SetCustomLightmapping()
        {
            Lightmapping.SetDelegate(RequestLights);
            SetLightsDirty();
        }

        void ResetCustomLightmapping()
        {
            Lightmapping.ResetDelegate();
            SetLightsDirty();
        }

        void OnEnable() => SetCustomLightmapping();
        void OnValidate() => SetCustomLightmapping();
        void OnDisable() => ResetCustomLightmapping();
        #endif
    }
}
