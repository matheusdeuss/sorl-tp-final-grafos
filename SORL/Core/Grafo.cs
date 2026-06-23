namespace SORL.Core;

public class Grafo
{
    private readonly IRepresentacao _rep;

    public int V { get; }
    public int E { get; private set; }
    public double Densidade { get; }
    public bool UsaMatriz { get; }

    private const double LIMIAR_DENSIDADE = 0.30;
    private const int MAX_V_MATRIZ = 5000;

    public Grafo(int v, int numArestasEstimado)
    {
        V = v;
        double maxArestas = (double)v * (v - 1);

        if (maxArestas > 0)
            Densidade = numArestasEstimado / maxArestas;
        else
            Densidade = 0;

        UsaMatriz = Densidade >= LIMIAR_DENSIDADE && v <= MAX_V_MATRIZ;

        if (UsaMatriz)
            _rep = new MatrizAdjacencia(v);
        else
            _rep = new ListaAdjacencia(v);
    }

    public void AdicionarAresta(Aresta a)
    {
        _rep.AdicionarAresta(a);
        E++;
    }

    public IEnumerable<Aresta> Vizinhos(int v) { return _rep.Vizinhos(v); }
    public double Peso(int u, int v) { return _rep.Peso(u, v); }
    public double Capacidade(int u, int v) { return _rep.Capacidade(u, v); }
    public IEnumerable<Aresta> TodasArestas() { return _rep.TodasArestas(); }
    public int GrauEntrada(int v) { return _rep.GrauEntrada(v); }
    public int GrauSaida(int v) { return _rep.GrauSaida(v); }

    public string TipoRepresentacao
    {
        get
        {
            if (UsaMatriz) return "MATRIZ";
            return "LISTA";
        }
    }

    public string JustificativaRepresentacao
    {
        get
        {
            if (UsaMatriz)
                return $"densidade={Densidade:F3} >= {LIMIAR_DENSIDADE}";
            else
                return $"densidade={Densidade:F3} < {LIMIAR_DENSIDADE}";
        }
    }
}
