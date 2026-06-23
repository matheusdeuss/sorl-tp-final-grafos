using SORL.Core;

namespace SORL.Algoritmos;

// Implementação do algoritmo de Kruskal para Árvore Geradora Mínima (AGM).
// Objetivo: encontrar o subconjunto de arestas de menor peso total que conecta
// todos os vértices sem formar ciclos (árvore geradora de custo mínimo).
//
// O grafo é tratado como NÃO-DIRIGIDO: cada aresta direcionada u→v é considerada
// como a aresta não-dirigida {u, v} para fins da AGM. Arestas paralelas (u→v e v→u)
// são deduplicadas mantendo apenas uma cópia por par {min, max}.
//
// Complexidade: O(E log E) dominado pela ordenação das arestas.
// Usa Union-Find (Disjoint Set Union) com compressão de caminho e union por rank.
public static class ArvoreGeradoraMinima
{
    // Retorna: (custo total da AGM, lista de arestas escolhidas com seus pesos).
    public static (double custo, List<(int u, int v, double peso)> arestas) Executar(Grafo g)
    {
        // === Etapa 1: deduplicação de arestas (dígrafo → grafo não-dirigido) ===
        // Como o grafo é dirigido, pode haver u→v e v→u. Para a AGM, tratamos
        // ambas como a mesma aresta não-dirigida. Guardamos sempre com (min, max).
        HashSet<(int, int)> visitadas = new HashSet<(int, int)>();
        List<(double peso, int u, int v)> lista = new List<(double peso, int u, int v)>();

        foreach (Aresta a in g.TodasArestas())
        {
            // Normaliza o par: menor índice primeiro, para identificar a aresta de forma única.
            int mn = Math.Min(a.Origem, a.Destino);
            int mx = Math.Max(a.Origem, a.Destino);

            // visitadas.Add retorna true se o par ainda não estava no conjunto (aresta nova).
            // Se retornar false, a aresta já foi vista (duplicata direcionada) — pulamos.
            if (visitadas.Add((mn, mx)))
                lista.Add((a.Peso, mn, mx));
        }

        // === Etapa 2: ordenar arestas por peso crescente ===
        // Kruskal processa arestas em ordem de menor para maior peso.
        lista.Sort((x, y) => x.peso.CompareTo(y.peso));

        // === Etapa 3: inicializar Union-Find ===
        // parent[i] = representante do componente de i. Inicialmente cada vértice é seu próprio pai.
        // rank[i] = altura aproximada da árvore do componente de i (para union por rank).
        int[] parent = new int[g.V];
        int[] rank   = new int[g.V];
        for (int i = 0; i < g.V; i++)
        {
            parent[i] = i; // cada vértice começa no seu próprio componente
            rank[i]   = 0; // altura 0
        }

        double custo = 0;
        List<(int, int, double)> escolhidas = new List<(int, int, double)>();

        // === Etapa 4: processar arestas em ordem crescente de peso ===
        foreach ((double peso, int u, int v) in lista)
        {
            // A AGM tem exatamente V-1 arestas. Para quando já temos todas.
            if (escolhidas.Count == g.V - 1)
                break;

            // Tenta unir os componentes de u e v.
            // Unir retorna false se u e v já estão no mesmo componente (adicionaria ciclo).
            if (Unir(parent, rank, u, v))
            {
                escolhidas.Add((u, v, peso)); // esta aresta faz parte da AGM
                custo += peso;
            }
            // Se já no mesmo componente: ignora (adicionaria ciclo → não é árvore).
        }

        return (custo, escolhidas);
    }

    // Union-Find: encontra o representante (raiz) do componente de x.
    // Usa compressão de caminho com "path splitting":
    // Enquanto caminha até a raiz, faz parent[x] apontar para o avô (parent[parent[x]]).
    // Isso "achata" a árvore progressivamente, tornando futuras buscas mais rápidas.
    private static int Encontrar(int[] parent, int x)
    {
        while (parent[x] != x)
        {
            parent[x] = parent[parent[x]]; // compressão: pula um nível (path splitting)
            x = parent[x];
        }
        return x; // retorna a raiz do componente
    }

    // Union-Find: une os componentes de a e b.
    // Retorna true se foram componentes distintos (union feito com sucesso).
    // Retorna false se já estavam no mesmo componente (union não feito — evita ciclo).
    // Usa union por rank: a árvore de menor rank fica abaixo da de maior rank,
    // mantendo a árvore do Union-Find balanceada (profundidade O(log V)).
    private static bool Unir(int[] parent, int[] rank, int a, int b)
    {
        int ra = Encontrar(parent, a); // raiz do componente de a
        int rb = Encontrar(parent, b); // raiz do componente de b

        if (ra == rb)
            return false; // mesma raiz = mesmo componente = adicionar esta aresta criaria ciclo

        // Garante que ra seja sempre a raiz de maior (ou igual) rank.
        // Se ra tem rank menor, troca ra e rb para que o de maior rank fique em ra.
        if (rank[ra] < rank[rb])
            (ra, rb) = (rb, ra); // swap usando tupla (C# 7+)

        // Faz a raiz de menor rank (rb) filha da raiz de maior rank (ra).
        parent[rb] = ra;

        // Só incrementa o rank quando os dois tinham rank igual (a árvore resultante cresce).
        if (rank[ra] == rank[rb])
            rank[ra]++;

        return true;
    }
}
