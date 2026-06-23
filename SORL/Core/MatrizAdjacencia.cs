namespace SORL.Core;

public class MatrizAdjacencia : IRepresentacao
{
    private readonly double[,] _peso;
    private readonly double[,] _cap;
    private readonly bool[,] _existe;
    private readonly int[] _grauEntrada;
    private readonly int[] _grauSaida;

    public int NumVertices { get; }

    public MatrizAdjacencia(int n)
    {
        NumVertices = n;
        _peso = new double[n, n];
        _cap = new double[n, n];
        _existe = new bool[n, n];
        _grauEntrada = new int[n];
        _grauSaida = new int[n];
    }

    public void AdicionarAresta(Aresta a)
    {
        _peso[a.Origem, a.Destino] = a.Peso;
        _cap[a.Origem, a.Destino] = a.Capacidade;
        _existe[a.Origem, a.Destino] = true;
        _grauEntrada[a.Destino]++;
        _grauSaida[a.Origem]++;
    }

    public IEnumerable<Aresta> Vizinhos(int v)
    {
        List<Aresta> vizinhos = new List<Aresta>();
        for (int j = 0; j < NumVertices; j++)
        {
            if (_existe[v, j])
                vizinhos.Add(new Aresta(v, j, _peso[v, j], _cap[v, j]));
        }
        return vizinhos;
    }

    public double Peso(int u, int v)
    {
        if (_existe[u, v])
            return _peso[u, v];
        return double.PositiveInfinity;
    }

    public double Capacidade(int u, int v)
    {
        if (_existe[u, v])
            return _cap[u, v];
        return 0;
    }

    public IEnumerable<Aresta> TodasArestas()
    {
        List<Aresta> todas = new List<Aresta>();
        for (int i = 0; i < NumVertices; i++)
        {
            for (int j = 0; j < NumVertices; j++)
            {
                if (_existe[i, j])
                    todas.Add(new Aresta(i, j, _peso[i, j], _cap[i, j]));
            }
        }
        return todas;
    }

    public int GrauEntrada(int v) { return _grauEntrada[v]; }
    public int GrauSaida(int v) { return _grauSaida[v]; }
}
