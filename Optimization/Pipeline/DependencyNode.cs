using System;
using System.Collections.Generic;
using Optimization.Pipeline.Interfaces;

namespace Optimization.Pipeline
{
    [Serializable]
    public class DependencyNode : Node, IOutputNode<object>
    {
        public DependencyNode(OperatorType opType) : base()
        {
            operatorType = opType;
        }

        /*
        public DependencyNode(OperatorType opType, params List<DependencyNode>[] andDependencies) : base()
        {
            operatorType = opType;
            foreach (var list in andDependencies)
                foreach (var child in list) AddChild(child);

            //AndDependencies = andDependencies;
        }*/

        //  removed this in favor of each operatornode explicity implementing information about which inputs it requires. this allows for more finegrained inputBounds
        // this dependencytree is only required to 
        /// <summary>
        /// And Dependencies are e.g. for ImageAndRegion2Region: they require an Input image, which may come from different dependencies (ImageToImage, or just the overall Input)
        /// and a Region which may come from RegionToRegion or ImageToRegion. The array represents the AND dependencies, the individual lists in each array represent the OR entries.
        /// </summary>
        /// <param name="andDependences"></param>
        public void AddAndDependencies(params List<DependencyNode>[] andDependencies)
        {
            foreach (var list in andDependencies)
                foreach (var child in list) AddChild(child);

            AndDependencies = andDependencies;

        }

        public List<DependencyNode>[] AndDependencies { get; set; } = null;

          
        private OperatorType operatorType;
        public override OperatorType OperatorType
        {
            get { return operatorType; }
        }
        
        public object Output
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string LabelToDot()
        {
            var label =  base.LabelToDot();
            label = label.Replace("DependencyNode", this.OperatorType.ToString());
            return label;
        }

        public override string ToString()
        {
            return OperatorType.ToString() + " " + base.ToString();
        }

      
        public object Execute()
        {
            throw new NotImplementedException("The dependency tree is not supposed to be executed.");
        }

        public object Execute(object input)
        {
            throw new NotImplementedException("The dependency tree is not supposed to be executed.");
        }

        public void ResetOutput()
        {
            throw new NotImplementedException();
        }
    }
}
