# Internal Types
There are two Types that are only used internally and may not be declared by a user. However, if you choose to use the REPL and view Parse Trees you may occasionally notice these Types.

## The Null Type
The Null Type is not really a Type at all. Null Types represent nothing, also known as Null. The Binder may insert Null Types into your program when it encounters invalid code to ensure it does not crash while Binding your program.

## The Any Type
The Any Type is the return Type used for the `$eval` Function. The `$eval` Function is wrapped around your entire script at compile time by the Lowerer. The Any Type simply represents a return value of an unknown Type. 
