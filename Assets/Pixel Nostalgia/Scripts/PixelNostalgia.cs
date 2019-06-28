#if (UNITY_5_6) || (UNITY_2017_1) || (UNITY_2017_2)

#else

#endif

#if (POSTPROCESSSTACK_NOT_DOWNLOADED_GITHUB)

#warning Post Processing Stack V2 required : Download the Post Processing Stack from Unity Technologies GitHub: "https:\\github.com\Unity-Technologies\PostProcessing"

/* +----------------------------------------------------------------------------------------------------------------------------------------+
   |                   ######                                        ######                          #     #                                |
   |                   #     # #      ######   ##    ####  ######    #     # ######   ##   #####     ##   ## ######                         |
   |                   #     # #      #       #  #  #      #         #     # #       #  #  #    #    # # # # #                              |
   |                   ######  #      #####  #    #  ####  #####     ######  #####  #    # #    #    #  #  # #####                          |
   |                   #       #      #      ######      # #         #   #   #      ###### #    #    #     # #                              |
   |                   #       #      #      #    # #    # #         #    #  #      #    # #    #    #     # #                              |
   |                   #       ###### ###### #    #  ####  ######    #     # ###### #    # #####     #     # ######                         |
   +----------------------------------------------------------------------------------------------------------------------------------------+
   | When you have downloaded PostProcessingStack V2, comment out the 2nd line in this file, and in the PixelNostalgiaEditor.cs file,       |
   |   Thank you,                                                                                                                           |
   +----------------------------------------------------------------------------------------------------------------------------------------+ */

#elif (POSTPROCESSSTACK_NOT_DOWNLOADED_PACKAGE_MANAGER)

#warning Post Processing Stack V2 required : Get the Post Processing Stack v2 from the Package Manager

/* +----------------------------------------------------------------------------------------------------------------------------------------+
   | When you have downloaded PostProcessingStack V2, comment out the 4th line in this file, and in the PixelNostalgiaEditor.cs file,       |
   |   Thank you,                                                                                                                           |
   +----------------------------------------------------------------------------------------------------------------------------------------+ */

#else

using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace DFA
{
    [Serializable]
    [PostProcess(typeof(PixelNostalgiaRenderer), PostProcessEvent.AfterStack, "DFA/Pixel Nostalgia")]
    public sealed class PixelNostalgia : PostProcessEffectSettings
    {
        // These warnings are disabled because, I use variables inside of this script to
        // track inspector options, because they save.

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

        public Material pixelNostalgiaMaterial;
        public Texture2D[] bayerTextures = new Texture2D[4];
        public Texture asciiLUT;
        public IntParameter bayerIndex = new IntParameter { value = 3 };

        public RenderTexture pixelatedTexture;

        public Vector2Parameter windowSize = new Vector2Parameter { value = new Vector2(256, 240) };

        [Range(1, 8)] public IntParameter rBitDepth = new IntParameter { value = 8 };
        [Range(1, 8)] public IntParameter gBitDepth = new IntParameter { value = 8 };
        [Range(1, 8)] public IntParameter bBitDepth = new IntParameter { value = 8 };

        public BoolParameter grayscale = new BoolParameter { value = false };
        [Range(1, 8)] public IntParameter grayscaleDepth = new IntParameter { value = 8 };

        public void RecreateTexture()
        {
            pixelatedTexture = new RenderTexture((int)Mathf.Max(1, windowSize.value.x), (int)Mathf.Max(1, windowSize.value.y), 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            pixelatedTexture.filterMode = FilterMode.Point;
        }

        public void CreateMaterial()
        {
            Shader shader = Shader.Find("Hidden/PixelNostalgia");
            pixelNostalgiaMaterial = new Material(shader);
        }

        public readonly int[,] dithers = new int[,]
        {
        {   // 2x2
            0, 2, 3, 1, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
        },
        {   // 3x3
            0, 7, 3, 6, 5, 2, 4, 1,
            8, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
        },
        {   // 4x4
            0,  8,  2, 10, 12,  4, 14,  6,
            3, 11,  1,  9, 15,  7, 13,  5,
            0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,
            0,  0,  0,  0,  0,  0,  0,  0,
        },
        {   // 8x8
            0,  48, 12, 60, 3,  51, 15, 63,
            32, 16, 44, 28, 35, 19, 47, 31,
            8,  56, 4,  52, 11, 59,  7, 55,
            40, 24, 36, 20, 43, 27, 39, 23,
            2,  50, 14, 62, 1,  49, 13, 61,
            34, 18, 47, 30, 33, 17, 45, 29,
            10, 58, 6,  54, 9,  57, 5,  53,
            42, 26, 38, 22, 41, 25, 37, 21
        }
        };

        public IntParameter autoSelectSeparation = new IntParameter { value = 0 };
        [Range(1, 4096)] public IntParameter ditherSeparation = new IntParameter { value = 256 };

        public BoolParameter doDithering = new BoolParameter { value = true };
        public BoolParameter matchCameraSize = new BoolParameter { value = true };
        public BoolParameter doAscii = new BoolParameter { value = false };
        public BoolParameter doCRT = new BoolParameter { value = false };

        // If match camera size is on, set this as a scalar value
        public FloatParameter scalarSize = new FloatParameter { value = 0.25f };

        public BoolParameter disableInScene = new BoolParameter { value = false };
        public Vector2Parameter customWindowSize = new Vector2Parameter { value = new Vector2(256, 240) };
        public IntParameter selectedSizeOption = new IntParameter { value = 3 };
        public IntParameter selectedBitDepthOption = new IntParameter { value = 0 };

        public BoolParameter recreateTexture = new BoolParameter { value = false };

#pragma warning restore 0168 // variable declared but not used.
#pragma warning restore 0219 // variable assigned but not used.
#pragma warning restore 0414 // private field assigned but not used.
    }

    public sealed class PixelNostalgiaRenderer : PostProcessEffectRenderer<PixelNostalgia>
    {
        public override void Init()
        {
            if (settings.pixelNostalgiaMaterial == null)
            {
                settings.CreateMaterial();
            }

            if (!settings.pixelatedTexture)
            {
                settings.RecreateTexture();
            }

            if (settings.asciiLUT == null)
            {
                settings.asciiLUT = Resources.Load<Texture>("ascii-lut-gradient");
                settings.asciiLUT.filterMode = FilterMode.Point;
            }

            for (int i = 0; i < 4; i++)
            {
                if (settings.bayerTextures[i] == null)
                {
                    int dim = i == 0 ? 2 : i == 1 ? 3 : i == 2 ? 4 : 8;

                    // TODO: Get a D3d error when using R8 format on certain GPUs
                    settings.bayerTextures[i] = new Texture2D(dim, dim, TextureFormat.RGBA32, false);
                    settings.bayerTextures[i].filterMode = FilterMode.Point;
                    settings.bayerTextures[i].wrapMode = TextureWrapMode.Repeat;

                    byte[] rawData = new byte[dim * dim * 4];

                    for (int x = 0; x < dim; x++)
                    {
                        for (int y = 0; y < dim; y++)
                        {
                            float col = settings.dithers[i, x + y * dim] / (float)(dim * dim - 1);
                            var d = (byte)(col * 255 % 256);

                            rawData[(x + y * dim) * 4 + 0] = d;
                            rawData[(x + y * dim) * 4 + 1] = d;
                            rawData[(x + y * dim) * 4 + 2] = d;
                            rawData[(x + y * dim) * 4 + 3] = d;
                        }
                    }

                    settings.bayerTextures[i].LoadRawTextureData(rawData);
                    settings.bayerTextures[i].Apply();
                }
            }
        }

        public override void Render(PostProcessRenderContext context)
        {
            if (context.isSceneView && settings.disableInScene)
            {
                context.command.Blit(context.source, context.destination);
                return;
            }

            if (settings.recreateTexture)
            {
                settings.RecreateTexture();
                settings.recreateTexture.value = false;
            }
            
            if (settings.pixelNostalgiaMaterial == null)
            {
                settings.CreateMaterial();
            }

            Vector4 colorAmounts = new Vector4();

            colorAmounts.x = 256 / Mathf.Pow(2, settings.rBitDepth); // 0 to 256
            colorAmounts.y = 256 / Mathf.Pow(2, settings.gBitDepth); // 0 to 256
            colorAmounts.z = 256 / Mathf.Pow(2, settings.bBitDepth); // 0 to 256

            if (settings.grayscale)
                colorAmounts.w = 256 / Mathf.Pow(2, settings.grayscaleDepth); // 0 to 256
            else
                colorAmounts.w = -1.0f;

            settings.pixelNostalgiaMaterial.SetVector("bitsPerChannel", colorAmounts);
            settings.pixelNostalgiaMaterial.SetTexture("orderedBayer", settings.bayerTextures[settings.bayerIndex]);
            settings.pixelNostalgiaMaterial.SetInt("levels", settings.ditherSeparation);

            int ditherDim = settings.bayerIndex == 0 ? 2 : settings.bayerIndex == 1 ? 3 : settings.bayerIndex == 2 ? 4 : 8;

            RenderTexture endRes = RenderTexture.GetTemporary(context.camera.pixelWidth, context.camera.pixelHeight, 0);

            // Downsample into the pixelated texture
            if (settings.matchCameraSize)
            {
                RenderTexture t = RenderTexture.GetTemporary((int)(context.camera.pixelWidth * settings.scalarSize), (int)(context.camera.pixelHeight * settings.scalarSize), 0);
                t.filterMode = FilterMode.Point;
                settings.pixelNostalgiaMaterial.SetVector("screenSize", new Vector3(t.width, t.height, ditherDim));

                context.command.Blit(context.source, t, settings.pixelNostalgiaMaterial, settings.doDithering ? 0 : 1);
                context.command.Blit(t, endRes);
                RenderTexture.ReleaseTemporary(t);
            }
            else
            {
                settings.pixelNostalgiaMaterial.SetVector("screenSize", new Vector3(settings.windowSize.value.x, settings.windowSize.value.y, ditherDim));
                context.command.Blit(context.source, settings.pixelatedTexture, settings.pixelNostalgiaMaterial, settings.doDithering ? 0 : 1);
                context.command.Blit(settings.pixelatedTexture, endRes);
            }

            // ASCII EFFECT IF APPLICABLE
            if (settings.doAscii)
            {
                settings.pixelNostalgiaMaterial.SetTexture("ascii", settings.asciiLUT);

                RenderTexture t = RenderTexture.GetTemporary(context.camera.pixelWidth, context.camera.pixelHeight, 0);
                context.command.Blit(endRes, t, settings.pixelNostalgiaMaterial, 2);
                context.command.Blit(t, context.destination);
                RenderTexture.ReleaseTemporary(t);
            }
            else if (settings.doCRT)
            {
                RenderTexture t = RenderTexture.GetTemporary(context.camera.pixelWidth, context.camera.pixelHeight, 0);
                context.command.Blit(endRes, t, settings.pixelNostalgiaMaterial, 3);
                context.command.Blit(t, context.destination);
                RenderTexture.ReleaseTemporary(t);
            }
            else
            {
                context.command.Blit(endRes, context.destination);
            }
            RenderTexture.ReleaseTemporary(endRes);
        }
    }

    [Serializable]
    public sealed class Vector2Parameter : ParameterOverride<Vector2>
    {
        public Vector2Parameter() { value = Vector2.zero; }

        public override void Interp(Vector2 from, Vector2 to, float t)
        {
            value = new Vector2(
                Mathf.RoundToInt(from.x + (to.x - from.x) * t),
                Mathf.RoundToInt(from.y + (to.y - from.y) * t));
        }
    }
}

#endif