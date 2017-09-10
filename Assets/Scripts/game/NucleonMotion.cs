using UnityEngine;

public class NucleonMotion : MonoBehaviour {

    public NucleonNode ParentNode;
    public Vector3 MassCenter;
    public Vector3 Direction;
    public Vector3 RotationAxis;
    public float Height;

    public float Speed = 2f;
    public Vector3 ShrinkPerFixedUpdate = Vector3.zero;

    private ParticleType particleType;
    public ParticleType ParticleType
    {
        get
        {
            return particleType;
        }
        set
        {
            if (particleType != value) changeType(value);
            particleType = value;
        }
    }

    private MotionType motionType;
    public MotionType MotionType
    {
        get
        {
            return motionType;
        }
        set
        {
            motionType = value;
        }
    }

    private Rigidbody rb;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Data.Paused)
        {
            rb.isKinematic = true;
        }
        else
        {
            if (rb.isKinematic) rb.isKinematic = false;

            if (MotionType == MotionType.Straight)
            {
                var dir = Direction;
                dir.Normalize();
                rb.velocity = dir * Speed;
            }
            if(MotionType == MotionType.Point)
            {
                if(ParticleType == ParticleType.Electron)
                {
                    //var pos = Vector3.RotateTowards(transform.localPosition - MassCenter, RotationAxis, AngleSpeed * Time.deltaTime, 0f);
                    //pos.Normalize();
                    //transform.localPosition = pos * Height;
                    var pos = transform.localPosition;
                    pos.Normalize();
                    transform.localPosition = pos * Height;
                    var dir = Vector3.Cross(MassCenter - transform.localPosition, RotationAxis);
                    dir.Normalize();
                    rb.velocity = dir * Speed;

                }
                else
                {
                    var dir = MassCenter - transform.localPosition;
                    dir.Normalize();
                    rb.AddForce(dir * Speed, ForceMode.Impulse);
                }
            }
            //if(MotionType == MotionType.ShrinkKill)
            //{
            //    var dir = transform.localPosition - MassCenter;
            //    dir.Normalize();
            //    rb.AddForce(dir * Speed, ForceMode.Impulse);
            //}
        }
    }

    void FixedUpdate()
    {
        if (!Data.Paused)
        {
            if(MotionType == MotionType.ShrinkKill)
            {
                transform.localScale -= ShrinkPerFixedUpdate;
            }
        }
    }

    public void changeType(ParticleType type)
    {
        var renderer = GetComponent<Renderer>();
        switch (type)
        {
            case ParticleType.Proton:
                renderer.material = Data.MatProton;
                transform.localScale = Data.SphereScaleProton;
                break;
            case ParticleType.Neutron:
                renderer.material = Data.MatNeutron;
                transform.localScale = Data.SphereScaleNeutron;
                break;
            case ParticleType.Electron:
                renderer.material = Data.MatElectron;
                transform.localScale = Data.SphereScaleElectron;
                break;
            default:
                break;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        var nm = col.gameObject.GetComponent<NucleonMotion>();
        if (nm == null) return;
        if (nm.ParentNode == ParentNode) return;
        if (MotionType == MotionType.ShrinkKill || nm.MotionType == MotionType.ShrinkKill) return;
        if (nm.gameObject.GetInstanceID() < gameObject.GetInstanceID()) return;
        var particletype = nm.ParticleType;

        if((ParticleType == ParticleType.Electron && particletype == ParticleType.Proton) || ( ParticleType == ParticleType.Proton && particletype == ParticleType.Electron))
        {
            if(ParticleType == ParticleType.Proton)
            {
                Destroy(col.gameObject, Time.fixedDeltaTime);
            }
            else if(ParticleType == ParticleType.Electron)
            {
                Destroy(this.gameObject, Time.fixedDeltaTime);
            }

            if (ParentNode != null && MotionType == MotionType.Point && (nm.ParentNode == null || nm.MotionType == MotionType.Straight))
            {
                if(ParticleType == ParticleType.Proton)
                {
                    ParentNode.GameAtomData.ProtonCount--;
                    ParentNode.GameAtomData.NeutronCount++;
                    ParentNode.applyNucleonData(gameObject, ParticleType.Neutron);
                }
                else if(ParticleType == ParticleType.Electron)
                {
                    var speed = Speed;
                    var height = Height;
                    var rotAxis = RotationAxis;
                    ParentNode.GameAtomData.NeutronCount++;
                    ParentNode.applyNucleonData(nm.gameObject, ParticleType.Neutron);
                    ParentNode.InvokeLater(() =>
                    {
                        ParentNode.spawnElectron(speed, height, rotAxis);
                    }, ParentNode.GameAtomData.ElectronRespawn);
                }
            }
            else if(nm.ParentNode != null && nm.MotionType == MotionType.Point && (ParentNode == null || MotionType == MotionType.Straight))
            {
                if (nm.ParticleType == ParticleType.Proton)
                {
                    nm.ParentNode.GameAtomData.ProtonCount--;
                    nm.ParentNode.GameAtomData.NeutronCount++;
                    nm.ParentNode.applyNucleonData(nm.gameObject, ParticleType.Neutron);
                }
                else if (nm.ParticleType == ParticleType.Electron)
                {
                    var speed = nm.Speed;
                    var height = nm.Height;
                    var rotAxis = nm.RotationAxis;
                    nm.ParentNode.GameAtomData.NeutronCount++;
                    nm.ParentNode.applyNucleonData(gameObject, ParticleType.Neutron);
                    nm.ParentNode.InvokeLater(() =>
                    {
                        nm.ParentNode.spawnElectron(speed, height, rotAxis);
                    }, nm.ParentNode.GameAtomData.ElectronRespawn);
                }
            }
            else if(MotionType == MotionType.Straight && nm.MotionType == MotionType.Straight)
            {
                if(ParticleType == ParticleType.Proton)
                {
                    ParticleType = ParticleType.Neutron;
                    Destroy(col.gameObject, Time.fixedDeltaTime);
                }
                else if(ParticleType == ParticleType.Electron)
                {
                    nm.ParticleType = ParticleType.Neutron;
                    Destroy(gameObject, Time.fixedDeltaTime);
                }
            }
        }
        else if((ParticleType == ParticleType.Neutron && particletype == ParticleType.Proton) || (ParticleType == ParticleType.Proton && particletype == ParticleType.Neutron))
        {
            if (nm.MotionType == MotionType.Point && MotionType == MotionType.Point) return;
            if(nm.MotionType == MotionType.Point && MotionType == MotionType.Straight && nm.ParentNode != null)
            {
                nm.ParentNode.applyNucleonData(gameObject, ParticleType);
                if (ParticleType == ParticleType.Neutron) nm.ParentNode.GameAtomData.NeutronCount++;
                if (ParticleType == ParticleType.Proton) nm.ParentNode.GameAtomData.ProtonCount++;
            }
            else if(MotionType == MotionType.Point && nm.MotionType == MotionType.Straight && ParentNode != null)
            {
                ParentNode.applyNucleonData(nm.gameObject, nm.ParticleType);
                if (nm.ParticleType == ParticleType.Neutron) ParentNode.GameAtomData.NeutronCount++;
                if (nm.ParticleType == ParticleType.Proton) ParentNode.GameAtomData.ProtonCount++;
            }
        }
        else if ((ParticleType == ParticleType.Neutron && particletype == ParticleType.Neutron) || (ParticleType == ParticleType.Proton && particletype == ParticleType.Proton))
        {
            if ((nm.MotionType == MotionType.Point && MotionType == MotionType.Point) || (nm.MotionType == MotionType.Straight && MotionType == MotionType.Straight)) return;
            if (nm.MotionType == MotionType.Point && MotionType == MotionType.Straight && nm.ParentNode != null)
            {
                nm.ParentNode.applyNucleonData(gameObject, ParticleType);
                if (ParticleType == ParticleType.Neutron) nm.ParentNode.GameAtomData.NeutronCount++;
                if (ParticleType == ParticleType.Proton) nm.ParentNode.GameAtomData.ProtonCount++;
            }
            else if (MotionType == MotionType.Point && nm.MotionType == MotionType.Straight && ParentNode != null)
            {
                ParentNode.applyNucleonData(nm.gameObject, nm.ParticleType);
                if (nm.ParticleType == ParticleType.Neutron) ParentNode.GameAtomData.NeutronCount++;
                if (nm.ParticleType == ParticleType.Proton) ParentNode.GameAtomData.ProtonCount++;
            }
        }
    }
}
