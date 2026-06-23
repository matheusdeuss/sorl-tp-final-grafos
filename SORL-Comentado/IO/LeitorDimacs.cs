using SORL.Core;

namespace SORL.IO;

// Lê arquivos no formato DIMACS adaptado usado por este projeto.
// Formato do arquivo:
//   Linha 1 (cabeçalho): <V> <E>        (número de vértices e arestas esperadas)
//   Demais linhas:        <u> <v> <peso> <capacidade>   (uma aresta por linha, base 1)
//   Linhas começando com '#' ou em branco são ignoradas (comentários).
// O leitor converte os índices de base 1 (arquivo) para base 0 (interno).
public class LeitorDimacs
{
    // O grafo construído após a leitura, acessível externamente para inspeção.
    public Grafo Grafo { get; private set; } = null!;

    // Número de vértices declarado no cabeçalho do arquivo.
    public int VDeclared { get; private set; }

    // Número de arestas declarado no cabeçalho (pode diferir do real se houver duplicatas).
    public int EDeclared { get; private set; }

    // Quantas arestas foram realmente inseridas (excluindo duplicatas e linhas malformadas).
    public int ELidas { get; private set; }

    // Quantas linhas de aresta foram ignoradas por já terem sido inseridas (aresta duplicada).
    public int Duplicatas { get; private set; }

    // Nome do arquivo (sem caminho), usado nos logs.
    public string NomeArquivo { get; private set; } = "";

    // Método principal: recebe o caminho completo do arquivo e retorna o Grafo construído.
    public Grafo Ler(string caminho)
    {
        // Extrai só o nome do arquivo para exibir em logs (ex: "grafo01.dimacs").
        NomeArquivo = Path.GetFileName(caminho);

        // Lê todas as linhas de uma vez para memória — simples e suficiente para arquivos típicos.
        string[] linhas = File.ReadAllLines(caminho);

        if (linhas.Length == 0)
            throw new InvalidDataException("Arquivo vazio.");

        // Processa o cabeçalho (linha 0): divide por espaços e tabs, ignora tokens vazios.
        // Exemplo: "  10   25  " → ["10", "25"]
        string[] cabecalho = linhas[0].Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        VDeclared = int.Parse(cabecalho[0]); // número de vértices declarado
        EDeclared = int.Parse(cabecalho[1]); // número de arestas declarado

        // Cria o grafo com a estimativa de arestas — usada internamente para escolher
        // entre lista e matriz (veja Grafo.cs). Não é um limite rígido.
        Grafo = new Grafo(VDeclared, EDeclared);

        // HashSet de pares (origem, destino) para detecção de arestas duplicadas.
        // Usa tuplas como chave: compara por valor automaticamente.
        HashSet<(int, int)> vistas = new HashSet<(int, int)>();

        // Itera a partir da linha 1 (pula o cabeçalho).
        for (int i = 1; i < linhas.Length; i++)
        {
            string linha = linhas[i].Trim();

            // Ignora linhas vazias e comentários (começam com '#').
            if (string.IsNullOrWhiteSpace(linha) || linha.StartsWith('#'))
                continue;

            // Divide a linha em tokens, ignorando múltiplos espaços/tabs.
            string[] partes = linha.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            // Linha malformada: precisa de pelo menos 4 campos (u v peso cap). Pula silenciosamente.
            if (partes.Length < 4)
                continue;

            // Converte para índice base 0: arquivo usa base 1, código usa base 0.
            int origem  = int.Parse(partes[0]) - 1;
            int destino = int.Parse(partes[1]) - 1;

            // InvariantCulture: interpreta '.' como separador decimal independente do SO.
            // Sem isso, em sistemas com locale pt-BR o '.' seria ignorado ou causaria erro.
            double peso       = double.Parse(partes[2], System.Globalization.CultureInfo.InvariantCulture);
            double capacidade = double.Parse(partes[3], System.Globalization.CultureInfo.InvariantCulture);

            // Verifica se esta aresta direcionada (origem → destino) já foi inserida.
            if (vistas.Contains((origem, destino)))
            {
                Duplicatas++; // conta mas não insere
                continue;
            }

            // Registra o par no conjunto para bloquear futuras duplicatas.
            vistas.Add((origem, destino));

            // Cria a Aresta e a adiciona ao grafo.
            Grafo.AdicionarAresta(new Aresta(origem, destino, peso, capacidade));
            ELidas++;
        }

        return Grafo;
    }
}
