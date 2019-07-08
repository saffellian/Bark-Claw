How to run demo from video:

You need Unity 2018.3 or higher. Anyway this should work even on lower version aswell. 

Please do all step 1 by 1. It will save your time and confusion. We will clean everything more (prefabs mostly) but now scene works properly. 

1) Import Vegetation Studio Standard
https://assetstore.unity.com/packages/tools/terrain/vegetation-studio-103389?aid=1011lGkb

2) Setup project to linear color space in player settings

3) Setup project deferred rendering in Graphics Settings (not necessary but it improve speed) 

4) Import Post Process Stack 2.0 into your project from Window ->Package Manager -> Post Processing (click update or instal)

5) Import Screen Space Shadow pack into your project (not necessary but nice and cheap shadows on grass are welcome)
https://assetstore.unity.com/packages/vfx/shaders/directx-11/se-screen-space-shadows-77836?aid=1011lGkb

6) Import Vegetation Studio Standard Demo to your project. It will change most standard shaders which doesn't support VS PRO instanced indirect
into our shaders which support it.

10) Find folder Vegetation Studio Standard Demo in NatureManufacture Assets folder.


11) Open scene called "Vegetation Studio Standard Demo Scene"

12) Click Play - you could fly and check demo scene.

13) Low FPS - > for low end gpu turn off screen space reflections from post processing object (it's expensive)
Make note that current scene setup spawn all foliage at  RUNTIME to the camera. You could bake it this will improve
fps too.

14) Optional: Bake ambient light in window -> rendering -> lighting settings 
(It will break reflections a bit probably but nothing special - check point "D" in more usefull options part).

15) We baked our foliage in the scene so it will work smoothly everywhere. We manualy removed foliage from roads - you could use  mask but we didn't as we manualy paint
foliage inside road for better result.

KNOWN ISSUES:
1) Seams that billboards doesn't cast shadows. Not sure from which unity engine versions
2) When you change tree into instanced indirect render billboards dissapear. 
All our shaders support instanced indirect - all LOD's work fine, seams there is hidden issue. We will try to fix this directly with Lennart.

More usefull options about visual effects and optimisation: 

A) You could expand foliage rendering distance in Vegetation System objects in Vegetation Setting. 
(we use 70 for grass and additional 100 for trees at video)

B) You could adjust shadow distance to 400 like we did and improve shadow resolution to Very High Resolution.

What else? ENJOY and play with it, build a game or nice video! 
All best from NatureManufacture team and thanks for supporting us!
