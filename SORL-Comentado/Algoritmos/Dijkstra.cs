using SORL.Core;

namespace SORL.Algoritmos;

// Implementação do algoritmo de Dijkstra para caminho mínimo em grafos dirigidos ponderados.
// Encontra o menor custo (soma de pesos) de um vértice s até um vértice t.
// Requisito: todos os pesos devem ser não-negativos (garantido pelo formato DIMACS deste projeto).
//
// Variante implementada: Dijkstra com conjunto explorado explícito (sem heap de prioridade).
// Complexidade: O(V² + E) — adequado para grafos densos; para grafos esparsos grandes
// uma fila de prioridade (PriorityQueue) reduziria para O((V + E) log V).
public static class Dijkstra
{
    // Retorna uma tupla: (custo total do caminho mínimo, lista de vértices do caminho).
    // Se t for inalcançável a partir de s, retorna (+∞, lista vazia).
    public static (double custo, List<int> caminho) Executar(Grafo g, int s, int t)
    {
        // dist[v] = menor distância conhecida de s até v.
        // Inicializado com +∞ (desconhecido / inalcançável).
        double[] dist = new double[g.V];

        // pred[v] = qual vértice antecede v no caminho mínimo.
        // Usado no final para reconstruir o caminho percorrendo de trás para frente.
        // -1 significa "sem predecessor" (vértice inicial ou ainda não atingido).
        int[] pred = new int[g.V];

        // explorado[v] = true quando a distância mínima até v já foi finalizada.
        // Uma vez explorado, o vértice não precisa ser revisitado.
        bool[] explorado = new bool[g.V];

        // Inicializa todos os vértices como inalcançáveis e sem predecessor.
        for (int i = 0; i < g.V; i++)
        {
            dist[i] = double.PositiveInfinity;
            pred[i] = -1;
        }

        // A distância de s para si mesmo é zero. Marca s como já explorado.
        dist[s] = 0;
        explorado[s] = true;

        // S é o conjunto de vértices já explorados (a "fronteira" da busca).
        // Começamos apenas com s.
        Queue<int> S = new();
        S.Enqueue(s);

        // Repetimos no máximo V-1 vezes: a cada iteração adicionamos um novo vértice à fronteira.
        // (Um caminho mínimo num grafo com V vértices tem no máximo V-1 arestas.)
        for (int i = 0; i < g.V - 1; i++)
        {
            // Procura a aresta de menor custo total (dist[v] + peso) que sai de algum
            // vértice já explorado e chega num vértice ainda não explorado.
            int    melhorV   = -1;              // vértice de origem da melhor aresta encontrada
            int    melhorW   = -1;              // vértice de destino da melhor aresta encontrada
            double menorDist = double.PositiveInfinity;

            foreach (int v in S) // para cada vértice já na fronteira
            {
                foreach (Aresta a in g.Vizinhos(v)) // para cada vizinho de v
                {
                    if (!explorado[a.Destino]) // só considera destinos ainda não explorados
                    {
                        double candidato = dist[v] + a.Peso; // custo total até este destino
                        if (candidato < menorDist)
                        {
                            menorDist = candidato;
                            melhorV   = v;
                            melhorW   = a.Destino;
                        }
                    }
                }
            }

            // Se nenhuma aresta útil foi encontrada, o grafo está desconectado a partir de s.
            // Podemos parar: os vértices restantes são todos inalcançáveis.
            if (melhorW == -1)
                break;

            // Registra a distância definitiva do vértice recém-descoberto
            // e qual vértice o antecede no caminho mínimo.
            dist[melhorW] = menorDist;
            pred[melhorW] = melhorV;

            // Marca w como explorado e o adiciona à fronteira para próximas iterações.
            explorado[melhorW] = true;
            S.Enqueue(melhorW);
        }

        // Se t ainda está com distância infinita, não há caminho de s até t.
        if (double.IsInfinity(dist[t]))
            return (double.PositiveInfinity, new List<int>());

        // Reconstrói o caminho seguindo os predecessores de t até s (de trás para frente).
        List<int> caminho = new List<int>();
        int atual = t;
        while (atual != -1)          // para quando pred[s] == -1 (chegou ao início)
        {
            caminho.Add(atual);
            atual = pred[atual];     // salta para o vértice anterior no caminho
        }
        caminho.Reverse(); // inverte para ordem s → ... → t

        return (dist[t], caminho);
    }
}
