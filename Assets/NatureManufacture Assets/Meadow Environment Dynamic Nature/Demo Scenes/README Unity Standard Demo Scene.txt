Unity Standard Demo Scene

Info about  SRP:
This demo "Unity Standard Demo Scene" scene doesn't work for hd srp until  few upcomming days. I hope it will be ready soon.
LW SRP please import correct HD and LW RP Support Packs. Import it only for lw srp projects and use pack which fit your hd or lw version.

How to run it:
- open demo scene 
- regenerate light in lighting window
- import post process stack 2.0 into your project if you need image effects (package manager--> post process stack )
- click play
- play with directional light angle at x = 40 it looks pretty nice.
- change shadow distance to 500 or higher in quality settings
- play with wind prefab and wind speed:)
- you can change anisotropic textures to "forced on" at project settings -> quality this will make scene look much better but older device can notice fps drop

Best performance:
- Set linear color space
- Set deferred render mode. This will increate performance beacuse of reflection probes alot
- If you want to use forward rendering disable reflection probes as they are heavy at forward and open space scene. 
  Screen space reflection at post process stack should replace them in most cases
- You could disable few features at post processing stack like:
   * Depth of field
   * Screen Space Reflections which are pretty heavy 
   * Play with ambient occlusion type
   * Turn off motion blur
 - Change Anti-aliasing at camera
 - Check anisotropic textures at project settings -> quality

Additional:
- you could import our cts profile if you use cts. It's in third party support files. "CTS Profile Standard Demo"

How we build it?
1) create terrain shape via Gaia with our stamps.
2) paint terrain automaticly via slopes:
   * grass on flat area
   * dirt/dry ground on small slopes
3) paint terrain manualy
   * chose places for trees by marking them via terrain leaves texture
   * create flow lines via sand/mood texture
   * create flow lines via stronger grass textures
4) create splines for roads via our R.A.M
   * R.A.M will automaticly paint and reshape terrain
   * adjust alpha of the road via vertex painter at road connections and just make more noise
5) setup bigger models and point of the interest in the middle od the scene
6) plant foliage manualy
   * put higher  forest trees in the middle
   * create forest wall from small and standalone trees
   * plant small bushes on forest border and randomly inside it
   * paint grass and flowers in relation to ground textures. More dry area less grass etc.
   * create forest/tree lines
7) Place few medium objects like rocks, trunks
8) spawn all small detail objects via Gena. 
9) we create fences by our own unreleased tool.
10) all detail objects are instanced in material (thats why they are so fast)
11) place reflection probes to get proper pbr in forest area and inside the meadow
12) place reflection probes so LOD's will get proper light... remember that at 2017.1 light probes 
break instancing and from 130 fps we get 30....In upper versions it will work fine. We will check it.
Thats why we turn them off for 2017.1 demo scene.
13) add background mountains
14) add post processing stack 2.0 and create profile
15) setup fog (take color from sky and adjust intensivity)

Note about grass:
Unity terrain doesn't support custom grass shaders that's why we use Vegetation Studio Pro in our video. 
Old Vegetation Studio could be used too or any other custom grass solution.
