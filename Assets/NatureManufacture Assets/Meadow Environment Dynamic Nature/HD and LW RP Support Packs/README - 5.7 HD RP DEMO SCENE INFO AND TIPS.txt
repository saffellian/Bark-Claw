About scene:
We set all plants and objects manualy as at moment that we made this scene unity HD SRp doesn't support terrain and any foliage systems. 
There are 2 demo scenes
- converted scene with unity terrain but without grass (It will work on hd rp 6.0 + when it will be released or something around)
- scene without terrain data but very pretty and small
No grass system affect the performance. That's why number of  saved by baching is so huge but...performance at scene should be good anyway. 
We will change this as soon unity will support terrain or something that we could use to buiuld proper scene. (It should be very soon)

BEFORE YOU START:
- you need Unity 2019.1 
- you need HD SRP pipline 5.7, if you use higher etc custom shaders will not work. 
- wind setup is gone in 5.7 but materials work. It will back just be patient.
Be patient this tech is so fluid... we coudn't fallow ever beta version


Step 1 - Setup Shadows and other render setups.
Find File "HDRenderPipelineAsset" and shadow atlas width and height to 4096 or 2048.
Find Material section at "HDRenderPipelineAsset" and drag and drop our SSS settings diffusion profiles for foliage into Diffusion profile list:
NM_SSSSettings_Skin_Foliage
NM_SSSSettings_Skin_NM Foliage
NM_SSSSettings_Skin_NM Foliage Trees
Without this foliage materials will not become affected by scattering.

Optionaly turn on "increase resolution of volumetrics (a bit expensive but I didn't notice big drop so..) 


Step 2 (not necessary) - fill spline system missings: 
Import R.A.M  2019 into project - just spline system folder 

Setp 3 Find HD SRP Demo Small and open it.

Step 4 - chose way of movment.
Chose camera which you want to use:
Movie track - turn on in hierarchy Movie Main Camera but keep Free Fly Main camera off.
Free Fly - turn on in hierarchy Free Fly Main Camera  but keep Movie Main Camera off.

Step 5 - HIT PLAY!:)

IF ANY SHADER IS PINK PLEASE right click on it and  click - reimport;) We found that sometimes shaders didn't compile at first import.
Like we said HD RP is really fluid now.

About scene construction:
- There are post process profiles: Default Post-process and Scene Post-process. Manage post process by scene post process object.
- There are 2! Render Settings objects, forest one is volume just for forest area. They are important like hell because basicly Meadow keep normal fog - expotential on outside area.
  When you enter the forest you are in Forest Render Settings for forest which change expotential fog into volumetric which gives cool forest depth.
  Even at scene view you could enter this area and check how visual aspect of the scene will change. Play with it. 
- There are R.A.M spline objects, with missings until you will import our R.A.M pack, then you could modify it and change  road shape.
- Terrain is based on simple mesh which was vertex painted by 4 layers. Like in book of the dead and other unity hd srp demo scenes.
- Foliage has been planted manualy until unity doesnt support foliage system and terrain on hd srp. We will re-adjust this as soon unity will add
such support.
- Prefab wind manage wind speed and direction at the scene

Play with it give us feedback and lern about hd srp power.

