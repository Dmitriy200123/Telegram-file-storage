﻿using System;
using System.Threading.Tasks;

namespace FileStorageAPI.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDownloadLinkProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<string> GetDownloadLink(Guid id);
    }
}