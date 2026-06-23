using SORL.Core;

namespace SORL.Algoritmos;

public static class Dijkstra
{
    public static (double custo, List<int> caminho) Executar(Grafo g, int s, int t)
    {
        double[] dist = new double[g.V];
        int[] prev = new int[g.V];
        bool[] visitado = new bool[g.V];

        for (int i = 0; i < g.V; i++)
        {
            dist[i] = double.PositiveInfinity;
            prev[i] = -1;
            visitado[i] = false;
        }

        dist[s] = 0;

        PriorityQueue<int, double> fila = new PriorityQueue<int, double>();
        fila.Enqueue(s, 0);

        while (fila.Count > 0)
        {
            fila.TryDequeue(out int u, out double _);

            if (visitado[u])
                continue;
            visitado[u] = true;

            foreach (Aresta a in g.Vizinhos(u))
            {
                double novaDist = dist[u] + a.Peso;
                if (novaDist < dist[a.Destino])
                {
                    dist[a.Destino] = novaDist;
                    prev[a.Destino] = u;
                    fila.Enqueue(a.Destino, novaDist);
                }
            }
        }

        if (double.IsInfinity(dist[t]))
            return (double.PositiveInfinity, new List<int>());

        List<int> caminho = new List<int>();
        int atual = t;
        while (atual != -1)
        {
            caminho.Add(atual);
            atual = prev[atual];
        }
        caminho.Reverse();

        return (dist[t], caminho);
    }
}
