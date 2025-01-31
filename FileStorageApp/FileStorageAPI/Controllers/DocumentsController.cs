﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using FileStorageAPI.Extensions;
using FileStorageAPI.Models;
using FileStorageAPI.Services;
using FileStorageApp.Data.InfoStorage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RightServices;
using Swashbuckle.AspNetCore.Annotations;
using NotFoundResult = API.NotFoundResult;

namespace FileStorageAPI.Controllers
{
    /// <summary>
    /// API информации о файлах типа "Текстовый документ".
    /// </summary>
    [ApiController]
    [Route("api/files/documents")]
    [SwaggerTag("Информация о документах")]
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly IDocumentsService _documentsService;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DocumentsController"/>.
        /// </summary>
        /// <param name="documentsService">Сервис для взаимодействия с документами</param>
        public DocumentsController(IDocumentsService documentsService)
        {
            _documentsService = documentsService ?? throw new ArgumentNullException(nameof(documentsService));
        }

        /// <summary>
        /// Возвращает количество файлов типа "Текстовый документ".
        /// </summary>
        /// <exception cref="ArgumentException">Может выброситься, если контроллер не ожидает такой HTTP код</exception>
        [HttpGet("count")]
        [SwaggerResponse(StatusCodes.Status200OK, "Возвращает количество файлов типа \"Текстовый документ\", содержащихся в хранилище", typeof(int))]
        public async Task<IActionResult> GetDocumentsCount([FromQuery] DocumentSearchParameters fileSearchParameters)
        {
            var count = await _documentsService.GetDocumentsCountAsync(fileSearchParameters, Request);

            return count.ResponseCode switch
            {
                HttpStatusCode.OK => Ok(count.Value),
                _ => throw new ArgumentException("Unknown response code")
            };
        }

        /// <summary>
        /// Возвращает список файлов типа "Текстовый документ".
        /// </summary>
        /// <param name="documentSearchParameters">Параметры поиска документов</param>
        /// <param name="skip">Количество пропускаемых элементов</param>
        /// <param name="take">Количество возвращаемых элементов</param>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "Возвращает все доступные файлы типа \"Текстовый документ\" для текущего пользователя", typeof(List<DocumentInfo>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Если skip или take меньше 0", typeof(string))]
        public async Task<IActionResult> GetDocumentInfos([FromQuery] DocumentSearchParameters documentSearchParameters, [FromQuery, Required] int skip, [FromQuery, Required] int take)
        {
            var files = await _documentsService.GetDocumentInfosAsync(documentSearchParameters, skip, take, Request);

            return files.ResponseCode switch
            {
                HttpStatusCode.OK => Ok(files.Value),
                HttpStatusCode.BadRequest => BadRequest(files.Message),
                _ => throw new ArgumentException("Unknown response code")
            };
        }

        /// <summary>
        /// Возвращает документ по Id
        /// </summary>
        /// <param name="documentId">Id документа</param>
        [HttpGet("{documentId:guid}")]
        [SwaggerResponse(StatusCodes.Status200OK, "Возвращает документ", typeof(DocumentInfo))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Если документ с таким Id не найден", typeof(string))]
        public async Task<IActionResult> FindDocumentById(Guid documentId)
        {
            var files = await _documentsService.FindDocumentById(documentId);

            return files.ResponseCode switch
            {
                HttpStatusCode.OK => Ok(files.Value),
                HttpStatusCode.NotFound => NotFound(files.Message),
                _ => throw new ArgumentException("Unknown response code")
            };
        }

        /// <summary>
        /// Возвращает классификацию по Id документа. Требуется право "ViewClassifications".
        /// </summary>
        /// <param name="documentId">Id документа</param>
        [HttpGet("{documentId:guid}/classification")]
        [SwaggerResponse(StatusCodes.Status200OK, "Возвращает классификацию", typeof(ClassificationInfo))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Если документ с таким Id не найден. ", typeof(NotFoundResult))]
        [RightsFilter(Access.ViewClassifications)]
        public async Task<IActionResult> FindClassificationByDocumentId(Guid documentId)
        {
            var result = await _documentsService.FindClassificationByDocumentId(documentId);

            return result.ResponseCode switch
            {
                HttpStatusCode.OK => Ok(result.Value),
                HttpStatusCode.NotFound => NotFound(result.ToNotFoundResult()),
                _ => throw new ArgumentException("Unknown response code")
            };
        }

        /// <summary>
        /// Добавляет классификацию документу. Требуется право "AssignClassificationsToDocuments".
        /// </summary>
        /// <param name="documentId">Id документа</param>
        /// <param name="documentClassificationId">Идентификатор классификации</param>
        [HttpPatch("{documentId:guid}/assign-classification")]
        [SwaggerResponse(StatusCodes.Status200OK, "Возвращает документ", typeof(DocumentInfo))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Если не найдено документа или классификации", typeof(string))]
        [RightsFilter(Access.AssignClassificationsToDocuments)]
        public async Task<IActionResult> AddClassification(Guid documentId, [FromBody] Guid documentClassificationId)
        {
            var result = await _documentsService.AddClassification(documentId, documentClassificationId);

            return result.ResponseCode switch
            {
                HttpStatusCode.OK => Ok(result.Value),
                HttpStatusCode.NotFound => NotFound(result.ToNotFoundResult()),
                _ => throw new ArgumentException("Unknown response code")
            };
        }

        /// <summary>
        /// Удаляет классификацию у документа. Требуется право "RevokeClassificationsFromDocument".
        /// </summary>
        /// <param name="documentId">Id документа</param>
        /// <param name="documentClassificationId">Идентификатор классификации</param>
        [HttpPatch("{documentId:guid}/revoke-classification")]
        [SwaggerResponse(StatusCodes.Status200OK, "Возвращает документ", typeof(DocumentInfo))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Если не найдено документа или классификации", typeof(string))]
        [RightsFilter(Access.RevokeClassificationsFromDocument)]
        public async Task<IActionResult> DeleteClassification(Guid documentId, [FromBody] Guid documentClassificationId)
        {
            var result = await _documentsService.DeleteClassification(documentId, documentClassificationId);

            return result.ResponseCode switch
            {
                HttpStatusCode.OK => Ok(result.Value),
                HttpStatusCode.NotFound => NotFound(result.ToNotFoundResult()),
                _ => throw new ArgumentException("Unknown response code")
            };
        }
    }
}