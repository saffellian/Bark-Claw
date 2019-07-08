About scene:
We set all plants and objects manualy as at moment that we made this scene unity HD SRp doesn't support terrain and any foliage systems. 
There are 2 demo scenes
- converted scene with unity terrain but without grass (It will work on hd rp 6.0 + when it will be released
- scene without terrain data but very pretty and small
No grass system affect the performance. That's why number of  saved by baching is so huge but...performance at scene should be good anyway. 
We will change this as soon unity will support terrain or something that we could use to buiuld proper scene. (It should be very soon)

BEFORE YOU START:
- you need Unity 2018.3+ 
- you need HD SRP pipline 4.9, if you use higher etc custom shaders will not work. We will recompile them for new version 6.0+ 
- wind setup is gone in 4.9 but materials work. We notice that in 5.x hd rp wind is back... just be patient.
Be patient this tech is so fluid... we coudn't fallow ever beta version


Step 1 - Setup Shadows and other render setups.
Find File "HDRenderPipelineAsset" and shadow atlas width and height to 4096 or 2048.
Optionaly turn on "increase resolution of volumetrics (a bit expensive but I didn't notice big drop so..) 

Step 2 (not necessary) - add foliage profiles:
Find File "HDRenderPipelineAsset" and in Diffusion Profile Settings change SSSSSettings into our NM_SSSSettings. This will give you 2 additional foliage profiles
which we made for this scene. They change a bit few objects colors, just to saturate them a bit.

Step 3 (necessary if you made Step 1) - setup profiles on materials:
Find materials:
- M_grass_meadow_03 
- M_flower_chamomile_01
and change Diffusion profile into NM Foliage

Find material: M_Poplar_leaves 
and change Diffusion profile into NM Foliage Trees

Step 4 (not necessary) - fill spline system missings: 
Import R.A.M into project - just spline system folder or later when we support hd srp you could import all components. This will fill missings in spline objects

Step 5 - chose way of movment.
Chose camera which you want to use:
Movie track - turn on in hierarchy Movie Main Camera but keep Free Fly Main camera off.
Free Fly - turn on in hierarchy Free Fly Main Camera  but keep Movie Main Camera off.

Step 6 - HIT PLAY!:)

IF ANY SHADER IS PINK PLEASE right click on it and  click - reimport;) We found that sometimes shaders didn't compile at first import.
Like we said HD SRP is really fluid now.

About scene construction:
- There is post process profile. 
- There are 2! Volume Settings objects. They are important like hell because basicly Meadow keep normal fog - expotential on outside area.
  When you enter the forest you are in Volume Settings for forest which change expotential fog into volumetric which gives cool forest depth.
  Even at scene view you could enter this area and check how visual aspect of the scene will change. Play with it. Go to Volume Settings Forest and by 
  Mean Free Path and albedo color in Volumetric fog you could manage fog color and depth.
- There are R.A.M spline objects, with missings until you will import our R.A.M pack, then you could modify it and change  road shape.
- Terrain is based on simple mesh which was vertex painted by 4 layers. Like in book of the dead and other unity hd srp demo scenes.
- Foliage has been planted manualy until unity doesnt support foliage system and terrain on hd srp. We will re-adjust this as soon unity will add
such support.
- Prefab wind manage wind speed and direction at the scene

Play with it give us feedback and lern about hd srp power.

