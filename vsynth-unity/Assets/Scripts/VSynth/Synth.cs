using System.Collections.Generic;

public class Synthesizer {
    public Envs envs;
    public Generator generator;
    public Searcher searcher;
    public int max_complexity;

    public Synthesizer(List<Example> exs, int max_r, int max_c) {
        envs = new(exs);
        generator = new Generator(envs);
        searcher = new Searcher(envs, max_r);
        max_complexity = max_c;
    }

    public List<ResultBuffer> Run() {
        generator.GenRows(envs, max_complexity);
        return searcher.FindAll(envs, generator);
    }
}