# Unity Omber GLTF Importer

This is code for importing vector graphics made in [Omber](https://www.wobastic.com/omber/) that have been exported in GLTF format into Unity. It should work with Unity 2017.3 and later. It allows GLTF files exported by Omber to be imported into your game from within the Unity editor. Loading GLTF files at runtime is not currently supported. The GLTF files will be imported as 3d meshes that can be inserted into 2d/3d Unity games and manipulated like normal Unity objects.

## Usage

First, add the GLTF importer code into your Unity project. You only need to download the [unitypackage](https://github.com/my2iu/UnityOmberGltfImporter/blob/master/UnityOmberGltfImporter.unitypackage), and not the full source code project for the importer. Just click on the "download" button to download the file to your computer, and then drag the downloaded file to Unity to import the package.

Then, export your vector art in GLTF format. Omber's help documentation provides [suggested export settings for Unity](https://www.wobastic.com/omber/help/gltf.html).

Finally, drag your `.glb` files into the Unity project window, and they will be imported. From there, you can just drag the imported vector art models into your scene.
