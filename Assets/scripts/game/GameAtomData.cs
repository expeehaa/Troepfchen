public class GameAtomData : Nuclide
{
    private float protonReload;
    public float ProtonReload
    {
        get
        {
            return protonReload;
        }
        set
        {
            protonReload = value < 0 ? 0 : value;
        }
    }

    private float neutronReload;
    public float NeutronReload
    {
        get
        {
            return neutronReload;
        }
        set
        {
            neutronReload = value < 0 ? 0 : value;
        }
    }

    private float electronReload;
    public float ElectronReload
    {
        get
        {
            return electronReload;
        }
        set
        {
            electronReload = value < 0 ? 0 : value;
        }
    }

    private float gammaReload;
    public float GammaReload
    {
        get
        {
            return gammaReload;
        }
        set
        {
            gammaReload = value < 0 ? 0 : value;
        }
    }

    private float protonRespawn;
    public float ProtonRespawn
    {
        get
        {
            return protonRespawn;
        }
        set
        {
            protonRespawn = value < 0 ? 0 : value;
        }
    }

    private float neutronRespawn;
    public float NeutronRespawn
    {
        get
        {
            return neutronRespawn;
        }
        set
        {
            neutronRespawn = value < 0 ? 0 : value;
        }
    }

    private float electronRespawn;
    public float ElectronRespawn
    {
        get
        {
            return electronRespawn;
        }
        set
        {
            electronRespawn = value < 0 ? 0 : value;
        }
    }

    public GameAtomData(int p, int n, float pReload, float nReload, float eReload, float gammaReload, float pRespawn, float nRespawn, float eRespawn) : base(p, n)
    {
        ProtonCount = p;
        NeutronCount = n;
        ProtonReload = pReload;
        NeutronReload = nReload;
        ElectronReload = eReload;
        GammaReload = gammaReload;
        ProtonRespawn = pRespawn;
        NeutronRespawn = nRespawn;
        ElectronRespawn = eRespawn;
    }

    public bool IsUnstableInGame()
    {
        return NeutronCount / (float)ProtonCount > 3 || ProtonCount / (float)NeutronCount > 3 ? true : false;
    }

    public GameAtomData Copy(){
        return new GameAtomData(ProtonCount, NeutronCount, ProtonReload, NeutronReload, ElectronReload, GammaReload, ProtonRespawn, NeutronRespawn, ElectronRespawn);
    }
}