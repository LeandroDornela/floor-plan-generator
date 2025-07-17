using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.UIElements;
using System;

namespace BuildingGenerator
{
    public class ZoneNode : Node
    {
        // Variables for graph logic.
        public string GUID;

        // Variables to export.
        public string _zoneID;
        public float _areaRatio = 1;
        public bool _hasOutsideDoor = false;
        public Texture2D _presetAreaTexture;
        public string _parentDataNodeGUID; // use guids to make easier to loading, to dont need to assign the acutual nodes.
        public List<string> _childZonesNodesGUIDs = new();
        public List<string> _adjacentNodesGUIDs = new();


        // Variables for fields.
        public bool _isRoot = false;
        private TextField zoneIdField;

        public Port _parentNodePort;
        public Port _childrenNodesPort;
        public Port _adjacenciesPort_In;
        public Port _adjacenciesPort_Out;


        private string _parentPrefix = "Parent Zone";
        private string _childPrefix = "Children Zones";

        private Color _rootColor =  new Color(0.125f, 0.125f, 0.125f); //new Color(1f, 0.8f, 0.5f); //new Color(0.9f, 0.9f, 0.9f);
        private Color _branchColor = new Color(0.168f, 0.168f, 0.168f); //new Color(0.4f, 0.2f, 0.0f); //new Color(0.5f, 0.5f, 0.5f);
        private Color _leafColor = new Color(0.4f, 0.4f, 0.4f); //new Color(0.3f, 0.5f, 0.1f); //new Color(0.1f, 0.1f, 0.1f);

        private Color _familyPortCol = Color.magenta;
        private Color _adjacencyPortColor = Color.cyan;


        public bool IsAdjacenciesPort_In(Port portToTest)
        {
            return portToTest == _adjacenciesPort_In;
        }

        public bool IsAdjacenciesPort_Out(Port portToTest)
        {
            return portToTest == _adjacenciesPort_Out;
        }

        public bool IsParentPort(Port portToTest)
        {
            return portToTest == _parentNodePort;
        }

        public bool IsChildrenPort(Port portToTest)
        {
            return portToTest == _childrenNodesPort;
        }


        public ZoneNode(bool isRoot, string guid, string zoneId = "")
        {
            GUID = guid;
            _zoneID = zoneId;
            _isRoot = isRoot;
        }

        public void CreateNodeElements()
        {
            title = $"{_zoneID}";

            var titleContainer = this.Q("title");
            if (titleContainer != null)
            {
                StyleColor backColor;
                if (_isRoot)
                {
                    backColor = new StyleColor(_rootColor);

                    var titleLabel = this.Q<Label>("title-label");
                    if (titleLabel != null)
                    {
                        titleLabel.style.color = new StyleColor(Color.white);
                    }
                }
                else
                {
                    if (_childZonesNodesGUIDs?.Count > 0)
                    {
                        backColor = new StyleColor(_branchColor);
                    }
                    else
                    {
                        backColor = new StyleColor(_leafColor);
                    }
                }

                titleContainer.style.backgroundColor = backColor;
            }

            // CHILDREN PORT
            var cPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ZoneNode));
            cPort.portName = _childPrefix;
            cPort.portColor = _familyPortCol;
            _childrenNodesPort = cPort;
            outputContainer.Add(cPort);

            // Input: Parent
            if (!_isRoot)
            {
                var parentPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ZoneNode));
                parentPort.portName = _parentPrefix;
                parentPort.portColor = _familyPortCol;
                _parentNodePort = parentPort;
                inputContainer.Add(parentPort);

                var port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ZoneNode));
                port.portName = $"Adjacencies In";
                port.portColor = _adjacencyPortColor;
                _adjacenciesPort_In = port;
                inputContainer.Add(port);

                port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ZoneNode));
                port.portName = $"Adjacencies Out";
                port.portColor = _adjacencyPortColor;
                _adjacenciesPort_Out = port;
                outputContainer.Add(port);
            }


            // Add Child Button
            /*
            var addChildButton = new Button(() => NodeButton_AddChildPort())
            {
                text = "Add Child"
            };
            outputContainer.Add(addChildButton);
            */

            // Label Field
            zoneIdField = new TextField("Zone ID");
            zoneIdField.value = _zoneID;
            zoneIdField.RegisterValueChangedCallback(evt => this.title = _zoneID = evt.newValue);
            mainContainer.Add(zoneIdField);

            var textureField = new ObjectField("Preset area")
            {
                objectType = typeof(Texture2D),
                allowSceneObjects = false,
                value = _presetAreaTexture
            };
            textureField.RegisterValueChangedCallback(evt =>
            {
                _presetAreaTexture = evt.newValue as Texture2D;
            });
            mainContainer.Add(textureField);

            if (!_isRoot)
            {
                // Create a slider
                var slider = new Slider($"Area ratio [{_areaRatio}]", 0f, 1f)
                {
                    value = _areaRatio
                };
                slider.RegisterValueChangedCallback(evt =>
                {
                    _areaRatio = evt.newValue;
                    slider.label = $"Area ratio [{_areaRatio}]";
                });
                mainContainer.Add(slider);


                var hasDoor = new Toggle("Has outside door");
                hasDoor.value = _hasOutsideDoor;
                hasDoor.RegisterValueChangedCallback(evt =>
                {
                    _hasOutsideDoor = evt.newValue;
                });
                mainContainer.Add(hasDoor);
            }

            //titleContainer.Add(new Label("TEST"));
            //extensionContainer.Add(new Label("TEST"));
            //titleButtonContainer.Add(new Label("TEST"));
            //mainContainer.Add(new Label("TEST"));
            //topContainer.Add(new Label("TEST"));

            // Children Header
            //outputContainer.Add(new Label("Children:"));


            RefreshExpandedState();
            RefreshPorts();
        }

        private void NodeButton_AddChildPort()
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ZoneNode));
            port.portName = _childPrefix;
            port.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
            {
                evt.menu.AppendAction("Delete Port", action =>
                {
                    // Disconnect edges
                    //graphView.DeleteElements(port.connections);

                    // Remove port from container
                    port.RemoveFromHierarchy();

                    // Optional: Refresh visuals
                    RefreshPorts();
                    RefreshExpandedState();
                }, DropdownMenuAction.AlwaysEnabled);
            });
            //childPorts.Add(port);
            outputContainer.Add(port);
            RefreshPorts();
            RefreshExpandedState();

            if (!_isRoot)
            {
                var titleContainer = this.Q("title");
                if (titleContainer != null)
                {
                    StyleColor backColor = new StyleColor(new Color(0.4f, 0.2f, 0.0f));
                    titleContainer.style.backgroundColor = backColor;
                }
            }
        }


        public void AssignParent(ZoneNode newParent, Port thisInPort, Port parentOutPort)
        {
            _parentDataNodeGUID = newParent.GUID;

            thisInPort.portName = $"{_parentPrefix} [{newParent._zoneID}]";
            //parentOutPort.portName = $"{_childPrefix} [{this._zoneID}]";
        }


        public void RemoveParent(ZoneNode parent, Port thisInPort, Port parentOutPort)
        {
            if (_parentDataNodeGUID != parent.GUID)
            {
                Debug.LogError("Parent don't match.");
            }

            _parentDataNodeGUID = null;

            thisInPort.portName = $"{_parentPrefix}";
            //parentOutPort.portName = $"{_childPrefix}";
        }


        public void AssignNewChild(ZoneNode newChild, Port thisOutPort, Port childInPort)
        {
            if (_childZonesNodesGUIDs == null)
            {
                _childZonesNodesGUIDs = new List<string>();
            }

            if (!_childZonesNodesGUIDs.Contains(newChild.GUID))
            {
                _childZonesNodesGUIDs.Add(newChild.GUID);
                //thisOutPort.portName = newChild._zoneID;
                //childInPort.portName = this._zoneID;
            }

            UpdateColors();
        }

        public void RemoveChild(ZoneNode child, Port thisOutPort, Port childInPort)
        {
            if (_childZonesNodesGUIDs == null)
            {
                return;
            }

            if (_childZonesNodesGUIDs.Contains(child.GUID))
            {
                _childZonesNodesGUIDs.Remove(child.GUID);
                //thisOutPort.portName = newChild._zoneID;
                //childInPort.portName = this._zoneID;
            }

            UpdateColors();
        }


        public void AddAdjacency(ZoneNode newAdjacent)
        {
            if (_adjacentNodesGUIDs == null)
            {
                _adjacentNodesGUIDs = new List<string>();
            }

            if (!_adjacentNodesGUIDs.Contains(newAdjacent.GUID))
            {
                _adjacentNodesGUIDs.Add(newAdjacent.GUID);
            }

            Debug.Log($"Adjacency added from {_zoneID} to {newAdjacent._zoneID}");
        }


        public void RemoveAdjacency(ZoneNode adjacent)
        {
            if (_adjacentNodesGUIDs == null)
            {
                return;
            }

            if (_adjacentNodesGUIDs.Contains(adjacent.GUID))
            {
                _adjacentNodesGUIDs.Remove(adjacent.GUID);
                //thisOutPort.portName = newChild._zoneID;
                //childInPort.portName = this._zoneID;
            }

            Debug.Log($"Adjacency removed from {_zoneID} to {adjacent._zoneID}");
        }



        public void UpdateColors()
        {
            if (!_isRoot)
            {
                var titleContainer = this.Q("title");
                if (titleContainer != null)
                {
                    if (_childZonesNodesGUIDs.Count > 0)
                    {
                        titleContainer.style.backgroundColor = new StyleColor(_branchColor);
                    }
                    else
                    {
                        titleContainer.style.backgroundColor = new StyleColor(_leafColor);
                    }
                }
            }
        }
    }
}
