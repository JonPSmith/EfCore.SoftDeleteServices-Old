// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayer.Interfaces;

namespace SoftDeleteServices.Concrete
{

    public interface ITestInterfacePart<TInterface>
        where TInterface : class
    {
        TInterface Base { get; set; }

        bool IsSoftDelete { get; }
        bool GetSoftDeleteValue { get; }
        Action<bool> SetSoftDeleteValue { get; }
    }

    public class UseInterface : ITestInterfacePart<ISoftDelete>
    {
        public UseInterface(object xxx)
        {
            Base = xxx as ISoftDelete;
        }

        public ISoftDelete Base { get; set; }

        public bool IsSoftDelete => Base != null;
        public bool GetSoftDeleteValue => Base.SoftDeleted;
        public Action<bool> SetSoftDeleteValue => input => Base.SoftDeleted = input;
    }

    public class UseUse
    {
        public UseInterface Acc { get; set; }

        public void DoSomething()
        {
            var value = Acc.GetSoftDeleteValue;
            Acc.SetSoftDeleteValue(false);
        }
    }
}