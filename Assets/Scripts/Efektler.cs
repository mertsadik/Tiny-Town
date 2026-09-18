using UnityEngine;

// Parcacik efektleri: karo yere degince toz, gorev tamamlaninca renkli konfeti.
// Iki parcacik sistemi bir kez kurulur, her seferinde istenen yere tasinip parcacik firlatilir.
public class Efektler : MonoBehaviour
{
    ParticleSystem toz;
    ParticleSystem konfeti;

    public void Kur()
    {
        // Saydam URP parcacik malzemesi, dokusu beyaz bir daire (Resources/Parcaciklar/daire.png)
        Material malzeme = Resources.Load<Material>("Parcaciklar/ParcacikMalzeme");
        if (malzeme == null)
        {
            Debug.LogError("Parcaciklar/ParcacikMalzeme bulunamadi, efektler calismayacak");
            return;
        }

        toz = ParcacikSistemiOlustur("Toz", malzeme);
        ParticleSystem.MainModule tozAna = toz.main;
        tozAna.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.6f);
        tozAna.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        tozAna.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
        tozAna.startColor = new Color(1f, 1f, 0.97f, 0.85f);
        tozAna.gravityModifier = -0.05f; // hafifce yukselsin

        // Toz karonun etrafinda halka seklinde yayilir
        ParticleSystem.ShapeModule tozSekli = toz.shape;
        tozSekli.shapeType = ParticleSystemShapeType.Circle;
        tozSekli.radius = 0.9f;
        tozSekli.radiusThickness = 0f;       // sadece halkanin kenarindan cikar
        toz.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // halka yere paralel olsun

        konfeti = ParcacikSistemiOlustur("Konfeti", malzeme);
        ParticleSystem.MainModule konfetiAna = konfeti.main;
        konfetiAna.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.1f);
        konfetiAna.startSpeed = new ParticleSystem.MinMaxCurve(4f, 6.5f);
        konfetiAna.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.3f);
        // Her parcacik bu iki renk arasinda rastgele bir renk alir
        konfetiAna.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.8f, 0.2f), new Color(1f, 0.45f, 0.35f));
        konfetiAna.gravityModifier = 1.2f;

        // Konfeti yukari dogru bir koni icinde firlar
        ParticleSystem.ShapeModule konfetiSekli = konfeti.shape;
        konfetiSekli.shapeType = ParticleSystemShapeType.Cone;
        konfetiSekli.angle = 30f;
        konfetiSekli.radius = 0.3f;
        konfeti.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // koni yukari baksin
    }

    public void TozCikar(Vector3 konum)
    {
        if (toz == null) return;
        toz.transform.position = konum + Vector3.up * 0.2f;
        toz.Emit(14);
    }

    public void KonfetiPatlat(Vector3 konum)
    {
        if (konfeti == null) return;
        konfeti.transform.position = konum + Vector3.up * 0.8f;
        konfeti.Emit(30);
    }

    ParticleSystem ParcacikSistemiOlustur(string isim, Material malzeme)
    {
        GameObject obj = new GameObject(isim);
        obj.transform.SetParent(transform, false);

        ParticleSystem sistem = obj.AddComponent<ParticleSystem>();
        // AddComponent sistemi hemen calistirir, once durduruyoruz ki ayarlar degistirilebilsin
        sistem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule ana = sistem.main;
        ana.playOnAwake = false;
        ana.loop = false;
        ana.duration = 1f;
        ana.maxParticles = 100;
        ana.simulationSpace = ParticleSystemSimulationSpace.World; // sistem tasininca eski parcaciklar yerinde kalsin

        // Kendiliginden parcacik uretmesin, sadece Emit ile
        ParticleSystem.EmissionModule uretim = sistem.emission;
        uretim.rateOverTime = 0f;

        // Omru bitene dogru saydamlasir
        ParticleSystem.ColorOverLifetimeModule renk = sistem.colorOverLifetime;
        renk.enabled = true;
        Gradient gecis = new Gradient();
        gecis.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
        renk.color = gecis;

        // Omru boyunca biraz kuculur
        ParticleSystem.SizeOverLifetimeModule boyut = sistem.sizeOverLifetime;
        boyut.enabled = true;
        boyut.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.4f));

        ParticleSystemRenderer cizici = obj.GetComponent<ParticleSystemRenderer>();
        cizici.sharedMaterial = malzeme;

        return sistem;
    }
}
