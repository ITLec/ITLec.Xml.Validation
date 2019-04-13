using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ITLec.XmlValidation.Xml
{
 public   class XmlHelper
    {


        public static Dictionary<string, string> GetAllNodesInXml(System.Xml.XmlNode xmlNode, Dictionary<string, string> nodesDic)
        {

            if (xmlNode.NodeType == XmlNodeType.Comment || xmlNode.NodeType == XmlNodeType.XmlDeclaration || xmlNode.NodeType == XmlNodeType.ProcessingInstruction)
            {
                return nodesDic;
            }

            if (!xmlNode.HasChildNodes ||

                (xmlNode.NodeType != XmlNodeType.Attribute && xmlNode.NodeType != XmlNodeType.Element && xmlNode.NodeType != XmlNodeType.Document))
            {
                nodesDic.Add(FindXPath(xmlNode), xmlNode.InnerText);
            }
            else
            {
                if (xmlNode.Attributes != null && xmlNode.Attributes.Count > 0)
                {
                    foreach (XmlAttribute att in xmlNode.Attributes)
                    {
                        //     nodesDic.Add(FindXPath(xmlNode) + "." + att.Name, att.InnerXml);
                        nodesDic.Add(FindXPath(att), att.InnerXml);
                    }
                }
            }

            foreach (XmlNode xmlChildNode in xmlNode.ChildNodes)
            {
                foreach (var dicElement in GetAllNodesInXml(xmlChildNode, nodesDic))
                {
                    if (!nodesDic.ContainsKey(dicElement.Key))
                    {
                        nodesDic.Add(dicElement.Key, dicElement.Value);
                    }
                }
            }

            return nodesDic;
        }

        static string FindXPath(XmlNode node)
        {
            StringBuilder builder = new StringBuilder();
            while (node != null)
            {
                switch (node.NodeType)
                {

                    case XmlNodeType.Text:
                        return FindXPath(node.ParentNode);
                        break;
                    case XmlNodeType.Attribute:
                        builder.Insert(0, "/@" + node.Name);
                        node = ((XmlAttribute)node).OwnerElement;
                        break;
                    case XmlNodeType.Element:
                        int index = FindElementIndex((XmlElement)node);
                        builder.Insert(0, "/" + node.Name + "[" + index + "]");
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Document:
                        return builder.ToString();
                    default:
                        throw new ArgumentException("Only elements and attributes are supported");
                }
            }
            throw new ArgumentException("Node was not in a document");
        }

        static int FindElementIndex(XmlElement element)
        {
            XmlNode parentNode = element.ParentNode;
            if (parentNode is XmlDocument)
            {
                return 1;
            }
            XmlElement parent = (XmlElement)parentNode;
            int index = 1;
            foreach (XmlNode candidate in parent.ChildNodes)
            {
                if (candidate is XmlElement && candidate.Name == element.Name)
                {
                    if (candidate == element)
                    {
                        return index;
                    }
                    index++;
                }
            }
            throw new ArgumentException("Couldn't find element within parent");
        }

    }
}
