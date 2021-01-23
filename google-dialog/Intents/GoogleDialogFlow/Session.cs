using System;
using System.Collections.Generic;
using System.Text;

namespace google_dialog.Intents.GoogleDialogFlow
{
    public class Session
    {
        private List<TypeOverride> typeOverrides;

        public string Id { get; set; }

        public Dictionary<string, dynamic> Params { get; set; }

        public IReadOnlyList<TypeOverride> TypeOverrides => typeOverrides;

        public void AddTypeOverride(TypeOverride item)
        {
            this.typeOverrides = this.typeOverrides ?? new List<TypeOverride>();

            this.typeOverrides.Add(item);
        }
    }
}
