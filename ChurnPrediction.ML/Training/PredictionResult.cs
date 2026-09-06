using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurnPrediction.ML.Training
{
    public class PredictionResult
    {
        public bool Label { get; set; }
        public float Probability { get; set; }
    }
}
