﻿using System;
using System.Text;
using DocumentsIndex.Config;
using Elasticsearch.Net;
using Nest;

namespace DocumentsIndex.Factories
{
    /// <summary>
    /// Базовый класс для создания индексации
    /// </summary>
    public abstract class BaseIndexFactory
    {
        protected IDocumentIndexStorage CreateDocumentIndexStorage(IElasticConfig elasticConfig,
            Func<PutPipelineDescriptor, PutPipelineDescriptor> pipelineDescriptor,
            Func<CreateIndexDescriptor, CreateIndexDescriptor> mapping)
        {
            var settings = new ConnectionSettings(new Uri(elasticConfig.Uri))
                .DefaultIndex(elasticConfig.Index)
                .DisableDirectStreaming()
                .PrettyJson()
                .DefaultFieldNameInferrer(p => p)
                .OnRequestCompleted(CreateLogging);

            var elasticClient = new ElasticClient(settings);

            if (!elasticClient.Indices.Exists(elasticConfig.Index).Exists)
                elasticClient.Indices.Create(elasticConfig.Index, mapping);

            elasticClient.Ingest.PutPipeline(elasticConfig.Index, pipelineDescriptor);
            return new DocumentIndexStorage(elasticClient);
        }

        private static void CreateLogging(IApiCallDetails callDetails)
        {
            {
                if (callDetails.RequestBodyInBytes != null)
                {
                    var requestBody = Encoding.UTF8.GetString(callDetails.RequestBodyInBytes);
                    Console.WriteLine(
                        $"{callDetails.HttpMethod} {callDetails.Uri} \n" +
                        $"{requestBody[..Math.Min(requestBody.Length, 500)]}");
                }
                else
                {
                    Console.WriteLine($"{callDetails.HttpMethod} {callDetails.Uri}");
                }

                Console.WriteLine();

                if (callDetails.ResponseBodyInBytes != null)
                {
                    var responseBody = Encoding.UTF8.GetString(callDetails.ResponseBodyInBytes);
                    Console.WriteLine($"Status: {callDetails.HttpStatusCode}\n" +
                                      $"{responseBody[..Math.Min(responseBody.Length, 500)]}\n" +
                                      $"{new string('-', 30)}\n");
                }
                else
                {
                    Console.WriteLine($"Status: {callDetails.HttpStatusCode}\n" +
                                      $"{new string('-', 30)}\n");
                }
            }
        }
    }
}