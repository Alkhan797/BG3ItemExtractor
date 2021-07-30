using System.Collections.Generic;

namespace StatDataLibrary.Services.Dto
{
    public class EntryTypeSelector
    {
        /// <summary> Name of the entry type/.txt file to be processed </summary>
        /// <remarks>Serves as an item type classifier, as .txt files regroup entries by categories/type</remarks>
        public string FileTypeName;

        /// <summary> Specific list of entries to be extracted based on their "using" property </summary>
        public IEnumerable<string> SubTypeWhiteList;

        /// <summary> Whether to use only (true), exclude (false) or include (null) base templates </summary>
        /// <remarks>
        /// Base templates are used to define the generic categories of items.
        /// Generally they either match an item category or an equipment slot
        /// </remarks>
        public bool? UseBases;

        /// <summary> Whether to only use (true), exclude (false) or include (null) reference templates </summary>
        /// <remarks> Reference templates are (probably) mainly used for development and testing by the devs </remarks>
        public bool? UseReferences;

        /// <summary>
        /// Creates a new EntryTypeSelector used to filter entries
        /// </summary>
        /// <param name="fileTypeName">Name of the entry type/.txt file to be processed</param>
        /// <param name="subTypeWhiteList">Specific list of entries to be extracted based on their "using" property</param>
        /// <param name="useBases">Whether to use only (true), exclude (false) or include (null) base templates</param>
        /// <param name="useReferences">Whether to only use (true), exclude (false) or include (null) reference templates</param>
        public EntryTypeSelector(
            string fileTypeName,
            IEnumerable<string> subTypeWhiteList = null,
            bool? useBases = null,
            bool? useReferences = null)
        {
            FileTypeName = fileTypeName;
            SubTypeWhiteList = subTypeWhiteList;
            UseBases = useBases;
            UseReferences = useReferences;
        }
    }
}
