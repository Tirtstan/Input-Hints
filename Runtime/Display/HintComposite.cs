using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

namespace InputHints.Display
{
    /// <summary>
    /// Spawns one <see cref="HintImage"/> child per binding for actions with multiple bindings
    /// (e.g. WASD). Layout-agnostic — children are parented under <see cref="container"/>,
    /// children handles its own binding layout independently.
    /// Children are pooled via <see cref="ObjectPool{T}"/>.
    /// </summary>
    public class HintComposite : HintDisplay
    {
        [Header("Composite")]
        [Tooltip("Prefab spawned for each resolved binding path.")]
        [SerializeField]
        private HintImage childPrefab;

        [Tooltip("Parent transform where spawned hint children are placed.")]
        [SerializeField]
        private Transform container;

        [Tooltip("Maximum number of binding hints spawned at once.")]
        [Min(1)]
        [SerializeField]
        private int maxBindings = 4;

        private const int DefaultPoolCapacity = 4;
        private const int MaxPoolSize = 12;

        private ObjectPool<HintImage> childPool;
        private readonly List<HintImage> activeChildren = new();
        private bool isPoolInitialized;

        protected override void OnEnable()
        {
            EnsurePoolInitialized();
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ReleaseAllChildren();
        }

        private void OnDestroy()
        {
            childPool?.Dispose();
            childPool = null;
        }

        /// <summary>
        /// Overrides the default single-binding behavior to handle multiple bindings.
        /// Spawns one child HintImage per resolved binding path.
        /// </summary>
        protected override void OnBindingsResolved(
            IReadOnlyList<InputDevice> devices,
            IReadOnlyList<string> bindingPaths
        )
        {
            if (!isPoolInitialized)
                return;

            ReleaseAllChildren();

            int count = Mathf.Min(bindingPaths.Count, maxBindings);
            for (int i = 0; i < count; i++)
            {
                if (HintManager.TryGetHint(devices, bindingPaths[i], out Sprite sprite))
                {
                    HintImage child = childPool.Get();
                    child.SetHint(sprite, bindingPaths[i]);

                    child.transform.SetParent(container, false);
                    child.transform.SetAsLastSibling();

                    activeChildren.Add(child);
                }
            }
        }

        protected override void ApplyHint(Sprite sprite, string controlPath) { }

        protected override void ClearHint()
        {
            ReleaseAllChildren();
        }

        private void EnsurePoolInitialized()
        {
            if (isPoolInitialized)
                return;

            if (container == null)
            {
                Debug.LogError($"{nameof(HintComposite)}: container is not set.", this);
                return;
            }

            if (childPrefab == null)
            {
                Debug.LogError($"{nameof(HintComposite)}: childPrefab is not set.", this);
                return;
            }

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
            HintImage child = Instantiate(childPrefab, container, false);
            child.gameObject.SetActive(false);
            child.enabled = false;

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
