using System.ComponentModel;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Terminators
{
    public class BackgroundWorkerTerminator : ITerminator
    {
        private BackgroundWorker Worker { get; set; }
        /// <summary>
        /// Checks if the executing BackgroundWorker has a pending cancellation request and terminates accordingly.
        /// </summary>
        /// <param name="evolutionStrategy"></param>
        /// <returns></returns>
        public BackgroundWorkerTerminator(BackgroundWorker worker)
        {
            Worker = worker;
        }

        public bool Terminate(EvolutionStrategy evolutionStrategy)
        {
            if (Worker.CancellationPending) return true;
            return false;
        }
    }
}
