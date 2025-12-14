/*
 * BSD 3-Clause License
 *
 * Copyright (c) 2016, Nicolas Weber, Sandra C. Amend / GCC / TU-Darmstadt
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of the <organization> nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * HLSL/Unity version Copyright (c) 2024, Chelsea "Feilen" Jaggi and VRChat Inc
 * The following is also licensed under the BSD 3-Clause License.
 *
 * This is a HLSL/Unity Compute shader implementation of the original cuda code 
 * available at https://github.com/mergian/dpid, adjusted to be used as an 
 * AssetPostprocessor (in-SDK) and a fully GPU-side texture updater (in-game) 
 * for generating mipmaps in Unity. The algorithm is intended to intentionally 
 * over-emphasize 'perceptually relevant' details, avoiding the need for 
 * post-sharpening typical in traditional downscaling algorithms.
 *
 * This file gets compiled into the SDKBase DLL, but we want to keep it open
 * source, so it's been copied to .Examples directory to keep Unity from trying
 * to compile it.
 */
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System;
using VRC.Core;
using VRC.SDKBase.Editor;
using System.IO;

public class DPIDMipmapper
{
    private static DPIDMipmapper Instance { get { return _instance ?? (_instance = new DPIDMipmapper()); } }
    private static DPIDMipmapper _instance;

    private ComputeShader computeShader;
    private int kernelDownsampling;
    private int kernelGuidance;

    public static bool ComputeShaderReady { get { return Instance.computeShader != null; } }

    private const int THREADS = 64;

    private static int TmpGuidanceProperty = Shader.PropertyToID("_TmpGuidance");
    private static int OutputProperty = Shader.PropertyToID("_Output");

    private DPIDMipmapper()
    {
        if (Application.isBatchMode)
        {
            Debug.Log("DPIDMipmapper is not supported in batch mode.");
            return;
        }
        computeShader = Resources.Load<ComputeShader>("PerceptualMipmapping/PerceptualPostProcessor");

        if (computeShader == null)
        {
            // ComputeShaders load after Textures import. We set a flag here and force all textures imported this way to re-import after the compute shader.
            // This also lets us detect when we need to forcibly re-import Kaiser textures the first time.
            return;
        }
        kernelGuidance = computeShader.FindKernel("KernelGuidance");
        kernelDownsampling = computeShader.FindKernel("KernelDownsampling");
    }

    /// <summary>
    /// Generate DPID mipmaps for a texture.
    /// </summary>
    /// <param name="input">The input texture to generate mipmaps for.</param>
    /// <param name="output">The output texture to write the mipmaps to.</param>
    /// <param name="alphaIsTransparency">Whether the alpha channel should be treated as transparency (premultiplied to prevent edge bleed).</param>
    /// <param name="sRGB">Whether the output texture should be sRGB.</param>
    /// <param name="asyncOnGPU">Whether to only use the GPU for mip generation - this doesn't read-back mips from the GPU, and expects input and output formats to be the same. This will immediately return control.</param>
    /// <param name="inPlace">Whether to generate mipmaps in-place (using the first mipmap as the input texture)</param>
    /// <param name="inputIsGuidance">Whether we expect our input texture is already what we use for a guidance texture, e.g. a box-mipped texture</param>
    /// <param name="conservative">Whether to use a more conservative algorithm that doesn't over-emphasize details</param>
    /// <param name="minimumSize">The minimum size we care about scaling. At the default of 4x4, we stop processing anything below 5 pixels in width or height (and therefore save 3 passes)</param>
    public static void GenerateDPIDMipmaps(Texture2D input, Texture2D output, bool alphaIsTransparency, bool sRGB, bool asyncOnGPU = false, bool inPlace = true, bool inputIsGuidance = false, bool conservative = false, uint minimumSize = 4U, bool normalMap = false)
    {
        Instance.ExecuteComputeShader(input, output, alphaIsTransparency, sRGB, asyncOnGPU, inPlace, inputIsGuidance, conservative, minimumSize, normalMap: normalMap);
    }

    /// <summary>
    /// Generate DPID mipmaps for a texture - convienience method for in-place, CPU+GPU generation for highest quality.
    /// </summary>
    /// <param name="input">The input texture to generate mipmaps for.</param>
    /// <param name="output">The output texture to write the mipmaps to.</param>
    /// <param name="alphaIsTransparency">Whether the alpha channel should be treated as transparency (premultiplied to prevent edge bleed).</param>
    /// <param name="sRGB">Whether the output texture should be sRGB.</param>
    /// <param name="conservative">Whether to use a more conservative algorithm that doesn't over-emphasize details</param>
    public static void GenerateDPIDMipmapsQuality(Texture2D input, Texture2D output, bool alphaIsTransparency, bool sRGB, bool conservative = false, bool normalMap = false)
    {
        bool inPlace = input.width == output.width && input.height == output.height;
        Instance.ExecuteComputeShader(input, output, alphaIsTransparency, sRGB, false, inPlace, false, conservative, normalMap: normalMap);
    }

    /// <summary>
    /// Generate DPID mipmaps for a texture - convienience method for in-place, GPU-only generation.
    /// </summary>
    /// <param name="texture">The texture to generate mipmaps for.</param>
    /// <param name="alphaIsTransparency">Whether the alpha channel should be treated as transparency (premultiplied to prevent edge bleed).</param>
    /// <param name="sRGB">Whether the output texture should be sRGB.</param>
    /// <param name="conservative">Whether to use a more conservative algorithm that doesn't over-emphasize details</param>
    public static void GenerateDPIDMipmapsFast(Texture2D texture, bool alphaIsTransparency, bool sRGB, bool conservative = false, bool normalMap = false)
    {
        Instance.ExecuteComputeShader(texture, texture, alphaIsTransparency, sRGB, true, true, true, conservative, normalMap: normalMap);
    }
    
    /// <summary>
    /// Execute the compute shader to generate mipmaps using the DPID algorithm.
    /// </summary>
    /// <param name="input">The input texture to generate mipmaps for.</param>
    /// <param name="output">The output texture to write the mipmaps to.</param>
    /// <param name="alphaIsTransparency">Whether the alpha channel should be treated as transparency (premultiplied to prevent edge bleed).</param>
    /// <param name="sRGB">Whether the output texture should be sRGB.</param>
    /// <param name="asyncOnGPU">Whether to only use the GPU for mip generation - this doesn't read-back mips from the GPU, and expects input and output formats to be the same. This will immediately return control.</param>
    /// <param name="inPlace">Whether to generate mipmaps in-place (using the first mipmap as the input texture)</param>
    /// <param name="inputIsGuidance">Whether we expect our input texture is already what we use for a guidance texture, e.g. a box-mipped texture</param>
    /// <param name="minimumSize">The minimum size we care about scaling. At the default of 4x4, we stop processing anything below 5 pixels in width or height (and therefore save 3 passes)</param> 
    private void ExecuteComputeShader(Texture2D input, Texture2D output, bool alphaIsTransparency, bool sRGB, bool asyncOnGPU = false, bool inPlace = true, bool inputIsGuidance = false, bool conservative = false, uint minimumSize = 4U, bool normalMap = false)
    {
        if (input == null || output == null)
        {
            Debug.LogError("Input and output textures must be non-null, input: {input != null}, output: {output != null}");
            return;
        }
        if (asyncOnGPU && (output.format != TextureFormat.RGBA32) && (output.format != TextureFormat.ARGB32))
        {
#if UNITY_EDITOR
            Debug.Log($"Currently DPIDMipmapper only supports ARGB textures when on-GPU, currently: {output.format}");
#endif
            return;
        }
        if (Application.isBatchMode)
        {
            Debug.LogError("DPIDMipmapper is not supported in batch mode.");
            return;
        }
        if (inputIsGuidance && (alphaIsTransparency || sRGB))
        {
            // Unity's runtime mipmapping always runs with the equivalent of alphaIsTransparency == false, so we have to run our own
            // InputIsGuidance also doesn't seem to like sRGB
            inputIsGuidance = false;
        }
        if (inputIsGuidance && 
            (input.format == TextureFormat.RGB24) || 
            (input.format == TextureFormat.RGB565) ||
            (input.format == TextureFormat.RGBA4444) ||
            (input.format == TextureFormat.R8) ||
            (input.format == TextureFormat.R16) ||
            (input.format == TextureFormat.RG16) ||
            (input.format == TextureFormat.ARGB4444) ||
            (input.format == TextureFormat.RGBA64))
        {
            // inputIsGuidance doesn't like non-RGBA formats
            inputIsGuidance = false;
        }
        if (sRGB && (input.format == TextureFormat.R16 ||
                     input.format == TextureFormat.RG16 ||
                     input.format == TextureFormat.RGFloat ||
                     input.format == TextureFormat.RFloat ||
                     input.format == TextureFormat.RHalf ||
                     input.format == TextureFormat.RGBA64 ||
                     input.format == TextureFormat.RGBAFloat ||
                     input.format == TextureFormat.RGBAHalf ||
                     input.format == TextureFormat.R8 ||
                     input.format == TextureFormat.RGBA4444 ||
                     input.format == TextureFormat.RGB565 ||
                     input.format == TextureFormat.ARGB4444))
        {
            // Texture format does not support sRGB, will be tested as Linear to maintain consistency with Unity importer behavior
            sRGB = false;
        }
        if (normalMap)
        {
            alphaIsTransparency = false;
        }
        if (!ComputeShaderReady)
        {
            // Double check that this is in fact the first import
            computeShader = (ComputeShader)Resources.Load("PerceptualMipmapping/PerceptualPostProcessor");
            if (!ComputeShaderReady)
            {
                // Textures must be marked for re-import outside of DPIDMipmapper, since we don't have UnityEditor access.
                return;
            }
            // If not, clear the flag and proceed as normal
            kernelGuidance = computeShader.FindKernel("KernelGuidance");
            kernelDownsampling = computeShader.FindKernel("KernelDownsampling");
        }

        CommandBuffer commandBuffer = new CommandBuffer();
        if (asyncOnGPU)
        {
            commandBuffer.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);
        }

        int startMip = inPlace? 1 : 0;

        commandBuffer.SetComputeTextureParam(computeShader, kernelDownsampling, "_Input", input);
        commandBuffer.SetComputeTextureParam(computeShader, kernelGuidance, "_Input", input);
        int iWidth = input.width;
        int iHeight = input.height;
        float lambda = conservative ? 0.5f : 1.0f;
        commandBuffer.SetComputeIntParam(computeShader, "iWidth", iWidth);
        commandBuffer.SetComputeIntParam(computeShader, "iHeight", iHeight);
        commandBuffer.SetComputeFloatParam(computeShader, "lambda", lambda);
        commandBuffer.SetComputeIntParam(computeShader, "premultiplyAlpha", alphaIsTransparency ? 1 : 0);
        // output sRGB from compute shader with a variant, avoid blits
        commandBuffer.SetComputeIntParam(computeShader, "sRGB", sRGB ? 1 : 0);
        commandBuffer.SetComputeIntParam(computeShader, "normalMap", normalMap ? 1 : 0);

        int rtWidth = output.width;
        int rtHeight = output.height;

        RenderTextureFormat intermediateFormat = RenderTextureFormat.ARGB32;
        switch(input.format)
        {
            case TextureFormat.RGBAFloat:
                intermediateFormat = RenderTextureFormat.ARGBFloat;
                break;
            case TextureFormat.RGBA32:
            case TextureFormat.ARGB32:
            case TextureFormat.RGB24:
            case TextureFormat.RGB565: // Not supported for random write
                intermediateFormat = RenderTextureFormat.ARGB32;
                break;
            case TextureFormat.RGBAHalf:
                intermediateFormat = RenderTextureFormat.ARGBHalf;
                break;
            case TextureFormat.RGBA4444:
                intermediateFormat = RenderTextureFormat.ARGB4444;
                break;
            case TextureFormat.RFloat:
                intermediateFormat = RenderTextureFormat.RFloat;
                break;
            case TextureFormat.RGFloat:
                intermediateFormat = RenderTextureFormat.RGFloat;
                break;
            case TextureFormat.RHalf:
                intermediateFormat = RenderTextureFormat.RHalf;
                break;
            case TextureFormat.RGHalf:
                intermediateFormat = RenderTextureFormat.RGHalf;
                break;
            case TextureFormat.R8:
                intermediateFormat = RenderTextureFormat.R8;
                break;
            case TextureFormat.R16:
                intermediateFormat = RenderTextureFormat.R16;
                break;
        }

        RenderTextureDescriptor outputTextureDesc = new RenderTextureDescriptor(rtWidth, rtHeight, intermediateFormat, 0);
        outputTextureDesc.sRGB = false;
        outputTextureDesc.autoGenerateMips = false;
        outputTextureDesc.enableRandomWrite = true;
        outputTextureDesc.useMipMap = true;
        commandBuffer.GetTemporaryRT(OutputProperty, outputTextureDesc);

        RenderTargetIdentifier outputTexture = new RenderTargetIdentifier(OutputProperty);
        RenderTargetIdentifier inputTexture = new RenderTargetIdentifier(input);

        for (int mip = startMip; mip < output.mipmapCount; mip++)
        {
            int downscaleFactor = 1 << mip;

            int oWidth = Math.Max(output.width / downscaleFactor, 1);
            int oHeight = Math.Max(output.height / downscaleFactor, 1);
            if (asyncOnGPU && (oWidth <= minimumSize && oHeight <= minimumSize))
            {
                break;
            }
            float pWidth = (float)input.width / oWidth;
            float pHeight = (float)input.height / oHeight;

            commandBuffer.SetComputeIntParam(computeShader, "oWidth", oWidth);
            commandBuffer.SetComputeIntParam(computeShader, "oHeight", oHeight);
            commandBuffer.SetComputeFloatParam(computeShader, "pWidth", pWidth);
            commandBuffer.SetComputeFloatParam(computeShader, "pHeight", pHeight);

            // TODO: apparently you can't bind input Texture2Ds by-mip, it'll just silently bind mip 0. Before removing this,
            //       confirm with DEBUG_GUIDANCE in the compute shader.
            RenderTextureDescriptor tmpGuidanceTextureRTDesc = new RenderTextureDescriptor(oWidth, oHeight, intermediateFormat, 0);
            tmpGuidanceTextureRTDesc.sRGB = false;
            tmpGuidanceTextureRTDesc.autoGenerateMips = false;
            tmpGuidanceTextureRTDesc.enableRandomWrite = !inputIsGuidance;
            tmpGuidanceTextureRTDesc.useMipMap = false;
            commandBuffer.GetTemporaryRT(TmpGuidanceProperty, tmpGuidanceTextureRTDesc);
            RenderTargetIdentifier tmpGuidanceTexture = new RenderTargetIdentifier(TmpGuidanceProperty);

            if (inputIsGuidance)
            {
                commandBuffer.CopyTexture(inputTexture, 0, mip, tmpGuidanceTexture, 0, 0);
            }
            else
            {
                commandBuffer.SetComputeTextureParam(computeShader, kernelGuidance, "_Output", tmpGuidanceTexture);
                commandBuffer.DispatchCompute(computeShader, kernelGuidance, Math.Max(oWidth, 1), Math.Max(oHeight, 1), 1);
            }

            // TODO: I might be able to modify the algorithm to perform some form of 'hinting' by having a cutoff for contribution at a certain range. Then you'd get even sharper edges on text and such
            // TODO: compute shader could batch based on samples, not output pixels - this would spread more evenly
            // TODO: how can we get rid of the alpha channel? downcasting RGBA to RGB is apparently weirdly difficult
            commandBuffer.SetComputeTextureParam(computeShader, kernelDownsampling, "_Guidance", tmpGuidanceTexture);
            commandBuffer.SetComputeTextureParam(computeShader, kernelDownsampling, "_Output", outputTexture, mip);
            commandBuffer.DispatchCompute(computeShader, kernelDownsampling, Math.Max(oWidth, 1), Math.Max(oHeight, 1), 1);
            if(asyncOnGPU)
            {
                commandBuffer.CopyTexture(outputTexture, 0, mip, output, 0, mip);
            }
            commandBuffer.ReleaseTemporaryRT(TmpGuidanceProperty);
        }
        if (!asyncOnGPU)
        {
            RenderTexture exportTexture = new RenderTexture(outputTextureDesc);
            for (int mip = startMip; mip < output.mipmapCount; mip++)
            {
                int downscaleFactor = 1 << mip;

                int oWidth = Math.Max(output.width / downscaleFactor, 1);
                int oHeight = Math.Max(output.height / downscaleFactor, 1);
                if (asyncOnGPU && (oWidth <= minimumSize && oHeight <= minimumSize))
                {
                    break;
                }
                commandBuffer.CopyTexture(outputTexture, 0, mip, exportTexture, 0, mip);
                int mipCopy = mip;
                commandBuffer.RequestAsyncReadback(exportTexture, mip, output.graphicsFormat, (AsyncGPUReadbackRequest req) =>
                        {
                            if (req.hasError)
                            {
                                Debug.LogError("GPU readback error detected.");
                                return;
                            }
                            if (req.done)
                            {
                                output.SetPixelData(req.GetData<byte>(), mipCopy);
                            }
                        });
            }
            commandBuffer.WaitAllAsyncReadbackRequests();
            commandBuffer.ReleaseTemporaryRT(OutputProperty);
            Graphics.ExecuteCommandBuffer(commandBuffer);
            exportTexture.Release();
        }
        else
        {
            commandBuffer.ReleaseTemporaryRT(OutputProperty);
            Graphics.ExecuteCommandBufferAsync(commandBuffer, ComputeQueueType.Background);
        }
    }
}
