using SORL.Core;

namespace SORL.Algoritmos;

public static class Dijkstra
{
    public static (double custo, List<int> caminho) Executar(Grafo g, int s, int t)
    {
        double[] dist = new double[g.V];
        int[] pred = new int[g.V];
        bool[] explorado = new bool[g.V];

        // Passo 1: inicializar distâncias e predecessores
        for (int i = 0; i < g.V; i++)
        {
            dist[i] = double.PositiveInfinity;
            pred[i] = -1;
        }

        // Passo 2-3: inserir raiz no conjunto S e definir dist[s] = 0
        dist[s] = 0;
        explorado[s] = true;
        Queue<int> S = new();
        S.Enqueue(s);

        // Passo 4: repetir |V| - 1 vezes
        for (int i = 0; i < g.V - 1; i++)
        {
            // Passo 4a: encontrar aresta (v, w) no corte(S) com dist[v] + d(v,w) mínimo
            int melhorV = -1;
            int melhorW = -1;
            double menorDist = double.PositiveInfinity;

            foreach (int v in S)
            {
                foreach (Aresta a in g.Vizinhos(v))
                {
                    if (!explorado[a.Destino])
                    {
                        double candidato = dist[v] + a.Peso;
                        if (candidato < menorDist)
                        {
                            menorDist = candidato;
                            melhorV = v;
                            melhorW = a.Destino;
                        }
                    }
                }
            }

            if (melhorW == -1)
                break; // grafo desconexo, nenhuma aresta de corte disponível

            // Passo 4b: atualizar distância e predecessor de w
            dist[melhorW] = menorDist;
            pred[melhorW] = melhorV;

            // Passo 4c: adicionar w ao conjunto S
            explorado[melhorW] = true;
            S.Enqueue(melhorW);
        }

        if (double.IsInfinity(dist[t]))
            return (double.PositiveInfinity, new List<int>());

        List<int> caminho = new List<int>();
        int atual = t;
        while (atual != -1)
        {
            caminho.Add(atual);
            atual = pred[atual];
        }
        caminho.Reverse();

        return (dist[t], caminho);
    }
}
