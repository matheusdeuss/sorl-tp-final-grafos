namespace SORL.IO;

// Logger com saída dupla: escreve simultaneamente no console e num arquivo de texto.
// Implementa IDisposable para garantir que o arquivo seja fechado corretamente ao
// sair do bloco "using" — mesmo se ocorrer uma exceção.
public class Logger : IDisposable
{
    // StreamWriter encapsula a escrita no arquivo de log em UTF-8.
    private readonly StreamWriter _file;

    // Abre (ou cria) o arquivo no caminho indicado.
    // append: false → sobrescreve o arquivo se já existir (cada execução cria um log limpo).
    // UTF8 → garante que acentos e caracteres especiais sejam salvos corretamente.
    public Logger(string caminhoArquivo)
    {
        _file = new StreamWriter(caminhoArquivo, append: false, encoding: System.Text.Encoding.UTF8);
    }

    // Escreve texto SEM quebra de linha — útil para construção incremental de saídas.
    // Espelha no console E no arquivo ao mesmo tempo.
    public void Escrever(string texto)
    {
        Console.Write(texto);
        _file.Write(texto);
    }

    // Escreve uma linha completa (com '\n' no final).
    // Parâmetro opcional: sem argumento escreve uma linha em branco (útil para separar seções).
    public void Linha(string texto = "")
    {
        Console.WriteLine(texto);
        _file.WriteLine(texto);
    }

    // Atalho para linha de separação visual com o caractere c repetido n vezes.
    // Padrão: 64 '=' → "================================================================"
    // Usado para separar blocos de grafos no log.
    public void Separador(char c = '=', int n = 64) => Linha(new string(c, n));

    // Variante com '-' para sub-separadores dentro de um bloco.
    public void SubSeparador() => Linha(new string('-', 64));

    // Chamado automaticamente ao sair do bloco "using Logger(...) { ... }".
    // Flush: garante que nenhum dado fique preso no buffer interno do StreamWriter.
    // Dispose: fecha o handle do arquivo, liberando o recurso do sistema operacional.
    public void Dispose()
    {
        _file.Flush();
        _file.Dispose();
    }
}
