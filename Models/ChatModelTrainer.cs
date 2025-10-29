﻿using Microsoft.ML;
using Microsoft.ML.Data;
using System.IO;

namespace AIandMLChatbotApp.Models
{

    public class ChatData {
        [LoadColumn (0)] public string Question { get; set; }
        [LoadColumn (1)] public string Answer { get; set; }
    }

    public class ChatPrediction
    {
        public string PredictedLabel { get; set; }
        public float[] Score { get; set; }
    }

    public class ChatResponse
    {
        public string Answer { get; set; }
        public float Confidence { get; set; }
    }
    public class ChatModelTrainer
    {

        public static void TrainModel(string csvPath = null)
        {
            var mlContext = new MLContext();

            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string modelPath = Path.Combine(folder, "chatbotModel.zip");
            string dataPath = Path.Combine(folder, "trainingData.csv");


            var data = mlContext.Data.LoadFromTextFile<ChatData>(
                path: dataPath,
                hasHeader: true,
                separatorChar: ',');

            var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

            var pipeline = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(ChatData.Question))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: nameof(ChatData.Answer), outputColumnName: "Label"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                    labelColumnName: "Label",
                    featureColumnName: "Features"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"))
                .AppendCacheCheckpoint(mlContext);

            var model = pipeline.Fit(split.TrainSet);

            var predictions = model.Transform(split.TestSet);
            var metrics = mlContext.MulticlassClassification.Evaluate(
                data: predictions,
                labelColumnName: "Label",
                scoreColumnName: "Score");
            System.Console.WriteLine($"MicroAccuracy: {metrics.MicroAccuracy:P2}");
            System.Console.WriteLine($"MacroAccuracy: {metrics.MacroAccuracy:P2}");

            mlContext.Model.Save(model, split.TrainSet.Schema, modelPath);
            System.Console.WriteLine("Model saved to " + modelPath);
        }

        public static ChatResponse Predict(string question)
        {
            var mlContext = new MLContext();

            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string modelPath = Path.Combine(folder, "chatbotModel.zip");

            ITransformer loadedModel = mlContext.Model.Load(modelPath, out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ChatData, ChatPrediction>(loadedModel);

            var input = new ChatData { Question = question };
            var prediction = predEngine.Predict(input);

            float maxScore = prediction.Score.Max();

            string answer;
            if (maxScore < 0.6)
            {
                answer = "I'm not confident in my answer. Could you please clarify or rephrase your question?.";
            }
            else
            {
                answer = prediction.PredictedLabel;
            }

            return new ChatResponse
            {
                Answer = answer,
                Confidence = maxScore
            };
        }
    }
}
