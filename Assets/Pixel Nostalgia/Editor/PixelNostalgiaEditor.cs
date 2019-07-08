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
   | When you have downloaded PostProcessingStack V2, comment out the 2nd line in this file, and in the PixelNostalgia.cs file,             |
   |   Thank you,                                                                                                                           |
   +----------------------------------------------------------------------------------------------------------------------------------------+ */

#elif (POSTPROCESSSTACK_NOT_DOWNLOADED_PACKAGE_MANAGER)

#warning Post Processing Stack V2 required : Get the Post Processing Stack v2 from the Package Manager

/* +----------------------------------------------------------------------------------------------------------------------------------------+
   | When you have downloaded PostProcessingStack V2, comment out the 4th line in this file, and in the PixelNostalgia.cs file,             |
   |   Thank you,                                                                                                                           |
   +----------------------------------------------------------------------------------------------------------------------------------------+ */

#else

using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.Rendering.PostProcessing;
using UnityEditor;

namespace DFA
{
    [PostProcessEditor(typeof(PixelNostalgia))]
    public sealed class PixelNostalgiaEditor : PostProcessEffectEditor<PixelNostalgia>
    {
        // What is used in the rebuild texture function
        SerializedParameterOverride windowSize;

        // The bits per channel
        SerializedParameterOverride rBitDepth;
        SerializedParameterOverride gBitDepth;
        SerializedParameterOverride bBitDepth;
        SerializedParameterOverride grayscale;
        SerializedParameterOverride grayscaleDepth;

        // Booleans
        SerializedParameterOverride doDithering;
        SerializedParameterOverride matchCameraSize;
        SerializedParameterOverride doAscii;
        SerializedParameterOverride doCRT;
        SerializedParameterOverride ignoreSceneCamera;

        // A second iVec2, specificly set to user input
        SerializedParameterOverride customWindowSize;

        // A multiplier on the camera size
        SerializedParameterOverride scalarSize;
        
        // Used to control recreating the texture
        SerializedParameterOverride recreateTexture;

        // Used for the enums
        SerializedParameterOverride selectedSizeOption;
        SerializedParameterOverride selectedBitDepthOption;
        SerializedParameterOverride bayerIndex;
        SerializedParameterOverride ditherSeparation;
        SerializedParameterOverride autoSelectSeparation;

        public Vector2[] resolutionValues =
        {
            new Vector2(80, 25),     // DOSASCII,
            new Vector2(160, 192),   // Atari2600,
            new Vector2(240, 160),   // GameboyAdvanced,
            new Vector2(256, 224),   // SuperNES,
            new Vector2(320, 200),   // DOOM,
            new Vector2(320, 224),   // SegaGenesis,
            new Vector2(320, 240),   // N64,
            new Vector2(256, 144),   // r144p,
            new Vector2(426, 240),   // r240p,
            new Vector2(480, 360),   // r360p,
            new Vector2(640, 480),   // r480p,
            new Vector2(1280, 720),  // r720p
        };

        public string[] resolutionStrings =
        {
            "DOS ASCII (80x25)",
            "Atari 2600 (160x192)",
            "Gameboy Advanced (240x160)",
            "Super NES (256x224)",
            "DOOM (320x200)",
            "Sega Genesis (320x224)",
            "N64 (320x240)",
            "144p (256x144)",
            "240p (426x240)",
            "360p (480x360)",
            "480p (640x480)",
            "720p (1280x720)",
        };

        public Vector3[] bitDepthValues =
        {
            new Vector3(3,3,2),
            new Vector3(4,4,4),
            new Vector3(5,5,5),
            new Vector3(5,6,5),
            new Vector3(6,6,6),
            new Vector3(8,8,8),
            new Vector3(8,8,8),
        };

        public string[] bitDepthStrings =
        {
            "BGR233 8bit",
            "RGB444 Direct Color",
            "RGB555 High Color",
            "RGB565 16bit Color",
            "RGB666 18bit Color",
            "RGB888 True Color",
            "Black and White",
            "Custom",
        };

        public string[] ditherStrings =
        {
            "2x2 Dither Matrix",
            "3x3 Dither Matrix",
            "4x4 Dither Matrix",
            "8x8 Dither Matrix",
        };

        public string[] selectStrings =
        {
            "Automatic Separation",
            "Custom Separation"
        };

        public override void OnEnable()
        {
            // Find and set the SerializedPropertyOverride values from the PixelNostalgia class
            windowSize = FindParameterOverride(x => x.windowSize); 

            rBitDepth = FindParameterOverride(x => x.rBitDepth);
            gBitDepth = FindParameterOverride(x => x.gBitDepth);
            bBitDepth = FindParameterOverride(x => x.bBitDepth);
            grayscale = FindParameterOverride(x => x.grayscale);
            grayscaleDepth = FindParameterOverride(x => x.grayscaleDepth);

            doDithering = FindParameterOverride(x => x.doDithering);
            matchCameraSize = FindParameterOverride(x => x.matchCameraSize);
            doAscii = FindParameterOverride(x => x.doAscii);
            doCRT = FindParameterOverride(x => x.doCRT);
            ignoreSceneCamera = FindParameterOverride(x => x.disableInScene);
             
            customWindowSize = FindParameterOverride(x => x.customWindowSize);
            
            scalarSize = FindParameterOverride(x => x.scalarSize);

            recreateTexture = FindParameterOverride(x => x.recreateTexture);

            selectedSizeOption = FindParameterOverride(x => x.selectedSizeOption);
            selectedBitDepthOption = FindParameterOverride(x => x.selectedBitDepthOption);
            bayerIndex = FindParameterOverride(x => x.bayerIndex);
            ditherSeparation = FindParameterOverride(x => x.ditherSeparation);
            autoSelectSeparation = FindParameterOverride(x => x.autoSelectSeparation);

            windowSize.overrideState.boolValue = true;
            doDithering.overrideState.boolValue = true;
            matchCameraSize.overrideState.boolValue = true;
            doAscii.overrideState.boolValue = true;
            doCRT.overrideState.boolValue = true;
            ignoreSceneCamera.overrideState.boolValue = true;
            customWindowSize.overrideState.boolValue = true;
            scalarSize.overrideState.boolValue = true;
            recreateTexture.overrideState.boolValue = true;
            selectedSizeOption.overrideState.boolValue = true;
            selectedBitDepthOption.overrideState.boolValue = true;
            bayerIndex.overrideState.boolValue = true;
            ditherSeparation.overrideState.boolValue = true;
            autoSelectSeparation.overrideState.boolValue = true;
            grayscale.overrideState.boolValue = true;
            grayscaleDepth.overrideState.boolValue = true;

            windowSize.overrideState.serializedObject.ApplyModifiedProperties();
        }

        private bool foldoutA = true, foldoutB = true, foldoutC = true, foldoutD = true;
        private bool crt, asc;
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            var originalColor = GUI.backgroundColor;

            foldoutA = EditorGUILayout.Foldout(foldoutA, "Resolution Settings");
            if (foldoutA)
            {
                EditorGUI.indentLevel++;
        
                GUI.backgroundColor = new Color32(249, 255, 214, 255);
                EditorGUILayout.PropertyField(customWindowSize.value, new GUIContent() { text = "New Resolution" });
        
                EditorGUI.indentLevel--;
        
                if (GUILayout.Button("Apply"))
                {
                    if (customWindowSize.value.vector2Value.x >= 1 && customWindowSize.value.vector2Value.y >= 1 &&
                        customWindowSize.value.vector2Value.x < 8192 && customWindowSize.value.vector2Value.y < 8192)
                    {   // Can increase this if you need a larger texture
                        matchCameraSize.value.boolValue = false;
                        windowSize.value.vector2Value = customWindowSize.value.vector2Value;

                        recreateTexture.value.boolValue = true;
                    }
                }

                GUI.backgroundColor = new Color32(214, 255, 224, 255);
                if (GUILayout.Button("Match Camera"))
                {
                    matchCameraSize.value.boolValue = true;
                    scalarSize.value.floatValue = 1.0f;
                }
        
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("1/8 Cam"))
                    {
                        matchCameraSize.value.boolValue = true;
                        scalarSize.value.floatValue = 0.125f;
                    }
                    if (GUILayout.Button("1/4 Cam"))
                    {
                        matchCameraSize.value.boolValue = true;
                        scalarSize.value.floatValue = 0.25f;
                    }
                    if (GUILayout.Button("1/2 Cam"))
                    {
                        matchCameraSize.value.boolValue = true;
                        scalarSize.value.floatValue = 0.5f;
                    }
                }
                EditorGUILayout.EndHorizontal();
        
                GUI.backgroundColor = new Color32(220, 214, 255, 255);
                selectedSizeOption.value.intValue = EditorGUILayout.Popup(selectedSizeOption.value.intValue, resolutionStrings);
                if (GUILayout.Button("Apply " + resolutionStrings[selectedSizeOption.value.intValue]))
                {
                    matchCameraSize.value.boolValue = false;
                    windowSize.value.vector2Value = resolutionValues[selectedSizeOption.value.intValue];

                    recreateTexture.value.boolValue = true;
                }

                string label = string.Format("Resolution: {0} Camera",
                    scalarSize.value.floatValue == 1.0f ? "Matching" :
                    scalarSize.value.floatValue == 0.5f ? "1/2" :
                    scalarSize.value.floatValue == 0.25f ? "1/4" :
                    "1/8"
                );

                if (!matchCameraSize.value.boolValue)
                {
                    var style = EditorStyles.wordWrappedLabel;
                    style.richText = true;

                    label = string.Format("Resolution: {0}x{1}", windowSize.value.vector2Value.x, windowSize.value.vector2Value.y);

                    EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("<b>Camera aspect ratio may not match texture aspect ratio</b>",
                        style);
                }
                else
                {
                    EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                }

                EditorGUILayout.Space();

                GUI.backgroundColor = originalColor;
            }
        
            foldoutB = EditorGUILayout.Foldout(foldoutB, "Color Depth Settings");
            if (foldoutB)
            {
                EditorGUI.indentLevel++;

                grayscale.value.boolValue = selectedBitDepthOption.value.intValue == bitDepthStrings.Length - 2;

                EditorGUI.BeginDisabledGroup(!grayscale.value.boolValue);
                {
                    GUI.backgroundColor = Color.white;
                    grayscaleDepth.value.intValue = EditorGUILayout.IntSlider("B&W", grayscaleDepth.value.intValue, 1, 8);
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(selectedBitDepthOption.value.intValue != bitDepthStrings.Length - 1 || grayscale.value.boolValue);
                {
                    GUI.backgroundColor = new Color32(255, 153, 153, 255);
                    rBitDepth.value.intValue = EditorGUILayout.IntSlider("Red", rBitDepth.value.intValue, 1, 8);
        
                    GUI.backgroundColor = new Color32(153, 255, 153, 255);
                    gBitDepth.value.intValue = EditorGUILayout.IntSlider("Green", gBitDepth.value.intValue, 1, 8);

                    GUI.backgroundColor = new Color32(153, 153, 255, 255);
                    bBitDepth.value.intValue = EditorGUILayout.IntSlider("Blue", bBitDepth.value.intValue, 1, 8);
                }
                GUI.backgroundColor = originalColor;
                EditorGUI.EndDisabledGroup();
                
                if (selectedBitDepthOption.value.intValue < bitDepthStrings.Length - 2)
                {
                    rBitDepth.value.intValue = (int)bitDepthValues[selectedBitDepthOption.value.intValue].x;
                    gBitDepth.value.intValue = (int)bitDepthValues[selectedBitDepthOption.value.intValue].y;
                    bBitDepth.value.intValue = (int)bitDepthValues[selectedBitDepthOption.value.intValue].z;
                }
        
                if (!grayscale.value.boolValue)
                {
                    string maxColors = (
                        Mathf.Pow(2, rBitDepth.value.intValue) *
                        Mathf.Pow(2, gBitDepth.value.intValue) *
                        Mathf.Pow(2, bBitDepth.value.intValue)).ToString("N0");
        
                    EditorGUILayout.LabelField(maxColors + " possible colors", EditorStyles.boldLabel);
                }
                else
                {
                    string maxColors = (
                        Mathf.Pow(2, grayscaleDepth.value.intValue)).ToString("N0");

                    EditorGUILayout.LabelField(maxColors + " possible shades", EditorStyles.boldLabel);
                }
        
                selectedBitDepthOption.value.intValue = EditorGUILayout.Popup(selectedBitDepthOption.value.intValue, bitDepthStrings);

                if (selectedBitDepthOption.value.intValue < bitDepthStrings.Length - 2)
                {
                    rBitDepth.value.intValue = (int)bitDepthValues[selectedBitDepthOption.value.intValue].x;
                    gBitDepth.value.intValue = (int)bitDepthValues[selectedBitDepthOption.value.intValue].y;
                    bBitDepth.value.intValue = (int)bitDepthValues[selectedBitDepthOption.value.intValue].z;
                }

                grayscale.value.boolValue = selectedBitDepthOption.value.intValue == bitDepthStrings.Length - 2;

                EditorGUI.indentLevel--;
            }
        
            foldoutC = EditorGUILayout.Foldout(foldoutC, "Dithering Settings");
            if (foldoutC)
            {
                EditorGUILayout.PropertyField(doDithering.value, new GUIContent() { text = "Dithering Enabled" });
        
                EditorGUI.BeginDisabledGroup(!doDithering.value.boolValue);
                {
                    EditorGUI.indentLevel++;
                    bayerIndex.value.intValue = EditorGUILayout.Popup(bayerIndex.value.intValue, ditherStrings);
        
                    EditorGUI.BeginDisabledGroup(autoSelectSeparation.value.intValue == 0);
                    {
                        EditorGUILayout.PropertyField(ditherSeparation.value, new GUIContent() { text = "Separation" });
                    }
                    EditorGUI.EndDisabledGroup();
        
                    autoSelectSeparation.value.intValue = EditorGUILayout.Popup(autoSelectSeparation.value.intValue, selectStrings);
        
                    if (autoSelectSeparation.value.intValue == 0)
                    {
                        // These values were chosen by me, they seem to work nicely
                        switch (bayerIndex.value.intValue)
                        {
                            case 0:     // 2x2
                                ditherSeparation.value.intValue = 422;
                                break;
                            case 1:     // 3x3
                                ditherSeparation.value.intValue = 323;
                                break;
                            case 2:     // 4x4
                                ditherSeparation.value.intValue = 285;
                                break;
                            case 3:     // 8x8
                                ditherSeparation.value.intValue = 256;
                                break;
                        }
                    }
        
                    EditorGUI.indentLevel--;
                }
                EditorGUI.EndDisabledGroup();
            }
        
            foldoutD = EditorGUILayout.Foldout(foldoutD, "Extra Effects");
            if (foldoutD)
            {
                EditorGUI.indentLevel++;

                // Simple radio toggle
                asc = EditorGUILayout.Toggle("ASCII Effect", doAscii.value.boolValue);
                if (asc != doAscii.value.boolValue)
                {
                    if (asc) doCRT.value.boolValue = false;
                    doAscii.value.boolValue = asc;
                }
                
                crt = EditorGUILayout.Toggle("CRT Effect", doCRT.value.boolValue);
                if (crt != doCRT.value.boolValue)
                {
                    if (crt) doAscii.value.boolValue = false;
                    doCRT.value.boolValue = crt;
                }

                EditorGUI.indentLevel--;
            }
        
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ignoreSceneCamera.value, new GUIContent() { text = "Disable in Scene View" });

            // Just make life easy, force these on
            rBitDepth.overrideState.boolValue = true;
            gBitDepth.overrideState.boolValue = true;
            bBitDepth.overrideState.boolValue = true;

            doAscii.overrideState.boolValue = true;
            doCRT.overrideState.boolValue = true;
            windowSize.overrideState.boolValue = true;

            doDithering.overrideState.boolValue = true;
            ditherSeparation.overrideState.boolValue = true;
            ignoreSceneCamera.overrideState.boolValue = true;

#if (UNITY_5) || (UNITY_2017)
            if (EditorGUI.EndChangeCheck())
            {
                (target as PixelNostalgia).RecreateTexture();
            }
#endif
        }
    }
}

#endif