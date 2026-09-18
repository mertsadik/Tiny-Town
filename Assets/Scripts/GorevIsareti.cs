using UnityEngine;
using UnityEngine.UI;

// Gorevli karonun ustunde duran "3/6" rozeti. Her karede karonun ekrandaki yerine tasinir.
public class GorevIsareti : MonoBehaviour
{
    public Vector3 dunyaKonumu;
    public Text yazi;

    // Kamera Update icinde hareket ettigi icin konumu LateUpdate'te aliyoruz
    void LateUpdate()
    {
        Camera kamera = Camera.main;
        if (kamera == null) return;

        Vector3 ekranKonumu = kamera.WorldToScreenPoint(dunyaKonumu);
        ekranKonumu.z = 0f;
        transform.position = ekranKonumu;
    }

    public void YaziGuncelle(int mevcut, int hedef)
    {
        yazi.text = mevcut + "/" + hedef;
    }
}
