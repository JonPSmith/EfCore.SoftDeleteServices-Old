// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace SoftDeleteServices.Configuration
{
    public interface ISoftDeleteAccess<TInterface>
        where TInterface : class
    {
        TInterface CurrentEntity { get; }

        bool IsSoftDelete { get; }
        bool GetSoftDeleteValue { get; }
        Action<bool> SetSoftDeleteValue { get; }
    }
}