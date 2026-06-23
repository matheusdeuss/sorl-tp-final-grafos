namespace SORL.Core;

// Contrato (interface) que toda estrutura de representação de grafo deve cumprir.
// Há duas implementações concretas: ListaAdjacencia e MatrizAdjacencia.
// O resto do sistema usa apenas IRepresentacao, não sabendo qual das duas está ativa.
// Isso é o padrão Strategy: a classe Grafo escolhe a implementação em tempo de execução.
public interface IRepresentacao
{
    // Número total de vértices do grafo.
    int NumVertices { get; }

    // Registra uma nova aresta na estrutura interna.
    void AdicionarAresta(Aresta a);

    // Retorna todas as arestas que partem do vértice v (vizinhos diretos).
    IEnumerable<Aresta> Vizinhos(int v);

    // Retorna o peso da aresta u→v, ou +∞ se não existir.
    double Peso(int u, int v);

    // Retorna a capacidade da aresta u→v, ou 0 se não existir.
    double Capacidade(int u, int v);

    // Retorna todas as arestas do grafo (iteração global).
    IEnumerable<Aresta> TodasArestas();

    // Quantidade de arestas que chegam ao vértice v (grau de entrada, grafo dirigido).
    int GrauEntrada(int v);

    // Quantidade de arestas que partem do vértice v (grau de saída, grafo dirigido).
    int GrauSaida(int v);
}
