using ITLec.XmlValidation.Csv;
using ITLec.XmlValidation.Xml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ITLec.XmlValidation
{
    class Program
    {
        static void Main(string[] args)
        {
            XMLValidation();
        }


        public static string XMLValidation()
        {
            string retVal = "";


            string csvRuleFilePath = @"ITLecRulesRegEx.csv";
            string xmlFile = @"ITLecXmlFile.xml";

            string generatedFilePath = @"ITLecValidationResult.csv";


            Console.WriteLine("Reading Xml file ...");
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(xmlFile);



            string str = doc.InnerXml;
            Dictionary<string, string> dic = new Dictionary<string, string>();

            var dicArr = XmlHelper.GetAllNodesInXml(doc, new Dictionary<string, string>());


            var csvRuleTable =Csv.CsvHelper.ConvertCSVToDataTable(csvRuleFilePath, "XPath,Desc,RegEx,RegExMSG,IsMandatory".Split(','));

            Console.WriteLine("Reading Validation Rules file ...");

            System.Data.DataTable validationTable = new System.Data.DataTable();
            validationTable.Columns.Add("XPath");
            validationTable.Columns.Add("Value");
            validationTable.Columns.Add("Desc");
            validationTable.Columns.Add("RegEx");
            validationTable.Columns.Add("RegExMSG");
            validationTable.Columns.Add("IsMandatory");


            List<KeyValuePair<string, bool>> xmlNodesExisted = new List<KeyValuePair<string, bool>>();


            foreach (System.Data.DataRow ruleRow in csvRuleTable.Rows)
            {


                string xPath = ruleRow["XPath"].ToString();
                bool isRegEx = (ruleRow["RegEx"] != null && !string.IsNullOrWhiteSpace(ruleRow["RegEx"].ToString())) ? true : false;
                bool isMandatory = (ruleRow["IsMandatory"] != null && !string.IsNullOrWhiteSpace(ruleRow["IsMandatory"].ToString())) ? (ruleRow["IsMandatory"].ToString().ToLower() == "true") ? true : false : false;
                string ruleXPath = xPath;

                if (isMandatory)
                {
                    var list = IsXmlNodeExisted(doc, xPath, "");

                    foreach (var item in list.Where(e => e.Value == false))
                    {
                        DataRow validationDataRow = validationTable.NewRow();

                        validationDataRow["XPath"] = item.Key;
                        validationDataRow["Value"] = "";
                        validationDataRow["Desc"] = ruleRow["Desc"];
                        validationDataRow["RegEx"] = ruleRow["RegEx"];
                        validationDataRow["IsMandatory"] = ruleRow["IsMandatory"];
                        validationDataRow["RegExMSG"] = "Data Required";
                        validationTable.Rows.Add(validationDataRow);
                    }
                }

                foreach (var dicElement in dicArr)
                {

                    bool isFound = false;

                    string itemXPathRegEx = dicElement.Key;
                    foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(dicElement.Key, @"\[\d]"))
                    {
                        itemXPathRegEx = itemXPathRegEx.Replace(match.Value, "\\[n]");
                    }
                    itemXPathRegEx = itemXPathRegEx + "$";
                    if (System.Text.RegularExpressions.Regex.IsMatch(ruleXPath, itemXPathRegEx, System.Text.RegularExpressions.RegexOptions.RightToLeft))
                    {
                        DataRow validationDataRow = validationTable.NewRow();

                        validationDataRow["XPath"] = dicElement.Key;
                        validationDataRow["Value"] = dicElement.Value;
                        validationDataRow["Desc"] = ruleRow["Desc"];
                        validationDataRow["RegEx"] = ruleRow["RegEx"];
                        validationDataRow["IsMandatory"] = ruleRow["IsMandatory"];

                        if (!string.IsNullOrEmpty(dicElement.Value))
                        {
                            if (isRegEx)
                            {
                                if (ruleRow["RegEx"].ToString().StartsWith("ITLec:"))
                                {
                                    if (!IsITLecCustomRegExValid(doc, dicElement.Key, dicElement.Value, ruleRow["RegEx"].ToString().Replace("ITLec:", "")))
                                    {
                                        validationDataRow["RegExMSG"] = ruleRow["RegExMSG"].ToString();
                                    }
                                }
                                else if (!
                                     System.Text.RegularExpressions.Regex.IsMatch(dicElement.Value, ruleRow["RegEx"].ToString()))
                                {
                                    validationDataRow["RegExMSG"] = ruleRow["RegExMSG"].ToString();
                                }
                            }
                        }
                        else if (isMandatory)
                        {

                            validationDataRow["RegExMSG"] = "Required Data";
                        }
                        validationTable.Rows.Add(validationDataRow);
                        isFound = true;
                    }
                    if (isFound)
                    {
                        break;
                    }
                }
            }




            CsvHelper.SaveLastPrintedTableToCSV(generatedFilePath, validationTable);


            Console.WriteLine("Validation Has been completed, FileName: " + generatedFilePath);


            return retVal;
        }

        private static List<KeyValuePair<string, bool>> IsXmlNodeExisted(XmlNode doc, string xPath, string fullXPath)
        {
            if (xPath.StartsWith("/"))
            {
                xPath = xPath.Substring(1);
            }

            var fff = doc.Name;
            //   throw new NotImplementedException();
            List<KeyValuePair<string, bool>> dic = new List<KeyValuePair<string, bool>>();
            string[] array = xPath.Split(new char[] { '/' }, 2);

            string path = array[0].Replace("[n]", "");
            var nodes = doc.SelectNodes("./" + path);


            int countAllSubItems = xPath.Split('/').ToList().Count;


            if (nodes.Count > 0 && countAllSubItems < 2)
            {
                fullXPath = fullXPath + "/" + path + "[0]";
                dic.Add(new KeyValuePair<string, bool>(fullXPath, true));
            }
            else
            if (nodes.Count == 0 && countAllSubItems < 2)
            {
                fullXPath = fullXPath + "/" + path + "[0]";
                dic.Add(new KeyValuePair<string, bool>(fullXPath, false));
            }
            else
            if (nodes.Count > 0)
            {
                int counter = 0;
                foreach (XmlNode node in nodes)
                {

                    dic.AddRange(IsXmlNodeExisted(node, $"{array[1]}", $"{fullXPath}/{path}[{counter++}]"));
                }
            }
            else
            {
                fullXPath = fullXPath + "/" + path + "[0]";
                dic.Add(new KeyValuePair<string, bool>(fullXPath, false));
            }

            return dic;
        }


        private static bool IsITLecCustomRegExValid(System.Xml.XmlDocument doc, string tagName, string tagValue, string customRegEx)
        {
            bool retVal = false;

            var regExBetween = new System.Text.RegularExpressions.Regex(@"Between\((\d*),(\d*)\)");
            var matchBetween = regExBetween.Match(customRegEx);
            if (matchBetween.Success)
            {
                decimal lower = decimal.Parse(matchBetween.Groups[1].Value);
                decimal upper = decimal.Parse(matchBetween.Groups[2].Value);
                decimal num;

                if (decimal.TryParse(tagValue, out num))
                {
                    if (lower <= num && num <= upper)
                    {
                        return true;
                    }
                }
            }

            var regExDecimal = new System.Text.RegularExpressions.Regex(@"Decimal\((\d*),(\d*)\)");
            var matchDecimal = regExDecimal.Match(customRegEx);
            if (matchDecimal.Success)
            {
                int decimalNum = int.Parse(matchDecimal.Groups[1].Value);
                int decimalPlaces = int.Parse(matchDecimal.Groups[2].Value);
                decimal num;

                if (decimal.TryParse(tagValue, out num))
                {
                    return IsDecimalFormat(num.ToString(), decimalNum, decimalPlaces);
                }
            }

            //=========

            var regExCount = new System.Text.RegularExpressions.Regex(@"Count\((.*)\)");
            var matchCount = regExCount.Match(customRegEx);

            if (matchCount.Success)
            {
                string _xpath = matchCount.Groups[1].Value;
                try
                {
                    string relativePathXPath = getRelateivePath(tagName, _xpath);
                    int numOfTags = doc.SelectNodes(relativePathXPath).Count;

                    if (numOfTags == int.Parse(tagValue))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }catch
                {
                    return false;
                }
            }

            //=========


            var regExRN = new System.Text.RegularExpressions.Regex(@"RequiredNode\((.*)\)");
            var matchRN = regExCount.Match(customRegEx);

            if (matchRN.Success)
            {
                string _xpath = matchCount.Groups[1].Value;
                string relativePathXPath = getRelateivePath(tagName, _xpath);
                int numOfTags = doc.SelectNodes(relativePathXPath).Count;

                if (numOfTags > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            //==============

            if (customRegEx.ToLower() == "uniquetagname")
            {

                string absoluteTag = getAbsolutePath(tagName);
                int numOfTagsWithTheSameName = doc.SelectNodes(absoluteTag).Count;
                if (numOfTagsWithTheSameName > 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }


            if (customRegEx.ToLower() == "uniquetagvalue")
            {

                string absoluteTag = getAbsolutePath(tagName);

                int numOfTagsWithTheSameValue = 0;
                foreach (System.Xml.XmlNode node in doc.SelectNodes(absoluteTag))
                {
                    if (node.InnerText == tagValue)
                    {
                        numOfTagsWithTheSameValue++;
                    }
                    if (numOfTagsWithTheSameValue > 1)
                    {
                        return false;
                    }
                }
                return true;
            }


            return retVal;
        }

        private static string getRelateivePath(string tagName, string regExTagName)
        {
            string retVal = "";
            List<KeyValuePair<string, int>> currentTagNodes = new List<KeyValuePair<string, int>>();
            foreach (var tag in tagName.Split('/').ToList())
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"(.*)\[(\d)\]");
                var matchCount = regex.Match(tag);

                if (matchCount.Success)
                {
                    KeyValuePair<string, int> keyValuePair = new KeyValuePair<string, int>(matchCount.Groups[1].Value, int.Parse(matchCount.Groups[2].Value));

                    currentTagNodes.Add(keyValuePair);
                }
            }

            foreach (var tag in regExTagName.Split('/').ToList())
            {


                if (!string.IsNullOrEmpty(tag))
                {
                    if (tag.EndsWith("[n]"))
                    {
                        string absolut = tag.Replace("[n]", "");

                        retVal = retVal + @"/" + absolut + "[" + currentTagNodes.Where(e => e.Key == absolut).First().Value.ToString() + "]";
                    }
                    else
                    {
                        retVal = retVal + @"/" + tag;
                    }
                }
            }



            return retVal;
        }
        private static string getAbsolutePath(string tagName)
        {
            string retVal = "";
            List<string> nodes = new List<string>();
            foreach (var tag in tagName.Split('/').ToList())
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"(.*)(\[\d\])");


                string cleanString = regex.Replace(tag, "$1");

                nodes.Add(cleanString);
            }




            foreach (var node in nodes)
            {
                if (!string.IsNullOrEmpty(node))
                    retVal = retVal + @"/" + node;

            }

            return retVal;
        }

        public static bool IsDecimalFormat(string val, int decimalNumber, int decimalPlaces)
        {
            bool retVal = false;

            try
            {
                decimal valDecimal = decimal.Parse(val);

                string pattern = "^[-+]{0,1}[0-9]{0,decimalNumber}\\.[0-9]{0,decimalPlaces}$|^[-+]{0,1}[0-9]{0,decimalNumber}$|^[-+]\\.[0-9]{0,decimalPlaces}$";

                pattern = pattern.Replace("decimalNumber", decimalNumber.ToString()).Replace("decimalPlaces", decimalPlaces.ToString());
                retVal = System.Text.RegularExpressions.Regex.IsMatch(val, pattern);
            }
            catch (Exception ex)
            {

            }

            return retVal;
        }

    }
}
