namespace Optimization.Serialization
{
    public static class SQLQueryStrings
    {
        public static string UnfinishedBatchRuns { get; } =
            "SELECT * FROM optimization_batchrun WHERE state='ready' ORDER BY date(date_created)";

        public static string BatchDataImageIDs(string batchDataPK)
        {
           return "Select * FROM optimization_batchdata_images WHERE batchdata_id="+batchDataPK;
        }

        public static string Image(string imagePK)
        {
            return "Select * FROM optimization_image WHERE id=" + imagePK;
        }

        public static string RegionsForImage(string imagePK)
        {
            return "Select * FROM optimization_region WHERE image_id=" + imagePK;
        }

        public static string CGPConfiguration(string cgpConfigPK)
        {
            return "SELECT * FROM optimization_cgpconfiguration WHERE id=" + cgpConfigPK;
        }

        public static string BatchRun(string batchRunPK)
        {
            return "SELECT * FROM optimization_batchrun WHERE id=" + batchRunPK;
        }

        public static string BatchData(string batchDataPK)
        {
            return "SELECT * FROM optimization_batchdata WHERE id=" + batchDataPK;
        }

        public static string WriteBatchRunResult(string iteration, string batchRunPK, string trainFitness, string valFitness)
        {
            return "UPDATE optimization_batchrunresult SET train_best_fitness=" + trainFitness + ""
                + ", val_best_fitness=" + valFitness + " WHERE batch_run_id=" + batchRunPK + " AND iteration=" + iteration;
        }

        public static string UpdateBatchRunState(string state, string id)
        {
           return "UPDATE optimization_batchrun SET state='" + state + "' WHERE id=" + id;
        }

        public static string UpdateBatchRunProgress(string progressPercentage, string id)
        {
            return "UPDATE optimization_batchrun SET progress=" + progressPercentage + " WHERE id=" + id;
        }
    }
}
