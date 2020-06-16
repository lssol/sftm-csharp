using tree_matching_csharp.Visualization.Models;

namespace tree_matching_csharp.Visualization.Utils
{
    public static class Utils
    {
        public static CytoElementDef ToCyto(this Node node)
        {
            return new CytoElementDef
            {
                Group = "nodes",
                Data = new CytoElementDef.DataClass
                {
                    Id = node.Id.ToString(),
                    Label = string.Join(" ", node.Value),
                    Parent = node.Parent?.Id.ToString(),
                    Value = node.Value
                }
            };
        }
    }
}