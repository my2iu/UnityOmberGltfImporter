using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System;
using System.IO;
using System.Collections.Generic;
using Omber.LitJson;

namespace Omber.UnityGltfImporter
{
    [ScriptedImporter(1, "glb")]
    public class OmberGltfImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            GltfLoader loader = new GltfLoader(ctx, ctx.assetPath);
            loader.LoadGlb();
        }
    }

    class GltfLoader
    {
        AssetImportContext ctx;
        string path;
        GltfRoot root;
        List<Texture2D> textures = new List<Texture2D>();
        List<Material> materials = new List<Material>();
        byte[] binBuffer;

        public GltfLoader(AssetImportContext ctx, string path)
        {
            this.path = path;
            this.ctx = ctx;
        }

        private void addAsset(UnityEngine.Object obj)
        {
            ctx.AddObjectToAsset("" + obj.GetInstanceID(), obj);
        }

        public void LoadGlb()
        {
            using (Stream s = new FileStream(path, FileMode.Open))
            {
                // TODO: Handle endianness issues
                BinaryReader dataView = new BinaryReader(s);
                var magic = dataView.ReadUInt32();
                if (magic != 0x46546C67) return;  // 'glTF'
                var version = dataView.ReadUInt32();
                if (version != 2) return;
                var fileLength = dataView.ReadUInt32();
                string json = null;
                binBuffer = null;
                for (long offset = 12; offset < fileLength;)
                {
                    int chunkLength = dataView.ReadInt32();
                    var chunkType = dataView.ReadUInt32();
                    if (chunkType == 0x4E4F534A)   // 'JSON'
                    {
                        var stringBytes = dataView.ReadBytes(chunkLength);
                        json = System.Text.Encoding.UTF8.GetString(stringBytes);
                    }
                    else if (chunkType == 0x004E4942)   // 'BIN'
                    {
                        binBuffer = dataView.ReadBytes(chunkLength);
                    }
                    offset = offset + 8 + chunkLength;
                }
                root = JsonMapper.ToObject<GltfRoot>(json);
                LoadGltf(json);

            }
        }

        void LoadGltf(string json)
        {
            LoadTextures();
            LoadMaterials();

            GameObject importRoot = new GameObject();
            importRoot.name = "importRoot";
            addAsset(importRoot);
            ctx.SetMainObject(importRoot);

            // Put each scene in a separate object
            List<GameObject> scenes = new List<GameObject>();
            for (int n = 0; n < root.scenes.Count; n++)
            {
                // I don't think it's actually possible to import multiple scenes
                // this way because Unity's ScriptedImporter will merge all the 
                // scenes together into a single prefab (or if you don't parent
                // things, then the ScriptedImporter will flatten all GameObject
                // hierarchies and leave each object in the hierarchy as a 
                // separate flat asset).
                var scene = root.scenes[n];
                GameObject go;
                go = new GameObject();
                foreach (int nodeIdx in scene.nodes)
                {
                    GameObject subGo = LoadGltfNode(root.nodes[nodeIdx]);
                    subGo.transform.parent = go.transform;
                }

                go.transform.parent = importRoot.transform;
                go.name = "scene " + n;
                scenes.Add(go);

            }
        }

        void LoadMaterials()
        {
            if (root.materials == null) return;
            foreach (GltfMaterial mat in root.materials)
            {
                bool isOpaque = false;
                if ("OPAQUE".Equals(mat.alphaMode) || mat.alphaMode == null)
                    isOpaque = true;
                Texture2D tex = null;
                if (mat.pbrMetallicRoughness != null && mat.pbrMetallicRoughness.baseColorTexture != null
                    && textures[mat.pbrMetallicRoughness.baseColorTexture.index] != null)
                {
                    tex = textures[mat.pbrMetallicRoughness.baseColorTexture.index];
                }

                // Create the actual material now
                Material newMat = null;
                if (isOpaque)
                    newMat = new Material(Shader.Find("Unlit/Omber/Opaque Shader"));
                else
                    newMat = new Material(Shader.Find("Unlit/Omber/Alpha Shader"));
                if (tex != null)
                {
                    newMat.mainTexture = tex;
                }
                addAsset(newMat);
                materials.Add(newMat);
            }
        }

        void LoadTextures()
        {
            if (root.textures == null) return;
            foreach (GltfTexture tex in root.textures)
            {
                if (!tex.source.HasValue)
                {
                    textures.Add(null);
                    continue;
                }
                var image = root.images[tex.source.Value];
                if (image.uri != null || !image.bufferView.HasValue)
                {
                    textures.Add(null);
                    continue;
                }
                var bufferView = root.bufferViews[image.bufferView.Value];
                if (root.buffers[bufferView.buffer].uri != null)
                {
                    textures.Add(null);
                    continue;
                }
                byte[] texData = new byte[bufferView.byteLength];
                Buffer.BlockCopy(binBuffer, bufferView.byteOffset, texData, 0, bufferView.byteLength);
                Texture2D newTex = new Texture2D(2, 2);   // Let texture be resized later on
                newTex.LoadImage(texData);
                textures.Add(newTex);
            }

            foreach (Texture2D tex in textures)
            {
                if (tex == null) continue;
                addAsset(tex);
            }
        }

        GameObject LoadGltfNode(GltfNode node)
        {
            GameObject go = new GameObject();
            if (node.children != null)
            {
                foreach (int nodeIdx in node.children)
                {
                    GameObject subGo = LoadGltfNode(root.nodes[nodeIdx]);
                    subGo.transform.parent = go.transform;
                }
            }
            if (node.mesh.HasValue)
                LoadGltfMesh(go, root.meshes[node.mesh.Value]);
            return go;
        }

        void LoadGltfMesh(GameObject parent, GltfMesh mesh)
        {
            GameObject go = parent;
            if (mesh.primitives.Length > 1)
            {
                go = new GameObject();
                //subAssetsToAdd.Add(go);
                go.transform.parent = parent.transform;
            }

            if (mesh.primitives != null)
            {
                foreach (GltfPrimitive primitive in mesh.primitives)
                {
                    Mesh m = primitiveToMesh(primitive);
                    Material mat = null;
                    if (primitive.material.HasValue)
                    {
                        mat = materials[primitive.material.Value];
                    }
                    if (m == null) continue;
                    if (mesh.primitives.Length == 1)
                    {
                        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
                        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
                        if (mat != null)
                           meshRenderer.material = mat;
                        meshFilter.mesh = m;
                    }
                    // TODO: Add a submesh, or add the primitive as a separate game object
                    addAsset(m);
                }
            }
        }

        Mesh primitiveToMesh(GltfPrimitive primitive)
        {
            // Check that mesh fits our expectations
            if (!(primitive.attributes.ContainsKey("COLOR_0"))) return null;
            if (!(primitive.attributes.ContainsKey("POSITION"))) return null;
            if (primitive.mode != 4) return null;
            var colorAccessor = root.accessors[primitive.attributes["COLOR_0"]];
            var positionAccessor = root.accessors[primitive.attributes["POSITION"]];
            if (colorAccessor.type != "VEC4") return null;
            if (colorAccessor.bufferView != positionAccessor.bufferView) return null;
            if (colorAccessor.count != positionAccessor.count) return null;
            if (positionAccessor.componentType != 5126) return null;
            if (colorAccessor.componentType != 5126 && colorAccessor.componentType != 5121) return null;
            GltfAccessor indexAccessor = null;
            if (primitive.indices.HasValue)
            {
                indexAccessor = root.accessors[primitive.indices.Value];
                if (indexAccessor.componentType != 5123) return null;
            }

            // Read out vertices and color data
            var bufferView = root.bufferViews[colorAccessor.bufferView.Value];
            var buffer = binBuffer;
            // Only do glb right now
            if (root.buffers[bufferView.buffer].uri != null) return null;
            Vector3[] vertices = new Vector3[positionAccessor.count];
            int positionStride = bufferView.byteStride.GetValueOrDefault(3 * 4);
            for (int n = 0; n < positionAccessor.count; n++)
            {
                int offset = bufferView.byteOffset + positionAccessor.byteOffset + n * positionStride;
                vertices[n] = new Vector3(
                    BitConverter.ToSingle(buffer, offset),
                    BitConverter.ToSingle(buffer, offset + 4),
                    -BitConverter.ToSingle(buffer, offset + 8));
            }
            // Load colors
            Color[] colors = null;
            Color32[] colors32 = null;
            if (colorAccessor.componentType == 5126)
            {
                int colorSize = 4;                
                int colorStride = bufferView.byteStride.GetValueOrDefault(colorSize * 4);
                colors = new Color[colorAccessor.count];
                for (int n = 0; n < colorAccessor.count; n++)
                {
                    int offset = bufferView.byteOffset + colorAccessor.byteOffset + n * colorStride;
                    colors[n] = new Color(BitConverter.ToSingle(buffer, offset),
                                          BitConverter.ToSingle(buffer, offset + 4),
                                          BitConverter.ToSingle(buffer, offset + 8),
                                          BitConverter.ToSingle(buffer, offset + 12));
                }
            }
            else if (colorAccessor.componentType == 5121)
            {
                int colorSize = 1;
                int colorStride = bufferView.byteStride.GetValueOrDefault(colorSize * 4);
                colors32 = new Color32[colorAccessor.count];
                for (int n = 0; n < colorAccessor.count; n++)
                {
                    int offset = bufferView.byteOffset + colorAccessor.byteOffset + n * colorStride;
                    colors32[n] = new Color32(buffer[offset + 0],
                                              buffer[offset + 1],
                                              buffer[offset + 2],
                                              buffer[offset + 3]);

                }
            }
            // Load UVs
            Vector2[] uvs = null;
            if (primitive.attributes.ContainsKey("TEXCOORD_0"))
            {
                var texAccessor = root.accessors[primitive.attributes["TEXCOORD_0"]];
                uvs = new Vector2[texAccessor.count];
                var texBufferView = root.bufferViews[texAccessor.bufferView.Value];
                int texStride = texBufferView.byteStride.GetValueOrDefault(2 * 4);
                for (int n = 0; n < texAccessor.count; n++)
                {
                    int offset = texBufferView.byteOffset + texAccessor.byteOffset + n * texStride;
                    uvs[n] = new Vector2(BitConverter.ToSingle(buffer, offset),
                                          BitConverter.ToSingle(buffer, offset + 4));
                }
            }

            // Load the triangle indices
            if (indexAccessor == null) return null;
            int[] tris = new int[indexAccessor.count];
            var idxBufferView = root.bufferViews[indexAccessor.bufferView.Value];
            // Only do glb right now
            if (root.buffers[idxBufferView.buffer].uri != null) return null;
            for (int n = 0; n < indexAccessor.count; n++)
            {
                // TODO: Handle other data types for the indices
                tris[n] = BitConverter.ToUInt16(binBuffer, idxBufferView.byteOffset + n * 2);
            }

            // Create the mesh
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            if (colors != null)
                mesh.colors = colors;
            if (colors32 != null)
                mesh.colors32 = colors32;
            if (uvs != null)
                mesh.uv = uvs;
            mesh.triangles = tris;
            return mesh;
        }

    }
}