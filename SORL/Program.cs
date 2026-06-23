using System.Diagnostics;
using SORL.Algoritmos;
using SORL.Core;
using SORL.IO;

string grafosDir = Path.Combine(AppContext.BaseDirectory, "Grafos");
string logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
Directory.CreateDirectory(logDir);

while (true)
{
    Console.WriteLine();
    Console.WriteLine("=== SORL — Sistema de Otimização de Rotas Logísticas ===");
    Console.WriteLine("1. Rodar todos os grafos da pasta Grafos/ (salva log)");
    Console.WriteLine("2. Rodar um grafo específico");
    Console.WriteLine("0. Sair");
    Console.Write("Escolha: ");

    string op = Console.ReadLine() ?? "";
    op = op.Trim();
    if (op == "0") break;

    if (op == "1")
    {
        string[] arquivos = Directory.GetFiles(grafosDir, "*.dimacs");
        Array.Sort(arquivos);

        if (arquivos.Length == 0)
        {
            Console.WriteLine("Nenhum .dimacs em: " + grafosDir);
            continue;
        }

        string nomeLog = Path.Combine(logDir, "resultados_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
        using (Logger logger = new Logger(nomeLog))
        {
            foreach (string arq in arquivos)
                ProcessarGrafo(arq, logger, 1, 0);
        }
    }
    else if (op == "2")
    {
        string[] arquivos = Directory.GetFiles(grafosDir, "*.dimacs");
        Array.Sort(arquivos);

        if (arquivos.Length == 0)
        {
            Console.WriteLine("Nenhum .dimacs encontrado.");
            continue;
        }

        Console.WriteLine("Arquivos disponíveis:");
        for (int i = 0; i < arquivos.Length; i++)
            Console.WriteLine("  " + (i + 1) + ". " + Path.GetFileName(arquivos[i]));

        Console.Write("Número do arquivo: ");
        int idx;
        if (!int.TryParse(Console.ReadLine(), out idx) || idx < 1 || idx > arquivos.Length)
        {
            Console.WriteLine("Opção inválida.");
            continue;
        }

        Console.Write("Vértice origem S (padrão 1): ");
        string sStr = Console.ReadLine() ?? "";
        int s;
        if (string.IsNullOrEmpty(sStr))
            s = 1;
        else
            s = int.Parse(sStr.Trim());

        Console.Write("Vértice destino T (padrão = último): ");
        string tStr = Console.ReadLine() ?? "";
        int t;
        if (string.IsNullOrEmpty(tStr))
            t = 0;
        else
            t = int.Parse(tStr.Trim());

        string nomeLog = Path.Combine(logDir, "grafo" + idx + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
        using (Logger logger = new Logger(nomeLog))
        {
            ProcessarGrafo(arquivos[idx - 1], logger, s, t);
        }
    }
}

static void ProcessarGrafo(string caminho, Logger log, int sExplicito, int tExplicito)
{
    LeitorDimacs leitor = new LeitorDimacs();
    Grafo g;

    try
    {
        g = leitor.Ler(caminho);
    }
    catch (Exception ex)
    {
        log.Linha("ERRO ao ler " + caminho + ": " + ex.Message);
        return;
    }

    int sIdx = sExplicito - 1;
    int tIdx;
    if (tExplicito == 0)
        tIdx = g.V - 1;
    else
        tIdx = tExplicito - 1;

    log.Separador('=');
    log.Linha($"GRAFO: {leitor.NomeArquivo} | V={g.V} | E={g.E} | densidade={g.Densidade:F3}");
    log.Linha($"Estrutura escolhida: {g.TipoRepresentacao} ({g.JustificativaRepresentacao})");

    if (leitor.EDeclared != leitor.ELidas)
        log.Linha($"AVISO: E declarado={leitor.EDeclared} mas E lidas={leitor.ELidas}");

    log.Linha($"Arestas duplicadas descartadas: {leitor.Duplicatas}");
    log.SubSeparador();

    // P1 — Dijkstra (direcionado)
    Stopwatch sw1 = Stopwatch.StartNew();
    (double custoDij, List<int> cam) = Dijkstra.Executar(g, sIdx, tIdx);
    sw1.Stop();

    if (double.IsInfinity(custoDij))
    {
        log.Linha($"[P1] Menor custo  S={sIdx + 1} T={tIdx + 1} -> INALCANÇÁVEL  tempo={sw1.Elapsed.TotalMilliseconds:F1}ms");
    }
    else
    {
        List<string> partesCam = new List<string>();
        foreach (int v in cam)
            partesCam.Add((v + 1).ToString());
        string path = string.Join("->", partesCam);
        log.Linha($"[P1] Menor custo  S={sIdx + 1} T={tIdx + 1} -> {path}  custo={custoDij}  tempo={sw1.Elapsed.TotalMilliseconds:F1}ms");
    }

    // P2 — Edmonds-Karp + corte mínimo (direcionado)
    Stopwatch sw2 = Stopwatch.StartNew();
    (double fluxo, List<(int u, int v)> corte) = FluxoMaximo.Executar(g, sIdx, tIdx);
    sw2.Stop();

    List<string> partesCorte = new List<string>();
    foreach ((int u, int v) in corte)
        partesCorte.Add("(" + (u + 1) + "," + (v + 1) + ")");
    string corteStr = string.Join(", ", partesCorte);
    log.Linha($"[P2] Fluxo máximo  S={sIdx + 1} T={tIdx + 1} -> fluxo={fluxo}  corte={{{corteStr}}}  tempo={sw2.Elapsed.TotalMilliseconds:F1}ms");

    // P3 — AGM Kruskal (não-direcionado)
    Stopwatch sw3 = Stopwatch.StartNew();
    (double custoAgm, List<(int u, int v, double peso)> arestas) = ArvoreGeradoraMinima.Executar(g);
    sw3.Stop();

    List<string> partesAgm = new List<string>();
    foreach ((int u, int v, double peso) in arestas)
        partesAgm.Add("(" + (u + 1) + "-" + (v + 1) + " p=" + peso + ")");
    string agmStr = string.Join(", ", partesAgm);
    log.Linha($"[P3] AGM Kruskal (não-dir.) -> custo={custoAgm}  [{agmStr}]  tempo={sw3.Elapsed.TotalMilliseconds:F1}ms");

    // P4 — Welsh-Powell (coloração de arestas via grafo linha)
    Stopwatch sw4 = Stopwatch.StartNew();
    List<Aresta> todasArestas = new List<Aresta>();
    foreach (Aresta a in g.TodasArestas())
        todasArestas.Add(a);
    (int numCores, Dictionary<int, int> rotaCor) = Coloracao.Executar(g);
    sw4.Stop();

    List<string> partesAloc = new List<string>();
    int contagem = 0;
    foreach (KeyValuePair<int, int> kv in rotaCor)
    {
        if (contagem >= 10) break;
        Aresta ar = todasArestas[kv.Key];
        partesAloc.Add("rota(" + (ar.Origem + 1) + "->" + (ar.Destino + 1) + ")=T" + (kv.Value + 1));
        contagem++;
    }
    string alocStr = string.Join(", ", partesAloc);
    if (rotaCor.Count > 10)
        alocStr += ", ...+" + (rotaCor.Count - 10) + " mais";
    log.Linha($"[P4] Coloração Welsh-Powell -> turnos={numCores}  [{alocStr}]  tempo={sw4.Elapsed.TotalMilliseconds:F1}ms");

    // P5A — Euleriano (direcionado)
    Stopwatch sw5a = Stopwatch.StartNew();
    (bool existeEul, string motivo, List<int> circuito) = Euleriano.Executar(g);
    sw5a.Stop();

    if (!existeEul)
    {
        log.Linha($"[P5A] Euleriano (dir.) -> NÃO EXISTE ({motivo})  tempo={sw5a.Elapsed.TotalMilliseconds:F1}ms");
    }
    else
    {
        List<string> partesEul = new List<string>();
        int limEul = Math.Min(circuito.Count, 20);
        for (int i = 0; i < limEul; i++)
            partesEul.Add((circuito[i] + 1).ToString());
        string seq = string.Join("->", partesEul);
        if (circuito.Count > 20)
            seq += "->...(" + circuito.Count + " total)";
        log.Linha($"[P5A] Euleriano (dir.) -> EXISTE: {seq}  tempo={sw5a.Elapsed.TotalMilliseconds:F1}ms");
    }

    // P5B — Hamiltoniano (não-direcionado, timeout 10s)
    TimeSpan limite = TimeSpan.FromSeconds(10);
    Stopwatch sw5b = Stopwatch.StartNew();
    (bool? existeHam, List<int>? ciclo) = Hamiltoniano.Executar(g, limite);
    sw5b.Stop();

    if (existeHam == null)
    {
        log.Linha($"[P5B] Hamiltoniano (não-dir.) -> NÃO CONCLUSIVO (timeout {limite.TotalSeconds}s)  tempo={sw5b.Elapsed.TotalMilliseconds:F1}ms");
    }
    else if (existeHam == false)
    {
        log.Linha($"[P5B] Hamiltoniano (não-dir.) -> NÃO EXISTE  tempo={sw5b.Elapsed.TotalMilliseconds:F1}ms");
    }
    else if (ciclo != null)
    {
        List<string> partesHam = new List<string>();
        int limHam = Math.Min(ciclo.Count, 20);
        for (int i = 0; i < limHam; i++)
            partesHam.Add((ciclo[i] + 1).ToString());
        string seq = string.Join("->", partesHam);
        if (ciclo.Count > 20)
            seq += "->...(" + ciclo.Count + " vértices)";
        log.Linha($"[P5B] Hamiltoniano (não-dir.) -> EXISTE: {seq}  tempo={sw5b.Elapsed.TotalMilliseconds:F1}ms");
    }

    log.Linha();
}
