using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NucleonNode : DelayedActionScript
{
    public GameObject SpherePrefab;
    public ArenaBoundaries ArenaBoundaries;
    
    public GameAtomData GameAtomData;

    private List<GameObject> particles = new List<GameObject>();
    public float Size;

    public List<UnityAction<GameObject, bool>> OnDestruction = new List<UnityAction<GameObject, bool>>();

    private System.Random rnd = new System.Random();

    private float cooldownProton = 0;
    private float cooldownNeutron = 0;
    private float cooldownElectron = 0;
    private float cooldownGamma = 0;
    private bool unstable = false;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        countdownActive = !Data.Paused;
        if (!Data.Paused)
        {
            cooldownElectron -= cooldownElectron <= 0 ? 0 : Time.deltaTime;
            cooldownGamma -= cooldownGamma <= 0 ? 0 : Time.deltaTime;
            cooldownNeutron -= cooldownNeutron <= 0 ? 0 : Time.deltaTime;
            cooldownProton -= cooldownProton <= 0 ? 0 : Time.deltaTime;

            if (this.GameAtomData.IsUnstableInGame() && !unstable)
            {
                foreach (var callback in OnDestruction)
                {
                    callback.Invoke(this.gameObject, this.GetComponent<PlayerNode>() == null ? false : true);
                }
            }
            unstable = GameAtomData.IsUnstableInGame();
        }
    }

    void OnDestroy()
    {
        particles.ForEach(particle =>
        {
            if (particle != null) Destroy(particle);
        });
    }

    #region ShootMethods

    public bool Shoot(Throwable throwable, Vector3 dir)
    {
        dir.Normalize();
        List<GameObject> pList;
        switch (throwable)
        {
            case Throwable.Proton:
                if (cooldownProton > 0) return false;
                pList = particles.FindAll(p => p.GetComponent<NucleonMotion>().ParticleType == ParticleType.Proton && Vector3.Angle(dir, p.transform.localPosition) <= 90);
                pList.Sort(new NucleonComparer());
                if (pList.Count > 0) shootObject(pList[0], dir, 60);
                else return false;
                cooldownProton = GameAtomData.ProtonReload;

                InvokeLater(() =>
                {
                    var spawnDistance = Mathf.Sqrt((SpherePrefab.transform.localScale.x * 10 * GameAtomData.NucleonCount) / (4 * Mathf.PI));
                    spawnProton(spawnDistance);
                }, GameAtomData.ProtonRespawn);
                break;
            case Throwable.Neutron:
                if (cooldownNeutron > 0) return false;
                pList = particles.FindAll(p => p.GetComponent<NucleonMotion>().ParticleType == ParticleType.Neutron && Vector3.Angle(dir, p.transform.localPosition) <= 90);
                pList.Sort(new NucleonComparer());
                if (pList.Count > 0) shootObject(pList[0], dir, 50);
                else return false;
                cooldownNeutron = GameAtomData.NeutronReload;

                InvokeLater(() =>
                {
                    var spawnDistance = Mathf.Sqrt((SpherePrefab.transform.localScale.x * 10 * GameAtomData.NucleonCount) / (4 * Mathf.PI));
                    spawnNeutron(spawnDistance);
                }, GameAtomData.ProtonRespawn);
                break;
            case Throwable.Electron:
                if (cooldownElectron > 0) return false;
                pList = particles.FindAll(p => p.GetComponent<NucleonMotion>().ParticleType == ParticleType.Electron && Vector3.Angle(dir, p.transform.localPosition) <= 120);
                pList.Sort(new NucleonComparer());
                if (pList.Count > 0) shootObject(pList[0], dir, 100);
                else return false;
                cooldownElectron = GameAtomData.ElectronReload;

                var speed = pList[0].GetComponent<NucleonMotion>().Speed;
                var rotAxis = pList[0].GetComponent<NucleonMotion>().RotationAxis;
                var height = pList[0].GetComponent<NucleonMotion>().Height;

                InvokeLater(() =>
                {
                    spawnElectron(speed, height, rotAxis);
                }, GameAtomData.ProtonRespawn);
                break;
            case Throwable.GammaRay:
                break;
            default:
                break;
        }

        return true;
    }

    private void shootObject(GameObject obj, Vector3 dir, float speed)
    {
        obj.transform.SetParent(transform.parent);
        var nucleonMotion = obj.GetComponent<NucleonMotion>();
        //obj.GetComponent<Rigidbody>().drag = 0;
        //obj.GetComponent<Rigidbody>().AddForce(dir * speed, ForceMode.VelocityChange);
        nucleonMotion.Direction = dir;
        nucleonMotion.Speed = speed;
        nucleonMotion.MotionType = MotionType.Straight;
        particles.Remove(obj);
        InvokeLater(() =>
        {
            Destroy(obj);
        }, 60);
        
    }

    #endregion

    #region SpawnMethods

    public void spawnNukleons(Nuclide nuclide)
    {
        resetAll();

        var spawnDistance = Mathf.Sqrt((SpherePrefab.transform.localScale.x * 10 * nuclide.NucleonCount) / (4 * Mathf.PI));
        Size = Data.SphereScaleElectron.x * (nuclide.ProtonCount + 5) + spawnDistance;

        if (transform.localPosition.y < Size * 2 + ArenaBoundaries.Thickness) transform.localPosition = new Vector3(transform.localPosition.x, Size * 2 + ArenaBoundaries.Thickness, transform.localPosition.z);

        for (int i = 0; i < nuclide.NucleonCount + nuclide.ProtonCount; i++)
        {
            if (i < nuclide.NeutronCount)
            {
                spawnNeutron(spawnDistance);
            }
            else if (i < nuclide.NucleonCount)
            {
                spawnProton(spawnDistance);
            }
            else
            {
                spawnElectron((50 - Mathf.Pow(getPrincipalQuantumNumber(i - nuclide.NucleonCount + 1), 1.5f)) * ((i - nuclide.NucleonCount + 2) / 2f == (i - nuclide.NucleonCount + 2) / 2 ? -1 : 1), Data.SphereScaleElectron.x * (i - nuclide.NucleonCount + 4) + spawnDistance, getElectronRotationAxis(i - nuclide.NucleonCount + 1));
            }
        }
    }

    public void spawnProton(float spawnDistance)
    {
        var obj = Instantiate(SpherePrefab);

        var nucleonMotion = obj.GetComponent<NucleonMotion>();
        nucleonMotion.MassCenter = Vector3.zero;
        nucleonMotion.MotionType = MotionType.Point;
        nucleonMotion.ParticleType = ParticleType.Proton;
        nucleonMotion.changeType(nucleonMotion.ParticleType);
        nucleonMotion.ParentNode = this;

        obj.transform.SetParent(transform);
        nucleonMotion.Speed = 2f;
        var vec = new Vector3((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f);
        vec.Normalize();
        obj.transform.localPosition = vec * spawnDistance;

        particles.Add(obj);
    }

    public void spawnNeutron(float spawnDistance)
    {
        var obj = Instantiate(SpherePrefab);

        var nucleonMotion = obj.GetComponent<NucleonMotion>();
        nucleonMotion.MassCenter = Vector3.zero;
        nucleonMotion.MotionType = MotionType.Point;
        nucleonMotion.ParticleType = ParticleType.Neutron;
        nucleonMotion.changeType(nucleonMotion.ParticleType);
        nucleonMotion.ParentNode = this;

        obj.transform.SetParent(transform);
        nucleonMotion.Speed = 2f;
        var vec = new Vector3((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f);
        vec.Normalize();
        obj.transform.localPosition = vec * spawnDistance;

        particles.Add(obj);
    }

    public void spawnElectron(float speed, float height, Vector3 rotAxis)
    {
        var obj = Instantiate(SpherePrefab);

        var nucleonMotion = obj.GetComponent<NucleonMotion>();
        nucleonMotion.MassCenter = Vector3.zero;
        nucleonMotion.MotionType = MotionType.Point;
        nucleonMotion.ParticleType = ParticleType.Electron;
        nucleonMotion.changeType(nucleonMotion.ParticleType);
        nucleonMotion.ParentNode = this;

        obj.transform.SetParent(transform);
        nucleonMotion.Speed = speed;
        nucleonMotion.RotationAxis = rotAxis;
        nucleonMotion.Height = height;
        var vec = Vector3.Cross(nucleonMotion.RotationAxis, Vector3.up);
        vec.Normalize();
        obj.transform.localPosition = vec * nucleonMotion.Height;

        particles.Add(obj);
    }

    #endregion

    #region Apply sphere configs

    public void applyNucleonData(GameObject obj, ParticleType particletype)
    {
        if (obj.GetComponent<NucleonMotion>() == null) return;
        if (particles.Contains(obj)) particles.Remove(obj);
        if (obj.GetComponent<NucleonMotion>().ParentNode != null && obj.GetComponent<NucleonMotion>().ParentNode != this && obj.GetComponent<NucleonMotion>().ParentNode.particles.Contains(obj)) obj.GetComponent<NucleonMotion>().ParentNode.particles.Remove(obj);

        var nucleonMotion = obj.GetComponent<NucleonMotion>();
        nucleonMotion.MassCenter = Vector3.zero;
        nucleonMotion.MotionType = MotionType.Point;
        nucleonMotion.ParticleType = particletype;
        nucleonMotion.changeType(nucleonMotion.ParticleType);
        nucleonMotion.ParentNode = this;

        obj.transform.SetParent(transform);
        nucleonMotion.Speed = 2f;
        obj.GetComponent<Rigidbody>().drag = SpherePrefab.GetComponent<Rigidbody>().drag;

        particles.Add(obj);
    }

    #endregion

    private void resetAll()
    {
        foreach (var particle in particles)
        {
            Destroy(particle);
        }
        particles = new List<GameObject>();
    }

    #region Helper

    private Vector3 getElectronRotationAxis(int i)
    {
        var principalQNumber = getPrincipalQuantumNumber(i);
        var eCountUnderPrincipal = 0;
        for (int o = 1; o < principalQNumber; o++)
        {
            eCountUnderPrincipal += 2 * (int)Mathf.Pow(o, 2);
        }
        var numberInPrincipal = i - eCountUnderPrincipal - 1;
        var result = new Vector3(Mathf.Cos((Mathf.PI * numberInPrincipal) / (2 * Mathf.Pow(principalQNumber, 2))), 0, Mathf.Sin((Mathf.PI * numberInPrincipal) / (2 * Mathf.Pow(principalQNumber, 2))));
        result.Normalize();
        return result;
    }

    private int getPrincipalQuantumNumber(int number)
    {
        if (number <= 0) return 0;
        var result = 0;
        var buffer = 0;
        while (buffer < number)
        {
            result++;
            buffer += 2 * (int)Mathf.Pow(result, 2);
        }
        return result;
    }

    public void AddExplosionForceToParticles(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier, ForceMode mode)
    {
        particles.ForEach(particle =>
        {
            if (particle != null) particle.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, explosionPosition, explosionRadius, upwardsModifier, mode);
        });
    }

    public void CreateDeathScene(float deathTime)
    {
        AddExplosionForceToParticles(100, transform.position, Size * 2, 2, ForceMode.VelocityChange);
        particles.ForEach(particle =>
        {
            if(particle != null)
            {
                var nm = particle.GetComponent<NucleonMotion>();
                var size = nm.ParticleType == ParticleType.Electron ? Data.SphereScaleElectron : (nm.ParticleType == ParticleType.Neutron ? Data.SphereScaleNeutron : Data.SphereScaleProton);
                var shrinkTimes = deathTime / (Time.fixedDeltaTime);
                nm.Speed = 50;
                nm.ShrinkPerFixedUpdate = size / shrinkTimes;
                nm.MotionType = MotionType.ShrinkKill;
            }
        });
    }

    #endregion

    #region Private Classes

    private class NucleonComparer : IComparer<GameObject>
    {
        public int Compare(GameObject x, GameObject y)
        {
            var dx = x.transform.position.magnitude;
            var dy = y.transform.position.magnitude;
            return dx > dy ? -1 : (dx < dy ? 1 : 0);
        }
    }

    #endregion
}
