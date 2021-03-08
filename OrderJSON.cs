using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataFactoryScriptComparer
{
    public class OrderJson
    {
        public static string Parse(string json)
        {
            var root = JToken.Parse(json);

            Order(root);



            return root.ToString(Newtonsoft.Json.Formatting.Indented);
        }
        static JTokenType[] ComplexJTypes = { JTokenType.Object, JTokenType.Array };
        static JTokenType[] SimpleJTypes = { JTokenType.Integer, JTokenType.Float, JTokenType.String, JTokenType.Boolean, JTokenType.Guid, JTokenType.Date };
        private static void Order(JToken root)
        {
            var type = root.Type;
            if (SimpleJTypes.Contains(type))
            {
                return;
            }
            if (!ComplexJTypes.Contains(type))
            {
                throw new Exception($"{type} not supported");
            }

            var children = root.Children().ToList();

            if (root.Type == JTokenType.Object)
                children = root.Children().OrderBy(c => ((JProperty)c).Name).ToList();
            foreach (var child in children)
            {

                var currentChild = (child.Type == JTokenType.Property)
                ? child.First()
                : child;

                Order(currentChild);
            }

            if (root.Type == JTokenType.Array && SortArrayOfObjects(root, children))
            {
                //root.AddAfterSelf
                var arrayRoot = (JArray)root;
                arrayRoot.RemoveAll();
                foreach (var child in children)
                {
                    arrayRoot.Add(child);
                }

            }
            else if (root.Type == JTokenType.Object && children.Count > 1)
            {
                var objRoot = (JObject)root;
                objRoot.RemoveAll();
                foreach (var child in children)
                {
                    objRoot.Add(child);
                }

            }
        }

        private static bool SortArrayOfObjects(JToken root, List<JToken> children)
        {
            bool changeOccurred = false;

            var kidCount = children.Count;
            if (kidCount > 1 && children.First().Type == JTokenType.Object)
            {

                var keepGoing = true;
                do
                {
                    keepGoing = false;
                    for (int i = 0; i < kidCount - 1; i++)
                    {
                        var currentChild = children[i];
                        var nextChild = children[i + 1];
                        if (CompareChildren(currentChild, nextChild) > 0)
                        {
                            changeOccurred = true;
                            children[i] = nextChild;
                            children[i + 1] = currentChild;
                            keepGoing = true;
                        }

                    }
                } while (keepGoing);

            }

            return changeOccurred;
        }

        private static int CompareChildren(JToken currentChild, JToken nextChild)
        {
            if (currentChild == null)
            {
                return nextChild == null
                    ? 0
                    : -1;
            }
            if (nextChild == null)
            {
                return 1;
            }

            if (currentChild.Type != nextChild.Type)
            {
                throw new Exception($"Can't have different types of children in array: {currentChild.Type}!={nextChild.Type}");
            }

            int comparisson = 0;
            if (SimpleJTypes.Contains(currentChild.Type))
            {
                switch (currentChild.Type)
                {
                    case JTokenType.Integer:
                        comparisson = currentChild.Value<int>().CompareTo(nextChild.Value<int>());
                        break;
                    case JTokenType.Float:
                        comparisson = currentChild.Value<float>().CompareTo(nextChild.Value<float>());
                        break;
                    case JTokenType.String:
                        comparisson = currentChild.Value<string>().CompareTo(nextChild.Value<string>());
                        break;
                    case JTokenType.Boolean:
                        comparisson = currentChild.Value<bool>().CompareTo(nextChild.Value<bool>());
                        break;
                    case JTokenType.Guid:
                        comparisson = currentChild.Value<Guid>().CompareTo(nextChild.Value<Guid>());
                        break;
                    case JTokenType.Date:
                        comparisson = currentChild.Value<DateTime>().CompareTo(nextChild.Value<DateTime>());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{currentChild.Type} not supported");

                }
                return comparisson;
            }
            if (currentChild.Type == JTokenType.Object)
            {
                var allFieldNames = UnionPropertyNames(currentChild, nextChild);
                foreach (var fieldName in allFieldNames)
                {
                    comparisson = CompareChildren(currentChild[fieldName], nextChild[fieldName]);
                    if (comparisson != 0)
                    {
                        return comparisson;
                    }

                }
                return comparisson;
            }
            //an array - don't compare for now
            return 0;
        }

        private static List<string> UnionPropertyNames(JToken currentChild, JToken nextChild)
        {
            var allProps = currentChild.Children().Select(c => ((JProperty)c).Name).ToHashSet();
            foreach (var prop in currentChild.Children())
            {
                var propName = ((JProperty)prop).Name;
                if (!allProps.Contains(propName))
                {
                    allProps.Add(propName);
                }
            }
            var props = allProps.ToList();
            props.Sort();
            return props;
        }
    }
}
