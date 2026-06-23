namespace SORL.Core;

// Representa uma aresta (ligação) do grafo dirigido.
// Cada aresta guarda: vértice de saída (Origem), vértice de chegada (Destino),
// um custo de travessia (Peso) e uma capacidade máxima de fluxo (Capacidade).
// A classe é imutável: todos os campos são somente leitura (get sem set).
public class Aresta
{
    // Índice do vértice onde a aresta começa (base 0 internamente).
    public int Origem { get; }

    // Índice do vértice onde a aresta termina (base 0 internamente).
    public int Destino { get; }

    // Custo (distância, tempo, etc.) para percorrer esta aresta.
    // Usado pelos algoritmos de caminho mínimo (Dijkstra) e AGM (Kruskal).
    public double Peso { get; }

    // Capacidade máxima de fluxo que pode passar por esta aresta.
    // Usada pelo algoritmo de fluxo máximo (Edmonds-Karp).
    public double Capacidade { get; }

    // Construtor: recebe os quatro campos e os armazena.
    // origem/destino chegam já convertidos para base 0 pelo LeitorDimacs.
    public Aresta(int origem, int destino, double peso, double capacidade)
    {
        Origem = origem;
        Destino = destino;
        Peso = peso;
        Capacidade = capacidade;
    }

    // Representação textual legível para exibição nos logs.
    // Soma +1 aos índices para mostrar numeração base 1 (amigável ao usuário).
    // Exemplo de saída: "(1->3 peso=2.5 cap=10)"
    public override string ToString()
    {
        return "(" + (Origem + 1) + "->" + (Destino + 1) + " peso=" + Peso + " cap=" + Capacidade + ")";
    }
}
