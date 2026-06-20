using UnityEngine;
using UnityEngine.EventSystems;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class StoveView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite closedSprite;
        [SerializeField] Sprite openSprite;
        [SerializeField] IngredientsScreenView ingredientsScreen;

        bool _isOpen;

        void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            SetClosed();
        }

        void OnMouseDown()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (_isOpen)
                Close();
            else
                Open();
        }

        public void Open()
        {
            _isOpen = true;
            if (spriteRenderer != null && openSprite != null)
                spriteRenderer.sprite = openSprite;
            if (ingredientsScreen != null)
                ingredientsScreen.Open();
        }

        public void Close()
        {
            _isOpen = false;
            SetClosed();
            if (ingredientsScreen != null)
                ingredientsScreen.gameObject.SetActive(false);
        }

        void SetClosed()
        {
            if (spriteRenderer != null && closedSprite != null)
                spriteRenderer.sprite = closedSprite;
        }

        // Подмена спрайтов плиты при смене локации; обновляет текущий показанный спрайт.
        public void SetSprites(Sprite closed, Sprite open)
        {
            if (closed != null) closedSprite = closed;
            openSprite = open; // null допустим — тогда «открытая» плита не используется
            if (spriteRenderer != null)
                spriteRenderer.sprite = (_isOpen && openSprite != null) ? openSprite : closedSprite;
        }

        public bool IsOpen => _isOpen;
    }
}
