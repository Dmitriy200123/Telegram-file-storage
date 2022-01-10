﻿namespace JwtAuth
{
    /// <summary>
    /// Класс отвечающий за создание рефреш токена.
    /// </summary>
    public interface IRefreshTokenGenerator
    {
        /// <summary>
        /// Создать рефреш токен.
        /// </summary>
        string GenerateToken();
    }
}