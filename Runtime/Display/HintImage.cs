using UnityEngine;
using UnityEngine.UI;

namespace InputHints.Display
{
    /// <summary>
    /// Displays an input hint as a UI <see cref="Image"/> sprite.
    /// Handles dynamic sprite assignments automatically.
    /// Implements <see cref="ILayoutElement"/> for layout system compatibility.
    /// </summary>
    public class HintImage : HintDisplay, ILayoutElement
    {
        [Header("Target")]
        [Tooltip("UI Image that receives the resolved hint sprite.")]
        public Image Image;

        [Header("Layout Element")]
        [Tooltip("Expose preferred size values to Unity layout components.")]
        public bool EnableLayoutElement;

        [Tooltip("Layout priority reported when this component acts as an ILayoutElement.")]
        public int LayoutElementPriority = 1;

        [Tooltip("Base height used for preferred size. Width preserves sprite aspect ratio.")]
        public float LayoutElementSize = 100f;

#if UNITY_EDITOR
        private void Reset()
        {
            Image = GetComponent<Image>();
            if (Image != null)
                Image.preserveAspect = true;
        }
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Image == null)
                Image = GetComponent<Image>();
        }

        /// <summary>
        /// Manually set the hint sprite and optional control path.
        /// Useful for composite displays that manage child images.
        /// </summary>
        public void SetHint(Sprite sprite, string controlPath = null)
        {
            ApplyHint(sprite, controlPath);
        }

        protected override void ApplyHint(Sprite sprite, string controlPath)
        {
            if (Image == null)
                return;

            Image.sprite = sprite;
            Image.enabled = sprite != null;
            SetDirty();
        }

        protected override void ClearHint()
        {
            if (Image == null)
                return;

            Image.sprite = null;
            Image.enabled = false;
            SetDirty();
        }

        public virtual void CalculateLayoutInputHorizontal() { }

        public virtual void CalculateLayoutInputVertical() { }

        public virtual int layoutPriority => EnableLayoutElement ? LayoutElementPriority : -1;
        public virtual float minWidth => -1;
        public virtual float minHeight => -1;
        public virtual float preferredWidth
        {
            get
            {
                if (Image == null || Image.sprite == null)
                    return LayoutElementSize;

                float ratio = Image.sprite.rect.width / Image.sprite.rect.height;
                return LayoutElementSize * ratio;
            }
        }
        public virtual float preferredHeight => LayoutElementSize;
        public virtual float flexibleWidth => -1;
        public virtual float flexibleHeight => -1;

        private void SetDirty()
        {
            if (!isActiveAndEnabled)
                return;

            RectTransform rect = transform as RectTransform;
            if (rect == null)
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rect);
        }
    }
}
