﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FileStorageApp.Data.InfoStorage.Models;

namespace FileStorageAPI.Providers
{
    /// <summary>
    /// Провайдер для получения прав пользователя по Id пользователя
    /// </summary>
    public interface IAccessesByUserIdProvider
    {
        /// <summary>
        /// Возвращает права для переданного Id пользователя
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        Task<IEnumerable<Accesses>> GetAccessesByUserIdAsync(Guid userId);
    }
}