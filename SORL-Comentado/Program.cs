// Ponto de entrada da aplicação SORL (Sistema de Otimização de Rotas Logísticas).
// Este arquivo orquestra o menu interativo, a leitura dos grafos DIMACS e a execução
// de todos os algoritmos do projeto, registrando os resultados em arquivos de log.

using System.Diagnostics;   // Stopwatch: mede tempo de execução de cada algoritmo
using SORL.Algoritmos;    // Dijkstra, FluxoMaximo, Kruskal, Coloracao, Euleriano, Hamiltoniano
using SORL.Core;           // Grafo, Aresta
using SORL.IO;             // LeitorDimacs, Logger

// Caminho da pasta "Grafos" relativo ao diretório onde o executável está rodando.
// AppContext.BaseDirectory aponta para bin/Debug ou bin/Release após compilar.
string grafosDir = Path.Combine(AppContext.BaseDirectory, "Grafos");

// Caminho da pasta "Logs" — criada automaticamente se não existir.
string logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
Directory.CreateDirectory(logDir); // não falha se a pasta já existir

// Loop principal do menu: repete até o usuário escolher "0" (Sair).
while (true)
{
    // Exibe o cabeçalho e as opções disponíveis.
    Console.WriteLine();
    Console.WriteLine("=== SORL — Sistema de Otimização de Rotas Logísticas ===");
    Console.WriteLine("1. Rodar todos os grafos da pasta Grafos/ (salva log)");
    Console.WriteLine("2. Rodar um grafo específico");
    Console.WriteLine("0. Sair");
    Console.Write("Escolha: ");

    // Lê a opção do usuário. O operador ?? "" evita null se o usuário pressionar Enter sem digitar.
    string op = Console.ReadLine() ?? "";
    op = op.Trim(); // remove espaços acidentais antes/depois

    if (op == "0") break; // encerra o loop e, consequentemente, o programa

    // ── Opção 1: processar TODOS os arquivos .dimacs da pasta Grafos ──
    if (op == "1")
    {
        // Busca todos os arquivos com extensão .dimacs no diretório configurado.
        string[] arquivos = Directory.GetFiles(grafosDir, "*.dimacs");
        Array.Sort(arquivos); // ordena alfabeticamente para resultados reproduzíveis

        if (arquivos.Length == 0)
        {
            Console.WriteLine("Nenhum .dimacs em: " + grafosDir);
            continue; // volta ao menu sem encerrar o programa
        }

        // Nome do log com timestamp: evita sobrescrever execuções anteriores.
        // Formato: resultados_20250623_143052.txt
        string nomeLog = Path.Combine(logDir, "resultados_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");

        // "using" garante que o Logger seja fechado (Dispose) mesmo se ocorrer exceção.
        using (Logger logger = new Logger(nomeLog))
        {
            // s=1 e t=0: origem = vértice 1, destino = último vértice (padrão do sistema).
            foreach (string arq in arquivos)
                ProcessarGrafo(arq, logger, 1, 0);
        }
    }
    // ── Opção 2: processar UM grafo escolhido pelo usuário ──
    else if (op == "2")
    {
        string[] arquivos = Directory.GetFiles(grafosDir, "*.dimacs");
        Array.Sort(arquivos);

        if (arquivos.Length == 0)
        {
            Console.WriteLine("Nenhum .dimacs encontrado.");
            continue;
        }

        // Lista numerada para o usuário escolher qual arquivo processar.
        Console.WriteLine("Arquivos disponíveis:");
        for (int i = 0; i < arquivos.Length; i++)
            Console.WriteLine("  " + (i + 1) + ". " + Path.GetFileName(arquivos[i]));

        Console.Write("Número do arquivo: ");
        int idx;
        // Valida se o número digitado é inteiro e está dentro do intervalo [1, arquivos.Length].
        if (!int.TryParse(Console.ReadLine(), out idx) || idx < 1 || idx > arquivos.Length)
        {
            Console.WriteLine("Opção inválida.");
            continue;
        }

        // Vértice origem S (base 1 na interface, convertido para base 0 dentro de ProcessarGrafo).
        Console.Write("Vértice origem S (padrão 1): ");
        string sStr = Console.ReadLine() ?? "";
        int s;
        if (string.IsNullOrEmpty(sStr))
            s = 1; // padrão: primeiro vértice
        else
            s = int.Parse(sStr.Trim());

        // Vértice destino T. Valor 0 significa "usar o último vértice do grafo".
        Console.Write("Vértice destino T (padrão = último): ");
        string tStr = Console.ReadLine() ?? "";
        int t;
        if (string.IsNullOrEmpty(tStr))
            t = 0; // sinal especial: ProcessarGrafo converte 0 → g.V - 1
        else
            t = int.Parse(tStr.Trim());

        // Log individual para este grafo: grafo3_20250623_143052.txt
        string nomeLog = Path.Combine(logDir, "grafo" + idx + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
        using (Logger logger = new Logger(nomeLog))
        {
            // idx - 1 porque a lista exibida começa em 1, mas o array C# começa em 0.
            ProcessarGrafo(arquivos[idx - 1], logger, s, t);
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ProcessarGrafo: lê um arquivo DIMACS, executa os 6 problemas (P1–P5B) e loga tudo.
//
// Parâmetros:
//   caminho     — caminho completo do arquivo .dimacs
//   log         — instância do Logger (console + arquivo)
//   sExplicito  — vértice origem em base 1 (1 = primeiro vértice)
//   tExplicito  — vértice destino em base 1; 0 = usar o último vértice (g.V)
// ─────────────────────────────────────────────────────────────────────────────
static void ProcessarGrafo(string caminho, Logger log, int sExplicito, int tExplicito)
{
    LeitorDimacs leitor = new LeitorDimacs();
    Grafo g;

    // Tenta ler o arquivo; se falhar (formato inválido, arquivo corrompido), registra erro e retorna.
    try
    {
        g = leitor.Ler(caminho);
    }
    catch (Exception ex)
    {
        log.Linha("ERRO ao ler " + caminho + ": " + ex.Message);
        return; // não tenta executar algoritmos sem grafo válido
    }

    // Converte índices de base 1 (interface do usuário) para base 0 (interno do código).
    int sIdx = sExplicito - 1;
    int tIdx;
    if (tExplicito == 0)
        tIdx = g.V - 1; // destino padrão: último vértice
    else
        tIdx = tExplicito - 1;

    // ── Cabeçalho do bloco de log para este grafo ──
    log.Separador('=');
    log.Linha($"GRAFO: {leitor.NomeArquivo} | V={g.V} | E={g.E} | densidade={g.Densidade:F3}");
    log.Linha($"Estrutura escolhida: {g.TipoRepresentacao} ({g.JustificativaRepresentacao})");

    // Aviso se o cabeçalho DIMACS declarou mais/menos arestas do que foram lidas de fato.
    if (leitor.EDeclared != leitor.ELidas)
        log.Linha($"AVISO: E declarado={leitor.EDeclared} mas E lidas={leitor.ELidas}");

    log.Linha($"Arestas duplicadas descartadas: {leitor.Duplicatas}");
    log.SubSeparador();

    // ── P1: Caminho mínimo (Dijkstra) — grafo tratado como DIRIGIDO ──
    Stopwatch sw1 = Stopwatch.StartNew();
    (double custoDij, List<int> cam) = Dijkstra.Executar(g, sIdx, tIdx);
    sw1.Stop();

    if (double.IsInfinity(custoDij))
    {
        // Não existe caminho de S até T no grafo dirigido.
        log.Linha($"[P1] Menor custo  S={sIdx + 1} T={tIdx + 1} -> INALCANÇÁVEL  tempo={sw1.Elapsed.TotalMilliseconds:F1}ms");
    }
    else
    {
        // Converte o caminho de índices base 0 para base 1 e junta com "->".
        List<string> partesCam = new List<string>();
        foreach (int v in cam)
            partesCam.Add((v + 1).ToString());
        string path = string.Join("->", partesCam);
        log.Linha($"[P1] Menor custo  S={sIdx + 1} T={tIdx + 1} -> {path}  custo={custoDij}  tempo={sw1.Elapsed.TotalMilliseconds:F1}ms");
    }

    // ── P2: Fluxo máximo (Edmonds-Karp) + corte mínimo — grafo DIRIGIDO ──
    Stopwatch sw2 = Stopwatch.StartNew();
    (double fluxo, List<(int u, int v)> corte) = FluxoMaximo.Executar(g, sIdx, tIdx);
    sw2.Stop();

    // Formata as arestas do corte mínimo como "(u,v)" em base 1.
    List<string> partesCorte = new List<string>();
    foreach ((int u, int v) in corte)
        partesCorte.Add("(" + (u + 1) + "," + (v + 1) + ")");
    string corteStr = string.Join(", ", partesCorte);
    log.Linha($"[P2] Fluxo máximo  S={sIdx + 1} T={tIdx + 1} -> fluxo={fluxo}  corte={{{corteStr}}}  tempo={sw2.Elapsed.TotalMilliseconds:F1}ms");

    // ── P3: Árvore Geradora Mínima (Kruskal) — grafo tratado como NÃO-DIRIGIDO ──
    Stopwatch sw3 = Stopwatch.StartNew();
    (double custoAgm, List<(int u, int v, double peso)> arestas) = ArvoreGeradoraMinima.Executar(g);
    sw3.Stop();

    List<string> partesAgm = new List<string>();
    foreach ((int u, int v, double peso) in arestas)
        partesAgm.Add("(" + (u + 1) + "-" + (v + 1) + " p=" + peso + ")");
    string agmStr = string.Join(", ", partesAgm);
    log.Linha($"[P3] AGM Kruskal (não-dir.) -> custo={custoAgm}  [{agmStr}]  tempo={sw3.Elapsed.TotalMilliseconds:F1}ms");

    // ── P4: Coloração de arestas (Welsh-Powell via grafo linha) ──
    // Cada aresta do grafo original vira um vértice no "grafo linha".
    // Duas arestas conflitam se compartilham um vértice → precisam de turnos (cores) diferentes.
    Stopwatch sw4 = Stopwatch.StartNew();

    // Guarda todas as arestas numa lista indexada — usada depois para exibir os primeiros resultados.
    List<Aresta> todasArestas = new List<Aresta>();
    foreach (Aresta a in g.TodasArestas())
        todasArestas.Add(a);

    (int numCores, Dictionary<int, int> rotaCor) = Coloracao.Executar(g);
    sw4.Stop();

    // Exibe no máximo 10 alocações de turno para não poluir o log em grafos grandes.
    List<string> partesAloc = new List<string>();
    int contagem = 0;
    foreach (KeyValuePair<int, int> kv in rotaCor)
    {
        if (contagem >= 10) break;
        Aresta ar = todasArestas[kv.Key]; // kv.Key = índice da aresta no grafo linha
        partesAloc.Add("rota(" + (ar.Origem + 1) + "->" + (ar.Destino + 1) + ")=T" + (kv.Value + 1));
        contagem++;
    }
    string alocStr = string.Join(", ", partesAloc);
    if (rotaCor.Count > 10)
        alocStr += ", ...+" + (rotaCor.Count - 10) + " mais";
    log.Linha($"[P4] Coloração Welsh-Powell -> turnos={numCores}  [{alocStr}]  tempo={sw4.Elapsed.TotalMilliseconds:F1}ms");

    // ── P5A: Circuito Euleriano (Hierholzer) — grafo DIRIGIDO ──
    Stopwatch sw5a = Stopwatch.StartNew();
    (bool existeEul, string motivo, List<int> circuito) = Euleriano.Executar(g);
    sw5a.Stop();

    if (!existeEul)
    {
        log.Linha($"[P5A] Euleriano (dir.) -> NÃO EXISTE ({motivo})  tempo={sw5a.Elapsed.TotalMilliseconds:F1}ms");
    }
    else
    {
        // Trunca a exibição a 20 vértices para grafos com circuitos muito longos.
        List<string> partesEul = new List<string>();
        int limEul = Math.Min(circuito.Count, 20);
        for (int i = 0; i < limEul; i++)
            partesEul.Add((circuito[i] + 1).ToString());
        string seq = string.Join("->", partesEul);
        if (circuito.Count > 20)
            seq += "->...(" + circuito.Count + " total)";
        log.Linha($"[P5A] Euleriano (dir.) -> EXISTE: {seq}  tempo={sw5a.Elapsed.TotalMilliseconds:F1}ms");
    }

    // ── P5B: Ciclo Hamiltoniano (backtracking com timeout) — grafo NÃO-DIRIGIDO ──
    // bool? existeHam: true = existe, false = não existe, null = timeout (não conclusivo).
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

    log.Linha(); // linha em branco ao final de cada bloco de grafo
}
