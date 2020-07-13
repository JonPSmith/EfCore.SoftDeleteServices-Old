// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.Interfaces;
using SoftDeleteServices.Configuration;

namespace Test.ExampleConfigs
{
    public class ConfigCascadeDelete : SoftDeleteConfiguration<ICascadeSoftDelete, byte>
    {

        public ConfigCascadeDelete()
        {
            GetSoftDeleteValue = entity => entity.SoftDeleteLevel;
            SetSoftDeleteValue = (entity, value) => { entity.SoftDeleteLevel = value; };
        }

    }
}