using SORL.Core;

namespace SORL.Algoritmos;

public static class ArvoreGeradoraMinima
{
    // Kruskal
    public static (double custo, List<(int u, int v, double peso)> arestas) Executar(Grafo g)
    {
        // coleta arestas sem duplicar: u->v e v->u representam a mesma aresta não-direcionada
        HashSet<(int, int)> visitadas = new HashSet<(int, int)>();
        List<(double peso, int u, int v)> lista = new List<(double peso, int u, int v)>();

        foreach (Aresta a in g.TodasArestas())
        {
            int mn = Math.Min(a.Origem, a.Destino);
            int mx = Math.Max(a.Origem, a.Destino);
            if (visitadas.Add((mn, mx)))
                lista.Add((a.Peso, mn, mx));
        }

        lista.Sort((x, y) => x.peso.CompareTo(y.peso));

        int[] parent = new int[g.V];
        int[] rank = new int[g.V];
        for (int i = 0; i < g.V; i++)
        {
            parent[i] = i;
            rank[i] = 0;
        }

        double custo = 0;
        List<(int, int, double)> escolhidas = new List<(int, int, double)>();

        foreach ((double peso, int u, int v) in lista)
        {
            if (escolhidas.Count == g.V - 1)
                break;

            if (Unir(parent, rank, u, v))
            {
                escolhidas.Add((u, v, peso));
                custo += peso;
            }
        }

        return (custo, escolhidas);
    }

    private static int Encontrar(int[] parent, int x)
    {
        while (parent[x] != x)
        {
            parent[x] = parent[parent[x]];
            x = parent[x];
        }
        return x;
    }

    private static bool Unir(int[] parent, int[] rank, int a, int b)
    {
        int ra = Encontrar(parent, a);
        int rb = Encontrar(parent, b);

        if (ra == rb)
            return false;

        if (rank[ra] < rank[rb])
        {
            int temp = ra;
            ra = rb;
            rb = temp;
        }

        parent[rb] = ra;

        if (rank[ra] == rank[rb])
            rank[ra]++;

        return true;
    }
}
