namespace SORL.IO;

public class Logger : IDisposable
{
    private readonly StreamWriter _file;

    public Logger(string caminhoArquivo)
    {
        _file = new StreamWriter(caminhoArquivo, append: false, encoding: System.Text.Encoding.UTF8);
    }

    public void Escrever(string texto)
    {
        Console.Write(texto);
        _file.Write(texto);
    }

    public void Linha(string texto = "")
    {
        Console.WriteLine(texto);
        _file.WriteLine(texto);
    }

    public void Separador(char c = '=', int n = 64) => Linha(new string(c, n));
    public void SubSeparador() => Linha(new string('-', 64));

    public void Dispose()
    {
        _file.Flush();
        _file.Dispose();
    }
}
