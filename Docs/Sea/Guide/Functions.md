# Functions
Functions are a core feature of Sea. They are declared with a name and a return Type.

## Declaration
You can declare a Function by using the `function <return type> <name>(<arg syntax?>) { ... }` syntax.<br>

## Calling 
Functions can be called with the `<name>(<args values?>) syntax.`<br>

## Return Types
A Function must be assigned a return Type. This specifies the Type of the value that the Function will return. Any Function that is not of Type Void must return a value corresponding to its return Type.<br>
Learn more about the [Void Type](./Types/Void.md)<br>
Learn more about [Types](./Types.md)<br>

## Arguments 
You can declare arguments for any given Function with the `<type> <name>, <type2> <name2>, ...` syntax. These arguments must all be given a value when the Function is called.<br>

## The Main Function
The Main Function is an optional Function which you can declare at the top of your Sea file. If a Sea File uses this Function Global Statements (Statements outside of Functions) are no longer allowed. This Function will be evaluated and ran at runtime in place of the Globals. The Main Functions must always be of Type Void and must have zero arguments.<br>
Learn more about the [Void Type](./Types/Void.md)<br>
Learn more about [Global Statements](./GlobalStatements.md)<br>

## Built-In Functions
Sea includes two built-in functions that come packaged with the language.<br>
- `print(<any>)`: Prints an input to the console
- `input()`: Stops program execution and asks the user for an input of the String Type. Resumes program execution when the input is given.
- `round(<float>)`: Rounds a Floating Point Number to the nearest Integral Number.
- `ceil(<float>)`: Returns the smallest Integral Value that is greater than a given Floating Point Number.
- `floor(<float>)`: Returns the largest Integral Value that is less than a given Floating Point Number.
- `length(<array OR string>)`: Returns the length of an Array or String as an integer.
- `unixTime()`: Returns the Unix Timestamp as an integer (includes milliseconds).
- `sleep(<int>)`: Pauses program execution for a given amount of milliseconds.
