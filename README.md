# Interactive Occlusion-Free System for Volume Exploration ([paper](http://doi.org/10.6345/NTNU202201788))


## **Introduction**
   
#### **System Downloads** ([Releases](https://github.com/NUZEROVI/Interactive-Occlusion-Free-Exploration-System/releases/tag/VolumVis_v1.0.0)) ([for Mac](https://github.com/NUZEROVI/Interactive-Occlusion-Free-Exploration-System/releases/download/VolumVis_v1.0.0/VolumeVis_Program_MacOS_Installer.dmg)) ([for Windows](https://github.com/NUZEROVI/Interactive-Occlusion-Free-Exploration-System/releases/download/VolumVis_v1.0.0/Occlusion-Free.VolumeVis_Program_Win_x86_64.zip)) 

 [![Watch the video](https://github.com/NUZEROVI/Interactive-Occlusion-Free-System-for-Accessible-Volume-Exploration/blob/VolumeVis/Screenshots/Video%20Cover.png)](https://youtu.be/PfOvNkHc_xI)




## **Algorithm Overview (step-by-step)**
#### The project can be divided into three stages :
   
     1. Volume Render (Load raw datasets and raymarching technique)
     2. Volume Pre-Computation (Material classfication and non-connected segmentation)
     3. Volume Exploration (Obstruction selection and 2 occision removal modes)

## 1. Volume Render 

* ### Fork from [`mlavik1/UnityVolumeRendering`](https://github.com/mlavik1/UnityVolumeRendering), describe how the volume rendering algorithm works. such as load raw datasets, raymarching technique, transfer functions, and direct volume rendering implementation.

<p align="center"><img src="https://github.com/NUZEROVI/Interactive_Occlusion_Free_Exploration_System/blob/VolumeVis/Screenshots/Algo_overview_a.png"></p>

## 2. Volume Pre-Computation

* ### You can see more details on two branches called ["Information theoretic measure of similarity"](https://github.com/NUZEROVI/Interactive_Occlusion_Free_Exploration_System/pull/1) & ["Isosurface-Mesh-Non-Connected-Segmentation"](https://github.com/NUZEROVI/Interactive_Occlusion_Free_Exploration_System/pull/2).

<p align="center"><img src="https://github.com/NUZEROVI/Interactive_Occlusion_Free_Exploration_System/blob/VolumeVis/Screenshots/Algo_overview_b.png"></p>

### [1] Material classfication (Isosurface Similarity &  Hierarchical Clustering)

> We were inspried by previous research from the paper "Isosurface similarity maps", which identify the most representative structures of material based on information theory.

> We implemented information theory to measure isosurface similarity map in `Scripts/VolumeObject/VolumeObjectFactory.cs`, by using the [`SeawispHunter.Maths`](https://github.com/shanecelis/SeawispHunter.Maths), a C# library with Unity as Plugins  `Resources/Maths.dll` and `Resources/xunit.assert.dll` to compute entropy, conditional entropy, and mutual information.

> In order to automatically identify ranges of iso-values that make up the major structure of the material, we next use a hierarchical clustering algorithm to cluster the isosurfaces of isovalues in `Scripts/PyCode_Clustering_Heapmap/Clustering.ipynb`.

### [2] Non-connected Segmentation (Approximate Convex Meshes & Collider Detect)

> We segment the material structure into connected components by using the [`V-HACD library`](https://github.com/kmammou/v-hacd) with Unity as Plugins `Resources/VHACD_DLL.dll` and `Resources/libvhacd.dll` to merge any triangle meshes as convex solutions. 

> Then, the approximate convex decomposition meshes that collide with each other are considered as connected structures and combined into the same piece.

## 3. Volume Exploration

* ### You can see more details regarding occlusion removal for interact on a main branch "VolumeVis" (from [Start](https://github.com/NUZEROVI/Interactive-Occlusion-Free-Exploration-System/tree/b3ecdcb8b36f0ee167fee97f3a2611d525781619) to [End](https://github.com/NUZEROVI/Interactive-Occlusion-Free-Exploration-System/tree/27c89dc23e87a3f25aa08f49e3b138d423c58eb3) Commits).
<p align="center"><img src="https://github.com/NUZEROVI/Interactive_Occlusion_Free_Exploration_System/blob/VolumeVis/Screenshots/Algo_overview_c.png"></p>


### [1] Obstruction Selection (Visibility-Driven)

> You can see `Shaders/SDF.cginc` for the lens placement is definition of [`Signed distance function (SDF)`](https://iquilezles.org/articles/distfunctions/) and `Shaders/DirectVolumeRenderingShader.shader` called that function to check `insideLens`, if true, we follow the priniple of WYSIWYG to design our obstruction selection function.

> We utilize the paper ["WYSIWYP: What You See Is What You Pick"](https://ieeexplore.ieee.org/document/6327228), a clssic visibility-oriented picking technique has to compute the first and second derivative of the accumulated opacity of the ray to determine the change in the maximum opacity value of the material to determine the selected material. `Unlike them, we calculate each material structure's ray-accumulated opacity, visibility (*alpha*), to determine the most visible material`.

### [2] Occulsion Removal Modes 

> **Local digging structure mode** ( `DiggingWidget` in `Shaders/DirectVolumeRenderingShader.shader`)
   > This mode removes the obstruction locally and keeps the context around the local region.

> **Global erasing structure mode** ( `ErasingWidget` in `Shaders/DirectVolumeRenderingShader.shader`)
   > This mode removes a whole connected material structure when users think that the whole material structure is well explored.

* Add new function : Inverse masking with occlusion component on a branch called ["InverseMaskingWithOcclusionComponent"](https://github.com/NUZEROVI/Interactive-Occlusion-Free-Exploration-System/pull/3)

## Examples of System Demos
<p align="center"><img src="https://github.com/NUZEROVI/Interactive_Occlusion_Free_Exploration_System/blob/only-for-survey-use/survey_use/Screenshots/Antialias_Interface_original_size.gif"></p>

|<p align="center"><img src="https://github.com/NUZEROVI/Interactive_Occlusion_Free_Exploration_System/blob/only-for-survey-use/survey_use/Demo/Find_lobster.gif"></p> | <p align="center"><img src="https://github.com/NUZEROVI/Interactive_Occlusion_Free_Exploration_System/blob/only-for-survey-use/survey_use/Demo/Foot_Structure.gif"></p>|
|-----|--------|

## VolumeVis-UX Survey Process [(Website)](https://webappdeployment.github.io/VolumeVis-UX-Survey/)
The questionnaire is designed to get feedback for three aspects of our system: usability, practicability, and applicability.

## Acknowledgements
I would like to thank my advisor, Professor Wang Ko-Chih, for his assistance and guidance during this research.

I would like to express my appreciation to the original author, [mlavik1](https://github.com/mlavik1), for releasing the [open source](https://github.com/mlavik1/UnityVolumeRendering) and [tutorial](https://matiaslavik.wordpress.com/2020/01/19/volume-rendering-in-unity/) to volume rendering, which enabled me to finish my master thesis study on visualization of volume data, and interactive visualization.
