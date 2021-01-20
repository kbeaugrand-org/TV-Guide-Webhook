using System;
using System.Collections.Generic;
using System.Text;

namespace google_dialog.Intents.GoogleDialogFlow
{
    public class SynonymEntryDisplayImage
    {
        public string Url { get; set; }

        public string Alt { get; set; }
    }

    public class SynonymEntryDisplay { 
        public string Title { get; set; }

        public string Description { get; set; }

        public SynonymEntryDisplayImage Image { get; set; }
    }

    public class TypeOverrideSynonymEntry
    {
        public string Name { get; set; }

        public IEnumerable<string> Synonyms { get; set; }

        public SynonymEntryDisplay Display { get; set; }
    }

    public class TypeOverrideSynonym
    {
        public List<TypeOverrideSynonymEntry> Entries { get; set; }
    }

    public class TypeOverride
    {
        public string Name { get; set; }

        public string TypeOverrideMode { get; set; }

        public TypeOverrideSynonym Synonym { get; set; }
    }
}
