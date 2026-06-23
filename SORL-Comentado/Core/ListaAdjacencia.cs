namespace SORL.Core;

// Representação do grafo por LISTA DE ADJACÊNCIA.
// Ideal para grafos ESPARSOS (poucas arestas em relação ao máximo possível).
// Custo de memória: O(V + E) — armazena apenas as arestas que existem.
// Custo de consulta de vizinhos: O(grau(v)) — muito eficiente para grafos esparsos.
// Custo de consulta Peso/Capacidade: O(grau(v)) — precisa varrer a lista.
public class ListaAdjacencia : IRepresentacao
{
    // Array de listas: _adj[v] contém todas as arestas que partem do vértice v.
    // Cada posição do array é uma lista de Aresta (vizinhos de v).
    private readonly List<Aresta>[] _adj;

    // Contador de quantas arestas chegam a cada vértice.
    // Mantido separadamente porque a lista só registra saídas, não entradas.
    private readonly int[] _grauEntrada;

    // Número de vértices, exposto pela interface.
    public int NumVertices { get; }

    // Construtor: inicializa o array com n posições, cada uma com lista vazia.
    public ListaAdjacencia(int n)
    {
        NumVertices = n;
        _adj = new List<Aresta>[n];    // cria o array de tamanho n
        _grauEntrada = new int[n];      // todos graus de entrada começam em 0
        for (int i = 0; i < n; i++)
            _adj[i] = new List<Aresta>(); // cada slot recebe uma lista vazia
    }

    // Adiciona a aresta à lista de saída do vértice Origem
    // e incrementa o contador de entrada do vértice Destino.
    public void AdicionarAresta(Aresta a)
    {
        _adj[a.Origem].Add(a);       // registra a aresta na lista do vértice de origem
        _grauEntrada[a.Destino]++;   // contabiliza mais uma chegada no destino
    }

    // Retorna diretamente a lista de arestas de v: O(1).
    // Os algoritmos de busca (Dijkstra, DFS, etc.) iteram sobre este resultado.
    public IEnumerable<Aresta> Vizinhos(int v)
    {
        return _adj[v];
    }

    // Busca linear na lista de u até encontrar a aresta para v.
    // Se nenhuma aresta u→v existir, retorna infinito (sem conexão).
    public double Peso(int u, int v)
    {
        foreach (Aresta a in _adj[u])
        {
            if (a.Destino == v)
                return a.Peso;
        }
        return double.PositiveInfinity; // convenção: aresta inexistente tem custo infinito
    }

    // Igual ao Peso, mas retorna a capacidade.
    // Retorna 0 se a aresta não existir (sem capacidade = sem fluxo possível).
    public double Capacidade(int u, int v)
    {
        foreach (Aresta a in _adj[u])
        {
            if (a.Destino == v)
                return a.Capacidade;
        }
        return 0; // convenção: aresta inexistente tem capacidade zero
    }

    // Concatena todas as listas de adjacência numa única lista plana.
    // Usado pelo Kruskal (precisa de todas as arestas) e pelo Euleriano.
    public IEnumerable<Aresta> TodasArestas()
    {
        List<Aresta> todas = new List<Aresta>();
        foreach (List<Aresta> lista in _adj)       // itera cada vértice
        {
            foreach (Aresta a in lista)             // itera as arestas desse vértice
                todas.Add(a);
        }
        return todas;
    }

    // Grau de entrada: leitura direta do array (O(1)).
    public int GrauEntrada(int v) { return _grauEntrada[v]; }

    // Grau de saída: equivale ao número de arestas na lista de v (O(1)).
    public int GrauSaida(int v) { return _adj[v].Count; }
}
