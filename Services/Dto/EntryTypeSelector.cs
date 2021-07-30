using System.Collections.Generic;

namespace GetAllItemsBG3.Services.Dto
{
    public class EntryTypeSelector
    {
        public string FileTypeName;
        public IEnumerable<string> SubTypeWhiteList;
        public bool? UseBase;
        public bool? UseReferences;

        public EntryTypeSelector(
            string fileTypeName,
            IEnumerable<string> subTypeWhiteList = null,
            bool? useBase = null,
            bool? useReferences = null)
        {
            FileTypeName = fileTypeName;
            SubTypeWhiteList = subTypeWhiteList;
            UseBase = useBase;
            UseReferences = useReferences;
        }
    }
}
