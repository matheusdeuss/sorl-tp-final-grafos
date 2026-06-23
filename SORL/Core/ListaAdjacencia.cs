namespace SORL.Core;

public class ListaAdjacencia : IRepresentacao
{
    private readonly List<Aresta>[] _adj;
    private readonly int[] _grauEntrada;

    public int NumVertices { get; }

    public ListaAdjacencia(int n)
    {
        NumVertices = n;
        _adj = new List<Aresta>[n];
        _grauEntrada = new int[n];
        for (int i = 0; i < n; i++)
            _adj[i] = new List<Aresta>();
    }

    public void AdicionarAresta(Aresta a)
    {
        _adj[a.Origem].Add(a);
        _grauEntrada[a.Destino]++;
    }

    public IEnumerable<Aresta> Vizinhos(int v)
    {
        return _adj[v];
    }

    public double Peso(int u, int v)
    {
        foreach (Aresta a in _adj[u])
        {
            if (a.Destino == v)
                return a.Peso;
        }
        return double.PositiveInfinity;
    }

    public double Capacidade(int u, int v)
    {
        foreach (Aresta a in _adj[u])
        {
            if (a.Destino == v)
                return a.Capacidade;
        }
        return 0;
    }

    public IEnumerable<Aresta> TodasArestas()
    {
        List<Aresta> todas = new List<Aresta>();
        foreach (List<Aresta> lista in _adj)
        {
            foreach (Aresta a in lista)
                todas.Add(a);
        }
        return todas;
    }

    public int GrauEntrada(int v) { return _grauEntrada[v]; }
    public int GrauSaida(int v) { return _adj[v].Count; }
}
