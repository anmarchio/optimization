namespace Optimization.Fitness.Interfaces
{
    public interface IConfusionMatrix
    {
        int TP { get; set; }
        int TN { get; set; }
        int FP { get; set; }
        int FN { get; set; }
        int ActualNegatives { get; set; }
        int ActualPositives { get; set; }
        int ReferenceNegatives { get; set; }
        int ReferencePositives { get; set; }

        string[,] Print();
    }
}
