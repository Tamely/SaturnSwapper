namespace Radon.CodeAnalysis.Emit;

public enum OpCode : ushort
{
    NoOperandMask = 0x1,
    ShiftAmount = 4,

    // Represents no operation
    Nop = 0,

    // Represents an addition operation
    Add = (15 << ShiftAmount) | NoOperandMask,

    // Represents a subtraction operation
    Sub = ((((Add ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount) | NoOperandMask,

    // Represents a multiplication operation
    Mul = ((((Sub ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount) | NoOperandMask,

    // Represents a division operation
    Div = ((((Mul ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount) | NoOperandMask,

    // Represents a concatenation operation
    Concat = ((((Div ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount) | NoOperandMask,


    // Represents loading a constant onto the evaluation stack
    Ldc = (((Concat ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,

    // Represents loading a string constant onto the evaluation stack
    Ldstr = (((Ldc ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,
    
    // Loads the default value for the given type onto the evaluation stack
    Lddft = (((Ldstr ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,


    // Loads the local variable at a specific index onto the evaluation stack
    Ldloc = (((Lddft ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,

    // Pops the current value from the top of the evaluation stack and stores it in a local variable
    Stloc = (((Ldloc ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,

    // Loads the argument at a specific index onto the evaluation stack
    Ldarg = (((Stloc ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,

    // Loads the field of an object pushed onto the stack onto the evaluation stack
    Ldfld = (((Ldarg ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,

    // Loads the static field specified onto the evaluation stack
    Ldsfld = (((Ldfld ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,
    
    Ldenum = (((Ldsfld ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,

    // Loads the current instance onto the evaluation stack
    Ldthis = (((Ldenum ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount | NoOperandMask,

    // Replaces the value of a field on the current instance or type with a new value
    Stfld = (((Ldthis ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,

    // Replaces the static field specified with a new value
    Stsfld = (((Stfld ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,

    // Loads the element at a specified offset from an object, array or a value type onto the evaluation stack as the type specified in the instruction
    Ldelem = (((Stsfld ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,

    // Replaces the value at the specified index from the array or value type with a new value
    Stelem = ((((Ldelem ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount) | NoOperandMask,


    // Calls a method specified in the instruction
    Call = (((Stelem ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,
    
    // Imports an archive using the specified path
    Import = (((Call ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,

    // Indicates that the return value is passed back to the caller
    Ret = (((Import ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,
    
    // Converts an expression to a specified type
    Conv = (((Ret ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,

    // Creates a new zero-based, one-dimensional array and initializes it with a specific number of elements
    Newarr = (((Conv ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,

    // Creates a new object or a new instance of a value type
    Newobj = (((Newarr ^ NoOperandMask) >> ShiftAmount) + 1) << ShiftAmount,
}
