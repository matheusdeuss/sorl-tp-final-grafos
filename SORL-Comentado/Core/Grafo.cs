namespace SORL.Core;

// Fachada principal do grafo (padrão Facade + Strategy).
// Esconde qual das duas estruturas internas (Lista ou Matriz) está sendo usada.
// Toda a lógica de algoritmos acessa o grafo somente por esta classe.
// A escolha de representação é feita automaticamente no construtor com base na densidade.
public class Grafo
{
    // A representação interna real — pode ser ListaAdjacencia ou MatrizAdjacencia.
    // O tipo é a interface IRepresentacao, portanto o código externo não sabe qual das duas é.
    private readonly IRepresentacao _rep;

    // V: número de vértices do grafo (fixo após a criação).
    public int V { get; }

    // E: número de arestas efetivamente adicionadas (incrementado em AdicionarAresta).
    public int E { get; private set; }

    // Densidade: proporção entre arestas existentes e o máximo possível em um dígrafo.
    // Para um dígrafo com V vértices, o máximo de arestas é V*(V-1) (sem laços).
    // Densidade = E_estimado / (V*(V-1)).  Varia de 0 (vazio) a 1 (completo).
    public double Densidade { get; }

    // Indica qual estrutura foi escolhida: true = MatrizAdjacencia, false = ListaAdjacencia.
    public bool UsaMatriz { get; }

    // Limite de densidade acima do qual a Matriz é preferida à Lista.
    // 30% foi escolhido como ponto de equilíbrio empírico entre memória e velocidade.
    private const double LIMIAR_DENSIDADE = 0.30;

    // Acima de 5000 vértices a Matriz ocuparia > 200 MB (5000² × 8 bytes × 3 matrizes).
    // Nesse caso forçamos Lista mesmo que a densidade seja alta.
    private const int MAX_V_MATRIZ = 5000;

    // Construtor: recebe o número de vértices e uma estimativa de arestas (do cabeçalho DIMACS).
    // A estimativa é usada só para calcular densidade e escolher a estrutura — não é um limite.
    public Grafo(int v, int numArestasEstimado)
    {
        V = v;

        // Máximo de arestas em um dígrafo simples (sem laços): V × (V-1).
        double maxArestas = (double)v * (v - 1);

        // Calcula densidade estimada. Evita divisão por zero para grafos com 0 ou 1 vértice.
        if (maxArestas > 0)
            Densidade = numArestasEstimado / maxArestas;
        else
            Densidade = 0;

        // Decide a representação: Matriz somente se denso E grafo pequeno o suficiente.
        UsaMatriz = Densidade >= LIMIAR_DENSIDADE && v <= MAX_V_MATRIZ;

        // Instancia a estrutura escolhida e armazena atrás da interface.
        if (UsaMatriz)
            _rep = new MatrizAdjacencia(v);
        else
            _rep = new ListaAdjacencia(v);
    }

    // Delega a inserção à estrutura interna e conta a aresta.
    public void AdicionarAresta(Aresta a)
    {
        _rep.AdicionarAresta(a);
        E++; // mantém o contador público de arestas atualizado
    }

    // Todos os métodos abaixo são simples delegações à estrutura interna (_rep).
    // O compilador normalmente inline essas chamadas triviais.
    public IEnumerable<Aresta> Vizinhos(int v)       { return _rep.Vizinhos(v); }
    public double Peso(int u, int v)                  { return _rep.Peso(u, v); }
    public double Capacidade(int u, int v)            { return _rep.Capacidade(u, v); }
    public IEnumerable<Aresta> TodasArestas()         { return _rep.TodasArestas(); }
    public int GrauEntrada(int v)                     { return _rep.GrauEntrada(v); }
    public int GrauSaida(int v)                       { return _rep.GrauSaida(v); }

    // Propriedade de conveniência para exibição no log ("MATRIZ" ou "LISTA").
    public string TipoRepresentacao
    {
        get
        {
            if (UsaMatriz) return "MATRIZ";
            return "LISTA";
        }
    }

    // Explica no log por que a representação foi escolhida, mostrando a densidade calculada.
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
