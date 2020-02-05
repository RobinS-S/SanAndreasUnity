using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Importing.Paths
{
	public class PathNode
	{
		public Vector3 Position { get; set; }
		public int LinkID { get; set; }
		public int AreaID { get; set; }
		public int NodeID { get; set; }
		public int PathWidth { get; set; }
		public int NodeType { get; set; } // enum
		public uint Flags { get; set; } // enum
	}
	public class NavNode
	{
		public Vector2 Position { get; set; }
		public int AreaID { get; set; }
		public int NodeID { get; set; }
		public Vector2 Direction { get; set; }
		public uint Flags { get; set; }
	}
	public class NodeLink
	{
		public int AreaID { get; set; }
		public int LinkID { get; set; }
	}
	public class NavNodeLink
	{
		public int AreaID { get; set; }
		public int NaviNodeID { get; set; }
	}
	public class NodeFile
	{
		public string Name { get; set; }
		public int NumOfNodes { get; set; }
		public int NumOfVehNodes { get; set; }
		public int NumOfPedNodes { get; set; }
		public int NumOfNavNodes { get; set; }
		public int NumOfLinks { get; set; }
		public List<PathNode> PathNodes { get; set; }
		public List<NavNode> NavNodes { get; set; }
		public List<NodeLink> NodeLinks { get; set; }
		public List<NavNodeLink> NavNodeLinks { get; set; }


		public NodeFile(Stream stream)
		{
			PathNodes = new List<PathNode>();
			NavNodes = new List<NavNode>();
			NodeLinks = new List<NodeLink>();
			NavNodeLinks = new List<NavNodeLink>();
			using (BinaryReader reader = new BinaryReader(stream))
			{
				ReadHeader(reader);
				UnityEngine.Debug.Log(NumOfNodes);
				ReadNodes(reader);
				ReadNavNodes(reader);
				UnityEngine.Debug.Log(NumOfNavNodes);
				ReadLinks(reader);
				reader.ReadBytes(768);
				ReadNavLinks(reader);
				ReadLinkLengths(reader);
				ReadPathIntersectionFlags(reader);
			}
			UnityEngine.Debug.Log($"Read paths. Nodes {NumOfNodes} VehNodes {NumOfVehNodes} PedNodes {NumOfPedNodes} NavNodes {NumOfNavNodes} Links {NumOfLinks}");
		}

		private void ReadNodes(BinaryReader reader)
		{
			for (int i = 0; i < NumOfNodes; i++)
			{
				PathNode node = new PathNode();
				reader.ReadUInt32();
				reader.ReadUInt32();
				float x = (float)reader.ReadInt16() / 8;
				float z = (float)reader.ReadInt16() / 8;
				float y = (float)reader.ReadInt16() / 8;
				node.Position = new Vector3(x, y, z);
				short heuristic = reader.ReadInt16();
				if (heuristic != 0x7FFE) UnityEngine.Debug.Log("corrupted path node?");
				node.LinkID = (int)reader.ReadUInt16();
				node.AreaID = (int)reader.ReadUInt16();
				node.NodeID = (int)reader.ReadUInt16();
				node.PathWidth = (int)reader.ReadByte();
				node.NodeType = (int)reader.ReadByte();
				node.Flags = reader.ReadUInt32();
				PathNodes.Add(node);
				//UnityEngine.Debug.Log($"Node {i}: POS [{node.Position.x} {node.Position.y} {node.Position.z}] LinkID {node.LinkID} AreaID {node.AreaID} NodeID {node.NodeID} PathWidth {node.PathWidth} NodeType {node.NodeType} Flags {node.Flags}");
			}
		}
		private void ReadNavNodes(BinaryReader reader)
		{
			for (int i = 0; i < NumOfNavNodes; i++)
			{
				NavNode node = new NavNode();
				node.Position = new Vector2(((float)reader.ReadInt16()), ((float)reader.ReadInt16()));
				node.AreaID = (int)reader.ReadUInt16();
				node.NodeID = (int)reader.ReadUInt16();
				node.Direction = new Vector2(((float)reader.ReadSByte()), ((float)reader.ReadSByte()));
				node.Flags = reader.ReadUInt32();
				NavNodes.Add(node);
				//UnityEngine.Debug.Log($"NavNode {i}: {node.Position.x} {node.Position.y} AreaID {node.AreaID} NodeID {node.NodeID} Direction {node.Direction.x} {node.Direction.y} Flags {node.Flags}");
			}
		}
		private void ReadLinks(BinaryReader reader)
		{
			for (int i = 0; i < NumOfLinks; i++)
			{
				NodeLink link = new NodeLink();
				link.AreaID = (int)reader.ReadUInt16();
				link.LinkID = (int)reader.ReadUInt16();
				NodeLinks.Add(link);
				//UnityEngine.Debug.Log($"NodeLink {i}: AreaID {link.AreaID} LinkID {link.LinkID}");
			}
		}

		private void ReadNavLinks(BinaryReader reader)
		{
			for (int i = 0; i < NumOfNavNodes; i++)
			{
				ushort bytes = reader.ReadUInt16();
				NavNodeLink link = new NavNodeLink();
				link.AreaID = (byte)(bytes >> 16);
				link.NaviNodeID = (byte)(bytes & 6);
				// this is still wrong i think
				NavNodeLinks.Add(link);
				//UnityEngine.Debug.Log($"NavLink {i} area ID {link.AreaID} NaviNodeID {link.NaviNodeID}");
			}
		}
		private void ReadLinkLengths(BinaryReader reader)
		{
			for (int i = 0; i < NumOfLinks; i++)
			{
				int length = (int)reader.ReadByte();
				//UnityEngine.Debug.Log($"Link length {i}: {length}");
			}
		}

		private void ReadPathIntersectionFlags(BinaryReader reader)
		{
			for (int i = 0; i < NumOfLinks; i++)
			{
				byte roadCross = reader.ReadByte();
				byte pedTrafficLight = reader.ReadByte();
				//UnityEngine.Debug.Log($"PathIntersectionFlags {i}: roadCross {roadCross} pedTrafficLight {pedTrafficLight}");
			}
		}

		private void ReadHeader(BinaryReader reader)
		{
			NumOfNodes = (int)reader.ReadUInt32();
			NumOfVehNodes = (int)reader.ReadUInt32();
			NumOfPedNodes = (int)reader.ReadUInt32();
			NumOfNavNodes = (int)reader.ReadUInt32();
			NumOfLinks = (int)reader.ReadUInt32();
		}
	}
}
