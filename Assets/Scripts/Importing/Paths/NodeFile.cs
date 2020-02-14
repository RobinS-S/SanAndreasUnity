﻿using System;
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
		public int BaseLinkID { get; set; }
		public int AreaID { get; set; }
		public int NodeID { get; set; }
		public float PathWidth { get; set; }
		public int NodeType { get; set; } // enum
		public int LinkCount { get; set; }
		public bool IsDeadEnd { get; set; }
		public bool IsIgnoredNode { get; set; }
		public bool IsRoadBlock { get; set; }
		public bool IsWaterNode { get; set; }
		public bool IsEmergencyVehicleOnly { get; set; }
		public bool IsRestrictedAccess { get; set; }
		public bool IsDontWander { get; set; }
		public bool Unknown2 { get; set; }
		public int Speedlimit { get; set; }
		public bool Unknown3 { get; set; }
		public bool Unknown4 { get; set; }
		public int SpawnProbability { get; set; }
		public int BehaviorType { get; set; }
		public bool IsPed { get; set; }
	}
	public class NavNode
	{
		public Vector2 Position { get; set; }
		public int TargetAreaID { get; set; }
		public int TargetNodeID { get; set; }
		public Vector2 Direction { get; set; }
		public int Width { get; set; }
		public int NumLeftLanes { get; set; }
		public int NumRightLanes { get; set; }
		public int TrafficLightDirection { get; set; }
		public int TrafficLightBehavior { get; set; }
		public int IsTrainCrossing { get; set; }
		public byte Flags { get; set; }
	}
	public class NodeLink
	{
		public int AreaID { get; set; }
		public int NodeID { get; set; }
		public int Length { get; set; }
	}
	public class PathIntersectionFlags
	{
		public bool IsRoadCross { get; set; }
		public bool IsTrafficLight { get; set; }
	}
	public class NavNodeLink
	{
		public int NodeLink { get; set; }
		public int AreaID { get; set; }
	}
	public class NodeReader
	{
		public static List<NodeFile> Nodes { get; set; }
		public static void StepLoadPaths()
		{
			for (int i = 0; i < 64; i++)
			{
				using (Stream node = SanAndreasUnity.Importing.Archive.ArchiveManager.ReadFile("nodes" + i + ".dat"))
				{
					NodeFile nf = new NodeFile(i, node);
					AddNode(nf);
				}
			}
		}
		public static void AddNode(NodeFile node)
		{
			if (Nodes == null) Nodes = new List<NodeFile>();
			Nodes.Add(node);
		}
	}
	public class NodeFile
	{
		public int Id { get; set; }
		public int NumOfNodes { get; set; }
		public int NumOfVehNodes { get; set; }
		public int NumOfPedNodes { get; set; }
		public int NumOfNavNodes { get; set; }
		public int NumOfLinks { get; set; }
		public List<PathNode> PathNodes { get; set; }
		public List<NavNode> NavNodes { get; set; }
		public List<NodeLink> NodeLinks { get; set; }
		public List<NavNodeLink> NavNodeLinks { get; set; }
		public List<PathIntersectionFlags> PathIntersections { get; set; }


		public NodeFile(int id, Stream stream)
		{
			Id = id;
			PathNodes = new List<PathNode>();
			NavNodes = new List<NavNode>();
			NodeLinks = new List<NodeLink>();
			NavNodeLinks = new List<NavNodeLink>();
			PathIntersections = new List<PathIntersectionFlags>();
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
				if (i > NumOfVehNodes) node.IsPed = true;
				reader.ReadUInt32();
				reader.ReadUInt32();
				float x = (float)reader.ReadInt16() / 8;
				float z = (float)reader.ReadInt16() / 8;
				float y = (float)reader.ReadInt16() / 8;
				node.Position = new Vector3(x, y, z);
				short heuristic = reader.ReadInt16();
				if (heuristic != 0x7FFE) UnityEngine.Debug.Log("corrupted path node?");
				node.BaseLinkID = reader.ReadUInt16();
				node.AreaID = reader.ReadUInt16();
				node.NodeID = reader.ReadUInt16();
				node.PathWidth = (float)reader.ReadByte() / 8;
				node.NodeType = reader.ReadByte();

				byte flag = reader.ReadByte();
				node.LinkCount = flag & 15;
				if (((flag >> 4) & 1) == 1) node.IsDeadEnd = true;
				if (((flag >> 5) & 1) == 1) node.IsIgnoredNode = true;
				if (((flag >> 6) & 1) == 1) node.IsRoadBlock = true;
				if (((flag >> 7) & 1) == 1) node.IsWaterNode = true;

				flag = reader.ReadByte();
				if ((flag & 1) == 1) node.IsEmergencyVehicleOnly = true;
				if ((flag >> 1) == 1) node.IsRestrictedAccess = true;
				if ((flag >> 2) == 1) node.IsDontWander = true;
				if ((flag >> 3) == 1) node.Unknown2 = true;
				node.Speedlimit = (flag >> 4) & 3;
				if ((flag >> 6) == 1) node.Unknown3 = true;
				if ((flag >> 7) == 1) node.Unknown4 = true;

				flag = reader.ReadByte();
				node.SpawnProbability = flag & 15;
				node.BehaviorType = (flag >> 4) & 15;
				flag = reader.ReadByte();
				PathNodes.Add(node);
				//UnityEngine.Debug.Log($"Node {i}: POS [{node.Position.x} {node.Position.y} {node.Position.z}] LinkID {node.LinkID} LinkCount {node.LinkCount} AreaID {node.AreaID} NodeID {node.NodeID} PathWidth {node.PathWidth} NodeType {node.NodeType} Flags {node.Flags}");
			}
		}
		private void ReadNavNodes(BinaryReader reader)
		{
			for (int i = 0; i < NumOfNavNodes; i++)
			{
				NavNode node = new NavNode();
				node.Position = new Vector2(reader.ReadInt16(), reader.ReadInt16());
				node.TargetAreaID = reader.ReadUInt16();
				node.TargetNodeID = reader.ReadUInt16();
				node.Direction = new Vector2(reader.ReadSByte(), reader.ReadSByte());
				node.Width = reader.ReadByte() / 8;

				byte flags = reader.ReadByte();
				node.NumLeftLanes = flags & 7;
				node.NumRightLanes = (flags >> 3) & 7;
				node.TrafficLightDirection = (flags >> 4) & 1;

				flags = reader.ReadByte();
				node.TrafficLightBehavior = flags & 3;
				node.IsTrainCrossing = (flags >> 2) & 1;
				node.Flags = reader.ReadByte();

				NavNodes.Add(node);
				//UnityEngine.Debug.Log($"NavNode {i}: {node.Position.x} {node.Position.y} AreaID {node.AreaID} NodeID {node.NodeID} Direction {node.Direction.x} {node.Direction.y} Flags {node.Flags}");
			}
		}
		private void ReadLinks(BinaryReader reader)
		{
			for (int i = 0; i < NumOfLinks; i++)
			{
				NodeLink link = new NodeLink();
				link.AreaID = reader.ReadUInt16();
				link.NodeID = reader.ReadUInt16();
				NodeLinks.Add(link);
				//UnityEngine.Debug.Log($"NodeLink {i}: AreaID {link.AreaID} NodeID {link.NodeID}");
			}
		}

		private void ReadNavLinks(BinaryReader reader)
		{
			for (int i = 0; i < NumOfNavNodes; i++)
			{
				ushort bytes = reader.ReadUInt16();
				NavNodeLink link = new NavNodeLink();
				link.NodeLink = bytes & 1023;
				link.AreaID = bytes >> 10;
				NavNodeLinks.Add(link);
				//UnityEngine.Debug.Log($"NavLink {i} area ID {link.AreaID} NaviNodeID {link.NaviNodeID}");
			}
		}
		private void ReadLinkLengths(BinaryReader reader)
		{
			for (int i = 0; i < NumOfLinks; i++)
			{
				ushort length = reader.ReadByte();
				NodeLinks[i].Length = length;
				//UnityEngine.Debug.Log($"Link length {i}: {length}");
			}
		}

		private void ReadPathIntersectionFlags(BinaryReader reader)
		{
			for (int i = 0; i < NumOfLinks; i++)
			{
				byte roadCross = reader.ReadByte();
				//byte pedTrafficLight = reader.ReadByte();
				/*
				PathIntersectionFlags pif = new PathIntersectionFlags()
				{
					IsRoadCross = (roadCross & 1) ? true : false,
					IsTrafficLight = (roadCross & 1) ? true : false
				};*/
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
