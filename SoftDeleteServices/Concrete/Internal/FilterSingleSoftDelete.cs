// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using SoftDeleteServices.Configuration;

namespace SoftDeleteServices.Concrete.Internal
{
    public class FilterSingleSoftDelete<TInterface>
        where TInterface : class
    {
        private readonly SoftDeleteConfiguration<TInterface, bool> _config;

        public FilterSingleSoftDelete(SoftDeleteConfiguration<TInterface, bool> config)
        {
            _config = config;
        }




    }
}