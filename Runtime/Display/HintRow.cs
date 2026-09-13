using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

namespace InputHints.Display
{
    /// <summary>
    /// Spawns one <see cref="HintImage"/> child per control path (e.g. composite
    /// part paths from a rebinding UI). Children are pooled from
    /// <see cref="childPrefab"/> like <see cref="HintComposite"/> and arranged by
    /// the container's own layout. All-or-nothing: any unresolvable path or
    /// overflow leaves the row untouched so callers can fall back to text.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Input Hints/Display/Hint Row")]
    public class HintRow : MonoBehaviour
    {
        [Header("Row Content")]
        [Tooltip("Prefab spawned for each resolved control path.")]
        [SerializeField]
        private HintImage childPrefab;

        [Tooltip("Parent transform for spawned glyphs. Uses this transform when unset.")]
        [SerializeField]
        private Transform container;

        [Tooltip("Maximum glyphs shown at once. Longer lists are rejected.")]
        [Min(1)]
        [SerializeField]
        private int maxGlyphs = 4;

        /// <summary>
        /// Whether the row is currently showing glyphs.
        /// </summary>
        public bool IsShowing => shownCount > 0;

        /// <summary>
        /// How many glyphs are currently shown.
        /// </summary>
        public int ShownCount => shownCount;

        private const int DefaultPoolCapacity = 4;
        private const int MaxPoolSize = 12;

        private ObjectPool<HintImage> childPool;
        private readonly List<HintImage> activeChildren = new();
        private readonly List<Sprite> resolvedSprites = new();
        private readonly List<string> cachedPaths = new();
        private IReadOnlyList<InputDevice> cachedDevices;
        private bool cachedUnbound;
        private int shownCount;
        private bool isPoolInitialized;

        private void OnEnable()
        {
            EnsurePoolInitialized();
            HintManager.ProvidersChanged += Refresh;
        }

        private void OnDisable()
        {
            HintManager.ProvidersChanged -= Refresh;
            ReleaseAllChildren();
        }

        private void OnDestroy()
        {
            childPool?.Dispose();
            childPool = null;
        }

        /// <summary>
        /// Shows one glyph per path using the given devices, falling back to the
        /// device-independent lookup per path. Returns false without touching the
        /// row when any path is unresolvable.
        /// </summary>
        public bool TryShow(IReadOnlyList<InputDevice> devices, IReadOnlyList<string> controlPaths)
        {
            if (!Application.isPlaying)
                return false;

            EnsurePoolInitialized();

            if (!CanShow(controlPaths))
                return false;

            resolvedSprites.Clear();
            for (int i = 0; i < controlPaths.Count; i++)
            {
                Sprite sprite = null;
                if (
                    (
                        devices == null
                        || devices.Count == 0
                        || !HintManager.TryGetHint(devices, controlPaths[i], out sprite)
                    ) && !HintManager.TryGetHintUnbound(controlPaths[i], out sprite)
                )
                {
                    resolvedSprites.Clear();
                    return false;
                }

                if (sprite == null)
                {
                    resolvedSprites.Clear();
                    return false;
                }

                resolvedSprites.Add(sprite);
            }

            ApplySprites(resolvedSprites, controlPaths);
            CacheInputs(devices, controlPaths, false);
            return true;
        }

        /// <summary>
        /// Shows one glyph per path without requiring connected devices.
        /// Returns false without touching the row when any path is unresolvable.
        /// </summary>
        public bool TryShowUnbound(IReadOnlyList<string> controlPaths)
        {
            if (!Application.isPlaying)
                return false;

            EnsurePoolInitialized();

            if (!CanShow(controlPaths))
                return false;

            resolvedSprites.Clear();
            for (int i = 0; i < controlPaths.Count; i++)
            {
                if (!HintManager.TryGetHintUnbound(controlPaths[i], out Sprite sprite) || sprite == null)
                {
                    resolvedSprites.Clear();
                    return false;
                }

                resolvedSprites.Add(sprite);
            }

            ApplySprites(resolvedSprites, controlPaths);
            CacheInputs(null, controlPaths, true);
            return true;
        }

        /// <summary>
        /// Re-resolves the last shown paths (e.g. after providers change).
        /// Clears the row when they no longer resolve.
        /// </summary>
        public void Refresh()
        {
            if (!Application.isPlaying)
                return;

            if (!isActiveAndEnabled || cachedPaths.Count == 0)
                return;

            bool resolved = cachedUnbound ? TryShowUnbound(cachedPaths) : TryShow(cachedDevices, cachedPaths);

            if (!resolved)
                Clear();
        }

        /// <summary>
        /// Hides the row and releases its glyph children.
        /// </summary>
        public void Clear()
        {
            ReleaseAllChildren();
            cachedPaths.Clear();
            cachedDevices = null;
            shownCount = 0;
            gameObject.SetActive(false);
        }

        private bool CanShow(IReadOnlyList<string> controlPaths)
        {
            if (!isPoolInitialized)
                return false;

            if (childPrefab == null)
            {
                Debug.LogError($"{nameof(HintRow)}: childPrefab is not set.", this);
                return false;
            }

            if (controlPaths == null || controlPaths.Count == 0 || controlPaths.Count > maxGlyphs)
                return false;

            return true;
        }

        private void ApplySprites(IReadOnlyList<Sprite> sprites, IReadOnlyList<string> controlPaths)
        {
            ReleaseAllChildren();

            for (int i = 0; i < sprites.Count; i++)
            {
                HintImage child = childPool.Get();
                child.SetHint(sprites[i], controlPaths[i]);

                child.transform.SetParent(TargetContainer, false);
                child.transform.SetAsLastSibling();

                activeChildren.Add(child);
            }

            shownCount = sprites.Count;
            gameObject.SetActive(true);
        }

        private Transform TargetContainer => container != null ? container : transform;

        private void CacheInputs(IReadOnlyList<InputDevice> devices, IReadOnlyList<string> controlPaths, bool unbound)
        {
            cachedPaths.Clear();
            cachedPaths.AddRange(controlPaths);
            cachedDevices = devices;
            cachedUnbound = unbound;
        }

        private void EnsurePoolInitialized()
        {
            if (isPoolInitialized)
                return;

            childPool = new ObjectPool<HintImage>(
                createFunc: CreateChild,
                actionOnGet: child => child.gameObject.SetActive(true),
                actionOnRelease: child =>
                {
                    if (child != null && child.gameObject != null)
                    {
                        child.Image.sprite = null;
                        child.gameObject.SetActive(false);
                    }
                },
                actionOnDestroy: child =>
                {
                    if (child != null && child.gameObject != null)
                        Destroy(child.gameObject);
                },
                collectionCheck: false,
                defaultCapacity: DefaultPoolCapacity,
                maxSize: MaxPoolSize
            );

            isPoolInitialized = true;
        }

        private HintImage CreateChild()
        {
            HintImage child = Instantiate(childPrefab, TargetContainer, false);

            // The row drives this HintImage directly. Other presentation and
            // layout behaviours on the prefab keep their authored state.
            child.enabled = false;

            child.gameObject.SetActive(false);
            return child;
        }

        private void ReleaseAllChildren()
        {
            if (!isPoolInitialized)
                return;

            for (int i = 0; i < activeChildren.Count; i++)
            {
                if (activeChildren[i] != null)
                    childPool.Release(activeChildren[i]);
            }

            activeChildren.Clear();
        }
    }
}
