using MediaPortal.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace MediaPortal.Player.TestPatterns
{
	public class TestPatternList
	{
		List<MediaPortal.Player.TestPatterns.TestPattern> _testPatterns = new List<Player.TestPatterns.TestPattern>();

		public TestPatternList()
		{			
		}

		public void Load()
		{
			XmlDocument patternsDoc = new XmlDocument();

			String patternFile = Path.Combine(Config.GetFolder(Config.Dir.Config), "TestPatterns\\TestPatterns.xml");

			patternsDoc.Load(patternFile);

			XmlNode deviceNode = patternsDoc.GetElementsByTagName("TestPatterns").Item(0);

			foreach (XmlNode node in deviceNode.ChildNodes)
			{
				_testPatterns.Add(new Player.TestPatterns.TestPattern(node));
			}
		}

		public void Save()
		{
			XmlDocument patternsDoc = new XmlDocument();

			String patternFile = Path.Combine(Config.GetFolder(Config.Dir.Config), "TestPatterns\\TestPatterns.xml");

			patternsDoc.Load(patternFile);

			XmlNode deviceNode = patternsDoc.GetElementsByTagName("TestPatterns").Item(0);

			deviceNode.RemoveAll();

			foreach (TestPattern tp in _testPatterns)
			{
				XmlNode node = patternsDoc.CreateNode(XmlNodeType.Element, "TestPattern", "");
				deviceNode.AppendChild(node);
				tp.ToNode(node);
			}

			patternsDoc.Save(patternFile);
		}

		public static String GetPath()
		{
			return Path.Combine(Config.GetFolder(Config.Dir.Config), "TestPatterns\\");
		}

		public List<TestPattern> TestPatterns
		{
			get
			{
				return _testPatterns;
			}

			set
			{
				_testPatterns = value;
			}
		}
	}
}
