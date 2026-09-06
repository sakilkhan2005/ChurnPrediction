using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Transforms;

namespace ChurnPrediction.ML.Training
{
    
    public class ChurnLabelInput
    {
        public string ChurnLabel { get; set; } = string.Empty;
    }

    public class ChurnLabelOutput
    {
        public bool Label { get; set; }
    }

    // The [CustomMappingFactoryAttribute] name MUST match the contractName
    // used when adding this to the pipeline — this is what lets ML.NET
    // serialize/deserialize the mapping when saving/loading model.zip.
    [CustomMappingFactoryAttribute("ChurnLabelMapping")]
    public class ChurnLabelMappingFactory : CustomMappingFactory<ChurnLabelInput, ChurnLabelOutput>
    {
        public static void MapChurnLabel(ChurnLabelInput input, ChurnLabelOutput output)
        {
            output.Label = input.ChurnLabel == "Yes";
        }

        public override Action<ChurnLabelInput, ChurnLabelOutput> GetMapping() => MapChurnLabel;
    }
}
