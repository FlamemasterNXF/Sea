using System.Collections.Specialized;
namespace Sea{
    public class ErrorConfig
    {
        // Edit this Dictionary to change what certain Flags do!
        private Dictionary<string, string> EDIT_ME_HUMANS = new Dictionary<string, string>()
        {
            // NONE, WARN, or ERROR
            // Don't change FORBIDDEN things unless you want your code to break :)
            // Make sure you read the line above this one ^^^^^^^^^

            {"NO_ACCESS_MOD", "ERROR"},        
            {"NO_MOD", "NONE"},
            {"NO_TYPE", "ERROR"},
            {"NO_NAME", "FORBIDDEN"}, //Don't change me!
            {"NULL_VALUE", "WARN"},
            {"DUPLICATE_VARIABLE", "ERROR"},
            {"COMP_INEQ_TYPES", "ERROR"},
            {"INT_TO_FLOAT", "ERROR"},
            {"APPROX_EQUALS_INT", "ERROR"},
            {"UNSIGNED_FLOAT", "FORBIDDEN"}, //Don't change me!
            {"OVERFLOW_UNDERFLOW", "ERROR"},
            {"CHAR_MULTIPLE_CHARS", "ERROR"},
            {"NO_FUNC_RETURN_TYPE", "ERROR"},
            {"DUPLICATE_FUNC", "NONE"},
            {"REASSIGN_CONST", "FORBIDDEN"}, //Don't change me!
            {"GLOBAL_IN_CLASS", "ERROR"},
            {"EMBEDDED_OUTSIDE_CLASS", "ERROR"},
            {"DUPLICATE_CLASS", "FORBIDDEN"}, //Don't change me!
        };
        // Don't edit beyond this point :)

        private byte ReadableToValue(string readable){
            if(readable=="NONE") return 0;
            if(readable=="WARN") return 1;
            if(readable=="ERROR") return 2;
            if(readable=="FORBIDDEN") return 2;
            else return 0;
        }
        internal static List<string> errorNames = new List<string>{};
        internal static List<byte> errorFlags = new List<byte>{};
        internal void ErrorSetup(){
            foreach (KeyValuePair<string, string> entry in EDIT_ME_HUMANS)
            {
                errorNames.Add(entry.Key);
                errorFlags.Add(ReadableToValue(entry.Value));
                Console.WriteLine(ReadableToValue(entry.Value));
            } 
        }
    };
}