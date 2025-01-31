﻿using System.Linq;
using DocumentClassificationsAPI.Models;
using MoreLinq;
using DocumentClassification = FileStorageApp.Data.InfoStorage.Contracts.Classification;
using DocumentClassificationWord = FileStorageApp.Data.InfoStorage.Contracts.ClassificationWord;

namespace DocumentClassificationsAPI.Mappers
{
    internal static class DocumentClassificationMapper
    {
        public static Classification ToClassification(this DocumentClassification documentClassification)
        {
            var words = documentClassification
                .ClassificationWords
                .Select(word => word.ToClassificationWord())
                .ToList();

            return new Classification
            {
                Id = documentClassification.Id,
                Name = documentClassification.Name,
                ClassificationWords = words
            };
        }

        public static DocumentClassification ToDocumentClassification(this ClassificationInsert classification)
        {
            var words = classification
                .ClassificationWords
                .DistinctBy(insert => insert.Value)
                .Select(word => word.ToDocumentClassificationWord())
                .ToList();

            return new DocumentClassification
            {
                Name = classification.Name,
                ClassificationWords = words
            };
        }

        public static DocumentClassificationWord ToDocumentClassificationWord(
            this ClassificationWordInsert classificationWord
        ) => new() { Value = classificationWord.Value };

        public static ClassificationWord ToClassificationWord(
            this DocumentClassificationWord documentClassificationWord
        ) => new()
        {
            Id = documentClassificationWord.Id,
            Value = documentClassificationWord.Value,
            ClassificationId = documentClassificationWord.ClassificationId
        };
    }
}