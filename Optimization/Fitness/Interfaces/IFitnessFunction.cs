namespace Optimization.Fitness.Interfaces
{
    public interface IFitnessFunction
    {
        double MCC();
        double F2Score();
        double F1Score();
        double F05Score();
        double J();
        double Recall();
        double Precision();

        double FPR();
        double TPR();

    }
}
