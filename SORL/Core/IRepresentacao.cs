namespace SORL.Core;

public interface IRepresentacao
{
    int NumVertices { get; }
    void AdicionarAresta(Aresta a);
    IEnumerable<Aresta> Vizinhos(int v);
    double Peso(int u, int v);
    double Capacidade(int u, int v);
    IEnumerable<Aresta> TodasArestas();
    int GrauEntrada(int v);
    int GrauSaida(int v);
}
