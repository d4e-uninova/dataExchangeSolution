using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataExchangeSolution
{
    class GraphContainer
    {

        public class Neighbor
        {
            public Node node { get; set; }
            public int distance { get; set; }
        }

        public GraphContainer()
        {
            InitializeGraph();
        }
        public class Node
        {

            public Service.DataFormat name { get; set; }
            public Dictionary<Service.DataFormat, List<Service.DataFormat>> distanceDict { get; set; }
            public Boolean visited { get; set; }
            public List<Neighbor> neighbors { get; set; }
        }
         List<Node> graph = new List<Node>() {
            new Node {name = Service.DataFormat.IFC, distanceDict = new Dictionary<Service.DataFormat,List<Service.DataFormat>>(){
                {Service.DataFormat.RVT,new List<Service.DataFormat>(){Service.DataFormat.RVT}},
                {Service.DataFormat.gbXML,new List<Service.DataFormat>(){Service.DataFormat.gbXML}}}, 
                visited = false},
            new Node {name = Service.DataFormat.RVT, distanceDict = new Dictionary<Service.DataFormat,List<Service.DataFormat>>(){
            {Service.DataFormat.IFC,new List<Service.DataFormat>(){Service.DataFormat.IFC}},
            {Service.DataFormat.gbXML,new List<Service.DataFormat>(){Service.DataFormat.gbXML}}},
                visited = false},
            new Node {name = Service.DataFormat.gbXML, distanceDict = new Dictionary<Service.DataFormat,List<Service.DataFormat>>(){
                {Service.DataFormat.IDF,new List<Service.DataFormat>(){Service.DataFormat.IDF}},},
                visited = false},
            new Node {name = Service.DataFormat.IDF, distanceDict = new Dictionary<Service.DataFormat,List<Service.DataFormat>>()
                {},
                visited = false}
        };




         void InitializeGraph()
        {

            //initialize neighbors using predefined dixtionary
            foreach (Node node in graph)
            {
                node.neighbors = new List<Neighbor>();
                foreach (KeyValuePair<Service.DataFormat, List<Service.DataFormat>> neighbor in node.distanceDict)
                {
                    Neighbor newNeightbor = new Neighbor();
                    foreach (Node graphNode in graph)
                    {
                        if (graphNode.name == neighbor.Key)
                        {
                            newNeightbor.node = graphNode;
                            newNeightbor.distance = neighbor.Value.Count;
                            node.neighbors.Add(newNeightbor);
                            break;
                        }
                    }
                }
            }
            for (var i = 0; i < graph.Count(); i++)
            {
                TransverNode(graph[i]);
            }
        }

         public List<Service.DataFormat> FindPath(Service.DataFormat inFormat, Service.DataFormat outFormat)
        {
            for (var i = 0; i < graph.Count(); i++)
            {
                if (graph[i].name == inFormat)
                {
                    if (graph[i].distanceDict.ContainsKey(outFormat))
                    {
                        return graph[i].distanceDict[outFormat];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return new List<Service.DataFormat>();
        }

         void TransverNode(Node node)
        {
            if (!node.visited)
            {
                node.visited = true;
                foreach (Neighbor neighbor in node.neighbors)
                {
                    TransverNode(neighbor.node);
                    Service.DataFormat neighborName = neighbor.node.name;
                    int neighborDistance = neighbor.distance;
                    //compair neibors dictionary with current dictionary
                    //update current dictionary as required
                    foreach (Service.DataFormat key in neighbor.node.distanceDict.Keys)
                    {
                        if (key != node.name)
                        {
                            int neighborKeyDistance = neighbor.node.distanceDict[key].Count;
                            if (node.distanceDict.ContainsKey(key))
                            {
                                int currentDistance = node.distanceDict[key].Count;
                                if (neighborKeyDistance + neighborDistance < currentDistance)
                                {
                                    List<Service.DataFormat> nodeList = new List<Service.DataFormat>();
                                    nodeList.AddRange(neighbor.node.distanceDict[key].ToArray());
                                    nodeList.Insert(0, neighbor.node.name);
                                    node.distanceDict[key] = nodeList;
                                }
                            }
                            else
                            {
                                List<Service.DataFormat> nodeList = new List<Service.DataFormat>();
                                nodeList.AddRange(neighbor.node.distanceDict[key].ToArray());
                                nodeList.Insert(0, neighbor.node.name);
                                node.distanceDict.Add(key, nodeList);
                            }
                        }
                    }
                }
            }
        }
    }
}
