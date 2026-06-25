using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

// ブースト入力中、プレイヤーの全メッシュの残像を生成する。
public class SC_AfterImage : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 0.02f; // 残像の生成間隔
    [SerializeField] private float lifeTime = 0.05f;      // 残像1枚の寿命

    [Header("Reference")]
    [Tooltip("子から自動取得")]
    [SerializeField] private SkinnedMeshRenderer[] skinnedMeshRenderers;
    [SerializeField] private Material ghostMaterial;       // 残像用マテリアル

    [Header("Boost Input")]
    [Tooltip("SC_PlayerStateManager から自動取得")]
    [SerializeField] private InputActionReference sprintInput;
    [SerializeField] private float inputThreshold = 0.1f; 

    [Header("Fade Settings")]
    [SerializeField, Range(0f, 1f)] private float startAlpha = 0.2f; // 残像の初期透明度

    private const int DefaultCapacity = 12; // プールの事前確保数
    private const int MaxCapacity = 64;     // プールの最大保持数

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private readonly List<Ghost> _active = new();
    private InputAction _sprintAction;
    private ObjectPool<Ghost> _pool;
    private Transform _container;
    private int _colorId;
    private float _spawnTimer;

    private void Awake()
    {
        if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);

        _sprintAction = ResolveSprintAction();
        _colorId = ResolveColorId(ghostMaterial);
        WarnIfMisconfigured();

        _container = new GameObject("AfterImageContainer").transform;
        _spawnTimer = spawnInterval;

        _pool = new ObjectPool<Ghost>(
            createFunc: CreateGhost,
            actionOnGet: ghost => ghost.GameObject.SetActive(true),
            actionOnRelease: ghost => ghost.GameObject.SetActive(false),
            actionOnDestroy: ghost => ghost.Dispose(),
            collectionCheck: false,
            defaultCapacity: DefaultCapacity,
            maxSize: MaxCapacity);

        WarmUp();
    }

    // 入力を読めるようアクションを有効化する
    private void OnEnable()
    {
        if (_sprintAction != null && !_sprintAction.enabled) _sprintAction.Enable();
    }

    // 入力中は残像を生成し、生成済みの残像をフェードさせる
    private void Update()
    {
        float deltaTime = Time.deltaTime;

        if (IsBoostHeld())
        {
            _spawnTimer += deltaTime;
            while (_spawnTimer >= spawnInterval)
            {
                _spawnTimer -= spawnInterval;
                SpawnGhosts();
            }
        }
        else
        {
            _spawnTimer = spawnInterval; // 次回ブースト開始時に即生成
        }

        TickGhosts(deltaTime);
    }

    // 無効化時は生成中の残像をすべてプールへ戻す
    private void OnDisable()
    {
        for (int i = _active.Count - 1; i >= 0; i--) _pool.Release(_active[i]);
        _active.Clear();
        _spawnTimer = spawnInterval;
    }

    // ブースト入力が閾値を超えて押されているか
    private bool IsBoostHeld() =>
        _sprintAction != null && _sprintAction.ReadValue<float>() > inputThreshold;

    // 全メッシュの残像を1枚ずつ生成する
    private void SpawnGhosts()
    {
        if (skinnedMeshRenderers == null) return;

        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            var smr = skinnedMeshRenderers[i];
            if (smr == null || !smr.gameObject.activeInHierarchy) continue;

            var ghost = _pool.Get();
            ghost.Play(smr, startAlpha, lifeTime);
            _active.Add(ghost);
        }
    }

    // 生成済み残像のフェードを進め、寿命切れをプールへ返す
    private void TickGhosts(float deltaTime)
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            if (!_active[i].Tick(deltaTime)) continue;
            _pool.Release(_active[i]);
            _active.RemoveAt(i);
        }
    }

    private Ghost CreateGhost() => new(_container, ghostMaterial, _colorId);

    // 起動時にプールを満たし、初回ブーストの生成コストを平準化
    private void WarmUp()
    {
        var warm = new Ghost[DefaultCapacity];
        for (int i = 0; i < DefaultCapacity; i++) warm[i] = _pool.Get();
        for (int i = 0; i < DefaultCapacity; i++) _pool.Release(warm[i]);
    }

    // ブースト入力を解決する
    private InputAction ResolveSprintAction()
    {
        if (sprintInput != null && sprintInput.action != null) return sprintInput.action;

        var stateManager = GetComponentInParent<SC_PlayerStateManager>();
        return stateManager != null && stateManager.sprintInput != null
            ? stateManager.sprintInput.action
            : null;
    }

    // マテリアルが持つカラープロパティIDを返す
    private static int ResolveColorId(Material material)
    {
        if (material == null) return BaseColorId;
        if (material.HasProperty(BaseColorId)) return BaseColorId;
        if (material.HasProperty(ColorId)) return ColorId;
        return BaseColorId;
    }

    // 残像が出ない典型原因を起動時に通知する
    private void WarnIfMisconfigured()
    {
        if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
            Debug.LogWarning("[SC_AfterImage] SkinnedMeshRenderer が見つかりません。", this);
        if (ghostMaterial == null)
            Debug.LogWarning("[SC_AfterImage] Ghost Material が未割り当てです。", this);
        if (_sprintAction == null)
            Debug.LogWarning("[SC_AfterImage] ブースト入力を解決できません。Sprint Input を割り当ててください。", this);
    }

    // 残像1枚分の軽量オブジェクト
    private sealed class Ghost
    {
        public readonly GameObject GameObject;

        private readonly Transform _transform;
        private readonly MeshFilter _meshFilter;
        private readonly MeshRenderer _meshRenderer;
        private readonly MaterialPropertyBlock _mpb = new();
        private readonly Mesh _bakedMesh = new() { name = "AfterImageBakedMesh" };
        private readonly int _colorId;

        private Color _color;
        private float _startAlpha;
        private float _lifeTime;
        private float _elapsed;

        // 描画用のGameObjectとコンポーネントを生成
        public Ghost(Transform parent, Material material, int colorId)
        {
            _colorId = colorId;
            _color = material != null && material.HasProperty(colorId)
                ? material.GetColor(colorId) : Color.white;

            GameObject = new GameObject("AfterImage");
            _transform = GameObject.transform;
            _transform.SetParent(parent);

            _meshFilter = GameObject.AddComponent<MeshFilter>();
            _meshRenderer = GameObject.AddComponent<MeshRenderer>();
            _meshRenderer.sharedMaterial = material;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
        }

        // 現在のポーズを既存メッシュへ焼き、フェード再生を開始
        public void Play(SkinnedMeshRenderer source, float startAlpha, float lifeTime)
        {
            source.BakeMesh(_bakedMesh);
            _meshFilter.sharedMesh = _bakedMesh;

            var src = source.transform;
            _transform.SetPositionAndRotation(src.position, src.rotation);
            _transform.localScale = src.lossyScale;

            _startAlpha = startAlpha;
            _lifeTime = Mathf.Max(0.0001f, lifeTime);
            _elapsed = 0f;
            ApplyAlpha(startAlpha);
        }

        // フェードを1フレーム進め、寿命が尽きたらtrueを返す
        public bool Tick(float deltaTime)
        {
            _elapsed += deltaTime;
            ApplyAlpha(Mathf.Lerp(_startAlpha, 0f, _elapsed / _lifeTime));
            return _elapsed >= _lifeTime;
        }

        // MaterialPropertyBlockでアルファのみ上書きする
        private void ApplyAlpha(float alpha)
        {
            _color.a = alpha;
            _mpb.SetColor(_colorId, _color);
            _meshRenderer.SetPropertyBlock(_mpb);
        }

        // 焼き込みメッシュとGameObjectを破棄する
        public void Dispose()
        {
            if (_bakedMesh != null) Object.Destroy(_bakedMesh);
            if (GameObject != null) Object.Destroy(GameObject);
        }
    }
}