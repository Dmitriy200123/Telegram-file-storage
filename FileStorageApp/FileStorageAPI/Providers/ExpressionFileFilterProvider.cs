﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FileStorageApp.Data.InfoStorage.Models;

namespace FileStorageAPI.Providers
{
    /// <inheritdoc />
    public class ExpressionFileFilterProvider : IExpressionFileFilterProvider
    {
        /// <inheritdoc />
        public Expression<Func<File, bool>> GetExpression(FileSearchParameters parameters, List<Guid>? chatsId = null)
        {
            var categories = parameters.Categories?.Cast<int>().ToList();
            return x => (parameters.FileName == null || x.Name == parameters.FileName) &&
                        (parameters.DateFrom == null || parameters.DateFrom <= x.UploadDate) &&
                        (parameters.DateTo == null || parameters.DateTo >= x.UploadDate) &&
                        (categories == null || categories.Contains(x.TypeId)) &&
                        (chatsId == null || chatsId.Contains(x.ChatId!.Value) || x.ChatId == null) &&
                        (parameters.SenderIds == null || parameters.SenderIds.Contains(x.FileSenderId)) &&
                        (parameters.ChatIds == null || x.ChatId != null && parameters.ChatIds.Contains(x.ChatId.Value) ||
                        x.ChatId == null && parameters.ChatIds.Contains(Guid.Empty));
        }

        /// <inheritdoc />
        public Expression<Func<File, bool>> GetDocumentExpression(FileSearchParameters parameters, List<Guid>? fileIds, 
            List<Guid>? chatsId = null, List<Guid>? classificationIds = null)
        {
            var basicExpression = GetExpression(parameters, chatsId);
            var expressionWithFileIds = TryCreateFileIdsExpression(basicExpression, chatsId);
            var expressionWithClassification = TryCreateClassificationExpression(expressionWithFileIds, classificationIds);
           
            return expressionWithClassification ;
        }

        private Expression<Func<File, bool>> TryCreateFileIdsExpression(Expression<Func<File, bool>> basicExpression, List<Guid>? chatsId)
        {
            var param = Expression.Parameter(typeof(File), "x");
            if (chatsId is null)
                return basicExpression;
            Expression<Func<File, bool>> currentExpression = x => chatsId.Contains(x.Id);
            var bodyResult = Expression.AndAlso(Expression.Invoke(basicExpression, param), Expression.Invoke(currentExpression, param));
            var lambda = Expression.Lambda<Func<File, bool>>(bodyResult, param);
            return lambda;
        }

        private Expression<Func<File, bool>> TryCreateClassificationExpression(Expression<Func<File, bool>> basicExpression, 
            List<Guid>? classificationIds)
        {
            var param = Expression.Parameter(typeof(File), "x");
            if (classificationIds is null)
                return basicExpression;
            Expression<Func<File, bool>> currentExpression = x => classificationIds.Contains(x.ClassificationId!.Value);
            var bodyResult = Expression.AndAlso(Expression.Invoke(basicExpression, param), Expression.Invoke(currentExpression, param));
            var lambda = Expression.Lambda<Func<File, bool>>(bodyResult, param);
            return lambda;
        }
    }
}