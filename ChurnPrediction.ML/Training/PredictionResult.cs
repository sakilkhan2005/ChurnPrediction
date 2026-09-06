using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace ChurnPrediction.ML.Training
{
    public class PredictionResult
    {
        public bool Label { get; set; }
        public float Probability { get; set; }
    }

    public class ChurnPredictionOutput
    {
        [ColumnName("PredictedLabel")]
        public bool PredictedLabel { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }
}
