using UnityEngine;

// Arayuz elemanini 3D dunyadaki bir noktanin ekrandaki yerinde tutar
public class DunyaTakip : MonoBehaviour
{
    public Vector3 dunyaKonumu;

    // Kamera Update icinde hareket ettigi icin konumu LateUpdate'te aliyoruz
    void LateUpdate()
    {
        Camera kamera = Camera.main;
        if (kamera == null) return;

        Vector3 ekranKonumu = kamera.WorldToScreenPoint(dunyaKonumu);
        ekranKonumu.z = 0f;
        transform.position = ekranKonumu;
    }
}
