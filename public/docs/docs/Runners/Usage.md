# Running Sea Files
There are two ways to run Sea Files.
Note: This usage of both the Compiler and the REPL is currently *command-line based*. This will hopefully be changed in the future.<br>

## Using the Compiler
Using Shore, the Sea Compiler, is the best way to run Sea files! <br>
To start, ensure you have cloned this repository to your local machine (or just the /Shore folder).<br>
Once you have the files, simply open a command window and move into the `/Shore` directory.<br>
After you're in that directory simply run `sc <path>` with the `<path>` being the path to your Sea file or a directory containing Sea files.<br>
Be sure to include the file extension in the file path.<br><br>
Example: `sc ../Examples`

## Using the REPL
Using the [Shore REPL](/#/docs/runners/SR) is an easy way to quickly test Sea scripts.<br>
Though the REPL is typically used to test small scripts written directly within the REPL, you can use the `#load <path>` to load an entire script into the REPL<br>
Note when using the REPL you can only load single files, not entire directories.<br>
Be sure to include the file extension in the file path.<br><br>
Example: `#load ../Examples/helloworld.sea`
