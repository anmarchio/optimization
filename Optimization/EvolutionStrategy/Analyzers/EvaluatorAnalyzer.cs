using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Optimization.EvolutionStrategy.Evaluators;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Analyzers
{
    public class EvaluatorAnalyzer<TData> : Analyzer
    {
        private Dictionary<int, List<object>> LoaderEvaluationObjects { get; } = new Dictionary<int, List<object>>();
        private Dictionary<int, List<object>> IndividualEvaluationObjects { get; } = new Dictionary<int, List<object>>();

        private int? CurrentGeneration { get; set; }

        private void LogLoaderEvaluation(object sender, Evaluator<TData>.EvaluationEventArgs e)
        {
            LoaderEvaluationObjects[CurrentGeneration ?? -1]
                = e.GetObjectForJson();
        }

        private void LogIndividualEvaluation(object sender, Evaluator<TData>.IndividualEvaluationEventArgs e)
        {
            if(!IndividualEvaluationObjects.ContainsKey(CurrentGeneration ?? -1))
                IndividualEvaluationObjects[CurrentGeneration ?? -1] = new List<object>();
            IndividualEvaluationObjects[CurrentGeneration ?? -1].Add(e.GetObjectForJson());
        }

        public Evaluator<TData> Evaluator { get; set; }

        public override void Analyze(EvolutionStrategy evolutionStrategy)
        {
            if (Evaluator == null)
            {
                var tmp = evolutionStrategy.Evaluator as Evaluator<TData>;
                if (tmp == null) return;
                Evaluator = tmp;

                Evaluator.IndividualEvaluationCompleted += LogIndividualEvaluation;
                Evaluator.LoaderEvaluationCompleted += LogLoaderEvaluation;
            }

            CurrentGeneration = evolutionStrategy.CurrentGeneration;

            if (evolutionStrategy.Terminator.Terminate(evolutionStrategy))
            {
                Evaluator.IndividualEvaluationCompleted -= LogIndividualEvaluation;
                Evaluator.LoaderEvaluationCompleted -= LogLoaderEvaluation;
            }
        }

        public override void Save(string directory)
        {
            using (var writer = new StreamWriter(Path.Combine(directory, "loader_evaluation_log.json")))
            {
                writer.WriteLine(JsonConvert.SerializeObject(LoaderEvaluationObjects, Formatting.Indented));
            }

            using (var writer = new StreamWriter(Path.Combine(directory, "individual_evaluation_log.json")))
            {
                writer.WriteLine(JsonConvert.SerializeObject(IndividualEvaluationObjects, Formatting.Indented));
            }
        }

        public override ICopyable Copy()
        {
            return new EvaluatorAnalyzer<TData>();
        }
    }
}
