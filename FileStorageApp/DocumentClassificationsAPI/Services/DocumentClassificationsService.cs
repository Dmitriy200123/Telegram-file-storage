﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API;
using Data;
using DocumentClassificationsAPI.Mappers;
using DocumentClassificationsAPI.Models;
using FileStorageApp.Data.InfoStorage.Factories;
using DocumentClassification = FileStorageApp.Data.InfoStorage.Contracts.Classification;
using DocumentClassificationWord = FileStorageApp.Data.InfoStorage.Contracts.ClassificationWord;

namespace DocumentClassificationsAPI.Services
{
    /// <inheritdoc />
    public class DocumentClassificationsService : IDocumentClassificationsService
    {
        private readonly IInfoStorageFactory _infoStorageFactory;

        /// <summary>
        /// Инициализирует экземпляр класса <see cref="DocumentClassificationsService"/>
        /// </summary>
        /// <param name="infoStorageFactory">Фабрика хранилищ</param>
        public DocumentClassificationsService(IInfoStorageFactory infoStorageFactory)
        {
            _infoStorageFactory = infoStorageFactory ??
                                  throw new ArgumentNullException(nameof(infoStorageFactory));
        }

        /// <inheritdoc />
        public async Task<RequestResult<Classification>> FindClassificationByIdAsync(
            Guid id,
            bool includeClassificationWords
        )
        {
            using var storage = _infoStorageFactory.CreateDocumentClassificationStorage();
            var documentClassification = await storage.FindByIdAsync(id, includeClassificationWords);

            return documentClassification == null
                ? RequestResult.NotFound<Classification>($"{nameof(Classification)} with Id {id} not found")
                : RequestResult.Ok(documentClassification.ToClassification());
        }

        /// <inheritdoc />
        public async Task<RequestResult<IEnumerable<Classification>>> FindClassificationByQueryAsync(
            string query,
            int skip,
            int take,
            bool includeClassificationWords
        )
        {
            using var storage = _infoStorageFactory.CreateDocumentClassificationStorage();
            var documentClassifications = await storage.FindByQueryAsync(
                query,
                skip,
                take,
                includeClassificationWords
            );
            var classifications = documentClassifications
                .Select(classification => classification.ToClassification());

            return RequestResult.Ok(classifications);
        }

        /// <inheritdoc />
        public async Task<RequestResult<bool>> AddClassificationAsync(ClassificationInsert classification)
        {
            using var storage = _infoStorageFactory.CreateDocumentClassificationStorage();
            var documentClassifications = classification.ToDocumentClassification();
            var added = await storage.AddAsync(documentClassifications);

            if (added)
                return RequestResult.Ok(true);

            var classifications = await storage
                .FindByQueryAsync(documentClassifications.Name, 0, 1);
            var dbClassification = classifications.FirstOrDefault();

            if (dbClassification == null)
                throw new InvalidOperationException($"Cannot save {nameof(DocumentClassification)} in database");

            return RequestResult.BadRequest<bool>(
                $"{nameof(classification)} with Name {classification.Name} already exist");
        }

        /// <inheritdoc />
        public async Task<RequestResult<bool>> DeleteClassificationAsync(Guid id)
        {
            using var storage = _infoStorageFactory.CreateDocumentClassificationStorage();

            try
            {
                var deleted = await storage.DeleteAsync(id);

                return deleted
                    ? RequestResult.NoContent<bool>()
                    : throw new InvalidOperationException(
                        $"Cannot delete {nameof(DocumentClassification)} with Id {id}");
            }
            catch (ArgumentException exception)
            {
                return RequestResult.NotFound<bool>(exception.Message);
            }
        }

        /// <inheritdoc />
        public async Task<RequestResult<bool>> RenameClassificationAsync(Guid id, string newName)
        {
            using var storage = _infoStorageFactory.CreateDocumentClassificationStorage();

            try
            {
                var renamed = await storage.RenameAsync(id, newName);
                return renamed
                    ? RequestResult.Ok(true)
                    : throw new InvalidOperationException(
                        $"Cannot rename {nameof(DocumentClassification)} with Id {id}"
                    );
            }
            catch (NotFoundException exception)
            {
                return RequestResult.NotFound<bool>(exception.Message);
            }
            catch (AlreadyExistException exception)
            {
                return RequestResult.BadRequest<bool>(exception.Message);
            }
        }

        /// <inheritdoc />
        public async Task<RequestResult<int>> GetCountClassificationsByQueryAsync(string query)
        {
            using var storage = _infoStorageFactory.CreateDocumentClassificationStorage();
            var documentsCount = await storage.GetCountByQueryAsync(query);

            return RequestResult.Ok(documentsCount);
        }

        /// <inheritdoc />
        public async Task<RequestResult<Guid>> AddWordToClassificationAsync(
            Guid classificationId,
            ClassificationWordInsert classificationWord
        )
        {
            using var storage = _infoStorageFactory.CreateDocumentClassificationStorage();
            var documentClassificationWord = classificationWord.ToDocumentClassificationWord();

            try
            {
                var wordId = await storage.AddWordAsync(classificationId, documentClassificationWord);

                return RequestResult.Ok(wordId);
            }
            catch (AlreadyExistException exception)
            {
                return RequestResult.BadRequest<Guid>(exception.Message);
            }
            catch (NotFoundException exception)
            {
                return RequestResult.NotFound<Guid>(exception.Message);
            }
        }

        /// <inheritdoc />
        public async Task<RequestResult<bool>> DeleteWordAsync(Guid wordId)
        {
            using var storage = _infoStorageFactory.CreateDocumentClassificationStorage();

            try
            {
                var deleted = await storage.DeleteWordAsync(wordId);

                return deleted
                    ? RequestResult.NoContent<bool>()
                    : throw new InvalidOperationException(
                        $"Cannot delete {nameof(DocumentClassificationWord)} with Id {wordId}"
                    );
            }
            catch (NotFoundException exception)
            {
                return RequestResult.NotFound<bool>(exception.Message);
            }
        }

        /// <inheritdoc />
        public async Task<RequestResult<IEnumerable<ClassificationWord>>> GetWordsByClassificationIdAsync(
            Guid classificationId
        )
        {
            using var storage = _infoStorageFactory.CreateDocumentClassificationStorage();

            try
            {
                var documentClassificationWords = await storage.GetWordsByIdAsync(classificationId);
                var classificationWords = documentClassificationWords.Select(word => word.ToClassificationWord());

                return RequestResult.Ok(classificationWords);
            }
            catch (NotFoundException exception)
            {
                return RequestResult.NotFound<IEnumerable<ClassificationWord>>(exception.Message);
            }
        }
    }
}