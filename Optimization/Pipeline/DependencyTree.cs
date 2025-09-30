using System;
using System.Collections.Generic;
using System.Linq;
using Optimization.HalconPipeline.Interfaces;

namespace Optimization.HalconPipeline
{
    [Serializable]
    public class DependencyTree : Pipeline<object, object, DependencyNode, DependencyNode, DependencyNode, object>
    {
        public DependencyTree(params DependencyNode[] outputNodes) : base(outputNodes)
        {

        }

        public override void Remove(DependencyNode node)
        {
            base.Remove(node);
            foreach (var remaining in Nodes)
            {                
                if (remaining.AndDependencies == null) continue;
                foreach (var dependency in remaining.AndDependencies)
                {
                    foreach (var dep in dependency)
                    {
                        if (!remaining.Children.Any(x => x.IsOrOperatorType(dep.OperatorType)))
                        {
                            /*throw new Exception(string.Format("Removed node with operatorType {0} from dependencytree"
                                                     + "in such a way that it removed an and dependency from node with operatortype {1}."
                                                     + "this usually means that you passed an operatorset to the operatormap that includes a node of type {1} while not providing the required type {0}",
                                                     node.OperatorType, remaining.OperatorType));*/
                        }
                    }
                }
            }
        }

        public override Dictionary<int, List<DependencyNode>> Layers
        {
            get
            {
                
                var layers = new Dictionary<int, List<DependencyNode>>();
                foreach (var node in TraverseBreadthBackward())
                {
                    if (node.IsInputNode) continue;
                    //var adjustedHeight = node.Height - 1;
                    var adjustedHeight = node.Height;
                    if (!layers.ContainsKey(adjustedHeight)) layers.Add(adjustedHeight, new List<DependencyNode>());
                    layers[adjustedHeight].Add(node);
                }
                
                return layers;

            }
        }

        public static IEnumerable<DependencyTree> DependencyCollection
        {
            get
            {
                return new List<DependencyTree>()
                {
                    GetSimpleDependencies(),
                    GetUnlimitedDependencies(),
                    MultipleImagesInput(),
                };
            }
        }


        /// <summary>
        /// Technically there are cycles in the dependency graph. However, we unroll it to avoid extreme overhead.
        /// </summary>
        /// <returns></returns>
        
            
            /*
        public static DependencyTree GetBasicDependencies()
        {
            var imageInput = new DependencyInputNode(-1);
            var img2img = new DependencyNode(OperatorType.ImageToImage);
            var img2r = new DependencyNode(OperatorType.ImageToRegion);
            var r2r_first = new DependencyNode(OperatorType.RegionToRegion);
            var r2r_end = new DependencyNode(OperatorType.RegionToRegion);

            r2r_end.AddChild(r2r_first);
            r2r_end.AddChild(img2r);

            r2r_first.AddChild(img2r);

            img2r.AddChild(img2img);
            img2r.AddChild(imageInput);

            img2img.AddChild(imageInput);
            img2img.AddChild(img2img);

            var p = new DependencyTree(r2r_end, img2r);

            return p;
        }
        */

        public static DependencyTree GetSimpleDependencies()
        {
            var r2r_first = new DependencyNode(OperatorType.RegionToRegion);
            var r2r_end = new DependencyNode(OperatorType.RegionToRegion);
            var imgAr2r = new DependencyNode(OperatorType.ImageAndRegionToRegion);
            var edgeAr2r = new DependencyNode(OperatorType.EdgeAmpAndRegionToRegion);
            var img2r = new DependencyNode(OperatorType.ImageToRegion);
            var img2img = new DependencyNode(OperatorType.ImageToImage);
            var edge = new DependencyNode(OperatorType.EdgeAmplitude);
            var imageInput = new DependencyInputNode(-1); // for standard case with only one input image

            r2r_end.AddChild(imgAr2r);
            r2r_end.AddChild(img2r);
            //r2r_end.AddChild(r2r_first);

            r2r_first.AddChild(img2r);

            edge.AddChild(img2img);
            
            //Maybe modify in such a way, that it allows graphs without all OperatorTypes
            // -> No exception for missing operatortypes in parameters for definied operatormap
            
            imgAr2r.AddAndDependencies(new List<DependencyNode> { r2r_first, img2r },
                                        new List<DependencyNode>() { img2img });
                                        
            edgeAr2r.AddAndDependencies(new List<DependencyNode> { edge },
                           new List<DependencyNode> { r2r_first, img2r });
                         
            img2r.AddChild(img2img);
            img2r.AddChild(edge);
            r2r_end.AddChild(edgeAr2r);
            r2r_end.AddChild(r2r_first);

            img2r.AddChild(imageInput);
            img2img.AddChild(imageInput);
            //img2img.AddChild(img2img);
            edge.AddChild(imageInput);

            var p = new DependencyTree(//new List<DependencyNode>() { imageInput }, // input nodes
                                         r2r_end, img2r/*, img2img*/ ); // output nodes
            return p;
        }

        /// <summary>
        /// Dependency graphs with no limitiations, allows all possible combinations.
        /// </summary>
        /// <returns></returns>
        public static DependencyTree GetUnlimitedDependencies()
        {
            /*
             * This dependency graph provides maximum freedom,
             * but keeps a layered DAG structure to avoid cycles.
             */

            var r2r_first = new DependencyNode(OperatorType.RegionToRegion);
            var r2r_end = new DependencyNode(OperatorType.RegionToRegion);
            // for potentially invalid combinations
            // r2r_center -> img2r -> img2img
            // r2r_center -> img2img
            var r2r_center = new DependencyNode(OperatorType.RegionToRegion);
            var imgAr2r = new DependencyNode(OperatorType.ImageAndRegionToRegion);
            var edgeAr2r = new DependencyNode(OperatorType.EdgeAmpAndRegionToRegion);
            var img2r = new DependencyNode(OperatorType.ImageToRegion);
            var img2img = new DependencyNode(OperatorType.ImageToImage);
            var edge = new DependencyNode(OperatorType.EdgeAmplitude);
            var imageInput = new DependencyInputNode(-1); // for standard case with only one input image

            r2r_end.AddChild(img2r);
            r2r_end.AddChild(imgAr2r);
            //r2r_end.AddChild(r2r_first);

            r2r_first.AddChild(img2r);

            edge.AddChild(img2img);

            //Maybe modify in such a way, that it allows graphs without all OperatorTypes
            // -> No exception for missing operatortypes in parameters for definied operatormap

            imgAr2r.AddAndDependencies(new List<DependencyNode> { r2r_first, img2r },
                                        new List<DependencyNode>() { img2img });

            edgeAr2r.AddAndDependencies(new List<DependencyNode> { edge },
                           new List<DependencyNode> { r2r_first, img2r });

            img2r.AddChild(img2img);
            img2r.AddChild(edge);
            r2r_end.AddChild(edgeAr2r);
            r2r_end.AddChild(r2r_first);

            img2r.AddChild(imageInput);
            img2img.AddChild(imageInput);
            //img2img.AddChild(img2img);
            edge.AddChild(imageInput);

            // The following extensions will definitely not work,
            // but has to be added for "free" combinations
            // that also can lead to failing pipelines
            r2r_first.AddChild(imageInput);
            r2r_end.AddChild(imageInput);
            //img2img -> r2r -> input
            img2img.AddChild(r2r_center);
            r2r_center.AddChild(imageInput);
            //img2r -> r2r -> input
            img2r.AddChild(r2r_center);
            //edge -> r2r -> input
            // edge -> input
            edge.AddChild(r2r_center);
            // edgeamp -> input
            // edgeamp -> r2r
            edgeAr2r.AddChild(r2r_center);

            var p = new DependencyTree(//new List<DependencyNode>() { imageInput }, // input nodes
                                         r2r_end, img2r, img2img, edge, edgeAr2r); // output nodes
            return p;
        }

        public static DependencyTree RegionsOnly()
        {
            var r2r = new DependencyNode(OperatorType.RegionToRegion);
            var regionInput = new DependencyInputNode(-1);
            r2r.AddChild(regionInput);

            var p = new DependencyTree(regionInput, r2r );
            return p;
        }

        public static DependencyTree MultipleImagesInput()
        {
            var r2r_first = new DependencyNode(OperatorType.RegionToRegion);
            var r2r_end = new DependencyNode(OperatorType.RegionToRegion);
            var imgAr2r = new DependencyNode(OperatorType.ImageAndRegionToRegion);
            var edgeAr2r = new DependencyNode(OperatorType.EdgeAmpAndRegionToRegion);
            var img2r = new DependencyNode(OperatorType.ImageToRegion);
            var img2img = new DependencyNode(OperatorType.ImageToImage);
            var edge = new DependencyNode(OperatorType.EdgeAmplitude);
            var imageInput1 = new DependencyInputNode(-1); // for standard case with only one input image
            var imageInput2 = new DependencyInputNode(-2); // for non standard case with more than one input image

            r2r_end.AddChild(imgAr2r);
            r2r_end.AddChild(img2r);

            r2r_first.AddChild(img2r);

            edge.AddChild(img2img);

            imgAr2r.AddAndDependencies(new List<DependencyNode> { r2r_first, img2r },
                                        new List<DependencyNode>() { img2img });

            edgeAr2r.AddAndDependencies(new List<DependencyNode> { edge },
                           new List<DependencyNode> { r2r_first, img2r });
            img2r.AddChild(img2img);
            img2r.AddChild(edge);
            r2r_end.AddChild(edgeAr2r);
            r2r_end.AddChild(r2r_first);

            img2r.AddChild(imageInput1);
            img2img.AddChild(imageInput1);

            edge.AddChild(imageInput2);

            var p = new DependencyTree(/*new List<DependencyNode>() { imageInput1, imageInput2 }, // input nodes
                                        new List<DependencyNode>() { */r2r_end, img2r ); // output nodes
            if (p.Nodes.Count != 9) throw new Exception("Not using all nodes defined above");
            return p;
        }

        /// <summary>
        /// This simple dependency tree corresponds to the deprecated filter- threshold - morphological architecture
        /// </summary>
        /// <returns></returns>
        public static DependencyTree DeprecatedFilterThresholdMorphologicalArchitectureDependencyTree()
        {
            var r2r = new DependencyNode(OperatorType.RegionToRegion);
            var img2r = new DependencyNode(OperatorType.ImageToRegion);
            var img2img = new DependencyNode(OperatorType.ImageToImage);
            var imageInput = new DependencyInputNode(-1);  // for standard case with one input image

            r2r.AddChild(img2r);
            img2r.AddChild(img2img);

            img2img.AddChild(imageInput);
            img2r.AddChild(imageInput);

            var p = new DependencyTree(/*new List<DependencyNode>() { imageInput }, new List<DependencyNode>() { */r2r );
            return p;
        }

        public static DependencyTree ImagesOnly()
        {
            var im2im = new DependencyNode(OperatorType.ImageToImage);
            var imageinput = new DependencyInputNode(-1);
            im2im.AddChild(imageinput);

            var p = new DependencyTree(im2im);
            return p;
        }

        public static DependencyTree MultipleImageTypesToRegion()
        {
            var imageinput = new DependencyInputNode(-1);

            // im2im stellt die Beziehungen von ColImg nach GrayImg und andersrum dar(inklusive complexImg)
            // Schleife zu sich selbst schon gedacht
            var im2im = new DependencyNode(OperatorType.ImageToImage);
            im2im.AddChild(imageinput);                       

            //oder OrientationImg als Region welche noch weiterführend unbekannt ist
            var im2orient = new DependencyNode(OperatorType.ImageToRegion);
            im2orient.AddChild(im2im);

            //oder MagnitudeImg als "Vor"-Region, also vor Binary
            //Also eher noch als Image interpretieren
            var im2magn = new DependencyNode(OperatorType.ImageToImage);

            // im2r stellt die Beziehungen zu BinaryImage dar
            var im2r = new DependencyNode(OperatorType.ImageToRegion);
            im2r.AddChild(im2im);

            //r2r um Binary in Morphs &| Regions zu überführen
            var r2r = new DependencyNode(OperatorType.RegionToRegion);
            r2r.AddChild(im2r);

            
            var p = new DependencyTree(r2r);
            return p;
        }

        public int EstimateCgpColumnCount()
        {
            return this.Nodes.Select(x => !x.IsInputNode).Count();
        }
    }
}
