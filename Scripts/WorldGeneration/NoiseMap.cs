using Godot;

namespace TerrainGeneration.Scripts.WorldGeneration;

[GlobalClass, Tool]
public partial class NoiseMap : Resource  {
    [Signal] public delegate void MapGeneratedEventHandler();
    private int _chunkSize = 200;
    private float _noiseScale = 1.0f;
    private int octaves = 5;
    private float persistence = 0.5f;
    private float lacunarity = 2.0f;
    private int _seed = 0;
    private Vector2 _offset = Vector2.Zero;
    private FastNoiseLite _noise = new FastNoiseLite() { FractalType = FastNoiseLite.FractalTypeEnum.None };
    private float[,] _noiseMap;
    
    [Export] public int ChunkSize {
        get => _chunkSize;
        set {
            _chunkSize = value;
            if (Engine.IsEditorHint()) GenerateMap();
        }
    }
    [Export] public int Seed {
        get => _seed;
        set {
            _seed = value;
            if (Engine.IsEditorHint()) GenerateMap();
        }
    }
    [Export] public FastNoiseLite.NoiseTypeEnum NoiseType {
        get => _noise.NoiseType;
        set {
            _noise.NoiseType = value;
            if (Engine.IsEditorHint()) GenerateMap();
        }
    }
    [Export] public float NoiseScale {
        get => _noiseScale;
        set {
            _noiseScale = value;
            if (Engine.IsEditorHint()) GenerateMap();
        }
    }
    [Export] public int Octaves {
        get => octaves;
        set {
            octaves = value;
            if (Engine.IsEditorHint()) GenerateMap();
        }
    }
    [Export] public float Persistence {
        get => persistence;
        set {
            persistence = value;
            if (Engine.IsEditorHint()) GenerateMap();
        }
    }
    [Export] public float Lacunarity {
        get => lacunarity;
        set {
            lacunarity = value;
            if (Engine.IsEditorHint()) GenerateMap();
        }
    }
    [Export] public Vector2 Offset {
        get => _offset;
        set {
            _offset = value;
            if (Engine.IsEditorHint()) GenerateMap();
        }
    }
    
    public NoiseMap() {
        GenerateMap();
    }
    public void GenerateMap() {
        int size = _chunkSize * 2 + 1;
        float[,] noiseMap = new float[size, size];
        float maxNoise = float.MinValue;
        float minNoise = float.MaxValue;
        float amplitude = 1;
        float frequency = 1;
        float maxPossibleHeight = 0;

        float _halfChunkSize = size / 2f;
    
        System.Random prng = new System.Random(_seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + _offset.X;
            float offsetY = prng.Next(-100000, 100000) + _offset.Y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
            
            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }
    
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                float noiseHeight = 0;
                amplitude = 1;
                frequency = 1;
                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - _halfChunkSize) / _noiseScale * frequency + octaveOffsets[i].X * frequency;
                    float sampleY = (y - _halfChunkSize) / _noiseScale * frequency + octaveOffsets[i].Y * frequency;

                    float perlinValue = _noise.GetNoise2D(sampleX, sampleY);
                    noiseHeight += perlinValue * amplitude;
                
                    amplitude *= persistence;
                    frequency *= lacunarity;
                
                }

                noiseMap[x, y] = noiseHeight;
            }
        }
        
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 1.75f);
                noiseMap[x, y] = normalizedHeight;
            }
        }
    
        _noiseMap = noiseMap;
        EmitSignal(SignalName.MapGenerated);
    }

    public float GetValue(int x, int y) {
        return _noiseMap[x, y];
    }
}