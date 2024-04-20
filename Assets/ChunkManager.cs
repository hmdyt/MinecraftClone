using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1チャンクは xz 平面上の 16x16 のブロックで構成される
// 初期化時にplayerのいるチャンクを基準に周囲のチャンクを生成 (initialChunkSize = 3)
// playerの周りの loadingChunkSize 以内のチャンクは読み込まれている(生成されていなかったら生成する)
// それ以外は非アクティブにしておく


public class ChunkManager : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private int initialChunkSize = 1;
    [SerializeField] private int loadingChunkSize = 1;
    private GameObject player;
    private (int, int) prevPlayerChunkIndex;
    private ChunkPool chunkPool;

    void Start()
    {
        player = GameObject.Find("Player");
        chunkPool = new ChunkPool(blockPrefab);
        GenerateInitialChunks();
    }

    void Update()
    {
        UpdateChunks();
    }

    private (int, int) ChunkIndexFromPosition(Vector3 position)
    {
        return ((int)position.x / 16, (int)position.z / 16);
    }

    private (int, int) GetPlayerChunkIndex()
    {
        return ChunkIndexFromPosition(player.transform.position);
    }

    private void GenerateInitialChunks()
    {
        var (playerChunkX, playerChunkZ) = GetPlayerChunkIndex();
        for (int i = -initialChunkSize; i <= initialChunkSize; i++)
        {
            for (int j = -initialChunkSize; j <= initialChunkSize; j++)
            {
                chunkPool.GetOrCreateChunk(playerChunkX + i, playerChunkZ + j);
            }
        }
    }

    // playerが初めてチャンクを跨いだframeで呼び出す
    // 全てのchunkを非アクティブにしてから、playerの周りのチャンクをアクティブにする
    private void UpdateChunks()
    {
        var nowPlayerChunkIndex = GetPlayerChunkIndex();
        if (nowPlayerChunkIndex != prevPlayerChunkIndex)
        {
            foreach (var chunk in chunkPool.GetChunks())
            {
                chunk.SetActive(false);
            }
            var (playerChunkX, playerChunkZ) = nowPlayerChunkIndex;
            for (int i = -loadingChunkSize; i <= loadingChunkSize; i++)
            {
                for (int j = -loadingChunkSize; j <= loadingChunkSize; j++)
                {
                    chunkPool.GetOrCreateChunk(playerChunkX + i, playerChunkZ + j).SetActive(true);
                }
            }
        }
        this.prevPlayerChunkIndex = nowPlayerChunkIndex;
    }
}


class ChunkPool
{
    private Dictionary<string, Chunk> chunks = new Dictionary<string, Chunk>();
    private GameObject blockPrefab;

    public ChunkPool(GameObject blockPrefab)
    {
        this.blockPrefab = blockPrefab;
    }

    public void SetChunk(int x, int z, Chunk chunk)
    {
        this.chunks[x + "_" + z] = chunk;
    }

    private Chunk GetChunk(int x, int z)
    {
        return this.chunks[x + "_" + z];
    }

    public Chunk GetOrCreateChunk(int x, int z)
    {
        if (this.chunks.ContainsKey(x + "_" + z))
        {
            return GetChunk(x, z);
        }
        else
        {
            var chunk = new Chunk(x, z, 1, blockPrefab);
            SetChunk(x, z, chunk);
            return chunk;
        }
    }

    public List<Chunk> GetChunks()
    {
        return new List<Chunk>(this.chunks.Values);
    }
}

// 1チャンクは xz 平面上の 16x16 のブロックで構成される
// chunkIndex (n, m) は xz 平面上の 16n ~ 16(n+1) - 1, 16m ~ 16(m+1) - 1 の範囲を表す
// 例えば chunkIndex (0, 0) は xz 平面上の 0 ~ 15, 0 ~ 15 の範囲を表す
class Chunk
{
    private List<GameObject> blocks = new List<GameObject>(16 * 16 * 300);
    private int chunkX;
    private int chunkZ;
    private int height;
    private GameObject blockPrefab;

    public Chunk(int chunkX, int chunkZ, int height, GameObject blockPrefab)
    {
        this.chunkX = chunkX;
        this.chunkZ = chunkZ;
        this.height = height;
        this.blockPrefab = blockPrefab;
        GenerateBlocks();
    }

    public void GenerateBlocks()
    {
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                for (int k = 0; k < height; k++)
                {
                    var pos = new Vector3(this.chunkX * 16 + i, -k, this.chunkZ * 16 + j);
                    GameObject block = GameObject.Instantiate(blockPrefab, pos, Quaternion.identity);
                    blocks.Add(block);
                }
            }
        }
    }

    public void SetActive(bool active)
    {
        foreach (var block in blocks)
        {
            block.SetActive(active);
        }
    }
}