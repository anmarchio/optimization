using Optimization.Data;

namespace Optimization.CVPipeline
{
    public class CVDataLoader : DataLoader<CVReferenceImage>
    {
        public CVDataLoader(DataSet<CVReferenceImage> dataset): base(dataset)
        {

        }

        public CVDataLoader(DataSet<CVReferenceImage> dataset, int batchSize) : base(dataset, batchSize)
        {

        }

    }
}
