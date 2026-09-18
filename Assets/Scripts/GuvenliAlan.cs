using UnityEngine;

// Telefonlardaki centik ve kamera deligi gibi alanlara arayuz girmesin diye
// bu objenin sinirlarini ekranin "guvenli alanina" (Screen.safeArea) oturtur.
public class GuvenliAlan : MonoBehaviour
{
    RectTransform alan;
    Rect sonGuvenliAlan;
    Vector2 sonEkranBoyutu;

    void Awake()
    {
        alan = GetComponent<RectTransform>();
        Uygula();
    }

    void Update()
    {
        // Ekran donerse ya da boyut degisirse yeniden hesapla
        if (Screen.safeArea != sonGuvenliAlan || Screen.width != sonEkranBoyutu.x || Screen.height != sonEkranBoyutu.y)
        {
            Uygula();
        }
    }

    void Uygula()
    {
        Rect guvenli = Screen.safeArea;
        sonGuvenliAlan = guvenli;
        sonEkranBoyutu = new Vector2(Screen.width, Screen.height);

        if (Screen.width <= 0 || Screen.height <= 0) return;

        Vector2 enAlt = guvenli.position;
        Vector2 enUst = guvenli.position + guvenli.size;
        enAlt.x /= Screen.width;
        enAlt.y /= Screen.height;
        enUst.x /= Screen.width;
        enUst.y /= Screen.height;

        alan.anchorMin = enAlt;
        alan.anchorMax = enUst;
        alan.offsetMin = Vector2.zero;
        alan.offsetMax = Vector2.zero;
    }
}
