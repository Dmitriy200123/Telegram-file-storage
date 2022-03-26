﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentsIndex.Model;

namespace DocumentsIndex
{
    /// <summary>
    /// Хранилище файлов в эластике
    /// </summary>
    public interface IDocumentIndexStorage
    {
        /// <summary>
        /// Поиск документов, в которых присутствует заданная подстрока
        /// </summary>
        /// <param name="subString">подстрока по которой происходит поиск</param>
        /// <returns></returns>
        Task<IEnumerable<Guid>> SearchBySubstringAsync(string subString);

        /// <summary>
        /// Поиск документов по названию
        /// </summary>
        /// <param name="name">имя документа, которое необходимо найти</param>
        /// <returns></returns>
        Task<IEnumerable<Guid>> SearchByNameAsync(string name);

        /// <summary>
        /// Индексация (добавление) документа в эластик
        /// </summary>
        /// <param name="document">документ, который необходимо добавить</param>
        /// <returns></returns>
        Task<bool> IndexDocumentAsync(Document document);

        /// <summary>
        /// Удаление документа из эластика
        /// </summary>
        /// <param name="guid">Идентификатор документа, который нужно удалить</param>
        /// <returns></returns>
        Task<bool> DeleteAsync(Guid guid);

        Task<IEnumerable<Guid>> FindInTextOrNameAsync(string query);

        Task<bool> IsContainsInNameAsync(Guid documentId, string[] subStrings);
    }
}