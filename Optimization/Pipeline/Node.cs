using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Extensions;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.HalconPipeline.Interfaces;

namespace Optimization.HalconPipeline
{
    #region XmlInclude Attributes required to write Xml, for halcon and cv nodes see respective subclasses
    #endregion
    [Serializable]
    public abstract class Node : INode, ICopyable, ICodeProducer
    {
        public Node()
        {
        }

        public Node(Node child)
        {
            Children.Add(child);
        }

        public Node(params Node[] children)
        {
            if (children != null)
            {
                Children = children.ToList();
                foreach (var child in Children) child.AddParent(this);
            }
        }

        public Node(List<Node> children)
        {
            if(children != null)
            {
                Children = children;
                foreach (var child in Children) child.AddParent(this);
            }
        }


        [XmlIgnore]
        public List<Node> Children { get; set; } = new List<Node>();

        [XmlIgnore]
        public List<Node> Parents { get; set; } = new List<Node>();

        /// <summary>
        /// Attempts to insert node into the list of Parents and also add this node to node's Children.
        /// </summary>
        /// <param name="node"></param>
        public void AddParent(Node node)
        {
            if (!node.Children.Contains(this))
                node.Children.Add(this);

            if (!Parents.Contains(node))
                Parents.Add(node);
        }
        /// <summary>
        /// Attempts to insert node into the list of Children and also add this node to node's Parents.
        /// </summary>
        /// <param name="node"></param>
        public void AddChild(Node node)
        {
            if (!node.Parents.Contains(this))
                node.Parents.Add(this);
            if (!Children.Contains(node))
                Children.Add(node);
        }


        /// <summary>
        /// Removes all references to children and parents, effectively removing the node from any graph it might be part of
        /// warning: might result in the graph becoming disconnected
        /// </summary>
        public void RemoveAll()
        {
            for(int i = Children.Count; i > 0; i--)
            {
                RemoveChild(Children.Last());
            }
            for(int i = Parents.Count; i > 0; i--)
            {
                RemoveParent(Parents.Last());
            }
        }

        public void RemoveChild(Node node)
        {
            if (Children.Contains(node)) Children.Remove(node);
            if (node.Parents.Contains(this)) node.Parents.Remove(this);
        }

        public void RemoveParent(Node node)
        {
            if (Parents.Contains(node)) Parents.Remove(node);
            if (node.Children.Contains(this)) node.Children.Remove(this);
        }

        public bool HasParents()
        {
            return Parents.Count > 0;
        }

        public bool HasChildren()
        {
            return Children.Count > 0;
        }

        public bool IsLeaf()
        {
            return !HasChildren();
        }

        #region INode Interface


        /// <summary>
        /// returns a dictionary from the distance (0 is the node itself, 1 are the children etc.)
        /// including distance. if distance == -1 returns all descendants (until leaf nodes)
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public Dictionary<int, List<Node>> GetDescendants(int distance = -1)
        {
            var ret = new Dictionary<int, List<Node>>();
            ret.Add(0, new List<Node>() { this });

            for (int i = 1; distance == -1 ? true : i <= distance; i++)
            {
                var descend = new List<Node>();
                foreach (var node in ret[i - 1])
                {
                    descend = descend.Union(node.Children).ToList();
                }
                if (descend.Count != 0) ret.Add(i, descend);
                if (distance != -1 && i >= distance) return ret;
                if (descend.Count == 0) return ret;
            }
            return ret;
        }

        public List<Node> GetLeafNodes()
        {
            var stack = new Stack<Node>();
            stack.Push(this);

            var leaves = new List<Node>();

            while (stack.Count != 0)
            {
                var current = stack.Pop();
                foreach (var child in current.Children)
                {
                    if (!stack.Contains(child)) stack.Push(child);
                    if (child.IsLeaf() && !leaves.Contains(child)) leaves.Add(child);
                }
            }
            return leaves;
        }


        /// <summary>
        /// Sometimes it's easier to assign a unique number to each node... if this is important, assign numbers first
        /// </summary>
        public virtual float NodeID { get; set; }


        /// <summary>
        /// The maximum distance to a leaf node
        /// </summary>
        public int Height
        {
            get
            {
                if (Children.All(x => x.IsInputNode)) return 0; // this ignores InputNodes if they are used to keep  behavior consistent across all types of pipelines
                if (IsLeaf()) return 0;
                return Children.Max(x => x.Height) + 1;
            }
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/DOT_(graph_description_language)
        /// </summary>
        /// <returns>a -> b; and a [label="text"]; information for all nodes</returns>
        public string ToDOTString()
        {
            string ret = LabelToDot();
            if (IsLeaf())
            {
                return LabelToDot();
            }
            else
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    ret += EdgeToDOT(i, "");
                }
                foreach (var child in Children)
                {
                    ret += child.ToDOTString();
                }
            }

            return ret;
        }

        public virtual string EdgeToDOT(int childIdx, string edgeLabel)
        {
            return NodeID.ToUnderscoredString() + " -> " + Children[childIdx].NodeID.ToUnderscoredString() + "[label=\"" + edgeLabel + "\"];\n";
        }

        public virtual string LabelToDot()
        {
            return NodeID.ToUnderscoredString() + " " + ToDOTNodeAttributes() + "\n";
        }

        public virtual string EdgesToDOT()
        {
            var ret = "";
            foreach (var child in Children)
            {
                ret += NodeID.ToUnderscoredString() + " -> " + child.NodeID.ToUnderscoredString() + " " + EdgeLabelToDOT(child.NodeID) + ";\n";
            }
            return ret;
        }

        /// <summary>
        /// return string containing label information in [];
        /// </summary>
        /// <returns></returns>
        public virtual string EdgeLabelToDOT(float childId)
        {
            return "[]";
        }

        public abstract OperatorType OperatorType
        {
            get;
        }


        /// <summary>
        /// Checks if this nodes operator type flags include operatorType
        /// </summary>
        /// <param name="operatorType"></param>
        /// <returns></returns>
        public bool IsOrOperatorType(OperatorType operatorType)
        {
            return (OperatorType & operatorType) == operatorType;
        }


        /// <summary>
        /// Checks if operatorType matches this Nodes operator type exactly, i.e. all flags are identical.
        /// </summary>
        /// <param name="operatorType"></param>
        /// <returns></returns>
        public bool IsAndOperatorType(OperatorType operatorType)
        {
            return (OperatorType & operatorType) == OperatorType;
        }

        public string ToDOTNodeAttributes()
        {
            var propInfo = GetType().GetProperties();
            var dotProperties = propInfo.Where(x => x.CanRead && x.CanWrite
                                        && (x.PropertyType.IsPrimitive || x.PropertyType.IsEnum)
                                        && (x.PropertyType.IsPublic || x.PropertyType.IsNestedPublic)
                                        && !x.Name.Equals("NodeID")).ToList();
            PropertyInfo prop;

            string ret = "[label=\"";
            ret += GetType().Name + @"\n";
            for (int i = 0; i < dotProperties.Count - 1; i++)
            {
                prop = dotProperties[i];
                ret += PropertyToString(prop);
                ret += @"\n ";

            }
            prop = dotProperties.LastOrDefault();
            if (prop != null)
                ret += PropertyToString(prop);

            return ret + "\"];";
        }

        private string PropertyToString(PropertyInfo prop)
        {
            return prop.Name + "=" + PropertyValueToString(prop);
        }

        private string PropertyValueToString(PropertyInfo prop)
        {
            if (prop.IsNumeric())
            {
                return prop.GetValue(this).ToInvariantString();
            }
            else
            {
                return prop.GetValue(this).ToString();
            }
        }

        public string PropertiesToString()
        {
            var propInfo = GetType().GetProperties();
            var dotProperties = propInfo.Where(x => x.CanRead && x.CanWrite
                                        && (x.PropertyType.IsPrimitive || x.PropertyType.IsEnum)
                                        && (x.PropertyType.IsPublic || x.PropertyType.IsNestedPublic)).ToList();
            return $"{string.Join(" ", dotProperties.Select(x => PropertyToString(x)))}";
        }

        public List<object> PropertiesToList()
        {
            var propertyList = new List<object>();
            var propInfo = GetType().GetProperties();
            var dotProperties = propInfo.Where(x => x.CanRead && x.CanWrite
                                                              && (x.PropertyType.IsPrimitive || x.PropertyType.IsEnum)
                                                              && (x.PropertyType.IsPublic || x.PropertyType.IsNestedPublic)).ToList();
            foreach (var prop in dotProperties)
            {
                if (prop.Name == "NodeID") continue;
                propertyList.Add(new {prop.Name, Value = PropertyValueToString(prop)});
            }

            return propertyList;
        }

        public Dictionary<string, string> PropertiesToDictionary()
        {
            var propInfo = GetType().GetProperties();
            var dotProperties = propInfo.Where(x => x.CanRead && x.CanWrite
                                        && (x.PropertyType.IsPrimitive || x.PropertyType.IsEnum)
                                        && (x.PropertyType.IsPublic || x.PropertyType.IsNestedPublic)).ToList();
            var dict = new Dictionary<string, string>();
            foreach (var x in dotProperties)
                dict.Add(x.Name, PropertyValueToString(x));
            return dict;
        }

        public ICopyable Copy()
        {
            var shallowCopy = this.MemberwiseClone() as Node;//Activator.CreateInstance(GetType());//, BindingFlags.Instance | BindingFlags.NonPublic) as Node;

            shallowCopy.Parents = new List<Node>();
            shallowCopy.Children = new List<Node>();
            return shallowCopy as ICopyable;
        }

        public string GetRepresentation()
        {
            return $"{GetType().Name}: {PropertiesToString()}";
        }

        public ICopyable Copy(IRandom rand)
        {
            throw new NotImplementedException();
        }

        public string AssemblyQualifiedName { get { return GetType().AssemblyQualifiedName; } }

        public virtual bool IsInputNode { get { return !HasChildren(); } }

        public virtual string OutputVariableName { get { return GetType().Name + NodeID.ToString(); } }


        public class IDEqualityComparer<TNode> : EqualityComparer<TNode> where TNode : Node
        {
            public override bool Equals(TNode x, TNode y)
            {
                return x.NodeID == y.NodeID;
            }

            public override int GetHashCode(TNode obj)
            {
                return (int)obj.NodeID;
            }
        }

        /// <summary>
        /// Equivalent to n1.addchild(n2)
        /// n2.addparent(n1)
        /// n1 -> n2
        /// 
        /// returning n2 should allow syntax like this:
        /// Tree: n1 -> n2 -> n3
        /// (n2 is a child of n1, n3 is a child of n2)
        /// construct using - operator: n1 - n2 - n3
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <returns>n2</returns>
        public static Node operator -(Node n1, Node n2)
        {
            n1.AddChild(n2);
            n2.AddParent(n1);
            return n2;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list">A list containing the assembly qualified name of operator nodes</param>
        /// <param name="type">The node type (CVNode, HalconOperatorNode) that should be returned</param>
        /// <returns></returns>
        public static List<INode> DeserializeOperatorList(List<string> list)
        {
            var nodes = new List<INode>();
            foreach (var n in list)
            {
                var tmp = Activator.CreateInstance(Type.GetType(n)) as INode;
                if (tmp == null) throw new Exception($"{n} could not be instantiated as operator node");
                nodes.Add(tmp);
            }

            return nodes;
        }
    }
}
