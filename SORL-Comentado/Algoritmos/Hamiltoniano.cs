using SORL.Core;

namespace SORL.Algoritmos;

// Busca de ciclo Hamiltoniano em grafo NÃO-DIRIGIDO por backtracking (DFS).
//
// Um ciclo Hamiltoniano visita TODOS os vértices exatamente uma vez e retorna ao início.
// Diferente do Euleriano (que visita arestas), o Hamiltoniano visita vértices.
//
// O grafo de entrada é dirigido (DIMACS), mas este algoritmo o trata como não-dirigido:
// cada aresta u→v adiciona v aos vizinhos de u E u aos vizinhos de v.
//
// Estratégia: backtracking a partir do vértice 0.
//   - Constrói um caminho vértice a vértice.
//   - Quando o caminho tem V vértices, verifica se o último é adjacente ao vértice 0.
//   - Se sim, encontrou um ciclo Hamiltoniano.
//
// Limitação: NP-completo — pode demorar exponencialmente.
// Por isso há um limite de tempo (timeout); se estourar, retorna null (não conclusivo).
//
// Retorno:
//   existe = true   → ciclo encontrado (lista em ciclo)
//   existe = false  → busca terminou sem encontrar ciclo
//   existe = null   → timeout antes de concluir (resultado inconclusivo)
public static class Hamiltoniano
{
    public static (bool? existe, List<int>? ciclo) Executar(Grafo g, TimeSpan limite)
    {
        // ── Etapa 1: construir lista de adjacência NÃO-DIRIGIDA ──
        // adj[v] = conjunto de vértices adjacentes a v (sem duplicatas, grafo simples).
        HashSet<int>[] adj = new HashSet<int>[g.V];
        for (int i = 0; i < g.V; i++)
            adj[i] = new HashSet<int>();

        // Para cada aresta dirigida u→v, registra a adjacência nos dois sentidos.
        foreach (Aresta a in g.TodasArestas())
        {
            adj[a.Origem].Add(a.Destino);
            adj[a.Destino].Add(a.Origem); // inverte o sentido → grafo não-dirigido
        }

        // ── Etapa 2: inicializar a busca a partir do vértice 0 ──
        // O caminho parcial começa sempre no vértice 0 (primeiro vértice, base 0).
        List<int> caminho = new List<int>();
        caminho.Add(0);

        // visitado[v] = true quando v já faz parte do caminho parcial.
        bool[] visitado = new bool[g.V];
        visitado[0] = true;

        List<int>? resultado = null; // será preenchido se um ciclo for encontrado
        DateTime inicio = DateTime.UtcNow; // referência para verificar timeout

        // Inicia a DFS recursiva com backtracking.
        BuscaDFS(adj, caminho, visitado, g.V, inicio, limite, ref resultado);

        // ── Etapa 3: interpretar o resultado ──
        // Se resultado ainda é null E o tempo estourou → inconclusivo (timeout).
        if (resultado == null && DateTime.UtcNow - inicio > limite)
            return (null, null);

        // Ciclo encontrado com sucesso.
        if (resultado != null)
            return (true, resultado);

        // Busca terminou dentro do limite, mas não encontrou ciclo.
        return (false, null);
    }

    // DFS com backtracking: tenta estender o caminho até formar um ciclo Hamiltoniano.
    //
    // Parâmetros:
    //   adj         — adjacência não-dirigida
    //   caminho     — caminho parcial construído até agora (modificado in-place)
    //   visitado    — quais vértices já estão no caminho
    //   numVertices — V (tamanho alvo do ciclo)
    //   inicio      — timestamp de início (para checar timeout)
    //   limite      — tempo máximo permitido
    //   resultado   — referência ao ciclo encontrado (preenchido quando sucesso)
    //
    // Retorna true se encontrou ciclo (resultado preenchido), false caso contrário.
    private static bool BuscaDFS(
        HashSet<int>[] adj,
        List<int> caminho,
        bool[] visitado,
        int numVertices,
        DateTime inicio,
        TimeSpan limite,
        ref List<int>? resultado)
    {
        // Verifica timeout antes de continuar a busca neste ramo.
        if (DateTime.UtcNow - inicio > limite)
            return false;

        int ultimo = caminho[caminho.Count - 1]; // vértice no fim do caminho parcial

        // ── Caso base: caminho já visitou todos os V vértices ──
        if (caminho.Count == numVertices)
        {
            // Para fechar o ciclo, o último vértice precisa ser adjacente ao vértice 0.
            if (adj[ultimo].Contains(0))
            {
                // Copia o caminho e adiciona 0 no final (retorno ao início).
                resultado = new List<int>(caminho);
                resultado.Add(0);
                return true; // ciclo encontrado — propaga sucesso para cima
            }
            return false; // visitou todos mas não consegue fechar o ciclo
        }

        // ── Caso recursivo: tenta estender o caminho para cada vizinho não visitado ──
        foreach (int viz in adj[ultimo])
        {
            if (visitado[viz])
                continue; // vértice já no caminho — pular (evita repetir vértice)

            // Escolhe viz: marca como visitado e adiciona ao caminho.
            visitado[viz] = true;
            caminho.Add(viz);

            // Explora recursivamente a partir de viz.
            if (BuscaDFS(adj, caminho, visitado, numVertices, inicio, limite, ref resultado))
                return true; // ciclo encontrado em sub-árvore — para imediatamente

            // Backtrack: desfaz a escolha de viz e tenta o próximo vizinho.
            caminho.RemoveAt(caminho.Count - 1);
            visitado[viz] = false;

            // Verifica timeout após cada backtrack (evita ficar preso em ramos longos).
            if (DateTime.UtcNow - inicio > limite)
                return false;
        }

        return false; // nenhum vizinho de ultimo levou a um ciclo
    }
}
