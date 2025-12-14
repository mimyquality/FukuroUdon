using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class StripDuplicatedStereo : IPreprocessShaders
{
    public int callbackOrder => 1024;

    private readonly ShaderKeyword _unitySinglePassStereoKeyword;
    private readonly ShaderKeyword _stereoInstancingKeyword;

    public StripDuplicatedStereo()
    {
        _unitySinglePassStereoKeyword = new ShaderKeyword("UNITY_SINGLE_PASS_STEREO");
        _stereoInstancingKeyword = new ShaderKeyword("STEREO_INSTANCING_ON");
    }

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        for(int i = data.Count - 1; i >= 0; i--)
        {
            ShaderCompilerData entry = data[i];
            if(entry.buildTarget == BuildTarget.StandaloneWindows64)
            {
                if(entry.shaderKeywordSet.IsEnabled(_unitySinglePassStereoKeyword) && entry.shaderKeywordSet.IsEnabled(_stereoInstancingKeyword))
                {
                    data.RemoveAt(i);
                }
            }
            else
            {
                if(entry.shaderKeywordSet.IsEnabled(_unitySinglePassStereoKeyword))
                {
                    data.RemoveAt(i);
                }
            }
        }
    }
}
