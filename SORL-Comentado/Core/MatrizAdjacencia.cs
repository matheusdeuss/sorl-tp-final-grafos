namespace SORL.Core;

// Representação do grafo por MATRIZ DE ADJACÊNCIA.
// Ideal para grafos DENSOS (muitas arestas em relação ao máximo possível).
// Custo de memória: O(V²) — aloca n×n células independentemente de quantas arestas existam.
// Custo de consulta Peso/Capacidade: O(1) — acesso direto por índice [u,v].
// Custo de consulta de vizinhos: O(V) — precisa varrer toda a linha.
public class MatrizAdjacencia : IRepresentacao
{
    // Matriz n×n com o peso de cada aresta. _peso[i,j] = peso da aresta i→j.
    private readonly double[,] _peso;

    // Matriz n×n com a capacidade de cada aresta. _cap[i,j] = capacidade de i→j.
    private readonly double[,] _cap;

    // Matriz booleana que indica quais arestas existem. _existe[i,j] = true se i→j existe.
    // Necessária porque peso=0 é um valor válido e não indicaria ausência de aresta.
    private readonly bool[,] _existe;

    // Contadores de grau: mantidos explicitamente para resposta em O(1).
    private readonly int[] _grauEntrada;
    private readonly int[] _grauSaida;

    // Número de vértices, exposto pela interface.
    public int NumVertices { get; }

    // Construtor: aloca as três matrizes n×n e os dois arrays de grau.
    // Em C#, double[,] é inicializado com 0.0 e bool[,] com false automaticamente.
    public MatrizAdjacencia(int n)
    {
        NumVertices = n;
        _peso = new double[n, n];     // todos os pesos começam em 0
        _cap  = new double[n, n];     // todas as capacidades começam em 0
        _existe = new bool[n, n];     // nenhuma aresta existe inicialmente
        _grauEntrada = new int[n];
        _grauSaida   = new int[n];
    }

    // Registra a aresta nas três matrizes e atualiza os graus.
    // Acesso O(1): simplesmente escreve nas posições [Origem, Destino].
    public void AdicionarAresta(Aresta a)
    {
        _peso[a.Origem, a.Destino]   = a.Peso;
        _cap[a.Origem, a.Destino]    = a.Capacidade;
        _existe[a.Origem, a.Destino] = true;   // marca que esta aresta existe
        _grauEntrada[a.Destino]++;
        _grauSaida[a.Origem]++;
    }

    // Varre a linha inteira do vértice v (O(V)) construindo a lista de vizinhos.
    // Somente inclui células onde _existe[v,j] for verdadeiro.
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

    // Acesso O(1): verifica _existe e retorna o peso armazenado.
    // +∞ indica "aresta inexistente" (mesmo comportamento da lista de adjacência).
    public double Peso(int u, int v)
    {
        if (_existe[u, v])
            return _peso[u, v];
        return double.PositiveInfinity;
    }

    // Acesso O(1): mesma lógica do Peso, mas para capacidade.
    // 0 indica "aresta inexistente" (sem fluxo possível).
    public double Capacidade(int u, int v)
    {
        if (_existe[u, v])
            return _cap[u, v];
        return 0;
    }

    // Varre a matriz inteira (O(V²)) coletando todas as arestas existentes.
    // Usado por Kruskal, Euleriano e Coloração que precisam de toda a lista de arestas.
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

    // Leitura direta dos arrays de grau: O(1).
    public int GrauEntrada(int v) { return _grauEntrada[v]; }
    public int GrauSaida(int v)   { return _grauSaida[v]; }
}
