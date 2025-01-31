﻿using System;
using System.Threading.Tasks;
using API;
using DocumentsIndex.Contracts;

namespace SearchDocumentsAPI.Services.DocumentsIndex
{
    /// <summary>
    /// Сервис для взаимодействия с индексацией документов.
    /// </summary>
    public interface IDocumentsIndexService
    {
        /// <summary>
        /// Индексирует документ для дальнейшего использования при поиске.
        /// </summary>
        /// <param name="document">Документ, который нужно проиндексировать</param>
        Task<RequestResult<Document>> IndexDocumentAsync(Document document);

        /// <summary>
        /// Удаляет документ из индексации.
        /// </summary>
        /// <param name="id">Id документа</param>
        Task<RequestResult<Document>> DeleteAsync(Guid id);
    }
}