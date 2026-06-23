using SORL.Core;

namespace SORL.Algoritmos;

// Implementação do algoritmo de Edmonds-Karp para fluxo máximo em grafos dirigidos.
// Edmonds-Karp é Ford-Fulkerson com BFS para encontrar caminhos aumentantes,
// o que garante complexidade O(V × E²) no pior caso.
//
// Após encontrar o fluxo máximo, identifica o corte mínimo pelo Teorema Max-Flow/Min-Cut:
// o corte mínimo é o conjunto de arestas que cruzam a fronteira da BFS final no grafo residual.
// O valor do fluxo máximo é igual à capacidade total do corte mínimo.
public static class FluxoMaximo
{
    // Retorna uma tupla: (fluxo máximo de s para t, lista de arestas do corte mínimo).
    public static (double fluxo, List<(int u, int v)> corteMinimo) Executar(Grafo g, int s, int t)
    {
        int n = g.V;

        // Matriz de capacidades residuais n×n.
        // cap[u,v] começa com a capacidade original de u→v.
        // Durante o algoritmo, cap[u,v] diminui quando fluxo passa por u→v
        // e cap[v,u] aumenta (aresta reversa, permite "cancelar" fluxo se necessário).
        double[,] cap = new double[n, n];

        // Inicializa a matriz somando capacidades (suporta múltiplas arestas paralelas).
        foreach (Aresta a in g.TodasArestas())
            cap[a.Origem, a.Destino] += a.Capacidade;

        // Acumula o fluxo total enviado de s para t ao longo de todas as iterações.
        double fluxoTotal = 0;

        // Loop principal: enquanto existir um caminho aumentante, envia fluxo por ele.
        while (true)
        {
            // === Fase BFS: encontrar caminho aumentante de s até t ===
            // pai[v] = de onde chegamos a v na BFS. -1 = não visitado ainda.
            int[] pai = new int[n];
            for (int i = 0; i < n; i++)
                pai[i] = -1;
            pai[s] = s; // s é seu próprio "pai" (sentinela para indicar que foi visitado)

            Queue<int> fila = new();
            fila.Enqueue(s);

            // BFS padrão: expande em largura até atingir t ou esgotar os alcançáveis.
            // Só atravessa arestas com capacidade residual positiva (cap[u,v] > 0).
            while (fila.Count > 0 && pai[t] == -1)
            {
                int u = fila.Dequeue();
                for (int v = 0; v < n; v++)
                {
                    if (pai[v] == -1 && cap[u, v] > 0) // não visitado E tem capacidade
                    {
                        pai[v] = u;         // registra que chegamos a v vindo de u
                        fila.Enqueue(v);
                    }
                }
            }

            // Se t não foi alcançado (pai[t] == -1), não existe mais caminho aumentante.
            // O fluxo máximo foi atingido. Encerra o loop.
            if (pai[t] == -1)
                break;

            // === Fase gargalo: encontrar a menor capacidade residual no caminho s→t ===
            // Caminhamos de t de volta a s seguindo os pais (caminho encontrado pela BFS).
            double gargalo = double.PositiveInfinity;
            int vertice = t;
            while (vertice != s)
            {
                // A capacidade disponível neste trecho do caminho é cap[pai[v], v].
                gargalo = Math.Min(gargalo, cap[pai[vertice], vertice]);
                vertice = pai[vertice]; // sobe um nível no caminho
            }
            // Ao final, gargalo = capacidade máxima que podemos enviar por este caminho.

            // === Fase atualização: aplica o fluxo = gargalo ao longo do caminho ===
            vertice = t;
            while (vertice != s)
            {
                // Reduz a capacidade residual da aresta "para frente" (u → v).
                cap[pai[vertice], vertice] -= gargalo;

                // Aumenta a capacidade residual da aresta "para trás" (v → u).
                // Isso permite ao algoritmo "desfazer" parte deste fluxo em iterações futuras
                // para encontrar um fluxo total ainda maior por outro caminho.
                cap[vertice, pai[vertice]] += gargalo;

                vertice = pai[vertice];
            }

            // Acumula o fluxo desta iteração no total.
            fluxoTotal += gargalo;
        }

        // === Determinação do corte mínimo ===
        // Pelo Teorema Max-Flow/Min-Cut, após esgotar os caminhos aumentantes,
        // fazemos uma última BFS no grafo residual para encontrar quais vértices
        // ainda são alcançáveis a partir de s.
        bool[] alcancavel = new bool[n];
        Queue<int> filaCorte = new();
        filaCorte.Enqueue(s);
        alcancavel[s] = true;

        while (filaCorte.Count > 0)
        {
            int u = filaCorte.Dequeue();
            for (int v = 0; v < n; v++)
            {
                // Só avança se ainda há capacidade residual (cap[u,v] > 0).
                if (!alcancavel[v] && cap[u, v] > 0)
                {
                    alcancavel[v] = true;
                    filaCorte.Enqueue(v);
                }
            }
        }

        // O corte mínimo é composto pelas arestas ORIGINAIS do grafo
        // cujo ORIGEM está no lado alcançável (S) e cujo DESTINO está no lado não-alcançável (T̄).
        // Essas são exatamente as arestas saturadas que "bloqueiam" o fluxo.
        List<(int, int)> corte = new();
        foreach (Aresta a in g.TodasArestas())
        {
            if (alcancavel[a.Origem] && !alcancavel[a.Destino])
                corte.Add((a.Origem, a.Destino));
        }

        return (fluxoTotal, corte);
    }
}
