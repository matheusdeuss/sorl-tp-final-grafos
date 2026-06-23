using SORL.Core;

namespace SORL.Algoritmos;

public static class Hamiltoniano
{
    // Busca exaustiva por DFS em grafo não-direcionado com limite de tempo
    public static (bool? existe, List<int>? ciclo) Executar(Grafo g, TimeSpan limite)
    {
        // constrói adjacência não-direcionada
        HashSet<int>[] adj = new HashSet<int>[g.V];
        for (int i = 0; i < g.V; i++)
            adj[i] = new HashSet<int>();

        foreach (Aresta a in g.TodasArestas())
        {
            adj[a.Origem].Add(a.Destino);
            adj[a.Destino].Add(a.Origem);
        }

        List<int> caminho = new List<int>();
        caminho.Add(0);

        bool[] visitado = new bool[g.V];
        visitado[0] = true;

        List<int>? resultado = null;
        DateTime inicio = DateTime.UtcNow;

        BuscaDFS(adj, caminho, visitado, g.V, inicio, limite, ref resultado);

        if (resultado == null && DateTime.UtcNow - inicio > limite)
            return (null, null);

        if (resultado != null)
            return (true, resultado);

        return (false, null);
    }

    private static bool BuscaDFS(
        HashSet<int>[] adj,
        List<int> caminho,
        bool[] visitado,
        int numVertices,
        DateTime inicio,
        TimeSpan limite,
        ref List<int>? resultado)
    {
        if (DateTime.UtcNow - inicio > limite)
            return false;

        int ultimo = caminho[caminho.Count - 1];

        if (caminho.Count == numVertices)
        {
            if (adj[ultimo].Contains(0))
            {
                resultado = new List<int>(caminho);
                resultado.Add(0);
                return true;
            }
            return false;
        }

        foreach (int viz in adj[ultimo])
        {
            if (visitado[viz])
                continue;

            visitado[viz] = true;
            caminho.Add(viz);

            if (BuscaDFS(adj, caminho, visitado, numVertices, inicio, limite, ref resultado))
                return true;

            caminho.RemoveAt(caminho.Count - 1);
            visitado[viz] = false;

            if (DateTime.UtcNow - inicio > limite)
                return false;
        }

        return false;
    }
}
