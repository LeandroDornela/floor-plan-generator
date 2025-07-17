using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildingGenerator
{
    public class DataGraphView : GraphView
    {
        public string PlanId = "New Floor Plan";
        public Vector2Int GridDimensions = new Vector2Int(10, 10);
        

        public DataGraphView()
        {
            style.flexGrow = 1;

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Background grid
            Insert(0, new GridBackground());

            // Setup interaction
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Default node style
            this.AddElement(GenerateEntryPointNode());

            this.graphViewChanged = OnGraphViewChanged;


        }

        public void CreateNode()
        {
            var node = new ZoneNode(false, Guid.NewGuid().ToString());
            node.SetPosition(new Rect(Vector2.zero, new Vector2(200, 150)));
            node.CreateNodeElements();
            AddElement(node);
        }

        public void CreateRootNode()
        {
            var node = new ZoneNode(true, Guid.NewGuid().ToString());
            node.SetPosition(new Rect(Vector2.zero, new Vector2(200, 150)));
            node.CreateNodeElements();
            AddElement(node);
        }

        private ZoneNode GenerateEntryPointNode()
        {
            var node = new ZoneNode(true, Guid.NewGuid().ToString());
            node.title = "Root";
            node.SetPosition(new Rect(100, 200, 200, 150));
            node.CreateNodeElements();
            return node;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="change"></param>
        /// <returns></returns>
        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    //Debug.Log($"Connected: {edge.output.node.name} → {edge.input.node.name}  :  {edge.output.portType}");

                    ZoneNode fromNode = (ZoneNode)edge.output.node;
                    ZoneNode toNode = (ZoneNode)edge.input.node;

                    if (fromNode.IsParentPort(edge.output) || fromNode.IsChildrenPort(edge.output))
                    {
                        fromNode.AssignNewChild(toNode, edge.output, edge.input);
                        toNode.AssignParent(fromNode, edge.input, edge.output);
                        continue;
                    }

                    if (fromNode.IsAdjacenciesPort_Out(edge.output) || fromNode.IsAdjacenciesPort_In(edge.output))
                    {
                        fromNode.AddAdjacency(toNode);
                        continue;
                    }
                }
            }

            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is Edge edge)
                    {
                        //Debug.Log($"Disconnected: {edge.output.node.name} → {edge.input.node.name}");
                        ZoneNode fromNode = (ZoneNode)edge.output.node;
                        ZoneNode toNode = (ZoneNode)edge.input.node;

                        if (fromNode.IsParentPort(edge.output) || fromNode.IsChildrenPort(edge.output))
                        {
                            if (fromNode != null && toNode != null)
                            {
                                fromNode.RemoveChild(toNode, edge.output, edge.input);
                                toNode.RemoveParent(fromNode, edge.input, edge.output);
                            }

                            continue;
                        }

                        if (fromNode.IsAdjacenciesPort_Out(edge.output) || fromNode.IsAdjacenciesPort_In(edge.output))
                        {
                            fromNode.RemoveAdjacency(toNode);
                            continue;
                        }
                    }
                }
            }

            return change;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="nodeAdapter"></param>
        /// <returns></returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            foreach (var port in ports)
            {
                // Skip same port
                if (port == startPort)
                    continue;

                // Skip ports on the same node
                if (port.node == startPort.node)
                    continue;

                // Only allow connections between opposite directions
                if (port.direction == startPort.direction)
                    continue;

                // Optional: Match data types
                if (port.portType != startPort.portType)
                    continue;

                ZoneNode start = (ZoneNode)startPort.node;
                ZoneNode end = (ZoneNode)port.node;

                // Avoid children connection to adjacency.
                if (start.IsChildrenPort(startPort) && end.IsAdjacenciesPort_In(port))
                    continue;

                // Avoid parent connection to adjacency.
                if (start.IsParentPort(startPort) && end.IsAdjacenciesPort_Out(port))
                    continue;

                // Avoid adjacency to children
                if (start.IsAdjacenciesPort_Out(startPort) && end.IsParentPort(port))
                    continue;

                // Avoid adjacency to parent
                if (start.IsAdjacenciesPort_In(startPort) && end.IsChildrenPort(port))
                    continue;

                if (ArePortsConnected(startPort, port))
                    continue;

                // TODO: Avoid Adjacency to parent and children.

                // TODO: Avoid cross children reference.
                if (IsCrossParentChildReference(start, end))
                    continue;

                // TODO: Avoid cross adjacency reference.
                if (IsCrossAdjacencyReference(start, end))
                    continue;


                //DataNode node = (DataNode)startPort.node;
                //if (node != null)
                //{
                //    Debug.Log(node._zoneID);
                //}

                compatiblePorts.Add(port);
            }

            return compatiblePorts;
        }


        bool IsCrossParentChildReference(ZoneNode from, ZoneNode to)
        {
            // ambas podem ser child ou parent.
            return false;
        }

        bool IsCrossAdjacencyReference(ZoneNode from, ZoneNode to)
        {
            return false;
        }

        bool ArePortsConnected(Port portA, Port portB)
        {
            return portA.connections.Any(edge => edge.input == portB || edge.output == portB);
        }



        #region  SAVE/LOAD
        public void SaveGraphTo(FloorPlanGraphData asset)
        {
            asset.planId = PlanId;
            asset.gridDimensions = GridDimensions;

            // Map GUID to node
            var nodeMap = nodes.OfType<ZoneNode>().ToDictionary(n => n.GUID, n => n);

            asset.nodes.Clear();

            foreach (var node in nodeMap.Values)
            {
                var model = new DataNodeModel
                {
                    guid = node.GUID,
                    position = node.GetPosition().position,
                    zoneId = node._zoneID,
                    areaRatio = node._areaRatio,
                    hasOutsideDoor = node._hasOutsideDoor,
                    presetAreaTexture = node._presetAreaTexture,
                    parentGUID = node._parentDataNodeGUID,
                    childrenGUIDs = new List<string>(node._childZonesNodesGUIDs),
                    adjacenciesGUIDs = new List<string>(node._adjacentNodesGUIDs)
                };

                asset.nodes.Add(model);
            }
        }


        public void LoadGraphFrom(FloorPlanGraphData asset)
        {
            PlanId = asset.planId;
            GridDimensions = asset.gridDimensions;

            graphElements.ForEach(RemoveElement);

            Dictionary<string, ZoneNode> nodeMap = new();

            // Create nodes first
            foreach (var model in asset.nodes)
            {
                bool isRoot = false;
                if (model.parentGUID == "")
                {
                    isRoot = true;
                }

                var node = new ZoneNode(isRoot, model.guid, model.zoneId)
                {
                    _areaRatio = model.areaRatio,
                    _hasOutsideDoor = model.hasOutsideDoor,
                    _presetAreaTexture = model.presetAreaTexture,
                    _parentDataNodeGUID = model.parentGUID,
                    _childZonesNodesGUIDs = model.childrenGUIDs,
                    _adjacentNodesGUIDs = model.adjacenciesGUIDs
                };
                node.SetPosition(new Rect(model.position, new Vector2(200, 150)));
                node.CreateNodeElements();
                AddElement(node);
                nodeMap[model.guid] = node;
            }

            foreach (var node in nodeMap.Values)
            {
                if (node._childZonesNodesGUIDs != null)
                {
                    foreach (var childGUID in node._childZonesNodesGUIDs)
                    {
                        ZoneNode fromNode = nodeMap[node.GUID];
                        ZoneNode toNode = nodeMap[childGUID];

                        CreateConnection(fromNode._childrenNodesPort, toNode._parentNodePort);
                    }
                }

                if (node._adjacentNodesGUIDs.Count > 0)
                {
                    foreach (var adjcGUID in node._adjacentNodesGUIDs)
                    {
                        ZoneNode fromNode = nodeMap[node.GUID];
                        ZoneNode toNode = nodeMap[adjcGUID];

                        CreateConnection(fromNode._adjacenciesPort_Out, toNode._adjacenciesPort_In);
                    }
                }
            }
        }

        void CreateConnection(Port from, Port to)
        {
            var edge = from.ConnectTo(to);
            AddElement(edge);
        }

        #endregion
    }
}
