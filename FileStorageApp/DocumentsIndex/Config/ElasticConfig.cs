﻿using System;

namespace DocumentsIndex.Config
{
    /// <inheritdoc />
    public class ElasticConfig : IElasticConfig
    {
        /// <inheritdoc />
        public string Uri { get; }
        
        /// <inheritdoc />
        public string Index { get; }

        /// <summary>
        /// Создание эластик конфига
        /// </summary>
        /// <param name="uri">Uri по которому можно подключиться к эластику</param>
        /// <param name="index">Название индекса по которому будет происходить индексация</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ElasticConfig(string uri, string index)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            Index = index ?? throw new ArgumentNullException(nameof(index));
        }
    }
}