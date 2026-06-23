using SORL.Core;

namespace SORL.Algoritmos;

public static class FluxoMaximo
{
    public static (double fluxo, List<(int u, int v)> corteMinimo) Executar(Grafo g, int s, int t)
    {
        int n = g.V;
        double[,] cap = new double[n, n];

        foreach (Aresta a in g.TodasArestas())
            cap[a.Origem, a.Destino] += a.Capacidade;

        double fluxoTotal = 0;

        while (true)
        {
            // BFS para encontrar caminho aumentante (Edmonds-Karp)
            int[] pai = new int[n];
            for (int i = 0; i < n; i++)
                pai[i] = -1;
            pai[s] = s;

            Queue<int> fila = new();
            fila.Enqueue(s);

            while (fila.Count > 0 && pai[t] == -1)
            {
                int u = fila.Dequeue();
                for (int v = 0; v < n; v++)
                {
                    if (pai[v] == -1 && cap[u, v] > 0)
                    {
                        pai[v] = u;
                        fila.Enqueue(v);
                    }
                }
            }

            if (pai[t] == -1)
                break;

            // encontra o gargalo (menor capacidade residual no caminho)
            double gargalo = double.PositiveInfinity;
            int vertice = t;
            while (vertice != s)
            {
                gargalo = Math.Min(gargalo, cap[pai[vertice], vertice]);
                vertice = pai[vertice];
            }

            // atualiza o grafo residual
            vertice = t;
            while (vertice != s)
            {
                cap[pai[vertice], vertice] -= gargalo;
                cap[vertice, pai[vertice]] += gargalo;
                vertice = pai[vertice];
            }

            fluxoTotal += gargalo;
        }

        // BFS no grafo residual para encontrar o corte mínimo
        bool[] alcancavel = new bool[n];
        Queue<int> filaCorte = new();
        filaCorte.Enqueue(s);
        alcancavel[s] = true;

        while (filaCorte.Count > 0)
        {
            int u = filaCorte.Dequeue();
            for (int v = 0; v < n; v++)
            {
                if (!alcancavel[v] && cap[u, v] > 0)
                {
                    alcancavel[v] = true;
                    filaCorte.Enqueue(v);
                }
            }
        }

        List<(int, int)> corte = new();
        foreach (Aresta a in g.TodasArestas())
        {
            if (alcancavel[a.Origem] && !alcancavel[a.Destino])
                corte.Add((a.Origem, a.Destino));
        }

        return (fluxoTotal, corte);
    }
}
