using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetAllItemsBG3.Services.Dto
{
    public class EntryTypeSelector
    {
        public string FileTypeName;
        public IEnumerable<string> SubTypeWhiteList;
        public bool? IncludeTemplates;
        public bool? IncludeReferences;

        public EntryTypeSelector(
            string fileTypeName,
            IEnumerable<string> subTypeWhiteList = null,
            bool? includeTemplates = false,
            bool? includeReferences = false)
        {
            FileTypeName = fileTypeName;
            SubTypeWhiteList = subTypeWhiteList;
            IncludeTemplates = includeTemplates;
            IncludeReferences = includeReferences;
        }
    }
}
