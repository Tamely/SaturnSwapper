namespace Radon.CodeAnalysis.Emit;

public enum OpCode : byte
{
    // - - - - - - - - - -
    //
    // Operations
    //
    // - - - - - - - - - -
    
    
    
    // No Operation
    Nop,
    
    
    
    // - - - - - - - - - -
    // Binary Arithmetic
    // - - - - - - - - - -
    
    
    
    // Adds the top two values on the stack and pushes the result onto the stack. 
    Add,
    
    // Subtracts the top two values on the stack and pushes the result onto the stack.
    Sub,
    
    // Multiplies the top two values on the stack and pushes the result onto the stack.
    Mul,
    
    // Divides the top two values on the stack and pushes the result onto the stack.
    Div,
    
    // Concatenates the top two values on the stack and pushes the result onto the stack.
    Cnct,
    
    // Divides the top two values on the stack and pushes the remainder onto the stack.
    Mod,
    
    // Performs a bitwise OR operation on the top two values on the stack and pushes the result onto the stack.
    Or,
    
    // Performs a bitwise AND operation on the top two values on the stack and pushes the result onto the stack.
    And,
    
    // Performs a bitwise XOR operation on the top two values on the stack and pushes the result onto the stack.
    Xor,
    
    // Shifts the top value on the stack to the left by the specified amount and pushes the result onto the stack.
    Shl,
    
    // Shifts the top value on the stack to the right by the specified amount and pushes the result onto the stack.
    Shr,
    
    
    
    // - - - - - - - - - -
    // Unary Arithmetic
    // - - - - - - - - - -
    
    
    
    // Pops the current value from the top of the stack and pushes the negated value onto the stack.
    Neg = 25,
    
    
    
    // - - - - - - - - - -
    // Stack manipulation
    // - - - - - - - - - -
    
    
    
    // Loads a constant onto the stack.
    Ldc = 50,
    
    // Loads the length of an array onto the stack.
    Ldlen,
    
    // Loads a string constant onto the stack.
    Ldstr,
    
    // Loads the default value for the given type onto the stack.
    Lddft,
    
    // Loads the local variable at a specific index onto the stack.
    Ldloc,
    
    // Pops the current value from the top of the stack and stores it in a local variable at a specific index.
    Stloc,
    
    // Loads the argument at a specific index onto the stack.
    Ldarg,
    
    // Pops the current value from the top of the stack and stores it in an argument at a specific index.
    Starg,
    
    // Loads the field at a specific index onto the stack.
    Ldfld,
    
    // Pops the current value from the top of the stack and stores it in a field at a specific index.
    Stfld,
    
    // Loads the static field at a specific index onto the stack.
    Ldsfld,
    
    // Pops the current value from the top of the stack and stores it in a static field at a specific index.
    Stsfld,
    
    // Loads the instance of the current type onto the stack.
    Ldthis,
    
    // Loads the element at a specific index from an array onto the stack.
    Ldelem,
    
    // Pops the current value from the top of the stack and stores it in an element at a specific index.
    Stelem,
    
    // Loads the address of the specified field onto the stack.
    Ldflda,
    
    // Loads the address of the specified static field onto the stack.
    Ldsflda,
    
    // Loads the address of the specified local variable onto the stack.
    Ldloca,
    
    // Loads the address of the specified argument onto the stack.
    Ldarga,
    
    // Loads the address of the specified element onto the stack.
    Ldelema,
    
    // Loads the value at the address on the top of the stack onto the stack.
    Ldind,
    
    // Pops the current value from the top of the stack and stores it at the address on the top of the stack.
    Stind,
    
    // Loads the type token for the specified type onto the stack.
    Ldtype,
    
    // - - - - - - - - - - - - - - -
    // Miscellaneous Instructions
    // - - - - - - - - - - - - - - -
    
    
    
    // Converts the value on the top of the stack to the specified type.
    Conv = 75,
    
    
    
    // - - - - - - - - - -
    //
    // Control Flow
    //
    // - - - - - - - - - -
    
    
    
    // - - - - - - - - - -
    // New objects
    // - - - - - - - - - -
    
    
    
    // Creates a new zero-based, one-dimensional array and pushes it onto the stack.
    Newarr = 100,
    
    // Creates a new object and pushes it onto the evaluation stack.
    Newobj,
    
    
    
    // - - - - - - - - - -
    // Control Transfer
    // - - - - - - - - - -
    
    
    
    // Calls the method at a specific index.
    Call = 110,
    // Pops the cu
    // rrent value from the top of the stack and returns it; no value is returned if the method is void.
    Ret,
    
    // Transfers control to a target instructions if the value on the top of the stack is true.
    Brtrue,
    
    // Transfers control to a target instructions if the value on the top of the stack is false.
    Brfalse,
    
    // Unconditionally transfers control to a target instruction.
    Br,
    
    
    
    // - - - - - - - - - -
    // Comparison
    // - - - - - - - - - -
    
    
    
    // Compares two values. If they are equal, the integer value 1 is pushed onto the evaluation stack;
    // otherwise 0 is pushed onto the evaluation stack
    Ceq = 125,
    
    // Compares two values. If they are not equal, the integer value 1 is pushed onto the evaluation stack;
    // otherwise 0 is pushed onto the evaluation stack
    Cne,
    
    // Compares two values. If the first value is greater than the second value, the integer value 1 is pushed onto the evaluation stack;
    // otherwise 0 is pushed onto the evaluation stack
    Cgt,
    
    // Compares two values. If the first value is greater than or equal to the second value, the integer value 1 is pushed onto the evaluation stack;
    // otherwise 0 is pushed onto the evaluation stack
    Cge,
    
    // Compares two values. If the first value is less than the second value, the integer value 1 is pushed onto the evaluation stack;
    // otherwise 0 is pushed onto the evaluation stack
    Clt,
    
    // Compares two values. If the first value is less than or equal to the second value, the integer value 1 is pushed onto the evaluation stack;
    // otherwise 0 is pushed onto the evaluation stack
    Cle,
}