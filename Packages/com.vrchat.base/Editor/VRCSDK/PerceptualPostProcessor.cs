/*
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
 * This is a C# implementation of the original cuda code available at 
 * https://github.com/mergian/dpid, adjusted to be used as an AssetPostprocessor
 * for generating mipmaps in Unity. The algorithm is intended to intentionally
 * over-emphasize 'perceptually relevant' details, avoiding the need for 
 * post-sharpening typical in traditional downscaling algorithms.
 */

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System;
using VRC.Core;
using VRC.SDKBase.Editor;
using System.IO;

public class PerceptualPostProcessor : AssetPostprocessor
{
    void OnPostprocessTexture(Texture2D texture)
    {
        if (!VRCPackageSettings.Instance.dpidMipmaps)
            return;
        TextureImporter textureImporter = (TextureImporter)assetImporter;
        // Skip for Box filtered textures
        if (textureImporter.mipmapFilter == TextureImporterMipFilter.BoxFilter)
            return;

        bool alphaIsTransparency = textureImporter.alphaIsTransparency;
        bool normalMap = textureImporter.textureType == TextureImporterType.NormalMap;

        bool fromFile = true;
        if (textureImporter.swizzleA != TextureImporterSwizzle.A || 
            textureImporter.swizzleB != TextureImporterSwizzle.B || 
            textureImporter.swizzleG != TextureImporterSwizzle.G || 
            textureImporter.swizzleR != TextureImporterSwizzle.R || 
            textureImporter.alphaSource != TextureImporterAlphaSource.FromInput ||
            textureImporter.convertToNormalmap != false ||
            textureImporter.fadeout != false ||
            textureImporter.flipGreenChannel != false ||
            textureImporter.textureShape != TextureImporterShape.Texture2D)
            fromFile = false;

        Texture2D fullTexture = new Texture2D(2, 2, texture.format, false, !((TextureImporter)assetImporter).sRGBTexture);
        if (fromFile)
        {
            if (!ImageConversion.LoadImage(fullTexture, File.ReadAllBytes(assetPath)))
            {
                fromFile = false;
                texture.Apply(false, false);
                fullTexture = texture;
            }
        }
        else
        {
            texture.Apply(false, false);
            fullTexture = texture;
        }

        DPIDMipmapper.GenerateDPIDMipmaps(fullTexture, texture, alphaIsTransparency, textureImporter.sRGBTexture, conservative: VRCPackageSettings.Instance.dpidConservative, inPlace: !fromFile, normalMap: normalMap);
        if (!DPIDMipmapper.ComputeShaderReady)
        {
            textureImporter.SaveAndReimport();
        }
    }
}
