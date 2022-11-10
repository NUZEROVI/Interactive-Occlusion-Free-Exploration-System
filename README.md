# Interactive Occlusion-Free System for Volume Exploration ([paper](http://doi.org/10.6345/NTNU202201788))

<p align="center"><img src="https://github.com/NUZEROVI/Interactive_Occlusion_Free_Exploration_System/blob/VolumeVis/Screenshots/Interface.png"></p>

## Algorithm Overview (step-by-step)
The project can be divided into three stages :
1. Volume Render (Load raw datasets and raymarching technique)
2. Volume Pre-Computation (Material classfication and non-connected segmentation)
3. Volume Exploration (Obstruction selection and 2 occision removal modes)
<p align="center"><img src="https://github.com/NUZEROVI/Interactive_Occlusion_Free_Exploration_System/blob/VolumeVis/Screenshots/Algo_overview.png"></p>

## Volume Render 
Fork from [`mlavik1/UnityVolumeRendering`](https://github.com/mlavik1/UnityVolumeRendering), describe how the volume rendering algorithm works. such as load raw datasets, raymarching technique, transfer functions, and direct volume rendering implementation.

## Volume Pre-Computation

You can see more details on a branch called "Isosurface-Mesh-Non-Connected-Segmentation".

### Material classfication (Isosurface Similarity &  Hierarchical Clustering)

We were inspried by previous research from the paper "Isosurface similarity maps", which identify the most representative structures of material based on information theory.
We implemented information theory to measure isosurface similarity map in `Scripts/VolumeObject/VolumeObjectFactory.cs`, by using the [`SeawispHunter.Maths`](https://github.com/shanecelis/SeawispHunter.Maths), a C# library with Unity as Plugins  `Resources/Maths.dll` and `Resources/xunit.assert.dll` to compute entropy, conditional entropy, and mutual information.

In order to automatically identify ranges of iso-values that make up the major structure of the material, we next use a hierarchical clustering algorithm to cluster the isosurfaces of isovalues in `Scripts/PyCode_Clustering_Heapmap/Clustering.ipynb`.

### Non-connected Segmentation (Approximate Convex Meshes & Collider Detect)
We segment the material structure into connected components by using the [`V-HACD library`](https://github.com/kmammou/v-hacd) with Unity as Plugins `Resources/VHACD_DLL.dll` and `Resources/libvhacd.dll` to merge any triangle meshes as convex solutions. 

Then, the approximate convex decomposition meshes that collide with each other are considered as connected structures and combined into the same piece.

## Volume Exploration
You can see more details regarding occlusion removal for interact on a branch called "VolumeVis".

### Obstruction Selection
You can see `Shaders/SDF.cginc` for the lens placement is definition of [`Signed distance function (SDF)`](https://iquilezles.org/articles/distfunctions/) and `Shaders/DirectVolumeRenderingShader.shader` called that function to check `insideLens`, if true, we follow the priniple of WYSIWYG to design our obstruction selection function.

We utilize the paper "WYSIWYP: What You See Is What You Pick", a clssic visibility-oriented picking technique has to compute the first and second derivative of the accumulated opacity of the ray to determine the change in the maximum opacity value of the material to determine the selected material. Unlike them, we calculate each material structure's ray-accumulated opacity, visibility (*alpha*), to determine the most visible material.

### Occulsion Removal Modes
1. local digging structure mode ( `DiggingWidget` in `Shaders/DirectVolumeRenderingShader.shader`)
   
   This mode removes the obstruction locally and keeps the context around the local region.
2. Global erasing structure mode ( `ErasingWidget` in `Shaders/DirectVolumeRenderingShader.shader`)
   
   This mode removes a whole connected material structure when users think that the whole material structure is well explored.
   
## Interactive System Interface
<p align="center"><img src="https://github.com/NUZEROVI/Interactive_Occlusion_Free_Exploration_System/blob/only-for-survey-use/survey_use/Screenshots/Antialias_Interface_original_size.gif"></p>


## Examples of system demos
|<p align="center"><img src="https://github.com/NUZEROVI/Interactive_Occlusion_Free_Exploration_System/blob/only-for-survey-use/survey_use/Demo/Find_lobster.gif"></p> | <p align="center"><img src="https://github.com/NUZEROVI/Interactive_Occlusion_Free_Exploration_System/blob/only-for-survey-use/survey_use/Demo/Foot_Structure.gif"></p>|
|-----|--------|

## System Downloads (for survey use) ([Website](https://nuzerovi.github.io/VolumeVis_UX_Survey/)) ([Releases](https://github.com/NUZEROVI/VolumeVis_UX_Survey/releases)) 

- [For Mac - VolumeVis_Program_MacOS_Installer.dmg](https://drive.google.com/drive/folders/1sApUv3nzVlSrI0xMHoo-4V8_G1Y6B6s8?usp=sharing)
- [For Windows - VolumeVis_Program_Win_x86_64.exe](https://drive.google.com/drive/folders/1sApUv3nzVlSrI0xMHoo-4V8_G1Y6B6s8?usp=sharing)

## Acknowledgements
I would like to express my appreciation to the original author, [mlavik1](https://github.com/mlavik1), <br> for releasing the [open source](https://github.com/mlavik1/UnityVolumeRendering) and [tutorial](https://matiaslavik.wordpress.com/2020/01/19/volume-rendering-in-unity/) to volume rendering,  <br> which enabled me to finish my master thesis study on volume rendering visualization.
