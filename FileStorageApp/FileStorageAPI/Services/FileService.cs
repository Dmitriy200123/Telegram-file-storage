﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FilesStorage.Interfaces;
using FileStorageAPI.Converters;
using FileStorageAPI.Extensions;
using FileStorageAPI.Models;
using FileStorageAPI.Providers;
using FileStorageApp.Data.InfoStorage.Enums;
using FileStorageApp.Data.InfoStorage.Factories;
using Microsoft.AspNetCore.Http;
using Chat = FileStorageApp.Data.InfoStorage.Models.Chat;
using DataBaseFile = FileStorageApp.Data.InfoStorage.Models.File;
using FileInfo = FileStorageAPI.Models.FileInfo;

namespace FileStorageAPI.Services
{
    /// <inheritdoc />
    public class FileService : IFileService
    {
        private readonly IFileInfoConverter _fileInfoConverter;
        private readonly IFileTypeProvider _fileTypeProvider;
        private readonly IFilesStorageFactory _filesStorageFactory;
        private readonly IInfoStorageFactory _infoStorageFactory;
        private readonly IExpressionFileFilterProvider _expressionFileFilterProvider;
        private readonly IDownloadLinkProvider _downloadLinkProvider;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FileService"/>
        /// </summary>
        /// <param name="infoStorageFactory">Фабрика для получения доступа к хранилищу файлов</param>
        /// <param name="fileInfoConverter">Конвертор для преобразования файлов в API-контракты</param>
        /// <param name="filesStorageFactory">Фабрика для получения доступа к физическому хранилищу чатов</param>
        /// <param name="fileTypeProvider">Поставщик типа файла</param>
        /// <param name="expressionFileFilterProvider">Поставщик query Expression для поиска данных</param>
        /// <param name="downloadLinkProvider">Поставщик для получения ссылки на файл</param>
        public FileService(IInfoStorageFactory infoStorageFactory,
            IFileInfoConverter fileInfoConverter,
            IFilesStorageFactory filesStorageFactory,
            IFileTypeProvider fileTypeProvider,
            IExpressionFileFilterProvider expressionFileFilterProvider,
            IDownloadLinkProvider downloadLinkProvider)
        {
            _infoStorageFactory = infoStorageFactory ?? throw new ArgumentNullException(nameof(infoStorageFactory));
            _fileInfoConverter = fileInfoConverter ?? throw new ArgumentNullException(nameof(fileInfoConverter));
            _filesStorageFactory = filesStorageFactory ?? throw new ArgumentNullException(nameof(filesStorageFactory));
            _fileTypeProvider = fileTypeProvider ?? throw new ArgumentNullException(nameof(fileTypeProvider));
            _expressionFileFilterProvider = expressionFileFilterProvider ??
                                            throw new ArgumentNullException(nameof(expressionFileFilterProvider));
            _downloadLinkProvider =
                downloadLinkProvider ?? throw new ArgumentNullException(nameof(downloadLinkProvider));
        }

        /// <inheritdoc />
        public async Task<RequestResult<List<FileInfo>>> GetFileInfosAsync(FileSearchParameters fileSearchParameters,
            int skip, int take, HttpRequest request)
        {
            if (skip < 0 || take < 0)
                return RequestResult.BadRequest<List<FileInfo>>($"Skip or take less than 0");

            using var filesStorage = _infoStorageFactory.CreateFileStorage();
            var expression = _expressionFileFilterProvider.GetExpression(fileSearchParameters);
            var filesFromDataBase = await filesStorage.GetByFilePropertiesAsync(expression, true, skip, take);
            var sender = await request.GetSenderFromToken(_infoStorageFactory);
            var filteredFiles = await filesFromDataBase.FilterFiles(sender!.Id, _infoStorageFactory);
            var convertedFiles = filteredFiles
                .Select(_fileInfoConverter.ConvertFileInfo)
                .ToList();

            return RequestResult.Ok(convertedFiles);
        }

        /// <inheritdoc />
        public async Task<RequestResult<FileInfo>> GetFileInfoByIdAsync(Guid id, HttpRequest request)
        {
            using var filesStorage = _infoStorageFactory.CreateFileStorage();
            var file = await filesStorage.GetByIdAsync(id, true);
            if (file is null)
                return RequestResult.NotFound<FileInfo>($"File with identifier {id} not found");
            var sender = await request.GetSenderFromToken(_infoStorageFactory);
            var filesToFilter = new List<DataBaseFile> {file};
            var filteredFiles = await filesToFilter.FilterFiles(sender!.Id, _infoStorageFactory);
            if (filteredFiles.Count == 0)
                return RequestResult.Unauthorized<FileInfo>("Don't have access to this file");


            return RequestResult.Ok(_fileInfoConverter.ConvertFileInfo(file));
        }

        /// <inheritdoc />
        public async Task<RequestResult<string>> GetFileDownloadLinkByIdAsync(Guid id, HttpRequest request)
        {
            using var filesStorage = _infoStorageFactory.CreateFileStorage();
            var file = await filesStorage.GetByIdAsync(id);
            if (file is null)
                return RequestResult.NotFound<string>($"File with identifier {id} not found");

            var sender = await request.GetSenderFromToken(_infoStorageFactory);
            var filesToFilter = new List<DataBaseFile> {file};
            var filteredFiles = await filesToFilter.FilterFiles(sender!.Id, _infoStorageFactory);
            if (filteredFiles.Count == 0)
                return RequestResult.Unauthorized<string>("Don't have access to this file");
            var result = await _downloadLinkProvider.GetDownloadLinkAsync(id, file.Name);

            return RequestResult.Ok(result);
        }

        /// <inheritdoc />
        public async Task<RequestResult<(string Uri, FileInfo Info)>> CreateFileAsync(IFormFile uploadFile,
            HttpRequest request)
        {
            var fileSender = await request.GetSenderFromToken(_infoStorageFactory);
            if (fileSender is null)
                return RequestResult.BadRequest<(string Uri, FileInfo Info)>("Does not have this sender in database");
            var file = new DataBaseFile
            {
                Id = Guid.NewGuid(),
                Name = uploadFile.FileName,
                Extension = Path.GetExtension(uploadFile.FileName),
                Type = _fileTypeProvider.GetFileType(uploadFile.Headers["Content-Type"]),
                UploadDate = DateTime.Now,
                FileSenderId = fileSender.Id
            };

            await using var memoryStream = new MemoryStream();
            await uploadFile.CopyToAsync(memoryStream);

            using var physicalFilesStorage = await _filesStorageFactory.CreateAsync();
            using var filesStorage = _infoStorageFactory.CreateFileStorage();
            await physicalFilesStorage.SaveFileAsync(file.Id.ToString(), memoryStream);
            if (!await filesStorage.AddAsync(file))
                return RequestResult.InternalServerError<(string uri, FileInfo info)>("Can't add to database");

            var chat = new Chat {Id = Guid.Empty, Name = "Ручная загрузка файла"};
            file.Chat = chat;
            using var fileSenderStorage = _infoStorageFactory.CreateFileSenderStorage();
            file.FileSender = await fileSenderStorage.GetByIdAsync(fileSender.Id);
            var downloadLink = await _downloadLinkProvider.GetDownloadLinkAsync(file.Id, file.Name);

            return RequestResult.Created<(string uri, FileInfo info)>((downloadLink,
                _fileInfoConverter.ConvertFileInfo(file)));
        }


        /// <inheritdoc />
        public async Task<RequestResult<(string Uri, FileInfo Info)>> UpdateFileAsync(Guid id, string fileName)
        {
            using var filesStorage = _infoStorageFactory.CreateFileStorage();
            using var senderStorage = _infoStorageFactory.CreateFileSenderStorage();
            var file = await filesStorage.GetByIdAsync(id, true);
            if (file is null)
                return RequestResult.NotFound<(string uri, FileInfo info)>($"File with identifier {id} not found");
            file.Name = fileName;
            await filesStorage.UpdateAsync(file);

            var downloadLink = await _downloadLinkProvider.GetDownloadLinkAsync(id, fileName);

            return RequestResult.Created<(string uri, FileInfo info)>((downloadLink,
                _fileInfoConverter.ConvertFileInfo(file)));
        }

        /// <inheritdoc />
        public async Task<RequestResult<FileInfo>> DeleteFileAsync(Guid id)
        {
            using var filesStorage = _infoStorageFactory.CreateFileStorage();

            using var physicalFilesStorage = await _filesStorageFactory.CreateAsync();
            try
            {
                var file = await filesStorage.DeleteAsync(id);
                await physicalFilesStorage.DeleteFileAsync(id.ToString());
                return file
                    ? RequestResult.NoContent<FileInfo>()
                    : RequestResult.InternalServerError<FileInfo>($"Something wrong with dataBase");
            }
            catch (Exception)
            {
                return RequestResult.NotFound<FileInfo>($"File with identifier {id} not found");
            }
        }

        /// <inheritdoc />
        public async Task<RequestResult<int>> GetFilesCountAsync(FileSearchParameters fileSearchParameters,
            HttpRequest request)
        {
            using var fileInfoStorage = _infoStorageFactory.CreateFileStorage();
            var files = await fileInfoStorage.GetAllAsync();
            var sender = await request.GetSenderFromToken(_infoStorageFactory);
            var filterFiles = await files.FilterFiles(sender!.Id, _infoStorageFactory);
            return RequestResult.Ok(filterFiles.Count);
        }

        /// <inheritdoc />
        public async Task<RequestResult<List<string>>> GetFileNamesAsync(HttpRequest request)
        {
            using var fileInfoStorage = _infoStorageFactory.CreateFileStorage();
            var files = await fileInfoStorage.GetAllAsync();
            var sender = await request.GetSenderFromToken(_infoStorageFactory);
            var filterFiles = await files.FilterFiles(sender!.Id, _infoStorageFactory);
            var filesNames = filterFiles.Select(x => x.Name).ToList();
            return RequestResult.Ok(filesNames);
        }

        /// <inheritdoc />
        public RequestResult<FileTypeDescription[]> GetFilesTypes()
        {
            var descriptions = Enum.GetValues(typeof(FileType))
                .Cast<FileType>()
                .Select(fileType => new FileTypeDescription {Id = (int) fileType, Name = fileType.GetEnumDescription()})
                .ToArray();

            return RequestResult.Ok(descriptions);
        }

        public async Task<RequestResult<string>> GetLink(Guid id, HttpRequest request)
        {
            using var filesStorage = _infoStorageFactory.CreateFileStorage();
            var file = await filesStorage.GetByIdAsync(id);
            if (file is null)
                return RequestResult.NotFound<string>($"Link with identifier {id} not found");

            var sender = await request.GetSenderFromToken(_infoStorageFactory);
            var filesToFilter = new List<DataBaseFile> {file};
            var filteredFiles = await filesToFilter.FilterFiles(sender!.Id, _infoStorageFactory);
            if (filteredFiles.Count == 0)
                return RequestResult.Unauthorized<string>("Don't have access to this link");
            var fileLink = await _downloadLinkProvider.GetDownloadLinkAsync(id, file.Name);
            var requestToS3 = WebRequest.Create(fileLink);

            using var response = await requestToS3.GetResponseAsync();
            var streamReader = new StreamReader(response.GetResponseStream());
            var text = await streamReader.ReadToEndAsync();
            return RequestResult.Ok(text);
        }
        public async Task<RequestResult<string>> GetMessage(Guid id, HttpRequest request)
        {
            using var filesStorage = _infoStorageFactory.CreateFileStorage();
            var file = await filesStorage.GetByIdAsync(id);
            if (file is null)
                return RequestResult.NotFound<string>($"Message with identifier {id} not found");

            var sender = await request.GetSenderFromToken(_infoStorageFactory);
            var filesToFilter = new List<DataBaseFile> {file};
            var filteredFiles = await filesToFilter.FilterFiles(sender!.Id, _infoStorageFactory);
            if (filteredFiles.Count == 0)
                return RequestResult.Unauthorized<string>("Don't have access to this message");
            var fileLink = await _downloadLinkProvider.GetDownloadLinkAsync(id, file.Name);
            var requestToS3 = WebRequest.Create(fileLink);

            using var response = await requestToS3.GetResponseAsync();
            var streamReader = new StreamReader(response.GetResponseStream());
            var text = await streamReader.ReadToEndAsync();
            return RequestResult.Ok(text);
        }
    }
}