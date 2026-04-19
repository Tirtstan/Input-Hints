using UnityEngine;

namespace InputHints.Display
{
    /// <summary>
    /// Displays an input hint using a world-space <see cref="UnityEngine.SpriteRenderer"/>.
    /// Handles dynamic sprite assignments automatically.
    /// </summary>
    public class HintSpriteRenderer : HintDisplay
    {
        [Header("Target")]
        [Tooltip("SpriteRenderer that receives the resolved hint sprite.")]
        public SpriteRenderer SpriteRenderer;

#if UNITY_EDITOR
        private void Reset()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            if (SpriteRenderer == null)
                SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected override void ApplyHint(Sprite sprite, string controlPath)
        {
            if (SpriteRenderer == null)
                return;

            SpriteRenderer.sprite = sprite;
            SpriteRenderer.enabled = sprite != null;
        }

        protected override void ClearHint()
        {
            if (SpriteRenderer == null)
                return;

            SpriteRenderer.sprite = null;
            SpriteRenderer.enabled = false;
        }
    }
}
