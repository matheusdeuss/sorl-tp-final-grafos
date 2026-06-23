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

        int maxCor = 0;

        foreach (int i in ordem)
        {
            // coleta as cores já usadas pelos vizinhos coloridos
            HashSet<int> coresUsadas = new HashSet<int>();
            foreach (int vizinho in adjLinha[i])
            {
                if (cor[vizinho] != -1)
                    coresUsadas.Add(cor[vizinho]);
            }

            // atribui a menor cor disponível
            int c = 0;
            while (coresUsadas.Contains(c))
                c++;

            cor[i] = c;

            if (c + 1 > maxCor)
                maxCor = c + 1;
        }

        Dictionary<int, int> resultado = new Dictionary<int, int>();
        for (int i = 0; i < m; i++)
            resultado[i] = cor[i];

        return (maxCor, resultado);
    }
}
