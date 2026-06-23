using SORL.Core;

namespace SORL.Algoritmos;

public static class Euleriano
{
    // Verifica e constrói circuito euleriano em dígrafo via DFS que consome arestas
    public static (bool existe, string motivo, List<int> circuito) Executar(Grafo g)
    {
        // 1) grau de entrada deve ser igual ao grau de saída em todo vértice
        for (int v = 0; v < g.V; v++)
        {
            if (g.GrauEntrada(v) != g.GrauSaida(v))
            {
                string msg = $"vértice {v + 1} tem grauEntrada={g.GrauEntrada(v)} != grauSaida={g.GrauSaida(v)}";
                return (false, msg, new List<int>());
            }
        }

        // vértices que participam de alguma aresta
        HashSet<int> comAresta = new HashSet<int>();
        foreach (Aresta a in g.TodasArestas())
        {
            comAresta.Add(a.Origem);
            comAresta.Add(a.Destino);
        }

        if (comAresta.Count == 0)
            return (false, "grafo sem arestas", new List<int>());

        // 2) verificar conectividade forte com duas DFS (grafo original + transposto)
        int inicio = -1;
        foreach (int v in comAresta)
        {
            inicio = v;
            break;
        }

        if (!FortementeConexo(g, comAresta, inicio))
            return (false, "grafo não é fortemente conexo", new List<int>());

        // 3) construção do circuito por DFS que consome arestas
        List<Aresta>[] adjResiduais = new List<Aresta>[g.V];
        for (int v = 0; v < g.V; v++)
        {
            adjResiduais[v] = new List<Aresta>();
            foreach (Aresta a in g.Vizinhos(v))
                adjResiduais[v].Add(a);
        }

        Stack<int> pilha = new Stack<int>();
        List<int> circuito = new List<int>();
        pilha.Push(inicio);

        while (pilha.Count > 0)
        {
            int v = pilha.Peek();
            if (adjResiduais[v].Count > 0)
            {
                Aresta a = adjResiduais[v][adjResiduais[v].Count - 1];
                adjResiduais[v].RemoveAt(adjResiduais[v].Count - 1);
                pilha.Push(a.Destino);
            }
            else
            {
                circuito.Add(pilha.Pop());
            }
        }

        circuito.Reverse();
        return (true, "OK", circuito);
    }

    private static bool FortementeConexo(Grafo g, HashSet<int> vertices, int origem)
    {
        if (!DfsAlcanca(g, vertices, origem, false))
            return false;
        if (!DfsAlcanca(g, vertices, origem, true))
            return false;
        return true;
    }

    private static bool DfsAlcanca(Grafo g, HashSet<int> vertices, int origem, bool transposto)
    {
        HashSet<int> visitado = new HashSet<int>();
        Stack<int> pilha = new Stack<int>();
        pilha.Push(origem);
        visitado.Add(origem);

        while (pilha.Count > 0)
        {
            int u = pilha.Pop();

            IEnumerable<Aresta> vizinhos;
            if (!transposto)
                vizinhos = g.Vizinhos(u);
            else
                vizinhos = VizinhosTranspostos(g, u);

            foreach (Aresta a in vizinhos)
            {
                int dest;
                if (transposto)
                    dest = a.Origem;
                else
                    dest = a.Destino;

                if (!visitado.Contains(dest) && vertices.Contains(dest))
                {
                    visitado.Add(dest);
                    pilha.Push(dest);
                }
            }
        }

        foreach (int v in vertices)
        {
            if (!visitado.Contains(v))
                return false;
        }
        return true;
    }

    private static List<Aresta> VizinhosTranspostos(Grafo g, int v)
    {
        List<Aresta> resultado = new List<Aresta>();
        foreach (Aresta a in g.TodasArestas())
        {
            if (a.Destino == v)
                resultado.Add(a);
        }
        return resultado;
    }
}
