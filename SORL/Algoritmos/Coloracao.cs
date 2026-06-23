using SORL.Core;

namespace SORL.Algoritmos;

public static class Coloracao
{
    // Welsh-Powell sobre o grafo linha (coloração de arestas do grafo original)
    // Dois nós do grafo linha conflitam se as arestas originais compartilham um hub
    public static (int numCores, Dictionary<int, int> rotaCor) Executar(Grafo g)
    {
        List<Aresta> arestas = new List<Aresta>();
        foreach (Aresta a in g.TodasArestas())
            arestas.Add(a);

        int m = arestas.Count;

        if (m == 0)
            return (0, new Dictionary<int, int>());

        // constrói o grafo linha: nó i conflita com nó j se compartilham um vértice
        int[] grauLinha = new int[m];
        List<int>[] adjLinha = new List<int>[m];
        for (int i = 0; i < m; i++)
            adjLinha[i] = new List<int>();

        for (int i = 0; i < m; i++)
        {
            for (int j = i + 1; j < m; j++)
            {
                Aresta ai = arestas[i];
                Aresta aj = arestas[j];

                bool conflito = ai.Origem == aj.Origem
                             || ai.Origem == aj.Destino
                             || ai.Destino == aj.Origem
                             || ai.Destino == aj.Destino;

                if (conflito)
                {
                    adjLinha[i].Add(j);
                    adjLinha[j].Add(i);
                    grauLinha[i]++;
                    grauLinha[j]++;
                }
            }
        }

        // Welsh-Powell: ordena índices por grau decrescente
        List<int> ordem = new List<int>();
        for (int i = 0; i < m; i++)
            ordem.Add(i);

        ordem.Sort((a, b) => grauLinha[b].CompareTo(grauLinha[a]));

        int[] cor = new int[m];
        for (int i = 0; i < m; i++)
            cor[i] = -1;

        int corAtual = 0;
        int coloridos = 0;

        // Passo 3-5: para cada cor, percorre a lista e atribui a cor a todos os
        // vértices não conectados a um vértice já colorido com essa mesma cor
        while (coloridos < m)
        {
            HashSet<int> coloridosNestaRodada = [];

            foreach (int i in ordem)
            {
                if (cor[i] != -1)
                    continue;

                bool conflito = false;
                foreach (int viz in adjLinha[i])
                {
                    if (coloridosNestaRodada.Contains(viz))
                    {
                        conflito = true;
                        break;
                    }
                }

                if (!conflito)
                {
                    cor[i] = corAtual;
                    coloridosNestaRodada.Add(i);
                    coloridos++;
                }
            }

            corAtual++;
        }

        Dictionary<int, int> resultado = new();
        for (int i = 0; i < m; i++)
            resultado[i] = cor[i];

        return (corAtual, resultado);
    }
}
