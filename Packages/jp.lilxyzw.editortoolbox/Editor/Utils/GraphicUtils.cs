using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    internal static class GraphicUtils
    {
        internal static Texture ProcessTexture(Material material, Texture texture, int width, int height, bool forceLinear = false)
        {
            var currentRT = RenderTexture.active;
            var renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, forceLinear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default);
            RenderTexture.active = renderTexture;
            Graphics.Blit(texture, renderTexture, material);
            var outTex = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
            outTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            outTex.Apply();
            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(renderTexture);
            return outTex;
        }

        internal static Texture ProcessTexture(Material material, Texture texture, bool forceLinear = false) => ProcessTexture(material, texture, texture.width, texture.height, forceLinear);
        internal static Texture ProcessTexture(string shaderName, Texture texture, bool forceLinear = false) => ProcessTexture(new Material(Shader.Find(shaderName)), texture, forceLinear);
    }
}
