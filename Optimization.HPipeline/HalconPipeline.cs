using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Extensions;
using HalconDotNet;
using Optimization.CartesianGeneticProgramming;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.HalconPipeline;
using Optimization.HalconPipeline.Interfaces;

namespace Optimization.HPipeline
{
    [Serializable]
    public class HalconPipeline : CGPPipeline<HObject, HObject, HalconOperatorNode, HalconInputNode, HalconOperatorNode, object>
    {

        public HalconPipeline() : base()
        {
        }

        public HalconPipeline(params HalconOperatorNode[] outputNodes) : base(outputNodes)
        {
        }

        public HalconPipeline(FloatVector vector, CGPConfiguration configuration) : base(vector, configuration)
        {
        }
        
        /// <summary>
        /// author: leen
        /// Invokes a shallow copy of a pipeline;
        /// After creating a copy the original is safe to be modified
        /// </summary>
        /// <returns></returns>
        public override ICopyable Copy()
        {
            var tmp = base.Copy() as Pipeline<HObject, HObject, HalconOperatorNode, HalconInputNode, HalconOperatorNode, object>;
            return new HalconPipeline(tmp.OutputNodes.ToArray());
        }

        public override void SerializeXml(string filename)
        {
            using (var writer = new StreamWriter(filename))
            {
                base.SerializeXml();
                var xml = new XmlSerializer(typeof(HalconPipeline));
                xml.Serialize(writer, this);
            }
        }
 

        public static HalconPipeline DeserializeXml(string filename)
        {
            using (var reader = new StreamReader(filename))
            {
                var xml = new XmlSerializer(typeof(HalconPipeline));
                var pipe = xml.Deserialize(reader) as HalconPipeline;
                pipe.DeserializeXml();
                return pipe;
            }
        }

        public override bool SerializeXmlSupported
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// Executes Pipeline with given inputs and logs outputs in an image format to directory.
        /// Waring: This execution frees all previously executed and reserved outputs.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="directory"></param>
        public void WriteOutputs(HObject input, string directory)
        {
            
            if (!AllNodesAreUniquelyIdentified()) InitializeNodeIDs();
            directory.CreateDirectory();
            input.WriteObject(Path.Combine(directory, "input.jpg"));
            var disposeTmp = AutoDisposeIntermediateOutputs;
            AutoDisposeIntermediateOutputs = false;
            base.ExecuteSingle(input);
            foreach (var node in Nodes)
            {
                var name = node.GetType().Name + node.NodeID;
                if (!node.Output.IsImage())
                {
                    input.Dump(Path.Combine(directory, name), node.Output);
                }
                else
                {
                    var tmp = node.Output.ConvertToStandardType();
                    tmp.Dump(Path.Combine(directory, name));
                }
            }
            AutoDisposeIntermediateOutputs = disposeTmp;
        }

        //Experimental Code for import from hdev file
        /*
        /// <summary>
        /// Produces a list of used/indicated operators in a hdev file to construct a pipeline
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> ReadFromHalconFile(String path)
        {
            List<string> results = new List<string>();

            using(var reader = new StreamReader(path + ".hdev"))
            {
                var line = reader.ReadLine();
                while (!(line == null))
                {
                    if (line.StartsWith("<l>#"))
                    {
                        var subLines = line.Split('#');
                        results.Add(subLines[1].Split('<')[0]);
                    }
                    line = reader.ReadLine();
                }
            }
            return results;
        }
        */

        /// <summary>
        /// Produces a hdev file, by converting each opeartor functionality of a pipiline into executable halcon code
        /// </summary>
        /// <param name="path"></param>
        /// <param name="inputImagePath"></param>
        public List<string> WriteToHalconFile(string path, string inputImagePath=null, string outputImagePath=null)
        {
            var dir = Path.Combine(path, Name);
            List<string> results = new List<string>();

            //Prep for order of nodes, if there are 2 inputs required
            if (!AllNodesAreUniquelyIdentified()) InitializeNodeIDs();
            for(int i = 0; i <InputNodes.Count; i++)
            {
                var leaf = InputNodes[i];
                if(!(leaf is HalconInputNode))
                {
                    var input = new HalconInputNode() { ProgramInputIdentifier = -1 - i, NodeID = -1 - i };
                    leaf.AddChild(input);
                    InputNodes.Remove(leaf);
                    InputNodes.Insert(i, input);
                }
            } 
            using (var writer = new StreamWriter(path + ".hdev"))
            {
                //string fileContent = "";
                //header
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<hdevelop file_version=\"1.1\" halcon_version=\"13.0\">");
                writer.WriteLine("<procedure name=\"main\">");
                writer.WriteLine("<interface/>");
                writer.WriteLine("<body>");

                //read image input
                //writer.WriteLine($"<l>read_image (Image, {inputImagePath.ToString()})</l>");
                
                var layers = Layers.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

                foreach(var input in InputNodes)
                {
                    //foreach(string line in input.GetFormattedHalconFunctionCall(inputImagePath.ToString()))
                    //{
                        //read image input
                        writer.WriteLine("<l>" + input.GetFormattedHalconFunctionCall(inputImagePath.ToString()) + "</l>");
                    //}
                    //fileContent += "<l>" + input.HalconFunctionCall + "</l>" + System.Environment.NewLine;
                }

                foreach (var layer in layers)
                {
                    var nodes = layer.Value;
                    foreach(var node in nodes)
                    {
                        foreach (var line in node.HalconFunctionCall())
                        {
                            writer.WriteLine("<l>" + line + "</l>");
                        }
                        //fileContent += "<l>" + node.HalconFunctionCall + "</l>" + System.Environment.NewLine;
                    }
                }

                //footer
                dir.CreateDirectory();
                foreach(var output in OutputNodes)
                {
                    writer.WriteLine(string.Format("<l>write_object ({0}, '{1}')</l>", output.OutputVariableName, 
                        Path.Combine(dir, output.OutputVariableName).Replace($@"\", $@"/")));
                    //results.Add($"{Path.Combine(dir, output.OutputVariableName).Replace($@"\", $@"/")}");
                    results.Add(Path.Combine(dir, output.OutputVariableName));
                }

                //Catch all possible exceptions like missing paths
                //writer.WriteLine("<l>catch (Exception)</l>");
                //writer.WriteLine("<l>throw ([Exception, 'unknown exception in myproc']</l>");
                //writer.WriteLine("<l>endtry</l>");

                //Termination and freeing ressources
                writer.WriteLine("<l>exit()</l>");
                writer.WriteLine("</body>");
                writer.WriteLine("<docu id=\"main\">");
                writer.WriteLine("<parameters/>");
                writer.WriteLine("</docu>");
                writer.WriteLine("</procedure>");
                writer.WriteLine("</hdevelop>");
            }
            return results;
        }
    }
}
