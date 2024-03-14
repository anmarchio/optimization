namespace Optimization.EvolutionStrategy.Interfaces
{

    /// <summary>
    /// Interface to check whether some solution candidate passes a validity check. Not to be confused with train/validate/test split. For the latter, see IValidator.
    /// </summary>
    public interface IValidityTester
    {
        bool IsValid(IIndividual individual);
    }
}
