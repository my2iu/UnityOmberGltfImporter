# Unity Omber GLTF Importer

This is code for importing vector graphics made in [Omber](https://www.wobastic.com/omber/) that have been exported in GLTF format into Unity. It should work with Unity 2017.3 and later. It allows GLTF files exported by Omber to be imported into your game from within the Unity editor. Loading GLTF files at runtime is not currently supported. The GLTF files will be imported as 3d meshes that can be inserted into 2d/3d Unity games and manipulated like normal Unity objects.

## Usage

First, add the GLTF importer code into your Unity project. You only need to download the [unitypackage](https://github.com/my2iu/UnityOmberGltfImporter/blob/master/UnityOmberGltfImporter.unitypackage), and not the full source code project for the importer. Just click on the "download" button to download the file to your computer, and then drag the downloaded file to Unity to import the package.

Then, export your vector art in GLTF format. Omber's help documentation provides [suggested export settings for Unity](https://www.wobastic.com/omber/help/gltf.html).

Finally, drag your `.glb` files into the Unity project window, and they will be imported. From there, you can just drag the imported vector art models into your scene.

## Provided Shaders

3d model meshes imported from Omber require special shaders because the default shaders provided by Unity cannot render vertex colors, which is needed for Omber's gradients. The Omber Unity package contains two sets of shaders for rendering the imported meshes. 

The Omber Opaque/Alpha Shaders are the default shaders used by the importer. These shaders treat the imported meshes as normal 3d meshes. Which mesh is rendered in front of the other is determined by their Z-position. You must assign different Z-values to every object yourself to ensure that nearer objects are, in fact, rendered in front of objects behind it. Opaque objects can be rendered in any order, but meshes that make use of transparency must be rendered separately after all opaque objects. Because of a lack of precision in the depth buffer, it may sometimes be necessary to scale the Z values of a mesh to ensure that all the layers of the mesh are sufficiently far apart that they have different Z values in the depth buffer.

The Omber Simple Shader ignores the Z-values of a mesh and simply renders everything in order. As long as all the triangles in the mesh are in back-to-front order, and all objects are rendered in back-to-front order, then nearer objects will appear in front of more distant objects. Opaque and transparent triangles can be rendered at the same time, so when exporting meshes from Omber, everything can be exported as a single mesh. It is not necessary to separate opaque meshes from transparent ones. Since this shader ignores the contents of the depth buffer, it may not work well with real 3d objects in a scene. Also, it is dependent on the graphics hardware rendering all triangles in the same order that they appear in the mesh. Although this is generally fine, it is a little vague as to whether it is safe to assume that all graphics hardware exhibits this behavior.
