// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.Interfaces;
using SoftDeleteServices.Configuration;

namespace Test.ExampleConfigs
{
    public class ConfigSoftDeleteDDD : SoftDeleteConfiguration<ISingleSoftDeletedDDD, bool>
    {

        public ConfigSoftDeleteDDD()
        {
            GetSoftDeleteValue = entity => entity.SoftDeleted;
            SetSoftDeleteValue = (entity, value) => entity.ChangeSoftDeleted(value);
        }
    }
}