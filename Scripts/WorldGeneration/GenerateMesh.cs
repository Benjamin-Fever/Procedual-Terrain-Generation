using Godot;

namespace TerrainGeneration.Scripts.WorldGeneration;

[GlobalClass, Tool]
public partial class GenerateMesh : StaticBody3D {
    private NoiseMap _noiseMap;

    [Export]
    private NoiseMap NoiseMap {
        get => _noiseMap;
        set {
            _noiseMap = value;
            if (Engine.IsEditorHint()) {
                Clear();
                _noiseMap.GenerateMap();
                _noiseMap.MapGenerated += Generate;
            }
        }
    }
    [Export] private Curve _curve;
    [Export] private float _heightMultiplier = 1.0f;
    [Export] private int _lod = 1;

    [Export] private Color grass;
    [Export] private Color sand;
    [Export] private Color water;
    public void Generate() {
        Clear();
        SurfaceTool surfaceTool = new();
        int subdivisions = _lod == 0 ? 1 : _lod * 2;
        
        int _chunkSize = _noiseMap.ChunkSize * 2 + 1;
        float _halfChunkSize = _chunkSize / 2.0f;
        
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        Image image = Image.Create(_chunkSize, _chunkSize, false, Image.Format.Rgb8);
        for (int y = 0; y < _chunkSize-subdivisions; y+=subdivisions) {
            for (int x = 0; x < _chunkSize-subdivisions; x+=subdivisions) {
                Vector3 vertex1 = new(
                    x - _halfChunkSize, 
                    _curve.Sample(_noiseMap.GetValue(x,y)) * _heightMultiplier, 
                    y - _halfChunkSize);
                Vector3 vertex2 = new(
                    x + subdivisions - _halfChunkSize, 
                    _curve.Sample(_noiseMap.GetValue(x+subdivisions,y)) * _heightMultiplier, 
                    y - _halfChunkSize);
                Vector3 vertex3 = new(
                    x + subdivisions - _halfChunkSize,
                    _curve.Sample(_noiseMap.GetValue(x + subdivisions, y + subdivisions)) * _heightMultiplier, 
                    y + subdivisions - _halfChunkSize);
                Vector3 vertex4 = new(
                    x - _halfChunkSize,
                    _curve.Sample(_noiseMap.GetValue(x, y + subdivisions)) * _heightMultiplier, 
                    y + subdivisions - _halfChunkSize);

                surfaceTool.SetUV(new Vector2(x/(float)_chunkSize, y/(float)_chunkSize));
                surfaceTool.AddVertex(vertex1);
                surfaceTool.SetUV(new Vector2((x + subdivisions) / (float)_chunkSize, y / (float)_chunkSize));
                surfaceTool.AddVertex(vertex2);
                surfaceTool.SetUV(new Vector2((x + subdivisions) / (float)_chunkSize, (y + subdivisions) / (float)_chunkSize));
                surfaceTool.AddVertex(vertex3);
                
                surfaceTool.SetUV(new Vector2(x / (float)_chunkSize, y / (float)_chunkSize));
                surfaceTool.AddVertex(vertex1);
                surfaceTool.SetUV(new Vector2((x + subdivisions) / (float)_chunkSize, (y + subdivisions) / (float)_chunkSize));
                surfaceTool.AddVertex(vertex3);
                surfaceTool.SetUV(new Vector2(x / (float)_chunkSize, (y + subdivisions) / (float)_chunkSize));
                surfaceTool.AddVertex(vertex4);
                
                float height = _noiseMap.GetValue(x,y);
                image.SetPixel(x,y,Colors.Black.Lerp(Colors.White, height));
                
                // if (height > 0.65f) {
                //     image.SetPixel(x,y,grass);
                // }
                // else if (height > 0.58f) {
                //     image.SetPixel(x, y, sand);
                // }
                // else {
                //     image.SetPixel(x, y, water);
                // }
            }
        }
        surfaceTool.GenerateNormals();
        ImageTexture texture = new();
        texture.SetImage(image);
        MeshInstance3D meshInstance = new() {
            Mesh = surfaceTool.Commit(),
            MaterialOverlay = new StandardMaterial3D() {
                AlbedoTexture = texture,
                TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest
            }
        };
        AddChild(meshInstance);
        meshInstance.Owner = Owner;
    }

    public void Clear() {
        foreach (Node child in GetChildren()) {
            child.QueueFree();
        }
    }
    
    #if TOOLS
    public void ExtendInspectorBegin(ExtendableInspector inspector) {
        Button button = new() {
            Text = "Generate"
        };
        button.Pressed += () => {
            Clear();
            Generate();
        };
        inspector.AddCustomControl(button);
    }
    #endif
}

