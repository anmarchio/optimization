namespace Optimization.Fitness
{  
    /// <summary>
    /// All implemented fitness functions. FScore, Recall and Precision are highly related; MCC (matthew's correlation coefficient) and 
    /// J (Youden's J index) are considered superior (yet seem to produce poorer results)
    /// AUC refers to area under the ROC (receiver operator characteristics) curve
    /// </summary>
    public enum FitnessFunction
    {
        //note that F1Score, F05Score and F2Score are not directly implemented but instead use FBetaScore and provide the FitnessConfiguration with the BetaSquare
        //the named functions are not listed in the dictionary within the FitnessEvaluator Initialize() method! (this raises an exception!)
        FBetaScore, MCC, Recall, Precision, J, RegionScoreLb, MCC_savezones, Specificity, IntersectionOverUnion,
        Accuracy,
        Sensitivity,
    }
    
    public enum RegionScore
    {
        RegionScore, 
    } 

    public enum ArtifactScore
    {
        Percentage
    }
}
