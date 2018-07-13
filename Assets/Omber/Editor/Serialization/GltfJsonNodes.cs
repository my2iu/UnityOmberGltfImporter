using System.Collections.Generic;

namespace Omber.UnityGltfImporter
{
    // Classes for holding raw data deserialized from the JSON of a GLTF file

    [System.Serializable]
    public class GltfRoot
    {
        public GltfAsset asset;
        public List<GltfScene> scenes;
        public int scene;
        public List<GltfNode> nodes;
        public List<GltfMesh> meshes;
        public List<GltfMaterial> materials;
        public List<GltfAccessor> accessors;
        public List<GltfBufferView> bufferViews;
        public List<GltfBuffer> buffers;
        public List<GltfTexture> textures;
        public List<GltfImage> images;
    }

    [System.Serializable]
    public class GltfAsset
    {
        public string version;
        public string generator;
    }

    [System.Serializable]
    public class GltfAccessor
    {
        public int? bufferView;
        public int byteOffset;
        public int componentType;
        public bool normalized;
        public int count;
        public string type;
        public double[] min;
        public double[] max;
    }

    [System.Serializable]
    public class GltfScene
    {
        public int[] nodes;
    }

    [System.Serializable]
    public class GltfNode
    {
        public int[] children;
        public int? mesh;
    }

    [System.Serializable]
    public class GltfBuffer
    {
        public string uri;
        public int byteLength;
    }

    [System.Serializable]
    public class GltfBufferView
    {
        public int buffer;
        public int byteOffset;
        public int byteLength;
        public int? byteStride;
    }

    [System.Serializable]
    public class GltfMesh
    {
        public GltfPrimitive[] primitives;
    }

    [System.Serializable]
    public class GltfPrimitive
    {
        public Dictionary<string, int> attributes;
        public int? indices;
        public int? material;
        public int mode = 4;

    }

    [System.Serializable]
    public class GltfMaterial
    {
        public string alphaMode;
        public bool doubleSided;
        public GltfPbrMetallicRoughness pbrMetallicRoughness;
    }

    [System.Serializable]
    public class GltfPbrMetallicRoughness
    {
        public GltfTextureInfo baseColorTexture;
    }

    [System.Serializable]
    public class GltfTextureInfo
    {
        public int index;
    }

    [System.Serializable]
    public class GltfTexture
    {
        public int? source;
    }

    [System.Serializable]
    public class GltfImage
    {
        public string uri;
        public string mimeType;
        public int? bufferView;
    }
}
