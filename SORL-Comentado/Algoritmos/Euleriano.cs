using SORL.Core;

namespace SORL.Algoritmos;

// Verifica se um grafo dirigido tem circuito Euleriano e, se sim, o encontra.
// Um circuito Euleriano visita TODAS as arestas exatamente uma vez e retorna à origem.
//
// Condições necessárias e suficientes para existência num dígrafo:
//   1. Todo vértice com arestas tem grauEntrada == grauSaida.
//   2. O subgrafo induzido pelos vértices com arestas é fortemente conexo.
//
// Se existir, o circuito é construído pelo algoritmo de Hierholzer (iterativo com pilha).
// Complexidade: O(V + E).
public static class Euleriano
{
    // Retorna: (se existe, mensagem explicativa, lista de vértices do circuito).
    // A lista inclui o vértice inicial repetido no final (circuito fechado).
    public static (bool existe, string motivo, List<int> circuito) Executar(Grafo g)
    {
        // === Condição 1: grauEntrada == grauSaida para todo vértice ===
        // Num circuito Euleriano, toda vez que "entramos" num vértice precisamos
        // também "sair" — portanto os graus de entrada e saída devem ser iguais.
        for (int v = 0; v < g.V; v++)
        {
            if (g.GrauEntrada(v) != g.GrauSaida(v))
            {
                string msg = $"vértice {v + 1} tem grauEntrada={g.GrauEntrada(v)} != grauSaida={g.GrauSaida(v)}";
                return (false, msg, new List<int>());
            }
        }

        // === Coleta vértices que participam de pelo menos uma aresta ===
        // Vértices isolados (sem nenhuma aresta) são ignorados na verificação de conectividade,
        // pois um circuito Euleriano só precisa cobrir as arestas existentes.
        HashSet<int> comAresta = new HashSet<int>();
        foreach (Aresta a in g.TodasArestas())
        {
            comAresta.Add(a.Origem);
            comAresta.Add(a.Destino);
        }

        // Grafo sem arestas: circuito trivialmente inexistente (nada a percorrer).
        if (comAresta.Count == 0)
            return (false, "grafo sem arestas", new List<int>());

        // Pega qualquer vértice com aresta como ponto de partida para o circuito.
        int inicio = -1;
        foreach (int v in comAresta) { inicio = v; break; }

        // === Condição 2: fortemente conexo (restrito aos vértices com arestas) ===
        // Verifica se todos os vértices com arestas podem ser alcançados a partir de
        // qualquer um deles, tanto no grafo original quanto no transposto.
        if (!FortementeConexo(g, comAresta, inicio))
            return (false, "grafo não é fortemente conexo", new List<int>());

        // === Algoritmo de Hierholzer (versão iterativa) ===
        // Copia as listas de adjacência do grafo para listas "residuais" mutáveis.
        // À medida que arestas são usadas, são removidas das listas residuais,
        // garantindo que cada aresta seja usada exatamente uma vez.
        List<Aresta>[] adjResiduais = new List<Aresta>[g.V];
        for (int v = 0; v < g.V; v++)
        {
            adjResiduais[v] = new List<Aresta>();
            foreach (Aresta a in g.Vizinhos(v))
                adjResiduais[v].Add(a); // cópia independente da lista original
        }

        Stack<int> pilha  = new Stack<int>();
        List<int> circuito = new List<int>();
        pilha.Push(inicio); // começa pelo vértice de partida

        // Núcleo do algoritmo de Hierholzer:
        while (pilha.Count > 0)
        {
            int v = pilha.Peek(); // observa o topo sem remover

            if (adjResiduais[v].Count > 0)
            {
                // O vértice ainda tem arestas não usadas: avança por uma delas.
                // Usa a última aresta da lista (remoção eficiente: O(1) pelo final).
                Aresta a = adjResiduais[v][adjResiduais[v].Count - 1];
                adjResiduais[v].RemoveAt(adjResiduais[v].Count - 1); // marca aresta como usada
                pilha.Push(a.Destino); // empilha o destino para continuar a partir dele
            }
            else
            {
                // O vértice não tem mais arestas disponíveis: é um ponto de retorno.
                // Remove da pilha e adiciona ao circuito (Hierholzer adiciona em ordem reversa).
                circuito.Add(pilha.Pop());
            }
        }

        // O algoritmo constrói o circuito de trás para frente — revertemos para a ordem correta.
        circuito.Reverse();
        return (true, "OK", circuito);
    }

    // Verifica conectividade forte restrita ao conjunto 'vertices' (vértices com arestas).
    // Usa duas DFS: uma no grafo original (alcance "para frente")
    // e uma no grafo transposto (alcance "para trás").
    // Se ambas alcançam todos os vértices, o subgrafo é fortemente conexo.
    private static bool FortementeConexo(Grafo g, HashSet<int> vertices, int origem)
    {
        if (!DfsAlcanca(g, vertices, origem, false)) // DFS no grafo original
            return false;
        if (!DfsAlcanca(g, vertices, origem, true))  // DFS no grafo transposto
            return false;
        return true;
    }

    // DFS genérica que funciona tanto no grafo original (transposto=false)
    // quanto no grafo transposto (transposto=true, inverte o sentido das arestas).
    // Verifica se todos os vértices em 'vertices' são alcançados a partir de 'origem'.
    private static bool DfsAlcanca(Grafo g, HashSet<int> vertices, int origem, bool transposto)
    {
        HashSet<int> visitado = []; // vértices já visitados nesta DFS
        Stack<int> pilha = new();
        pilha.Push(origem);
        visitado.Add(origem);

        while (pilha.Count > 0)
        {
            int u = pilha.Pop();

            // No modo transposto, usamos VizinhosTranspostos (quem aponta PARA u).
            // No modo normal, usamos os vizinhos diretos de u (quem u aponta).
            IEnumerable<Aresta> vizinhos;
            if (!transposto)
                vizinhos = g.Vizinhos(u);
            else
                vizinhos = VizinhosTranspostos(g, u);

            foreach (Aresta a in vizinhos)
            {
                // No transposto, a "aresta" que usamos tem origem e destino invertidos,
                // portanto o próximo vértice a visitar é a.Origem (quem enviou a aresta para u).
                int dest = transposto ? a.Origem : a.Destino;

                if (!visitado.Contains(dest) && vertices.Contains(dest))
                {
                    visitado.Add(dest);
                    pilha.Push(dest);
                }
            }
        }

        // Confere se todos os vértices com arestas foram visitados.
        foreach (int v in vertices)
        {
            if (!visitado.Contains(v))
                return false; // pelo menos um vértice não foi alcançado
        }
        return true;
    }

    // Retorna todas as arestas que chegam ao vértice v (vizinhos no grafo transposto).
    // Implementado por varredura linear de todas as arestas — O(E).
    // Usado apenas na verificação de conectividade (não no Hierholzer).
    private static List<Aresta> VizinhosTranspostos(Grafo g, int v)
    {
        List<Aresta> resultado = new List<Aresta>();
        foreach (Aresta a in g.TodasArestas())
        {
            if (a.Destino == v) // aresta que CHEGA em v
                resultado.Add(a);
        }
        return resultado;
    }
}
