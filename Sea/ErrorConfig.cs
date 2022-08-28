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

            // REGION: VARIABLE/OBJECT DECLARATION
            {"NO_ACCESS_MOD", "ERROR"},        
            {"NO_MOD", "NONE"},
            {"NO_TYPE", "ERROR"},
            {"NO_NAME", "FORBIDDEN"}, //Don't change me!
            {"NULL_VALUE", "WARN"}, //No "=" when making a variable.
            {"NO_VALUE", "FORBIDDEN"}, //Don't change me! | "=" used but no value assigned.
            {"DUPLICATE_VARIABLE", "ERROR"},
            
            // REGION: COMPARISONS 
            {"COMP_INEQ_TYPES", "ERROR"},

            // REGION: NUMBERS
            {"INT_TO_FLOAT", "ERROR"},
            {"APPROX_EQUALS_INT", "ERROR"},
            {"OVERFLOW_UNDERFLOW", "ERROR"},

            // REGION: MATH
            {"EXPECTED_OP", "FORBIDDEN"}, //Don't change me!
            {"EXPECTED_NUM", "FORBIDDEN"}, //Don't change me!

            // REGION: STRINGS AND CHARACTERS
            {"CHAR_MULTIPLE_CHARS", "ERROR"},

            // REGION: FUNCTIONS
            {"NO_FUNC_RETURN_TYPE", "ERROR"},
            {"DUPLICATE_FUNC", "NONE"},

            // REGION: CLASSES
            {"GLOBAL_IN_CLASS", "ERROR"},
            {"EMBEDDED_OUTSIDE_CLASS", "ERROR"},
            {"DUPLICATE_CLASS", "FORBIDDEN"}, //Don't change me!

            // REGION: MISC. TYPE SAFETY
            {"REASSIGN_CONST", "FORBIDDEN"}, //Don't change me!
            {"UNSIGNED_FLOAT", "FORBIDDEN"}, //Don't change me!
        };
        // Don't edit beyond this point :)

        private byte ReadableToValue(string readable){
            if(readable=="NONE") return 1;
            if(readable=="WARN") return 2;
            if(readable=="ERROR") return 3;
            if(readable=="FORBIDDEN") return 3;
            else{ Message._throw(3, $"Invalid Error Flag Value.\n\"{readable}\" is not a valid Flag Value."); return 0; };
        }
        internal static Dictionary<string, byte> _errors = new Dictionary<string, byte>(){};
        internal void ErrorSetup(){
            foreach (KeyValuePair<string, string> entry in EDIT_ME_HUMANS)
            {
                _errors.Add(entry.Key, ReadableToValue(entry.Value));
            } 
        }
    };
}