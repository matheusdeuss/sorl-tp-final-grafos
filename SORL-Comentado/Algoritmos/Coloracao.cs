using SORL.Core;

namespace SORL.Algoritmos;

// Coloração de arestas via algoritmo Welsh-Powell aplicado ao GRAFO LINHA (line graph).
//
// Problema logístico modelado:
//   Cada aresta do grafo original representa uma "rota" (trecho de entrega).
//   Duas rotas conflitam se compartilham um mesmo vértice (mesmo depósito/cidade).
//   Rotas conflitantes não podem operar no mesmo turno → precisam de cores diferentes.
//   Objetivo: minimizar o número de turnos (cores) necessários.
//
// Transformação grafo linha:
//   Cada aresta eᵢ do grafo original vira um vértice no grafo linha.
//   Dois vértices eᵢ e eⱼ são adjacentes no grafo linha se eᵢ e eⱼ compartilham
//   um vértice comum no grafo original (conflito de rota).
//
// Welsh-Powell (heurística gulosa para coloração de vértices):
//   1. Ordena vértices por grau decrescente (mais conflitos primeiro).
//   2. Atribui a mesma cor a um conjunto máximo de vértices não-adjacentes.
//   3. Repete com a próxima cor até colorir todos.
//
// Complexidade: O(m²) para construir o grafo linha, onde m = número de arestas.
public static class Coloracao
{
    // Retorna:
    //   numCores  — quantidade de turnos (cores) utilizados
    //   rotaCor   — dicionário: índice da aresta → índice da cor (turno), ambos base 0
    public static (int numCores, Dictionary<int, int> rotaCor) Executar(Grafo g)
    {
        // ── Etapa 1: coletar todas as arestas do grafo original numa lista indexada ──
        // O índice i na lista será o "vértice" correspondente no grafo linha.
        List<Aresta> arestas = new List<Aresta>();
        foreach (Aresta a in g.TodasArestas())
            arestas.Add(a);

        int m = arestas.Count; // número de vértices no grafo linha

        // Grafo sem arestas → nenhum turno necessário.
        if (m == 0)
            return (0, new Dictionary<int, int>());

        // ── Etapa 2: construir o grafo linha ──
        // grauLinha[i] = grau do vértice i no grafo linha (quantos conflitos a aresta i tem).
        int[]      grauLinha = new int[m];

        // adjLinha[i] = lista de índices de arestas que conflitam com a aresta i.
        List<int>[] adjLinha  = new List<int>[m];
        for (int i = 0; i < m; i++)
            adjLinha[i] = new List<int>();

        // Para cada par de arestas (i, j), verifica se há conflito.
        // Conflito: as duas arestas compartilham pelo menos um vértice (origem ou destino).
        // Exemplo: aresta (1→2) conflita com (2→3) porque compartilham o vértice 2.
        for (int i = 0; i < m; i++)
        {
            for (int j = i + 1; j < m; j++) // j > i evita verificar o mesmo par duas vezes
            {
                Aresta ai = arestas[i];
                Aresta aj = arestas[j];

                // Quatro condições de conflito: qualquer extremidade de ai coincide com qualquer de aj.
                bool conflito = ai.Origem  == aj.Origem
                             || ai.Origem  == aj.Destino
                             || ai.Destino == aj.Origem
                             || ai.Destino == aj.Destino;

                if (conflito)
                {
                    // Grafo linha é não-dirigido: conflito é simétrico.
                    adjLinha[i].Add(j);
                    adjLinha[j].Add(i);
                    grauLinha[i]++;
                    grauLinha[j]++;
                }
            }
        }

        // ── Etapa 3: Welsh-Powell — ordenar vértices por grau decrescente ──
        // Vértices com mais conflitos são coloridos primeiro (heurística para usar menos cores).
        List<int> ordem = new List<int>();
        for (int i = 0; i < m; i++)
            ordem.Add(i);

        // Sort decrescente: grauLinha[b] comparado com grauLinha[a] → maior grau primeiro.
        ordem.Sort((a, b) => grauLinha[b].CompareTo(grauLinha[a]));

        // ── Etapa 4: atribuir cores (turnos) ──
        // cor[i] = índice da cor atribuída à aresta i. -1 = ainda não colorida.
        int[] cor = new int[m];
        for (int i = 0; i < m; i++)
            cor[i] = -1;

        int corAtual  = 0;  // índice da cor (turno) sendo atribuída nesta rodada
        int coloridos = 0;  // contador de arestas já coloridas

        // Repete enquanto houver arestas sem cor.
        while (coloridos < m)
        {
            // Conjunto de vértices coloridos com a corAtual nesta rodada.
            // Usado para verificar conflito: não podemos colorir i se algum vizinho
            // conflitante já recebeu corAtual nesta mesma rodada.
            HashSet<int> coloridosNestaRodada = [];

            // Percorre todos os vértices na ordem Welsh-Powell.
            foreach (int i in ordem)
            {
                if (cor[i] != -1)
                    continue; // já colorido em rodada anterior

                // Verifica se algum vizinho conflitante já foi colorido com corAtual nesta rodada.
                bool conflito = false;
                foreach (int viz in adjLinha[i])
                {
                    if (coloridosNestaRodada.Contains(viz))
                    {
                        conflito = true;
                        break; // basta um conflito para não poder usar esta cor
                    }
                }

                if (!conflito)
                {
                    // Atribui a cor atual à aresta i.
                    cor[i] = corAtual;
                    coloridosNestaRodada.Add(i);
                    coloridos++;
                }
            }

            // Próxima rodada usará a próxima cor (próximo turno).
            corAtual++;
        }

        // ── Etapa 5: montar o dicionário de saída ──
        // Chave = índice da aresta no grafo original; Valor = índice do turno (cor).
        Dictionary<int, int> resultado = new();
        for (int i = 0; i < m; i++)
            resultado[i] = cor[i];

        // corAtual foi incrementado uma vez a mais após a última rodada,
        // então o número total de cores usadas é exatamente corAtual.
        return (corAtual, resultado);
    }
}
