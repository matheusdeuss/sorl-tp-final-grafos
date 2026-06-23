using SORL.Core;

namespace SORL.IO;

public class LeitorDimacs
{
    public Grafo Grafo { get; private set; } = null!;
    public int VDeclared { get; private set; }
    public int EDeclared { get; private set; }
    public int ELidas { get; private set; }
    public int Duplicatas { get; private set; }
    public string NomeArquivo { get; private set; } = "";

    public Grafo Ler(string caminho)
    {
        NomeArquivo = Path.GetFileName(caminho);
        string[] linhas = File.ReadAllLines(caminho);

        if (linhas.Length == 0)
            throw new InvalidDataException("Arquivo vazio.");

        string[] cabecalho = linhas[0].Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        VDeclared = int.Parse(cabecalho[0]);
        EDeclared = int.Parse(cabecalho[1]);

        Grafo = new Grafo(VDeclared, EDeclared);

        HashSet<(int, int)> vistas = new HashSet<(int, int)>();

        for (int i = 1; i < linhas.Length; i++)
        {
            string linha = linhas[i].Trim();

            if (string.IsNullOrWhiteSpace(linha) || linha.StartsWith('#'))
                continue;

            string[] partes = linha.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (partes.Length < 4)
                continue;

            int origem = int.Parse(partes[0]) - 1;
            int destino = int.Parse(partes[1]) - 1;
            double peso = double.Parse(partes[2], System.Globalization.CultureInfo.InvariantCulture);
            double capacidade = double.Parse(partes[3], System.Globalization.CultureInfo.InvariantCulture);

            if (vistas.Contains((origem, destino)))
            {
                Duplicatas++;
                continue;
            }

            vistas.Add((origem, destino));
            Grafo.AdicionarAresta(new Aresta(origem, destino, peso, capacidade));
            ELidas++;
        }

        return Grafo;
    }
}
