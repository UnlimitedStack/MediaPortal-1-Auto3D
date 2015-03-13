using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace MediaPortal.Player.TestPatterns
{
	public class TestPattern
	{
		public TestPattern(XmlNode node)
		{
			Name = node.ChildNodes.Item(0).InnerText;

			Image16by9 = node.ChildNodes.Item(1).InnerText;
			Image16by9x = node.ChildNodes.Item(2).InnerText;
			Image21by9 = node.ChildNodes.Item(3).InnerText;
			Image21by9x = node.ChildNodes.Item(4).InnerText;

			IsAlternating = node.ChildNodes.Item(5).InnerText == "1" ? true : false;
			AlternatingTime = int.Parse(node.ChildNodes.Item(6).InnerText);

			Description = node.ChildNodes.Item(7).InnerText;
			Link = node.ChildNodes.Item(8).InnerText;
		}

		private void AppendChild(XmlNode node, String name, String value)
		{
			XmlNode nameNode = node.OwnerDocument.CreateNode(XmlNodeType.Element, name, "");
			nameNode.InnerText = value;
			node.AppendChild(nameNode);		
		}

		public void ToNode(XmlNode node)
		{
			AppendChild(node, "Name", Name);
			AppendChild(node, "Image16by9", Image16by9);
			AppendChild(node, "Image16by9x", Image16by9x);
			AppendChild(node, "Image21by9", Image21by9);
			AppendChild(node, "Image21by9x", Image21by9x);
			AppendChild(node, "Alternating", IsAlternating ? "1" : "0");
			AppendChild(node, "AlternatingTime", AlternatingTime.ToString());
			AppendChild(node, "Description", Description);
			AppendChild(node, "Link", Link);
		}

		public TestPattern(String image16by9, String image21by9, String image16by9x = null, String image21by9x = null)
		{
			Image16by9 = image16by9;
			Image16by9x = image16by9x;
			Image21by9 = image21by9;
			Image21by9x = image21by9x;

			IsAlternating = false;
			AlternatingTime = 0;
		}

		public TestPattern(TestPattern tp)
		{
			Name = tp.Name;

			Image16by9 = tp.Image16by9;
			Image16by9x = tp.Image16by9x;
			Image21by9 = tp.Image21by9;
			Image21by9x = tp.Image21by9x;

			IsAlternating = tp.IsAlternating;
			AlternatingTime = tp.AlternatingTime;

			Description = tp.Description;
			Link = tp.Link;
		}

		public String Name
		{
			get;
			set;
		}

		public String Image16by9
		{
			get;
			set;
		}

		public String Image16by9x
		{
			get;
			set;
		}

		public String Image21by9
		{
			get;
			set;
		}

		public String Image21by9x
		{
			get;
			set;
		}

		public bool IsAlternating
		{
			get;
			set;
		}

		public int AlternatingTime
		{
			get;
			set;
		}

		public String Description
		{
			get;
			set;
		}

		public String Link
		{
			get;
			set;
		}

		public override String ToString()
		{
			return Name;
		}
	}
}
