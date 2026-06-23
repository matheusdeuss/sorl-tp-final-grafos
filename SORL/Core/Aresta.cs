namespace SORL.Core;

public class Aresta
{
    public int Origem { get; }
    public int Destino { get; }
    public double Peso { get; }
    public double Capacidade { get; }

    public Aresta(int origem, int destino, double peso, double capacidade)
    {
        Origem = origem;
        Destino = destino;
        Peso = peso;
        Capacidade = capacidade;
    }

    public override string ToString()
    {
        return "(" + (Origem + 1) + "->" + (Destino + 1) + " peso=" + Peso + " cap=" + Capacidade + ")";
    }
}
